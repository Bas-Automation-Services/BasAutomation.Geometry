namespace BasAutomation.Geometry.Curves;

/// <summary>
/// Represents an ellipse in 3D space defined by center, semi-axes, normal, and major axis direction.
/// </summary>
public readonly struct Ellipse3D : IEquatable<Ellipse3D>, IGeometryComparable<Ellipse3D>, ICurve3D
{
    /// <summary>
    /// Gets the center point of the ellipse.
    /// </summary>
    public Point3D Center { get; }

    /// <summary>
    /// Gets the semi-major axis length (radius along the major axis direction).
    /// </summary>
    public double SemiMajorAxis { get; }

    /// <summary>
    /// Gets the semi-minor axis length (radius along the minor axis direction).
    /// </summary>
    public double SemiMinorAxis { get; }

    /// <summary>
    /// Gets the normal vector of the plane containing the ellipse (normalized).
    /// </summary>
    public Vector3D Normal { get; }

    /// <summary>
    /// Gets the direction of the major axis in 3D space (normalized, lies in the ellipse plane).
    /// </summary>
    public Vector3D MajorAxisDirection { get; }

    /// <summary>
    /// Gets the direction of the minor axis in 3D space (derived from Normal cross MajorAxisDirection).
    /// </summary>
    public Vector3D MinorAxisDirection => Normal.Cross(MajorAxisDirection).Normalized();

    /// <summary>
    /// Initializes a new instance of Ellipse3D.
    /// </summary>
    /// <param name="center">The center point of the ellipse.</param>
    /// <param name="semiMajorAxis">The semi-major axis length.</param>
    /// <param name="semiMinorAxis">The semi-minor axis length.</param>
    /// <param name="normal">The normal vector of the plane containing the ellipse.</param>
    /// <param name="majorAxisDirection">The direction of the major axis in 3D space.</param>
    public Ellipse3D(Point3D center, double semiMajorAxis, double semiMinorAxis, Vector3D normal, Vector3D majorAxisDirection)
    {
        if (semiMajorAxis <= 0)
            throw new ArgumentException("Semi-major axis must be positive.", nameof(semiMajorAxis));
        if (semiMinorAxis <= 0)
            throw new ArgumentException("Semi-minor axis must be positive.", nameof(semiMinorAxis));

        Center = center;
        SemiMajorAxis = semiMajorAxis;
        SemiMinorAxis = semiMinorAxis;
        Normal = normal.Normalized();
        MajorAxisDirection = majorAxisDirection.Normalized();
    }

    /// <summary>
    /// Gets the plane containing the ellipse.
    /// </summary>
    public Plane3D Plane => new Plane3D(Center, Normal);

    // ICurve3D implementation
    public Point3D StartPoint => PointAt(0);
    public Point3D EndPoint => StartPoint;

    /// <summary>
    /// Gets the approximate perimeter using Ramanujan's approximation.
    /// </summary>
    public double Length
    {
        get
        {
            var a = SemiMajorAxis;
            var b = SemiMinorAxis;
            var h = Math.Pow(a - b, 2) / Math.Pow(a + b, 2);
            return Math.PI * (a + b) * (1 + (3 * h) / (10 + Math.Sqrt(4 - 3 * h)));
        }
    }

    /// <summary>
    /// Gets a point on the ellipse at the specified parameter.
    /// </summary>
    /// <param name="t">The parameter (0 to 1 for one full revolution).</param>
    public Point3D PointAt(double t)
    {
        var minorAxis = MinorAxisDirection;
        var angle = t * 2 * Math.PI;
        return Center + SemiMajorAxis * Math.Cos(angle) * MajorAxisDirection + SemiMinorAxis * Math.Sin(angle) * minorAxis;
    }

    /// <summary>
    /// Gets the tangent vector at the specified parameter.
    /// </summary>
    /// <param name="t">The parameter (0 to 1 for one full revolution).</param>
    public Vector3D TangentAt(double t)
    {
        var minorAxis = MinorAxisDirection;
        var angle = t * 2 * Math.PI;
        var tangent = -SemiMajorAxis * Math.Sin(angle) * MajorAxisDirection + SemiMinorAxis * Math.Cos(angle) * minorAxis;
        return tangent.Normalized();
    }

    /// <summary>
    /// Gets the normal vector at the specified parameter (points toward center).
    /// </summary>
    public Vector3D NormalAt(double t)
    {
        var point = PointAt(t);
        return (Center - point).Normalized();
    }

    /// <summary>
    /// Gets the parameter at the specified distance along the ellipse.
    /// </summary>
    public double ParameterAtDistance(double distance)
    {
        return distance / Length;
    }

    /// <inheritdoc/>
    public ICurve3D Clone()
    {
        return new Ellipse3D(Center, SemiMajorAxis, SemiMinorAxis, Normal, MajorAxisDirection);
    }

    /// <summary>
    /// Creates a plane perpendicular to the curve at the specified parameter.
    /// </summary>
    public Plane3D CreatePlaneAt(double t)
    {
        var point = PointAt(t);
        var tangent = TangentAt(t);
        return new Plane3D(point, tangent);
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Ellipse3D does not support reversal as it is a closed curve.</exception>
    ICurve3D ICurve3D.Reverse()
    {
        throw new NotSupportedException("Ellipse3D does not support reversal as it is a closed curve with no defined start or end point.");
    }

    /// <summary>
    /// Determines if a point lies on the ellipse within the specified tolerance.
    /// </summary>
    public bool ContainsPoint(Point3D point, double tolerance = Constants.Tolerance)
    {
        var plane = new Plane3D(Center, Normal);
        if (!plane.ContainsPoint(point, tolerance))
            return false;

        var toPoint = point - Center;
        var x = toPoint.Dot(MajorAxisDirection);
        var y = toPoint.Dot(MinorAxisDirection);

        // Check if (x/a)^2 + (y/b)^2 ≈ 1
        var value = (x * x) / (SemiMajorAxis * SemiMajorAxis) + (y * y) / (SemiMinorAxis * SemiMinorAxis);
        return Math.Abs(value - 1.0) < tolerance;
    }

    /// <summary>
    /// Gets the bounding box of the ellipse.
    /// </summary>
    public Box3D GetBoundingBox()
    {
        var majorDir = MajorAxisDirection;
        var minorDir = MinorAxisDirection;

        // The extent in each global axis is the max of the ellipse projected onto that axis
        var extentX = Math.Sqrt(Math.Pow(SemiMajorAxis * majorDir.X, 2) + Math.Pow(SemiMinorAxis * minorDir.X, 2));
        var extentY = Math.Sqrt(Math.Pow(SemiMajorAxis * majorDir.Y, 2) + Math.Pow(SemiMinorAxis * minorDir.Y, 2));
        var extentZ = Math.Sqrt(Math.Pow(SemiMajorAxis * majorDir.Z, 2) + Math.Pow(SemiMinorAxis * minorDir.Z, 2));

        return new Box3D(
            new Point3D(Center.X - extentX, Center.Y - extentY, Center.Z - extentZ),
            new Point3D(Center.X + extentX, Center.Y + extentY, Center.Z + extentZ));
    }

    /// <summary>
    /// Determines whether this ellipse is equal to another ellipse within the specified tolerance.
    /// </summary>
    public bool IsEqualTo(Ellipse3D other, double tolerance = Constants.Tolerance)
    {
        return Center.IsEqualTo(other.Center, tolerance) &&
               Math.Abs(SemiMajorAxis - other.SemiMajorAxis) < tolerance &&
               Math.Abs(SemiMinorAxis - other.SemiMinorAxis) < tolerance &&
               (Normal.IsEqualTo(other.Normal, tolerance) || Normal.IsEqualTo(-other.Normal, tolerance)) &&
               (MajorAxisDirection.IsEqualTo(other.MajorAxisDirection, tolerance) || MajorAxisDirection.IsEqualTo(-other.MajorAxisDirection, tolerance));
    }

    /// <inheritdoc/>
    public override bool Equals(object obj) => obj is Ellipse3D other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(Ellipse3D other) => IsEqualTo(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Center, SemiMajorAxis, SemiMinorAxis, Normal);

    public static bool operator ==(Ellipse3D left, Ellipse3D right) => left.Equals(right);
    public static bool operator !=(Ellipse3D left, Ellipse3D right) => !left.Equals(right);

    /// <inheritdoc/>
    public override string ToString() => $"Ellipse3D(Center: {Center}, SemiMajor: {SemiMajorAxis}, SemiMinor: {SemiMinorAxis}, Normal: {Normal}, MajorDir: {MajorAxisDirection})";
}
