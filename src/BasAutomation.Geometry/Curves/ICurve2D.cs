namespace BasAutomation.Geometry.Curves;

/// <summary>
/// Interface for parametric 2D curves.
/// </summary>
public interface ICurve2D
{
    /// <summary>
    /// Gets the start point of the curve.
    /// </summary>
    Point2D StartPoint { get; }

    /// <summary>
    /// Gets the end point of the curve.
    /// </summary>
    Point2D EndPoint { get; }

    /// <summary>
    /// Gets the length of the curve.
    /// </summary>
    double Length { get; }

    /// <summary>
    /// Gets the bounding box of the curve.
    /// </summary>
    Box2D BoundingBox { get; }

    /// <summary>
    /// Gets a point on the curve at the specified parameter.
    /// </summary>
    /// <param name="t">The parameter (0 = start, 1 = end).</param>
    /// <returns>The point at the specified parameter.</returns>
    Point2D PointAt(double t);

    /// <summary>
    /// Gets the tangent vector at the specified parameter.
    /// </summary>
    /// <param name="t">The parameter (0 = start, 1 = end).</param>
    /// <returns>The tangent vector at the specified parameter.</returns>
    Vector2D TangentAt(double t);

    /// <summary>
    /// Clones the curve
    /// </summary>
    /// <returns></returns>
    ICurve2D Clone();
    
    /// <summary>
    /// Transforms this curve using the specified transformation matrix.
    /// </summary>
    /// <param name="transform">The transformation matrix to apply.</param>
    /// <returns>A new transformed curve.</returns>
    ICurve2D Transform(Matrix2D transform);

    /// <summary>
    /// Reverses the direction of the curve (swaps start and end points).
    /// </summary>
    /// <returns>A new curve with reversed direction.</returns>
    /// <exception cref="NotSupportedException">Thrown when the curve type does not support reversal (e.g., Circle2D).</exception>
    ICurve2D Reverse();
}