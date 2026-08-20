namespace BasAutomation.Geometry;

/// <summary>
/// Represents an axis-aligned 2D rectangle defined by its minimum and maximum corner points.
/// </summary>
public class Rectangle2D(Point2D min, Point2D max)
{
    /// <summary>
    /// Gets or sets the minimum point (bottom-left) of the rectangle.
    /// </summary>
    public Point2D MinPoint { get; set; } = min;

    /// <summary>
    /// Gets or sets the maximum point (top-right) of the rectangle.
    /// </summary>
    public Point2D MaxPoint { get; set; } = max;

    /// <summary>
    /// Initializes a new instance of the <see cref="Rectangle2D"/> class 
    /// using minimum and maximum corner points specified by coordinates.
    /// </summary>
    /// <param name="minX">The X-coordinate of the minimum point (bottom-left corner).</param>
    /// <param name="minY">The Y-coordinate of the minimum point (bottom-left corner).</param>
    /// <param name="maxX">The X-coordinate of the maximum point (top-right corner).</param>
    /// <param name="maxY">The Y-coordinate of the maximum point (top-right corner).</param>
    public Rectangle2D(double minX, double minY, double maxX, double maxY)
        : this(new Point2D(minX, minY), new Point2D(maxX, maxY))
    {
        MinPoint = new Point2D(minX, minY);
        MaxPoint = new Point2D(maxX, maxY);
    }

    /// <summary>
    /// Determines whether this rectangle intersects with another rectangle.
    /// </summary>
    /// <param name="other">The other rectangle to test for intersection.</param>
    /// <returns>
    /// <c>true</c> if the rectangles intersect; otherwise, <c>false</c>.
    /// </returns>
    public bool Intersects(Rectangle2D other)
    {
        return !(other.MaxPoint.X < MinPoint.X || other.MinPoint.X > MaxPoint.X ||
                 other.MaxPoint.Y < MinPoint.Y || other.MinPoint.Y > MaxPoint.Y);
    }

    /// <summary>
    /// Determines whether a specified point is inside or on the boundary of this rectangle.
    /// </summary>
    /// <param name="point">The point to test.</param>
    /// <returns>
    /// <c>true</c> if the point lies inside or on the boundary of the rectangle; 
    /// otherwise, <c>false</c>.
    /// </returns>
    public bool Contains(Point2D point)
    {
        return point.X >= MinPoint.X && point.X <= MaxPoint.X &&
               point.Y >= MinPoint.Y && point.Y <= MaxPoint.Y;
    }
}