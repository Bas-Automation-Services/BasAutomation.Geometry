namespace BasAutomation.Geometry;

/// <summary>
/// Represents a 2D axis-aligned bounding box defined by minimum and maximum points.
/// </summary>
/// <param name="min">The minimum point (bottom-left corner) of the box.</param>
/// <param name="max">The maximum point (top-right corner) of the box.</param>
public readonly struct Box2D(Point2D min, Point2D max) : IEquatable<Box2D>, IGeometryComparable<Box2D>
{
    /// <summary>
    /// Gets or sets the minimum point (bottom-left corner) of the box.
    /// </summary>
    public Point2D Min { get; } = min;

    /// <summary>
    /// Gets or sets the maximum point (top-right corner) of the box.
    /// </summary>
    public Point2D Max { get; } = max;

    /// <summary>
    /// Gets the width of the box.
    /// </summary>
    public double Width => Max.X - Min.X;

    /// <summary>
    /// Gets the height of the box.
    /// </summary>
    public double Height => Max.Y - Min.Y;

    /// <summary>
    /// Gets the center point of the box.
    /// </summary>
    public Point2D Center => new((Min.X + Max.X) / 2, (Min.Y + Max.Y) / 2);

    /// <summary>
    /// Gets the area of the box.
    /// </summary>
    public double Area => Width * Height;

    /// <summary>
    /// Determines whether the box contains the specified point.
    /// </summary>
    /// <param name="p">The point to test.</param>
    /// <returns>true if the box contains the point; otherwise, false.</returns>
    public bool Contains(Point2D p) => p.X >= Min.X && p.X <= Max.X && p.Y >= Min.Y && p.Y <= Max.Y;

    /// <summary>
    /// Determines whether this box intersects with another box.
    /// </summary>
    /// <param name="other">The other box to test.</param>
    /// <returns>true if the boxes intersect; otherwise, false.</returns>
    public bool Intersects(Box2D other) => Min.X <= other.Max.X && Max.X >= other.Min.X &&
                                           Min.Y <= other.Max.Y && Max.Y >= other.Min.Y;

    /// <summary>
    /// Creates a new box that encompasses both this box and another box.
    /// </summary>
    /// <param name="other">The other box to union with.</param>
    /// <returns>A new box that contains both boxes.</returns>
    public Box2D Union(Box2D other) => new(
        new Point2D(Math.Min(Min.X, other.Min.X), Math.Min(Min.Y, other.Min.Y)),
        new Point2D(Math.Max(Max.X, other.Max.X), Math.Max(Max.Y, other.Max.Y))
    );

    /// <summary>
    /// Determines whether this box is equal to another box within the specified tolerance.
    /// </summary>
    /// <param name="other">The other box to compare.</param>
    /// <param name="tolerance">The tolerance for floating-point comparison. Default is 1e-10.</param>
    /// <returns>true if the boxes are equal within tolerance; otherwise, false.</returns>
    public bool IsEqualTo(Box2D other, double tolerance = 1e-10)
    {
        return Min.IsEqualTo(other.Min, tolerance) && Max.IsEqualTo(other.Max, tolerance);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj) => obj is Box2D other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(Box2D other) => IsEqualTo(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Min, Max);

    /// <summary>
    /// Determines whether two boxes are equal.
    /// </summary>
    public static bool operator ==(Box2D left, Box2D right) => left.Equals(right);

    /// <summary>
    /// Determines whether two boxes are not equal.
    /// </summary>
    public static bool operator !=(Box2D left, Box2D right) => !left.Equals(right);
}