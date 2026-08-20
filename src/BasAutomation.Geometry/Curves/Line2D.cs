namespace BasAutomation.Geometry.Curves;

/// <summary>
/// Represents a 2D line segment defined by start and end points.
/// </summary>
/// <param name="startPoint">The start point of the line.</param>
/// <param name="endPoint">The end point of the line.</param>
public readonly struct Line2D(Point2D startPoint, Point2D endPoint) : IEquatable<Line2D>, IGeometryComparable<Line2D>, ICurve2D
{
    /// <summary>
    /// Gets or sets the start point of the line.
    /// </summary>
    public Point2D StartPoint { get; } = startPoint;

    /// <summary>
    /// Gets or sets the end point of the line.
    /// </summary>
    public Point2D EndPoint { get; } = endPoint;

    /// <summary>
    /// Gets the normalized direction vector of the line.
    /// </summary>
    public Vector2D Direction => (EndPoint - StartPoint).Normalized();

    /// <summary>
    /// Gets the length of the line.
    /// </summary>
    public double Length => StartPoint.DistanceTo(EndPoint);

    /// <summary>
    /// Gets a point along the line at the specified parameter.
    /// </summary>
    /// <param name="t">The parameter (0 = start, 1 = end).</param>
    /// <returns>The point at the specified parameter.</returns>
    public Point2D PointAt(double t) => StartPoint + t * (EndPoint - StartPoint);

    /// <summary>
    /// Gets the midpoint of the line.
    /// </summary>
    public Point2D MidPoint => PointAt(0.5);

    /// <summary>
    /// Calculates the shortest distance from a point to this line segment.
    /// </summary>
    /// <param name="p">The point to measure distance from.</param>
    /// <returns>The shortest distance from the point to the line segment.</returns>
    public double DistanceToPoint(Point2D p)
    {
        var v = EndPoint - StartPoint;
        var w = p - StartPoint;
        var c1 = w.Dot(v);
        if (c1 <= 0) return p.DistanceTo(StartPoint);
        var c2 = v.Dot(v);
        if (c1 >= c2) return p.DistanceTo(EndPoint);
        var b = c1 / c2;
        var pb = StartPoint + b * v;
        return p.DistanceTo(pb);
    }

    /// <summary>
    /// Calculates the parameter t where a point would lie on the infinite line defined by this segment.
    /// The parameter is 0 at StartPoint and 1 at EndPoint.
    /// Values outside [0, 1] indicate the point projection is outside the segment.
    /// </summary>
    /// <param name="point">The point to calculate the parameter for.</param>
    /// <param name="tolerance">The tolerance for zero-length check.</param>
    /// <returns>The parameter t, or 0 if the line has zero length.</returns>
    public double GetParameter(Point2D point, double tolerance = Constants.Tolerance)
    {
        var lineVec = EndPoint - StartPoint;
        var pointVec = point - StartPoint;
        var lengthSq = lineVec.LengthSquared;

        if (lengthSq < tolerance * tolerance)
            return 0;

        return pointVec.Dot(lineVec) / lengthSq;
    }

    /// <summary>
    /// Determines if a point lies on this line segment within the specified tolerance.
    /// </summary>
    /// <param name="point">The point to test.</param>
    /// <param name="tolerance">The tolerance for the test.</param>
    /// <returns>true if the point is on the line segment; otherwise, false.</returns>
    public bool ContainsPoint(Point2D point, double tolerance = Constants.Tolerance)
    {
        var distance = DistanceToPoint(point);
        if (distance > tolerance)
            return false;

        // Check if point is between start and end
        var t = GetParameter(point, tolerance);
        return t >= -tolerance && t <= 1 + tolerance;
    }

    Box2D ICurve2D.BoundingBox => new Box2D(
        new Point2D(Math.Min(StartPoint.X, EndPoint.X), Math.Min(StartPoint.Y, EndPoint.Y)),
        new Point2D(Math.Max(StartPoint.X, EndPoint.X), Math.Max(StartPoint.Y, EndPoint.Y))
    );

    Vector2D ICurve2D.TangentAt(double t) => Direction;
    public ICurve2D Clone()
    {
        return new Line2D(StartPoint, EndPoint);
    }

    /// <summary>
    /// Determines whether this line is equal to another line within the specified tolerance.
    /// </summary>
    /// <param name="other">The other line to compare.</param>
    /// <param name="tolerance">The tolerance for floating-point comparison. Default is 1e-10.</param>
    /// <returns>true if the lines are equal within tolerance; otherwise, false.</returns>
    public bool IsEqualTo(Line2D other, double tolerance = 1e-10)
    {
        return StartPoint.IsEqualTo(other.StartPoint, tolerance) && EndPoint.IsEqualTo(other.EndPoint, tolerance);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj) => obj is Line2D other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(Line2D other) => IsEqualTo(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(StartPoint, EndPoint);

    /// <summary>
    /// Determines whether two lines are equal.
    /// </summary>
    public static bool operator ==(Line2D left, Line2D right) => left.Equals(right);

    /// <summary>
    /// Determines whether two lines are not equal.
    /// </summary>
    public static bool operator !=(Line2D left, Line2D right) => !left.Equals(right);

    /// <inheritdoc/>
    public override string ToString() => $"Line2D(Start: {StartPoint}, End: {EndPoint}, Direction: {Direction})";

    /// <inheritdoc/>
    public ICurve2D Transform(Matrix2D transform)
    {
        return new Line2D(
            transform.Transform(StartPoint),
            transform.Transform(EndPoint)
        );
    }

    /// <summary>
    /// Reverses the direction of the line.
    /// </summary>
    /// <returns>A new line with reversed direction.</returns>
    public Line2D Reverse()
    {
        return new Line2D(EndPoint, StartPoint);
    }

    /// <inheritdoc/>
    ICurve2D ICurve2D.Reverse()
    {
        return Reverse();
    }

    /// <summary>
    /// Finds intersection points between this line segment and another line segment.
    /// </summary>
    /// <param name="line">The other line segment.</param>
    /// <returns>List of intersection points (0 or 1 point).</returns>
    public List<Point2D> IntersectWithLine(Line2D line)
    {
        var result = new List<Point2D>();

        var d1 = EndPoint - StartPoint;
        var d2 = line.EndPoint - line.StartPoint;

        var cross = d1.X * d2.Y - d1.Y * d2.X;

        if (Math.Abs(cross) < Constants.Tolerance)
            return result; // Lines are parallel

        var diff = line.StartPoint - StartPoint;
        var t = (diff.X * d2.Y - diff.Y * d2.X) / cross;
        var s = (diff.X * d1.Y - diff.Y * d1.X) / cross;

        if (t < -Constants.Tolerance || t > 1 + Constants.Tolerance ||
            s < -Constants.Tolerance || s > 1 + Constants.Tolerance)
            return result;

        result.Add(PointAt(Math.Max(0, Math.Min(1, t))));
        return result;
    }

    /// <summary>
    /// Finds intersection points between this line segment and a ray.
    /// </summary>
    /// <param name="origin">The ray origin point.</param>
    /// <param name="direction">The ray direction (extends infinitely in this direction).</param>
    /// <returns>List of intersection points (0 or 1 point).</returns>
    public List<Point2D> IntersectWithRay(Point2D origin, Vector2D direction)
    {
        var result = new List<Point2D>();

        var d1 = EndPoint - StartPoint;
        var cross = d1.X * direction.Y - d1.Y * direction.X;

        if (Math.Abs(cross) < Constants.Tolerance)
            return result; // Lines are parallel

        var diff = origin - StartPoint;
        var t = (diff.X * direction.Y - diff.Y * direction.X) / cross;
        var s = (diff.X * d1.Y - diff.Y * d1.X) / cross;

        if (t < -Constants.Tolerance || t > 1 + Constants.Tolerance || s < -Constants.Tolerance)
            return result;

        result.Add(PointAt(Math.Max(0, Math.Min(1, t))));
        return result;
    }
}