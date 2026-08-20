namespace BasAutomation.Geometry.RayCasting;

/// <summary>
/// Represents a ray in 3D space defined by an origin point and a direction.
/// </summary>
public readonly struct Ray3D : IEquatable<Ray3D>
{
    /// <summary>
    /// Gets the origin point of the ray.
    /// </summary>
    public Point3D Origin { get; }

    /// <summary>
    /// Gets the normalized direction vector of the ray.
    /// </summary>
    public Vector3D Direction { get; }

    /// <summary>
    /// Initializes a new instance of the Ray3D struct.
    /// </summary>
    /// <param name="origin">The origin point of the ray.</param>
    /// <param name="direction">The direction of the ray (will be normalized).</param>
    public Ray3D(Point3D origin, Vector3D direction)
    {
        Origin = origin;
        Direction = direction.Normalized();
    }

    /// <summary>
    /// Gets a point along the ray at the specified distance from the origin.
    /// </summary>
    /// <param name="distance">The distance from the origin along the ray direction.</param>
    /// <returns>The point at the specified distance.</returns>
    public Point3D PointAt(double distance)
    {
        return Origin + Direction * distance;
    }

    /// <summary>
    /// Determines whether this ray is equal to another ray within the specified tolerance.
    /// </summary>
    /// <param name="other">The other ray to compare.</param>
    /// <param name="tolerance">The tolerance for floating-point comparison.</param>
    /// <returns>true if the rays are equal within tolerance; otherwise, false.</returns>
    public bool IsEqualTo(Ray3D other, double tolerance = Constants.Tolerance)
    {
        return Origin.IsEqualTo(other.Origin, tolerance) &&
               Direction.IsEqualTo(other.Direction, tolerance);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Ray3D other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(Ray3D other) => IsEqualTo(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Origin, Direction);

    /// <summary>
    /// Determines whether two rays are equal.
    /// </summary>
    public static bool operator ==(Ray3D left, Ray3D right) => left.Equals(right);

    /// <summary>
    /// Determines whether two rays are not equal.
    /// </summary>
    public static bool operator !=(Ray3D left, Ray3D right) => !left.Equals(right);

    /// <inheritdoc/>
    public override string ToString() => $"Ray3D(Origin: {Origin}, Direction: {Direction})";

    /// <summary>
    /// Creates a ray from two points (from start towards end).
    /// </summary>
    /// <param name="start">The start point (ray origin).</param>
    /// <param name="towards">The point the ray points towards.</param>
    /// <returns>A new ray.</returns>
    public static Ray3D FromTwoPoints(Point3D start, Point3D towards)
    {
        var direction = towards - start;
        return new Ray3D(start, direction);
    }

    /// <summary>
    /// Returns a reversed ray (same origin, opposite direction).
    /// </summary>
    public Ray3D Reverse()
    {
        return new Ray3D(Origin, -Direction);
    }
}
