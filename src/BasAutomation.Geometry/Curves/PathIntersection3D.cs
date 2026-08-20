namespace BasAutomation.Geometry.Curves;

/// <summary>
/// Represents an intersection between a path and another geometric entity.
/// </summary>
public readonly struct PathIntersection3D : IEquatable<PathIntersection3D>
{
    /// <summary>
    /// Gets the intersection point in 3D space.
    /// </summary>
    public Point3D Point { get; }

    /// <summary>
    /// Gets the index of the segment that was intersected.
    /// </summary>
    public int SegmentIndex { get; }

    /// <summary>
    /// Gets the parameter value (0-1) on the intersected segment.
    /// </summary>
    public double Parameter { get; }

    /// <summary>
    /// Gets the tangent vector of the path at the intersection point.
    /// </summary>
    public Vector3D Tangent { get; }

    /// <summary>
    /// Creates a new path intersection.
    /// </summary>
    /// <param name="point">The intersection point.</param>
    /// <param name="segmentIndex">The index of the intersected segment.</param>
    /// <param name="parameter">The parameter value (0-1) on the segment.</param>
    /// <param name="tangent">The tangent vector at the intersection.</param>
    public PathIntersection3D(Point3D point, int segmentIndex, double parameter, Vector3D tangent)
    {
        Point = point;
        SegmentIndex = segmentIndex;
        Parameter = parameter;
        Tangent = tangent.Normalized();
    }

    /// <summary>
    /// Creates a perpendicular plane at the intersection point.
    /// The plane normal is the path tangent, useful for miter calculations.
    /// </summary>
    /// <returns>A plane perpendicular to the path at the intersection.</returns>
    public Plane3D GetPerpendicularPlane() => new Plane3D(Point, Tangent);

    /// <summary>
    /// Creates a coordinate system at the intersection point.
    /// Z-axis is the tangent direction.
    /// </summary>
    /// <param name="upHint">Optional hint vector for the Y-axis direction.</param>
    /// <returns>A coordinate system at the intersection.</returns>
    public CoordinateSystem3D GetCoordinateSystem(Vector3D? upHint = null)
    {
        var z = Tangent;
        var up = upHint ?? Vector3D.ZAxis;

        // If tangent is parallel to up, use alternative
        if (Math.Abs(z.Dot(up)) > 0.99)
        {
            up = Math.Abs(z.Dot(Vector3D.XAxis)) < 0.99 ? Vector3D.XAxis : Vector3D.YAxis;
        }

        var x = up.Cross(z).Normalized();
        var y = z.Cross(x).Normalized();

        return new CoordinateSystem3D(Point, x, y, z);
    }

    /// <inheritdoc/>
    public bool Equals(PathIntersection3D other) =>
        Point.IsEqualTo(other.Point) &&
        SegmentIndex == other.SegmentIndex &&
        Math.Abs(Parameter - other.Parameter) < Constants.Tolerance;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is PathIntersection3D other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Point, SegmentIndex, Parameter);

    /// <summary>
    /// Determines whether two path intersections are equal.
    /// </summary>
    public static bool operator ==(PathIntersection3D left, PathIntersection3D right) => left.Equals(right);

    /// <summary>
    /// Determines whether two path intersections are not equal.
    /// </summary>
    public static bool operator !=(PathIntersection3D left, PathIntersection3D right) => !left.Equals(right);

    /// <inheritdoc/>
    public override string ToString() =>
        $"PathIntersection3D(Point: {Point}, Segment: {SegmentIndex}, t: {Parameter:F4})";
}
