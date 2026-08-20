namespace BasAutomation.Geometry;

/// <summary>
/// Provides utility methods for geometric calculations.
/// </summary>
public class Intersections
{
    /// <summary>
    /// Calculates the intersection point of two infinite lines in 2D space.
    /// </summary>
    /// <param name="x1">X coordinate of the first point on line 1.</param>
    /// <param name="y1">Y coordinate of the first point on line 1.</param>
    /// <param name="x2">X coordinate of the second point on line 1.</param>
    /// <param name="y2">Y coordinate of the second point on line 1.</param>
    /// <param name="x3">X coordinate of the first point on line 2.</param>
    /// <param name="y3">Y coordinate of the first point on line 2.</param>
    /// <param name="x4">X coordinate of the second point on line 2.</param>
    /// <param name="y4">Y coordinate of the second point on line 2.</param>
    /// <returns>A tuple containing the intersection point coordinates, or (NaN, NaN) if the lines are parallel.</returns>
    public static (double X, double Y) IntersectInfiniteLines(double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4)
    {
        // Calculate the denominator for t and u
        var denominator = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);

        // If denominator is within the tolerance, the lines are parallel or coincident (no intersection)
        if (Math.Abs(denominator) < Constants.Tolerance)
        {
            return (double.NaN, double.NaN);
        }

        // Calculate the intersection point using parameter t
        var t = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / denominator;

        var intersectionX = x1 + t * (x2 - x1);
        var intersectionY = y1 + t * (y2 - y1);

        return (intersectionX, intersectionY);
    }
}
