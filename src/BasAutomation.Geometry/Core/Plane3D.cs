using BasAutomation.Geometry.Curves;

namespace BasAutomation.Geometry;

/// <summary>
/// Represents a plane in 3D space defined by a point and a normal vector.
/// </summary>
public readonly struct Plane3D : IEquatable<Plane3D>, IGeometryComparable<Plane3D>
{
    /// <summary>
    /// Gets or sets a point on the plane.
    /// </summary>
    public Point3D Origin { get; }

    /// <summary>
    /// Gets or sets the normal vector of the plane (will be normalized).
    /// </summary>
    public Vector3D Normal { get; }

    /// <summary>
    /// Initializes a new instance of the Plane3D struct.
    /// </summary>
    /// <param name="origin">A point on the plane.</param>
    /// <param name="normal">The normal vector of the plane (will be normalized).</param>
    public Plane3D(Point3D origin, Vector3D normal)
    {
        Origin = origin;
        Normal = normal.Normalized();
    }

    /// <summary>
    /// Creates a plane from three points.
    /// </summary>
    /// <param name="p1">First point on the plane.</param>
    /// <param name="p2">Second point on the plane.</param>
    /// <param name="p3">Third point on the plane.</param>
    /// <returns>A new plane passing through the three points.</returns>
    public static Plane3D FromThreePoints(Point3D p1, Point3D p2, Point3D p3)
    {
        var v1 = p2 - p1;
        var v2 = p3 - p1;
        var normal = v1.Cross(v2).Normalized();
        return new Plane3D(p1, normal);
    }

    /// <summary>
    /// Creates a plane from a point and two vectors lying in the plane.
    /// </summary>
    /// <param name="origin">A point on the plane.</param>
    /// <param name="u">First vector in the plane.</param>
    /// <param name="v">Second vector in the plane.</param>
    /// <returns>A new plane.</returns>
    public static Plane3D FromPointAndVectors(Point3D origin, Vector3D u, Vector3D v)
    {
        var normal = u.Cross(v).Normalized();
        return new Plane3D(origin, normal);
    }

    /// <summary>
    /// Gets the signed distance from a point to the plane.
    /// Positive if the point is on the side of the normal, negative otherwise.
    /// </summary>
    /// <param name="point">The point to measure distance from.</param>
    /// <returns>The signed distance.</returns>
    public double SignedDistanceToPoint(Point3D point)
    {
        return (point - Origin).Dot(Normal);
    }

    /// <summary>
    /// Gets the absolute distance from a point to the plane.
    /// </summary>
    /// <param name="point">The point to measure distance from.</param>
    /// <returns>The absolute distance.</returns>
    public double DistanceToPoint(Point3D point)
    {
        return Math.Abs(SignedDistanceToPoint(point));
    }

    /// <summary>
    /// Projects a point onto the plane.
    /// </summary>
    /// <param name="point">The point to project.</param>
    /// <returns>The projected point on the plane.</returns>
    public Point3D ProjectPoint(Point3D point)
    {
        var distance = SignedDistanceToPoint(point);
        return point - distance * Normal;
    }

    /// <summary>
    /// Projects a vector onto the plane.
    /// </summary>
    /// <param name="vector">The vector to project.</param>
    /// <returns>The projected vector lying in the plane.</returns>
    public Vector3D ProjectVector(Vector3D vector)
    {
        var normalComponent = vector.Dot(Normal);
        return vector - normalComponent * Normal;
    }

    /// <summary>
    /// Determines if a point lies on the plane within the specified tolerance.
    /// </summary>
    /// <param name="point">The point to test.</param>
    /// <param name="tolerance">The tolerance for the test.</param>
    /// <returns>true if the point is on the plane; otherwise, false.</returns>
    public bool ContainsPoint(Point3D point, double tolerance = Constants.Tolerance)
    {
        return DistanceToPoint(point) < tolerance;
    }

    /// <summary>
    /// Finds the intersection of a line with this plane.
    /// </summary>
    /// <param name="line">The line to intersect with.</param>
    /// <param name="intersection">The intersection point if it exists.</param>
    /// <returns>true if the line intersects the plane; otherwise, false.</returns>
    public bool IntersectLine(Line3D line, out Point3D intersection)
    {
        intersection = new Point3D(0, 0, 0);

        var lineDirection = line.Direction;
        var denominator = lineDirection.Dot(Normal);

        // Check if line is parallel to plane
        if (Math.Abs(denominator) < Constants.Tolerance)
            return false;

        var t = (Origin - line.StartPoint).Dot(Normal) / denominator;

        // Check if intersection is within line segment
        if (t < 0 || t > line.Length)
            return false;

        intersection = line.PointAt(t / line.Length);
        return true;
    }

    /// <summary>
    /// Finds the intersection of a ray with this plane.
    /// </summary>
    /// <param name="rayOrigin">The origin of the ray.</param>
    /// <param name="rayDirection">The direction of the ray (will be normalized).</param>
    /// <param name="intersection">The intersection point if it exists.</param>
    /// <returns>true if the ray intersects the plane; otherwise, false.</returns>
    public bool IntersectRay(Point3D rayOrigin, Vector3D rayDirection, out Point3D intersection)
    {
        intersection = new Point3D(0, 0, 0);

        rayDirection = rayDirection.Normalized();
        var denominator = rayDirection.Dot(Normal);

        // Check if ray is parallel to plane
        if (Math.Abs(denominator) < Constants.Tolerance)
            return false;

        var t = (Origin - rayOrigin).Dot(Normal) / denominator;

        // Check if intersection is in the positive direction of the ray
        if (t < 0)
            return false;

        intersection = rayOrigin + t * rayDirection;
        return true;
    }

    /// <summary>
    /// Finds the intersection of an unbounded line (defined by a point and a direction) with this plane.
    /// Unlike <see cref="IntersectRay(Point3D, Vector3D, out Point3D)"/> the direction is not restricted
    /// to the positive half-line, so the intersection point can lie on either side of
    /// <paramref name="pointOnLine"/>. Returns false only when the line is parallel to the plane.
    /// </summary>
    /// <param name="pointOnLine">Any point on the line.</param>
    /// <param name="lineDirection">The direction of the line (will be normalized).</param>
    /// <param name="intersection">The intersection point if it exists.</param>
    /// <returns>true if the line intersects the plane; otherwise, false.</returns>
    public bool IntersectInfiniteLine(Point3D pointOnLine, Vector3D lineDirection, out Point3D intersection)
    {
        intersection = new Point3D(0, 0, 0);

        lineDirection = lineDirection.Normalized();
        var denominator = lineDirection.Dot(Normal);

        if (Math.Abs(denominator) < Constants.Tolerance)
            return false;

        var t = (Origin - pointOnLine).Dot(Normal) / denominator;
        intersection = pointOnLine + t * lineDirection;
        return true;
    }

    /// <summary>
    /// Flips the normal direction of the plane.
    /// </summary>
    /// <returns>A new plane with flipped normal.</returns>
    public Plane3D Flip()
    {
        return new Plane3D(Origin, -Normal);
    }

    /// <summary>
    /// Moves the plane along its normal by the specified distance.
    /// </summary>
    /// <param name="distance">The distance to move (positive moves in normal direction).</param>
    /// <returns>A new moved plane.</returns>
    public Plane3D Offset(double distance)
    {
        return new Plane3D(Origin + distance * Normal, Normal);
    }

    /// <summary>
    /// Transforms the plane by the specified transformation matrix.
    /// </summary>
    /// <param name="transformation">The transformation matrix.</param>
    /// <returns>The transformed plane.</returns>
    public Plane3D Transform(Matrix3D transformation)
    {
        var newOrigin = transformation.Transform(Origin);
        var newNormal = transformation.Transform(Normal).Normalized();
        return new Plane3D(newOrigin, newNormal);
    }

    /// <summary>
    /// Determines whether this plane is equal to another plane within the specified tolerance.
    /// </summary>
    /// <param name="other">The other plane to compare.</param>
    /// <param name="tolerance">The tolerance for floating-point comparison.</param>
    /// <returns>true if the planes are equal within tolerance; otherwise, false.</returns>
    public bool IsEqualTo(Plane3D other, double tolerance = Constants.Tolerance)
    {
        // Check if normals are parallel (either same or opposite direction)
        var normalDot = Math.Abs(Normal.Dot(other.Normal));
        if (Math.Abs(normalDot - 1.0) > tolerance)
            return false;

        // Check if origins lie on the same plane
        return ContainsPoint(other.Origin, tolerance);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj) => obj is Plane3D other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(Plane3D other) => IsEqualTo(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Origin, Normal);

    /// <summary>
    /// Determines whether two planes are equal.
    /// </summary>
    public static bool operator ==(Plane3D left, Plane3D right) => left.Equals(right);

    /// <summary>
    /// Determines whether two planes are not equal.
    /// </summary>
    public static bool operator !=(Plane3D left, Plane3D right) => !left.Equals(right);

    /// <inheritdoc/>
    public override string ToString() => $"Plane (Origin: {Origin}, Normal: {Normal})";

    /// <summary>
    /// Checks if a point is on the positive side of the plane (side the normal points to).
    /// </summary>
    public bool IsPointOnPositiveSide(Point3D point)
    {
        return SignedDistanceToPoint(point) > 0;
    }

    /// <summary>
    /// Checks if a vector points away from the plane (same general direction as normal).
    /// </summary>
    public bool IsVectorPointingAway(Vector3D vector)
    {
        return Normal.Dot(vector) > 0;
    }

    /// <summary>
    /// Mirrors a point across the plane.
    /// </summary>
    public Point3D MirrorPoint(Point3D point)
    {
        var distance = SignedDistanceToPoint(point);
        return point - 2 * distance * Normal;
    }

    /// <summary>
    /// Mirrors a vector across the plane.
    /// </summary>
    public Vector3D MirrorVector(Vector3D vector)
    {
        var normalComponent = vector.Dot(Normal);
        return vector - 2 * normalComponent * Normal;
    }

    /// <summary>
    /// Finds the intersection line between this plane and another plane.
    /// </summary>
    /// <param name="other">The other plane.</param>
    /// <param name="lineOrigin">The origin point of the intersection line.</param>
    /// <param name="lineDirection">The direction of the intersection line.</param>
    /// <returns>True if planes intersect, false if they are parallel.</returns>
    public bool IntersectPlane(Plane3D other, out Point3D lineOrigin, out Vector3D lineDirection)
    {
        lineDirection = Normal.Cross(other.Normal);

        // Check if planes are parallel
        if (lineDirection.Length < Constants.Tolerance)
        {
            lineOrigin = new Point3D(0, 0, 0);
            lineDirection = new Vector3D(0, 0, 0);
            return false;
        }

        lineDirection = lineDirection.Normalized();

        // Find a point on the intersection line
        // We need to solve: n1·p = d1 and n2·p = d2
        // where d1 and d2 are the plane constants

        var n1 = Normal;
        var n2 = other.Normal;
        var d1 = -n1.Dot(new Vector3D(Origin.X, Origin.Y, Origin.Z));
        var d2 = -n2.Dot(new Vector3D(other.Origin.X, other.Origin.Y, other.Origin.Z));

        // Choose the coordinate axis that gives the most stable solution
        // (the one most perpendicular to the line direction)
        int maxAxis = 0;
        double maxValue = Math.Abs(lineDirection.X);
        if (Math.Abs(lineDirection.Y) > maxValue)
        {
            maxAxis = 1;
            maxValue = Math.Abs(lineDirection.Y);
        }
        if (Math.Abs(lineDirection.Z) > maxValue)
        {
            maxAxis = 2;
        }

        // Solve for a point on the line by setting one coordinate to 0
        double x = 0, y = 0, z = 0;

        switch (maxAxis)
        {
            case 0: // Set x = 0, solve for y and z
                x = 0;
                var det = n1.Y * n2.Z - n1.Z * n2.Y;
                if (Math.Abs(det) > Constants.Tolerance)
                {
                    y = (n2.Z * (-d1) - n1.Z * (-d2)) / det;
                    z = (n1.Y * (-d2) - n2.Y * (-d1)) / det;
                }
                break;

            case 1: // Set y = 0, solve for x and z
                y = 0;
                det = n1.X * n2.Z - n1.Z * n2.X;
                if (Math.Abs(det) > Constants.Tolerance)
                {
                    x = (n2.Z * (-d1) - n1.Z * (-d2)) / det;
                    z = (n1.X * (-d2) - n2.X * (-d1)) / det;
                }
                break;

            case 2: // Set z = 0, solve for x and y
                z = 0;
                det = n1.X * n2.Y - n1.Y * n2.X;
                if (Math.Abs(det) > Constants.Tolerance)
                {
                    x = (n2.Y * (-d1) - n1.Y * (-d2)) / det;
                    y = (n1.X * (-d2) - n2.X * (-d1)) / det;
                }
                break;
        }

        lineOrigin = new Point3D(x, y, z);
        return true;
    }

    /// <summary>
    /// Translates the plane by a vector.
    /// </summary>
    public Plane3D Translate(Vector3D vector)
    {
        return new Plane3D(Origin + vector, Normal);
    }

    /// <summary>
    /// Creates a plane mirrored across another plane.
    /// </summary>
    public Plane3D GetMirrored(Plane3D mirrorPlane)
    {
        var mirroredOrigin = mirrorPlane.MirrorPoint(Origin);
        var mirroredNormal = mirrorPlane.MirrorVector(Normal);
        return new Plane3D(mirroredOrigin, mirroredNormal);
    }

    /// <summary>
    /// Gets the angle between this plane and another plane in radians (0 to π/2).
    /// </summary>
    public double AngleTo(Plane3D other)
    {
        var dot = Math.Abs(Normal.Dot(other.Normal));
        return Math.Acos(Math.Min(1.0, dot));
    }

    /// <summary>
    /// Checks if two planes are parallel.
    /// </summary>
    public bool IsParallelTo(Plane3D other, double tolerance = Constants.Tolerance)
    {
        return Normal.IsParallel(other.Normal, tolerance);
    }

    /// <summary>
    /// Checks if a direction vector is parallel to this plane (perpendicular to the normal).
    /// </summary>
    public bool IsParallelTo(Vector3D direction, double tolerance = Constants.Tolerance)
    {
        return Normal.IsPerpendicular(direction, tolerance);
    }

    /// <summary>
    /// Checks if two planes are perpendicular.
    /// </summary>
    public bool IsPerpendicularTo(Plane3D other, double tolerance = Constants.Tolerance)
    {
        return Normal.IsPerpendicular(other.Normal, tolerance);
    }

    /// <summary>
    /// Gets the distance between two parallel planes. Returns NaN if not parallel.
    /// </summary>
    public double DistanceTo(Plane3D other)
    {
        if (!IsParallelTo(other))
            return double.NaN;

        return Math.Abs(SignedDistanceToPoint(other.Origin));
    }

    #region 2D Plane Coordinate System

    /// <summary>
    /// Gets the U basis vector for the plane's 2D coordinate system.
    /// This is perpendicular to the normal and provides a consistent X-axis on the plane.
    /// </summary>
    public Vector3D GetUAxis()
    {
        var u = Normal.Cross(Vector3D.ZAxis);
        if (u.Length < Constants.Tolerance)
            u = Normal.Cross(Vector3D.XAxis);
        return u.Normalized();
    }

    /// <summary>
    /// Gets the V basis vector for the plane's 2D coordinate system.
    /// This is perpendicular to both the normal and U-axis, providing a consistent Y-axis on the plane.
    /// Uses U.Cross(Normal) to ensure a right-handed coordinate system.
    /// </summary>
    public Vector3D GetVAxis()
    {
        return GetUAxis().Cross(Normal).Normalized();
    }

    /// <summary>
    /// Transforms a 3D point onto the plane's 2D coordinate system.
    /// The point is projected onto the plane and expressed in U/V coordinates.
    /// </summary>
    /// <param name="point">The 3D point to transform.</param>
    /// <returns>The 2D coordinates on the plane.</returns>
    public Point2D TransformToPlane(Point3D point)
    {
        var local = point - Origin;
        var u = GetUAxis();
        var v = GetVAxis();
        return new Point2D(local.Dot(u), local.Dot(v));
    }

    /// <summary>
    /// Transforms a 3D vector onto the plane's 2D coordinate system.
    /// The vector is expressed in U/V coordinates.
    /// </summary>
    /// <param name="vector">The 3D vector to transform.</param>
    /// <returns>The 2D vector on the plane.</returns>
    public Vector2D TransformToPlane(Vector3D vector)
    {
        var u = GetUAxis();
        var v = GetVAxis();
        return new Vector2D(vector.Dot(u), vector.Dot(v));
    }

    /// <summary>
    /// Transforms a 2D point from the plane's coordinate system back to 3D space.
    /// </summary>
    /// <param name="point">The 2D coordinates on the plane.</param>
    /// <returns>The 3D point in world space.</returns>
    public Point3D TransformFromPlane(Point2D point)
    {
        var u = GetUAxis();
        var v = GetVAxis();
        return Origin + u * point.X + v * point.Y;
    }

    /// <summary>
    /// Transforms a 2D vector from the plane's coordinate system back to 3D space.
    /// </summary>
    /// <param name="vector">The 2D vector on the plane.</param>
    /// <returns>The 3D vector in world space.</returns>
    public Vector3D TransformFromPlane(Vector2D vector)
    {
        var u = GetUAxis();
        var v = GetVAxis();
        return u * vector.X + v * vector.Y;
    }

    #endregion
}