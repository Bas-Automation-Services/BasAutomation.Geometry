namespace BasAutomation.Geometry.Curves;

/// <summary>
/// Outward offset of a closed 2D contour, built for small clearance cuts: the exact contour is
/// tessellated to a polygon (arcs sampled) and each vertex is pushed out by the offset distance
/// with mitered corners (beveled past the miter limit). For small clearance distances
/// (a few mm against profile features of tens of mm) this tracks the true offset closely and can
/// only ever over-cut slightly at corners — the safe direction for a clearance cut. Not a general
/// polygon-offset: self-intersection cleanup is deliberately out of scope, so keep the distance
/// small relative to the contour's features.
/// </summary>
public static class ContourOffset
{
    private const double Epsilon = 1e-9;
    private const double PointTolerance = 1e-3; // mm — same connectivity tolerance the renderers use
    private const double MiterLimitFactor = 4.0;

    /// <summary>
    /// Offsets a closed contour outward by <paramref name="distance"/> mm and returns the result
    /// as a closed loop of line segments. Throws when the contour is degenerate.
    /// </summary>
    public static List<ICurve2D> OffsetOutward(IReadOnlyList<ICurve2D> contour, double distance,
        double maxSegmentAngleDegrees = 12)
    {
        if (distance <= 0)
            throw new ArgumentOutOfRangeException(nameof(distance), "Offset distance must be positive.");

        var points = Tessellate(contour, maxSegmentAngleDegrees);
        if (points.Count < 3)
            throw new InvalidOperationException("Contour tessellation produced a degenerate polygon.");

        if (SignedArea(points) < 0)
            points.Reverse(); // canonical counter-clockwise so 'outward' is well-defined

        var offset = OffsetCcwPolygon(points, distance);

        var result = new List<ICurve2D>(offset.Count);
        for (var i = 0; i < offset.Count; i++)
            result.Add(new Line2D(offset[i], offset[(i + 1) % offset.Count]));
        return result;
    }

    // ─────────────────────────── offset core ───────────────────────────

    private static List<Point2D> OffsetCcwPolygon(List<Point2D> pts, double d)
    {
        var n = pts.Count;
        var result = new List<Point2D>(n);

        for (var i = 0; i < n; i++)
        {
            var prev = pts[(i - 1 + n) % n];
            var curr = pts[i];
            var next = pts[(i + 1) % n];

            var (n1, ok1) = OutwardNormal(prev, curr);
            var (n2, ok2) = OutwardNormal(curr, next);
            if (!ok1 && !ok2)
                continue; // duplicate-point noise
            if (!ok1) { result.Add(Shift(curr, n2, d)); continue; }
            if (!ok2) { result.Add(Shift(curr, n1, d)); continue; }

            // Intersect the two offset edge lines (each edge translated along its outward normal).
            var a1 = Shift(prev, n1, d);
            var a2 = Shift(curr, n1, d);
            var b1 = Shift(curr, n2, d);
            var b2 = Shift(next, n2, d);

            if (TryIntersect(a1, a2, b1, b2, out var miter)
                && Distance(miter, curr) <= MiterLimitFactor * d + d)
            {
                result.Add(miter);
            }
            else
            {
                // Near-parallel edges or a spiked corner: bevel with both edge offsets.
                result.Add(a2);
                result.Add(b1);
            }
        }

        return result;
    }

    private static (Point2D Normal, bool Ok) OutwardNormal(Point2D from, Point2D to)
    {
        var dx = to.X - from.X;
        var dy = to.Y - from.Y;
        var len = Math.Sqrt(dx * dx + dy * dy);
        if (len < Epsilon)
            return (new Point2D(0, 0), false);
        // CCW polygon: interior lies left of the edge, so outward is the right normal.
        return (new Point2D(dy / len, -dx / len), true);
    }

    private static Point2D Shift(Point2D p, Point2D normal, double d) =>
        new(p.X + normal.X * d, p.Y + normal.Y * d);

    private static bool TryIntersect(Point2D a1, Point2D a2, Point2D b1, Point2D b2, out Point2D result)
    {
        var r = (X: a2.X - a1.X, Y: a2.Y - a1.Y);
        var s = (X: b2.X - b1.X, Y: b2.Y - b1.Y);
        var cross = r.X * s.Y - r.Y * s.X;
        if (Math.Abs(cross) < Epsilon)
        {
            result = default;
            return false;
        }
        var t = ((b1.X - a1.X) * s.Y - (b1.Y - a1.Y) * s.X) / cross;
        result = new Point2D(a1.X + t * r.X, a1.Y + t * r.Y);
        return true;
    }

    private static double Distance(Point2D a, Point2D b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    private static double SignedArea(IReadOnlyList<Point2D> pts)
    {
        double area = 0;
        for (var i = 0; i < pts.Count; i++)
        {
            var p = pts[i];
            var q = pts[(i + 1) % pts.Count];
            area += p.X * q.Y - q.X * p.Y;
        }
        return area / 2;
    }

    // ─────────────────────────── tessellation ───────────────────────────

    /// <summary>
    /// Walks the (head-to-toe ordered) contour and emits a closed polygon. Each curve is traversed
    /// from whichever endpoint matches the pen position — the same connectivity convention the
    /// section renderers use — and arcs are sampled at most every
    /// <paramref name="maxSegmentAngleDegrees"/>.
    /// </summary>
    private static List<Point2D> Tessellate(IReadOnlyList<ICurve2D> contour, double maxSegmentAngleDegrees)
    {
        var points = new List<Point2D>();
        if (contour.Count == 0)
            return points;

        // A lone full circle/ellipse is its own closed contour.
        if (contour.Count == 1 && contour[0] is Circle2D circle)
            return SampleEllipse(circle.Center, circle.Radius, circle.Radius);
        if (contour.Count == 1 && contour[0] is Ellipse2D ellipse)
            return SampleEllipse(ellipse.Center, ellipse.MajorRadius, ellipse.MinorRadius);

        var current = FigureStart(contour);
        Append(points, current);

        foreach (var curve in contour)
        {
            switch (curve)
            {
                case Line2D line:
                {
                    var reversed = !Near(current, line.StartPoint) && Near(current, line.EndPoint);
                    current = reversed ? line.StartPoint : line.EndPoint;
                    Append(points, current);
                    break;
                }

                case Arc2D arc:
                {
                    var reversed = !Near(current, arc.StartPoint) && Near(current, arc.EndPoint);
                    foreach (var p in SampleArc(arc, reversed, maxSegmentAngleDegrees))
                        Append(points, p);
                    current = reversed ? arc.StartPoint : arc.EndPoint;
                    break;
                }

                default:
                    throw new NotSupportedException(
                        $"Curve type {curve.GetType().Name} is not supported in contour offsetting.");
            }
        }

        // Drop the closing duplicate of the start point.
        if (points.Count > 1 && Near(points[0], points[^1]))
            points.RemoveAt(points.Count - 1);
        return points;
    }

    private static Point2D FigureStart(IReadOnlyList<ICurve2D> contour)
    {
        var (s0, e0) = Endpoints(contour[0]);
        if (contour.Count < 2)
            return s0;
        var (s1, e1) = Endpoints(contour[1]);
        var s0Shared = Near(s0, s1) || Near(s0, e1);
        return s0Shared ? e0 : s0;
    }

    private static (Point2D Start, Point2D End) Endpoints(ICurve2D curve) => curve switch
    {
        Line2D l => (l.StartPoint, l.EndPoint),
        Arc2D a => (a.StartPoint, a.EndPoint),
        _ => throw new NotSupportedException(
            $"Curve type {curve.GetType().Name} is not supported in contour offsetting."),
    };

    private static IEnumerable<Point2D> SampleArc(Arc2D arc, bool reversed, double maxSegmentAngleDegrees)
    {
        var sweep = arc.EndAngle - arc.StartAngle;
        if (arc.IsCounterClockwise)
        {
            while (sweep < 0) sweep += 2 * Math.PI;
        }
        else
        {
            while (sweep > 0) sweep -= 2 * Math.PI;
        }

        var steps = Math.Max(1, (int)Math.Ceiling(Math.Abs(sweep) / (maxSegmentAngleDegrees * Math.PI / 180)));
        for (var i = 1; i <= steps; i++)
        {
            var t = reversed ? 1 - (double)i / steps : (double)i / steps;
            var angle = arc.StartAngle + sweep * t;
            yield return new Point2D(
                arc.Center.X + arc.Radius * Math.Cos(angle),
                arc.Center.Y + arc.Radius * Math.Sin(angle));
        }
    }

    private static List<Point2D> SampleEllipse(Point2D center, double a, double b)
    {
        const int segments = 36;
        var points = new List<Point2D>(segments);
        for (var i = 0; i < segments; i++)
        {
            var angle = 2 * Math.PI * i / segments;
            points.Add(new Point2D(center.X + a * Math.Cos(angle), center.Y + b * Math.Sin(angle)));
        }
        return points;
    }

    private static void Append(List<Point2D> points, Point2D p)
    {
        if (points.Count == 0 || !Near(points[^1], p))
            points.Add(p);
    }

    private static bool Near(Point2D a, Point2D b) => Distance(a, b) <= PointTolerance;
}
