namespace BasAutomation.Geometry;

/// <summary>
/// Represents a 3D axis-aligned bounding box defined by minimum and maximum points.
/// </summary>
/// <param name="min">The minimum point (bottom-left-front corner) of the box.</param>
/// <param name="max">The maximum point (top-right-back corner) of the box.</param>
public readonly struct Box3D(Point3D min, Point3D max) : IEquatable<Box3D>, IGeometryComparable<Box3D>
{
    /// <summary>
    /// Gets or sets the minimum point (bottom-left-front corner) of the box.
    /// </summary>
    public Point3D Min { get; } = min;

    /// <summary>
    /// Gets or sets the maximum point (top-right-back corner) of the box.
    /// </summary>
    public Point3D Max { get; } = max;

    /// <summary>
    /// Gets the width of the box (X dimension).
    /// </summary>
    public double Width => Max.X - Min.X;

    /// <summary>
    /// Gets the height of the box (Y dimension).
    /// </summary>
    public double Height => Max.Y - Min.Y;

    /// <summary>
    /// Gets the depth of the box (Z dimension).
    /// </summary>
    public double Depth => Max.Z - Min.Z;

    /// <summary>
    /// Gets the center point of the box.
    /// </summary>
    public Point3D Center => new((Min.X + Max.X) / 2, (Min.Y + Max.Y) / 2, (Min.Z + Max.Z) / 2);

    /// <summary>
    /// Gets the volume of the box.
    /// </summary>
    public double Volume => Width * Height * Depth;

    /// <summary>
    /// Determines whether the box contains the specified point.
    /// </summary>
    /// <param name="p">The point to test.</param>
    /// <returns>true if the box contains the point; otherwise, false.</returns>
    public bool Contains(Point3D p) => p.X >= Min.X && p.X <= Max.X &&
                                       p.Y >= Min.Y && p.Y <= Max.Y &&
                                       p.Z >= Min.Z && p.Z <= Max.Z;

    /// <summary>
    /// Determines whether this box intersects with another box.
    /// </summary>
    /// <param name="other">The other box to test.</param>
    /// <returns>true if the boxes intersect; otherwise, false.</returns>
    public bool Intersects(Box3D other) => Min.X <= other.Max.X && Max.X >= other.Min.X &&
                                           Min.Y <= other.Max.Y && Max.Y >= other.Min.Y &&
                                           Min.Z <= other.Max.Z && Max.Z >= other.Min.Z;

    /// <summary>
    /// Creates a new box that encompasses both this box and another box.
    /// </summary>
    /// <param name="other">The other box to union with.</param>
    /// <returns>A new box that contains both boxes.</returns>
    public Box3D Union(Box3D other) => new(
        new Point3D(Math.Min(Min.X, other.Min.X), Math.Min(Min.Y, other.Min.Y), Math.Min(Min.Z, other.Min.Z)),
        new Point3D(Math.Max(Max.X, other.Max.X), Math.Max(Max.Y, other.Max.Y), Math.Max(Max.Z, other.Max.Z))
    );

    /// <summary>
    /// Determines whether this box is equal to another box within the specified tolerance.
    /// </summary>
    /// <param name="other">The other box to compare.</param>
    /// <param name="tolerance">The tolerance for floating-point comparison. Default is 1e-10.</param>
    /// <returns>true if the boxes are equal within tolerance; otherwise, false.</returns>
    public bool IsEqualTo(Box3D other, double tolerance = 1e-10)
    {
        return Min.IsEqualTo(other.Min, tolerance) && Max.IsEqualTo(other.Max, tolerance);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj) => obj is Box3D other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(Box3D other) => IsEqualTo(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Min, Max);

    /// <summary>
    /// Determines whether two boxes are equal.
    /// </summary>
    public static bool operator ==(Box3D left, Box3D right) => left.Equals(right);

    /// <summary>
    /// Determines whether two boxes are not equal.
    /// </summary>
    public static bool operator !=(Box3D left, Box3D right) => !left.Equals(right);
}