namespace BasAutomation.Geometry.Curves;

/// <summary>
/// Represents a circle in 3D space defined by center, radius, and normal vector.
/// </summary>
public readonly struct Circle3D : IEquatable<Circle3D>, IGeometryComparable<Circle3D>, ICurve3D
{
    /// <summary>
    /// Gets or sets the center point of the circle.
    /// </summary>
    public Point3D Center { get; }

    /// <summary>
    /// Gets or sets the radius of the circle.
    /// </summary>
    public double Radius { get; }

    /// <summary>
    /// Gets or sets the normal vector of the plane containing the circle (normalized).
    /// </summary>
    public Vector3D Normal { get; }

    /// <summary>
    /// Initializes a new instance of Circle3D from center, radius, and normal vector.
    /// </summary>
    /// <param name="center">The center point of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <param name="normal">The normal vector of the plane containing the circle.</param>
    public Circle3D(Point3D center, double radius, Vector3D normal)
    {
        if (radius < 0)
            throw new ArgumentException("Radius must be non-negative.", nameof(radius));

        Center = center;
        Radius = radius;
        Normal = normal.Normalized();
    }

    /// <summary>
    /// Creates a circle from three points in 3D space.
    /// </summary>
    /// <param name="p1">First point on the circle.</param>
    /// <param name="p2">Second point on the circle.</param>
    /// <param name="p3">Third point on the circle.</param>
    /// <returns>A new Circle3D instance.</returns>
    public static Circle3D FromThreePoints(Point3D p1, Point3D p2, Point3D p3)
    {
        // Calculate vectors from p1 to p2 and p1 to p3
        var v1 = p2 - p1;
        var v2 = p3 - p1;

        // Calculate normal vector
        var normal = v1.Cross(v2);
        if (normal.LengthSquared < Constants.Tolerance * Constants.Tolerance)
        {
            throw new ArgumentException("The three points are collinear and do not define a unique circle.");
        }
        normal = normal.Normalized();

        // Create plane from the three points
        var plane = Plane3D.FromThreePoints(p1, p2, p3);

        // Calculate center using circumcenter formula in the plane
        var d1 = v1.LengthSquared;
        var d2 = v2.LengthSquared;
        var d3 = v1.Dot(v2);

        var denominator = 2 * (d1 * d2 - d3 * d3);
        if (Math.Abs(denominator) < Constants.Tolerance)
        {
            throw new ArgumentException("The three points are collinear and do not define a unique circle.");
        }

        var s = (d1 * d2 - d2 * d3) / denominator;
        var t = (d2 * d1 - d1 * d3) / denominator;

        var center = p1 + s * v1 + t * v2;
        var radius = center.DistanceTo(p1);

        return new Circle3D(center, radius, normal);
    }

    /// <summary>
    /// Gets the diameter of the circle.
    /// </summary>
    public double Diameter => 2 * Radius;

    /// <summary>
    /// Gets the circumference of the circle.
    /// </summary>
    public double Circumference => 2 * Math.PI * Radius;

    /// <summary>
    /// Gets the area of the circle.
    /// </summary>
    public double Area => Math.PI * Radius * Radius;

    /// <summary>
    /// Gets the plane containing the circle.
    /// </summary>
    public Plane3D Plane => new Plane3D(Center, Normal);

    // ICurve3D implementation
    public Point3D StartPoint => PointAt(0);
    public Point3D EndPoint => StartPoint;
    public double Length => Circumference;

    /// <summary>
    /// Gets a point on the circle at the specified parameter.
    /// </summary>
    /// <param name="t">The parameter (0 to 1 for one full revolution).</param>
    /// <returns>The point at the specified parameter.</returns>
    public Point3D PointAt(double t)
    {
        // Create an orthonormal basis for the plane
        var xAxis = GetXAxis();
        var yAxis = Normal.Cross(xAxis).Normalized();

        var angle = t * 2 * Math.PI;
        var localPoint = Radius * Math.Cos(angle) * xAxis + Radius * Math.Sin(angle) * yAxis;

        return Center + localPoint;
    }

    /// <summary>
    /// Gets the tangent vector at the specified parameter.
    /// </summary>
    /// <param name="t">The parameter (0 to 1 for one full revolution).</param>
    /// <returns>The normalized tangent vector at the specified parameter.</returns>
    public Vector3D TangentAt(double t)
    {
        var xAxis = GetXAxis();
        var yAxis = Normal.Cross(xAxis).Normalized();

        var angle = t * 2 * Math.PI;
        var tangent = -Math.Sin(angle) * xAxis + Math.Cos(angle) * yAxis;

        return tangent.Normalized();
    }

    /// <summary>
    /// Gets the normal vector at the specified parameter.
    /// </summary>
    /// <param name="t">The parameter (0 to 1 for one full revolution).</param>
    /// <param name="upHint">A hint vector for determining the normal direction (not used for circles).</param>
    /// <returns>The normal vector pointing toward the center.</returns>
    public Vector3D NormalAt(double t)
    {
        var point = PointAt(t);
        return (Center - point).Normalized();
    }

    /// <summary>
    /// Gets the parameter at the specified distance along the circle.
    /// </summary>
    /// <param name="distance">The distance along the circle.</param>
    /// <returns>The parameter value.</returns>
    public double ParameterAtDistance(double distance)
    {
        return distance / Circumference;
    }

    /// <summary>
    /// Creates a copy of the circle.
    /// </summary>
    /// <returns>A new circle instance.</returns>
    public ICurve3D Clone()
    {
        return new Circle3D(Center, Radius, Normal);
    }

    /// <summary>
    /// Determines if a point lies on the circle within the specified tolerance.
    /// </summary>
    /// <param name="point">The point to test.</param>
    /// <param name="tolerance">The tolerance for the test.</param>
    /// <returns>true if the point is on the circle; otherwise, false.</returns>
    public bool ContainsPoint(Point3D point, double tolerance = Constants.Tolerance)
    {
        // Check if point is on the plane
        var plane = new Plane3D(Center, Normal);
        if (!plane.ContainsPoint(point, tolerance))
            return false;

        // Check if point is at the correct distance from center
        var distance = point.DistanceTo(Center);
        return Math.Abs(distance - Radius) < tolerance;
    }

    /// <summary>
    /// Projects a point onto the circle.
    /// </summary>
    /// <param name="point">The point to project.</param>
    /// <returns>The closest point on the circle.</returns>
    public Point3D ProjectPoint(Point3D point)
    {
        // First project the point onto the plane
        var plane = new Plane3D(Center, Normal);
        var projectedPoint = plane.ProjectPoint(point);

        // If projected point is at center, return any point on circle
        var toProjected = projectedPoint - Center;
        if (toProjected.LengthSquared < Constants.Tolerance * Constants.Tolerance)
        {
            return PointAt(0);
        }

        // Scale the vector to radius length
        return Center + toProjected.Normalized() * Radius;
    }

    /// <summary>
    /// Transforms the circle by the specified transformation matrix.
    /// </summary>
    /// <param name="transformation">The transformation matrix.</param>
    /// <returns>The transformed circle.</returns>
    public Circle3D Transform(Matrix3D transformation)
    {
        // Transform center and normal
        var transformedCenter = transformation.Transform(Center);
        var transformedNormal = transformation.Transform(Normal).Normalized();

        // Transform two points on the circle to determine scale
        var p1 = PointAt(0);
        var p2 = PointAt(0.25); // 90 degrees
        var tp1 = transformation.Transform(p1);
        var tp2 = transformation.Transform(p2);

        // Calculate average radius (assumes uniform scaling)
        var r1 = transformedCenter.DistanceTo(tp1);
        var r2 = transformedCenter.DistanceTo(tp2);
        var transformedRadius = (r1 + r2) / 2;

        return new Circle3D(transformedCenter, transformedRadius, transformedNormal);
    }

    /// <summary>
    /// Converts this circle to an arc curve representing the full circle.
    /// </summary>
    /// <returns>An ArcCurve3D representing the full circle.</returns>
    public Arc3D ToArc()
    {
        var xAxis = GetXAxis();
        return new Arc3D(Center, Normal, xAxis, Radius, 0, 2 * Math.PI, true);
    }

    /// <summary>
    /// Gets the bounding box of the circle.
    /// </summary>
    /// <returns>The bounding box containing the circle.</returns>
    public Box3D GetBoundingBox()
    {
        // Calculate extent in each direction based on normal vector
        var absNormal = new Vector3D(Math.Abs(Normal.X), Math.Abs(Normal.Y), Math.Abs(Normal.Z));
        var extent = new Vector3D(
            Radius * Math.Sqrt(1 - absNormal.X * absNormal.X),
            Radius * Math.Sqrt(1 - absNormal.Y * absNormal.Y),
            Radius * Math.Sqrt(1 - absNormal.Z * absNormal.Z)
        );

        return new Box3D(
            new Point3D(Center.X - extent.X, Center.Y - extent.Y, Center.Z - extent.Z),
            new Point3D(Center.X + extent.X, Center.Y + extent.Y, Center.Z + extent.Z)
        );
    }

    /// <summary>
    /// Gets a consistent X-axis for the circle's local coordinate system.
    /// </summary>
    private Vector3D GetXAxis()
    {
        // For circles in the XY plane (normal along Z), use X-axis as the reference
        if (Math.Abs(Normal.Z - 1) < Constants.Tolerance)
        {
            return new Vector3D(1, 0, 0);
        }
        if (Math.Abs(Normal.Z + 1) < Constants.Tolerance)
        {
            return new Vector3D(1, 0, 0);
        }

        // For other orientations, choose a vector that's not parallel to the normal
        Vector3D xAxis;
        if (Math.Abs(Normal.X) < 0.9)
        {
            xAxis = new Vector3D(1, 0, 0).Cross(Normal).Normalized();
        }
        else
        {
            xAxis = new Vector3D(0, 1, 0).Cross(Normal).Normalized();
        }
        return xAxis;
    }

    /// <summary>
    /// Determines whether this circle is equal to another circle within the specified tolerance.
    /// </summary>
    /// <param name="other">The other circle to compare.</param>
    /// <param name="tolerance">The tolerance for floating-point comparison.</param>
    /// <returns>true if the circles are equal within tolerance; otherwise, false.</returns>
    public bool IsEqualTo(Circle3D other, double tolerance = Constants.Tolerance)
    {
        return Center.IsEqualTo(other.Center, tolerance) &&
               Math.Abs(Radius - other.Radius) < tolerance &&
               (Normal.IsEqualTo(other.Normal, tolerance) || Normal.IsEqualTo(-other.Normal, tolerance));
    }

    /// <inheritdoc/>
    public override bool Equals(object obj) => obj is Circle3D other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(Circle3D other) => IsEqualTo(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Center, Radius, Normal);

    /// <summary>
    /// Determines whether two circles are equal.
    /// </summary>
    public static bool operator ==(Circle3D left, Circle3D right) => left.Equals(right);

    /// <summary>
    /// Determines whether two circles are not equal.
    /// </summary>
    public static bool operator !=(Circle3D left, Circle3D right) => !left.Equals(right);

    /// <inheritdoc/>
    public override string ToString() => $"Circle3D(Center: {Center}, Radius: {Radius}, Normal: {Normal})";

    /// <summary>
    /// Creates a plane perpendicular to the curve at the specified parameter.
    /// </summary>
    /// <param name="t">The parameter along the curve (0 = start, 1 = end).</param>
    /// <returns>A plane positioned at the curve point with normal aligned to the curve tangent.</returns>
    public Plane3D CreatePlaneAt(double t)
    {
        var point = PointAt(t);
        var tangent = TangentAt(t);

        return new Plane3D(point, tangent);
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Circle3D does not support reversal as it is a closed curve.</exception>
    ICurve3D ICurve3D.Reverse()
    {
        throw new NotSupportedException("Circle3D does not support reversal as it is a closed curve with no defined start or end point.");
    }
}