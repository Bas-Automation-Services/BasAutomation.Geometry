namespace BasAutomation.Geometry.Curves;

/// <summary>
/// Extension methods for ICurve2D and curve collections.
/// </summary>
public static class Curve2DExtensions
{
    /// <summary>
    /// Gets the bounding box that contains all curves in a contour.
    /// </summary>
    /// <param name="curves">The collection of curves.</param>
    /// <returns>The combined bounding box.</returns>
    public static Box2D GetBoundingBox(this IEnumerable<ICurve2D> curves)
    {
        using var enumerator = curves.GetEnumerator();

        if (!enumerator.MoveNext())
        {
            return new Box2D(new Point2D(0, 0), new Point2D(0, 0));
        }

        var box = enumerator.Current.BoundingBox;

        while (enumerator.MoveNext())
        {
            box = box.Union(enumerator.Current.BoundingBox);
        }

        return box;
    }

    /// <summary>
    /// Finds intersection points between two Line2D segments.
    /// </summary>
    /// <param name="line1">The first line.</param>
    /// <param name="line2">The second line.</param>
    /// <param name="tolerance">The tolerance for intersection detection.</param>
    /// <returns>A list of intersection points (0 or 1 point for line segments).</returns>
    public static List<Point2D> Intersect(this Line2D line1, Line2D line2, double tolerance = Constants.Tolerance)
    {
        var intersections = new List<Point2D>();

        var (x, y) = Intersections.IntersectInfiniteLines(
            line1.StartPoint.X, line1.StartPoint.Y, line1.EndPoint.X, line1.EndPoint.Y,
            line2.StartPoint.X, line2.StartPoint.Y, line2.EndPoint.X, line2.EndPoint.Y);

        if (double.IsNaN(x) || double.IsNaN(y))
            return intersections; // Lines are parallel

        var intersection = new Point2D(x, y);

        // Check if intersection is within both line segments
        if (line1.ContainsPoint(intersection, tolerance) &&
            line2.ContainsPoint(intersection, tolerance))
        {
            intersections.Add(intersection);
        }

        return intersections;
    }

    /// <summary>
    /// Finds intersection points between a Line2D and an Arc2D.
    /// </summary>
    /// <param name="line">The line.</param>
    /// <param name="arc">The arc.</param>
    /// <param name="tolerance">The tolerance for intersection detection.</param>
    /// <returns>A list of intersection points (0, 1, or 2 points).</returns>
    public static List<Point2D> Intersect(this Line2D line, Arc2D arc, double tolerance = Constants.Tolerance)
    {
        var intersections = new List<Point2D>();

        // Use the circle's line intersection method
        var circle = new Circle2D(arc.Center, arc.Radius);
        var count = circle.IntersectLine(line, out var p1, out var p2);

        if (count >= 1 && arc.ContainsPoint(p1, tolerance))
        {
            intersections.Add(p1);
        }

        if (count >= 2 && arc.ContainsPoint(p2, tolerance))
        {
            intersections.Add(p2);
        }

        return intersections;
    }

    /// <summary>
    /// Finds intersection points between an Arc2D and a Line2D.
    /// </summary>
    /// <param name="arc">The arc.</param>
    /// <param name="line">The line.</param>
    /// <param name="tolerance">The tolerance for intersection detection.</param>
    /// <returns>A list of intersection points (0, 1, or 2 points).</returns>
    public static List<Point2D> Intersect(this Arc2D arc, Line2D line, double tolerance = Constants.Tolerance)
    {
        return line.Intersect(arc, tolerance);
    }

    /// <summary>
    /// Finds intersection points between two Arc2D curves.
    /// </summary>
    /// <param name="arc1">The first arc.</param>
    /// <param name="arc2">The second arc.</param>
    /// <param name="tolerance">The tolerance for intersection detection.</param>
    /// <returns>A list of intersection points (0, 1, or 2 points).</returns>
    public static List<Point2D> Intersect(this Arc2D arc1, Arc2D arc2, double tolerance = Constants.Tolerance)
    {
        var intersections = new List<Point2D>();

        var circle1 = new Circle2D(arc1.Center, arc1.Radius);
        var circle2 = new Circle2D(arc2.Center, arc2.Radius);

        var count = circle1.IntersectCircle(circle2, out var p1, out var p2);

        if (count >= 1 && arc1.ContainsPoint(p1, tolerance) && arc2.ContainsPoint(p1, tolerance))
        {
            intersections.Add(p1);
        }

        if (count >= 2 && arc1.ContainsPoint(p2, tolerance) && arc2.ContainsPoint(p2, tolerance))
        {
            intersections.Add(p2);
        }

        return intersections;
    }

    /// <summary>
    /// Finds intersection points between a Line2D and a Circle2D.
    /// </summary>
    /// <param name="line">The line.</param>
    /// <param name="circle">The circle.</param>
    /// <returns>A list of intersection points (0, 1, or 2 points).</returns>
    public static List<Point2D> Intersect(this Line2D line, Circle2D circle)
    {
        var intersections = new List<Point2D>();

        var count = circle.IntersectLine(line, out var p1, out var p2);

        if (count >= 1)
            intersections.Add(p1);
        if (count >= 2)
            intersections.Add(p2);

        return intersections;
    }

    /// <summary>
    /// Finds intersection points between a Circle2D and a Line2D.
    /// </summary>
    /// <param name="circle">The circle.</param>
    /// <param name="line">The line.</param>
    /// <returns>A list of intersection points (0, 1, or 2 points).</returns>
    public static List<Point2D> Intersect(this Circle2D circle, Line2D line)
    {
        return line.Intersect(circle);
    }

    /// <summary>
    /// Finds intersection points between two Circle2D curves.
    /// </summary>
    /// <param name="circle1">The first circle.</param>
    /// <param name="circle2">The second circle.</param>
    /// <returns>A list of intersection points (0, 1, or 2 points).</returns>
    public static List<Point2D> Intersect(this Circle2D circle1, Circle2D circle2)
    {
        var intersections = new List<Point2D>();

        var count = circle1.IntersectCircle(circle2, out var p1, out var p2);

        if (count >= 1)
            intersections.Add(p1);
        if (count >= 2)
            intersections.Add(p2);

        return intersections;
    }

    /// <summary>
    /// Finds intersection points between two curves (generic).
    /// </summary>
    /// <param name="curve1">The first curve.</param>
    /// <param name="curve2">The second curve.</param>
    /// <param name="tolerance">The tolerance for intersection detection.</param>
    /// <returns>A list of intersection points.</returns>
    public static List<Point2D> Intersect(this ICurve2D curve1, ICurve2D curve2, double tolerance = Constants.Tolerance)
    {
        // Handle different curve type combinations
        if (curve1 is Line2D line1 && curve2 is Line2D line2)
        {
            return line1.Intersect(line2, tolerance);
        }
        else if ((curve1 is Line2D || curve2 is Line2D) && (curve1 is Arc2D || curve2 is Arc2D))
        {
            var theLine = curve1 is Line2D l1 ? l1 : (Line2D)curve2;
            var theArc = curve1 is Arc2D a1 ? a1 : (Arc2D)curve2;
            return theLine.Intersect(theArc, tolerance);
        }
        else if (curve1 is Arc2D arc1 && curve2 is Arc2D arc2)
        {
            return arc1.Intersect(arc2, tolerance);
        }
        else if (curve1 is Circle2D circle1 && curve2 is Circle2D circle2)
        {
            return circle1.Intersect(circle2);
        }
        else if ((curve1 is Line2D || curve2 is Line2D) && (curve1 is Circle2D || curve2 is Circle2D))
        {
            var theLine2 = curve1 is Line2D l2 ? l2 : (Line2D)curve2;
            var theCircle = curve1 is Circle2D c1 ? c1 : (Circle2D)curve2;
            return theLine2.Intersect(theCircle);
        }

        // Arc-Circle or other unsupported combinations
        return new List<Point2D>();
    }
}
