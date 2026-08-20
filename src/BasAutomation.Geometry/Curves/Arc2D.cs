namespace BasAutomation.Geometry.Curves;

/// <summary>
/// Represents a 2D circular arc defined by center, radius, and angles.
///
/// Arc angle convention:
///
///              90°
///               |
///               |
///    180°  ---- o ----  0°
///               |
///               | 
///             270°
///
/// Rotation: Counterclockwise (CCW)
/// 0° = Right, 90° = Top, 180° = Left, 270° = Bottom
/// 
/// </summary>
public readonly struct Arc2D : IEquatable<Arc2D>, IGeometryComparable<Arc2D>, ICurve2D
{
    /// <summary>
    /// Gets or sets the center point of the arc.
    /// </summary>
    public Point2D Center { get; }

    /// <summary>
    /// Gets or sets the radius of the arc.
    /// </summary>
    public double Radius { get; }

    /// <summary>
    /// Gets or sets the start angle in radians (measured from positive X-axis, counterclockwise).
    /// </summary>
    public double StartAngle { get; }

    /// <summary>
    /// Gets or sets the end angle in radians (measured from positive X-axis, counterclockwise).
    /// </summary>
    public double EndAngle { get; }

    /// <summary>
    /// Gets or sets whether the arc is drawn counterclockwise (true) or clockwise (false).
    /// </summary>
    public bool IsCounterClockwise { get; }

    /// <summary>
    /// Initializes a new instance of Arc2D from center, radius, start angle, and end angle.
    /// </summary>
    /// <param name="center">The center point of the arc.</param>
    /// <param name="radius">The radius of the arc.</param>
    /// <param name="startAngle">The start angle in radians.</param>
    /// <param name="endAngle">The end angle in radians.</param>
    /// <param name="isCounterClockwise">Whether the arc is drawn counterclockwise.</param>
    public Arc2D(Point2D center, double radius, double startAngle, double endAngle, bool isCounterClockwise = true)
    {
        Center = center;
        Radius = radius;
        StartAngle = NormalizeAngle(startAngle);
        EndAngle = NormalizeAngle(endAngle);
        IsCounterClockwise = isCounterClockwise;
    }

    /// <summary>
    /// Initializes a new instance of Arc2D from three points on the arc.
    /// The arc is created from point1 through point2 to point3.
    /// </summary>
    /// <param name="point1">First point on the arc (start).</param>
    /// <param name="point2">Second point on the arc (intermediate).</param>
    /// <param name="point3">Third point on the arc (end).</param>
    public Arc2D(Point2D point1, Point2D point2, Point2D point3)
    {
        // Calculate the center of the circle passing through all three points
        var center = CalculateCircleCenter(point1, point2, point3);
        var radius = point1.DistanceTo(center);

        // Calculate angles for all three points
        var angle1 = Math.Atan2(point1.Y - center.Y, point1.X - center.X);
        var angle2 = Math.Atan2(point2.Y - center.Y, point2.X - center.X);
        var angle3 = Math.Atan2(point3.Y - center.Y, point3.X - center.X);

        // Normalize angles to [0, 2π)
        angle1 = NormalizeAngle(angle1);
        angle2 = NormalizeAngle(angle2);
        angle3 = NormalizeAngle(angle3);

        // Determine the order and direction
        // Check if point2 is between point1 and point3 in counterclockwise direction
        bool isCounterClockwise;
        double startAngle, endAngle;

        // Check if going from point1 to point3 counterclockwise passes through point2
        if (IsAngleBetweenCounterClockwise(angle2, angle1, angle3))
        {
            isCounterClockwise = true;
            startAngle = angle1;
            endAngle = angle3;
        }
        else
        {
            isCounterClockwise = false;
            startAngle = angle1;
            endAngle = angle3;
        }

        Center = center;
        Radius = radius;
        StartAngle = startAngle;
        EndAngle = endAngle;
        IsCounterClockwise = isCounterClockwise;
    }

    /// <summary>
    /// Initializes a new instance of Arc2D from start point, end point, and center point.
    /// </summary>
    /// <param name="startPoint">The start point of the arc.</param>
    /// <param name="endPoint">The end point of the arc.</param>
    /// <param name="centerPoint">The center point of the arc.</param>
    /// <param name="isCounterClockwise">Whether the arc is drawn counterclockwise.</param>
    public Arc2D(Point2D startPoint, Point2D endPoint, Point2D centerPoint, bool isCounterClockwise = true)
    {
        var radius = startPoint.DistanceTo(centerPoint);

        // Verify that end point is also on the circle (within tolerance)
        var endRadius = endPoint.DistanceTo(centerPoint);
        if (Math.Abs(radius - endRadius) > Constants.Tolerance)
        {
            throw new ArgumentException("Start and end points must be equidistant from center point.");
        }

        // Calculate angles
        var startVector = startPoint - centerPoint;
        var startAngle = Math.Atan2(startVector.Y, startVector.X);

        var endVector = endPoint - centerPoint;
        var endAngle = Math.Atan2(endVector.Y, endVector.X);

        Center = centerPoint;
        Radius = radius;
        StartAngle = NormalizeAngle(startAngle);
        EndAngle = NormalizeAngle(endAngle);
        IsCounterClockwise = isCounterClockwise;
    }

    /// <summary>
    /// Checks if an angle is between two other angles in the counterclockwise direction.
    /// </summary>
    private static bool IsAngleBetweenCounterClockwise(double angle, double startAngle, double endAngle)
    {
        // Normalize all angles to [0, 2π)
        angle = NormalizeAngle(angle);
        startAngle = NormalizeAngle(startAngle);
        endAngle = NormalizeAngle(endAngle);

        if (startAngle <= endAngle)
        {
            return angle >= startAngle && angle <= endAngle;
        }

        return angle >= startAngle || angle <= endAngle;
    }
    
    /// <summary>
    /// Gets the start point of the arc.
    /// </summary>
    public Point2D StartPoint => Center + new Vector2D(Radius * Math.Cos(StartAngle), Radius * Math.Sin(StartAngle));

    /// <summary>
    /// Gets the end point of the arc.
    /// </summary>
    public Point2D EndPoint => Center + new Vector2D(Radius * Math.Cos(EndAngle), Radius * Math.Sin(EndAngle));

    /// <summary>
    /// Gets the sweep angle of the arc in radians.
    /// </summary>
    public double SweepAngle
    {
        get
        {
            var sweep = EndAngle - StartAngle;
            if (IsCounterClockwise)
            {
                if (sweep <= 0) sweep += 2 * Math.PI;
            }
            else
            {
                if (sweep >= 0) sweep -= 2 * Math.PI;
            }
            return sweep;
        }
    }

    /// <summary>
    /// Gets the arc length.
    /// </summary>
    public double Length => Math.Abs(SweepAngle) * Radius;

    /// <summary>
    /// Gets the midpoint of the arc.
    /// </summary>
    public Point2D MidPoint => PointAt(0.5);

    /// <summary>
    /// Gets a point on the arc at the specified parameter.
    /// </summary>
    /// <param name="t">The parameter (0 = start, 1 = end).</param>
    /// <returns>The point at the specified parameter.</returns>
    public Point2D PointAt(double t)
    {
        var angle = StartAngle + t * SweepAngle;
        return Center + new Vector2D(Radius * Math.Cos(angle), Radius * Math.Sin(angle));
    }

    /// <summary>
    /// Gets the tangent vector at the specified parameter.
    /// </summary>
    /// <param name="t">The parameter (0 = start, 1 = end).</param>
    /// <returns>The tangent vector at the specified parameter.</returns>
    public Vector2D TangentAt(double t)
    {
        var angle = StartAngle + t * SweepAngle;
        var tangent = new Vector2D(-Math.Sin(angle), Math.Cos(angle));
        return IsCounterClockwise ? tangent : -tangent;
    }

    public ICurve2D Clone()
    {
        return new Arc2D(Center, Radius, StartAngle, EndAngle, IsCounterClockwise);
    }

    /// <summary>
    /// Determines if a point lies on the arc within the specified tolerance.
    /// </summary>
    /// <param name="point">The point to test.</param>
    /// <param name="tolerance">The tolerance for the test.</param>
    /// <returns>true if the point is on the arc; otherwise, false.</returns>
    public bool ContainsPoint(Point2D point, double tolerance = Constants.Tolerance)
    {
        // Check if point is on the circle
        var distance = point.DistanceTo(Center);
        if (Math.Abs(distance - Radius) > tolerance)
            return false;

        // Check if point is within the arc's angular range
        var angle = Math.Atan2(point.Y - Center.Y, point.X - Center.X);
        return IsAngleInArcRange(angle);
    }

    /// <summary>
    /// Gets the bounding box of the arc.
    /// </summary>
    /// <returns>The bounding box containing the arc.</returns>
    public Box2D GetBoundingBox()
    {
        var points = new List<Point2D> { StartPoint, EndPoint };

        // Check if the arc crosses the cardinal directions (0, π/2, π, 3π/2)
        var cardinalAngles = new[] { 0, Math.PI / 2, Math.PI, 3 * Math.PI / 2 };
        foreach (var angle in cardinalAngles)
        {
            if (IsAngleInArcRange(angle))
            {
                points.Add(Center + new Vector2D(Radius * Math.Cos(angle), Radius * Math.Sin(angle)));
            }
        }

        // Find min and max coordinates
        var minX = points.Min(p => p.X);
        var minY = points.Min(p => p.Y);
        var maxX = points.Max(p => p.X);
        var maxY = points.Max(p => p.Y);

        return new Box2D(new Point2D(minX, minY), new Point2D(maxX, maxY));
    }

    // ICurve2D implementation
    Box2D ICurve2D.BoundingBox => GetBoundingBox();

    /// <summary>
    /// Transforms the arc by the specified matrix.
    /// </summary>
    /// <param name="matrix">The transformation matrix.</param>
    /// <returns>The transformed arc.</returns>
    public ICurve2D Transform(Matrix2D matrix)
    {
        // Transform center and two points on the arc
        var transformedCenter = matrix.Transform(Center);
        var transformedStart = matrix.Transform(StartPoint);
        var transformedEnd = matrix.Transform(EndPoint);

        // Check if the transformation includes a reflection (negative determinant)
        // Reflections reverse the winding order of arcs
        var isReflection = matrix.Determinant < 0;
        var newDirection = isReflection ? !IsCounterClockwise : IsCounterClockwise;

        // Note: This assumes uniform scaling. Non-uniform scaling would create an ellipse.
        return new Arc2D(transformedStart, transformedEnd, transformedCenter, newDirection);
    }

    /// <summary>
    /// Reverses the direction of the arc.
    /// </summary>
    /// <returns>A new arc with reversed direction.</returns>
    public Arc2D Reverse()
    {
        return new Arc2D(Center, Radius, EndAngle, StartAngle, !IsCounterClockwise);
    }

    /// <inheritdoc/>
    ICurve2D ICurve2D.Reverse()
    {
        return Reverse();
    }

    /// <summary>
    /// Normalizes an angle to the range [0, 2π).
    /// </summary>
    private static double NormalizeAngle(double angle)
    {
        angle = angle % (2 * Math.PI);
        if (angle < 0) angle += 2 * Math.PI;
        return angle;
    }

    /// <summary>
    /// Checks if an angle is within the arc's range.
    /// </summary>
    private bool IsAngleInArcRange(double angle)
    {
        angle = NormalizeAngle(angle);
        var start = NormalizeAngle(StartAngle);
        var end = NormalizeAngle(EndAngle);

        if (IsCounterClockwise)
        {
            if (start <= end)
                return angle >= start && angle <= end;
            else
                return angle >= start || angle <= end;
        }
        else
        {
            if (start >= end)
                return angle <= start && angle >= end;
            else
                return angle <= start || angle >= end;
        }
    }

    /// <summary>
    /// Calculates the center of a circle passing through three points.
    /// </summary>
    private static Point2D CalculateCircleCenter(Point2D p1, Point2D p2, Point2D p3)
    {
        var d = 2 * (p1.X * (p2.Y - p3.Y) + p2.X * (p3.Y - p1.Y) + p3.X * (p1.Y - p2.Y));

        if (Math.Abs(d) < Constants.Tolerance)
        {
            throw new ArgumentException("The three points are collinear and do not define a unique circle.");
        }

        var ux = ((p1.X * p1.X + p1.Y * p1.Y) * (p2.Y - p3.Y) +
                  (p2.X * p2.X + p2.Y * p2.Y) * (p3.Y - p1.Y) +
                  (p3.X * p3.X + p3.Y * p3.Y) * (p1.Y - p2.Y)) / d;

        var uy = ((p1.X * p1.X + p1.Y * p1.Y) * (p3.X - p2.X) +
                  (p2.X * p2.X + p2.Y * p2.Y) * (p1.X - p3.X) +
                  (p3.X * p3.X + p3.Y * p3.Y) * (p2.X - p1.X)) / d;

        return new Point2D(ux, uy);
    }

    /// <summary>
    /// Determines whether this arc is equal to another arc within the specified tolerance.
    /// </summary>
    /// <param name="other">The other arc to compare.</param>
    /// <param name="tolerance">The tolerance for floating-point comparison.</param>
    /// <returns>true if the arcs are equal within tolerance; otherwise, false.</returns>
    public bool IsEqualTo(Arc2D other, double tolerance = Constants.Tolerance)
    {
        return Center.IsEqualTo(other.Center, tolerance) &&
               Math.Abs(Radius - other.Radius) < tolerance &&
               Math.Abs(NormalizeAngle(StartAngle) - NormalizeAngle(other.StartAngle)) < tolerance &&
               Math.Abs(NormalizeAngle(EndAngle) - NormalizeAngle(other.EndAngle)) < tolerance &&
               IsCounterClockwise == other.IsCounterClockwise;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj) => obj is Arc2D other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(Arc2D other) => IsEqualTo(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Center, Radius, StartAngle, EndAngle, IsCounterClockwise);

    /// <summary>
    /// Determines whether two arcs are equal.
    /// </summary>
    public static bool operator ==(Arc2D left, Arc2D right) => left.Equals(right);

    /// <summary>
    /// Determines whether two arcs are not equal.
    /// </summary>
    public static bool operator !=(Arc2D left, Arc2D right) => !left.Equals(right);

    /// <inheritdoc/>
    public override string ToString() => $"Arc2D(Start: {StartPoint}, End: {EndPoint}, Center: {Center}, Radius: {Radius}, StartAngle: {StartAngle:F2}, EndAngle: {EndAngle:F2}, CCW: {IsCounterClockwise})";

    /// <summary>
    /// Finds intersection points between this arc and a line segment.
    /// </summary>
    /// <param name="line">The line segment to intersect with.</param>
    /// <returns>List of intersection points (0, 1, or 2 points).</returns>
    public List<Point2D> IntersectWithLine(Line2D line)
    {
        var results = new List<Point2D>();

        var d = line.EndPoint - line.StartPoint;
        var w = line.StartPoint - Center;

        var a = d.Dot(d);
        var b = 2 * w.Dot(d);
        var c = w.Dot(w) - Radius * Radius;

        var discriminant = b * b - 4 * a * c;

        if (discriminant < -Constants.Tolerance)
            return results; // No intersection

        if (Math.Abs(discriminant) < Constants.Tolerance)
        {
            // Tangent - single intersection
            var t = -b / (2 * a);
            if (t >= -Constants.Tolerance && t <= 1 + Constants.Tolerance)
            {
                var point = line.PointAt(Math.Max(0, Math.Min(1, t)));
                if (ContainsPoint(point))
                    results.Add(point);
            }
        }
        else
        {
            var sqrtD = Math.Sqrt(discriminant);
            var t1 = (-b - sqrtD) / (2 * a);
            var t2 = (-b + sqrtD) / (2 * a);

            if (t1 >= -Constants.Tolerance && t1 <= 1 + Constants.Tolerance)
            {
                var point1 = line.PointAt(Math.Max(0, Math.Min(1, t1)));
                if (ContainsPoint(point1))
                    results.Add(point1);
            }

            if (t2 >= -Constants.Tolerance && t2 <= 1 + Constants.Tolerance)
            {
                var point2 = line.PointAt(Math.Max(0, Math.Min(1, t2)));
                if (ContainsPoint(point2))
                    results.Add(point2);
            }
        }

        return results;
    }

    /// <summary>
    /// Finds intersection points between this arc and a ray.
    /// </summary>
    /// <param name="origin">The ray origin point.</param>
    /// <param name="direction">The ray direction (extends infinitely in this direction).</param>
    /// <returns>List of intersection points (0, 1, or 2 points).</returns>
    public List<Point2D> IntersectWithRay(Point2D origin, Vector2D direction)
    {
        var results = new List<Point2D>();

        var d = direction;
        var w = origin - Center;

        var a = d.Dot(d);
        var b = 2 * w.Dot(d);
        var c = w.Dot(w) - Radius * Radius;

        var discriminant = b * b - 4 * a * c;

        if (discriminant < -Constants.Tolerance)
            return results; // No intersection

        if (Math.Abs(discriminant) < Constants.Tolerance)
        {
            var t = -b / (2 * a);
            if (t >= -Constants.Tolerance)
            {
                var point = origin + Math.Max(0, t) * d;
                if (ContainsPoint(point))
                    results.Add(point);
            }
        }
        else
        {
            var sqrtD = Math.Sqrt(discriminant);
            var t1 = (-b - sqrtD) / (2 * a);
            var t2 = (-b + sqrtD) / (2 * a);

            if (t1 >= -Constants.Tolerance)
            {
                var point1 = origin + Math.Max(0, t1) * d;
                if (ContainsPoint(point1))
                    results.Add(point1);
            }

            if (t2 >= -Constants.Tolerance)
            {
                var point2 = origin + Math.Max(0, t2) * d;
                if (ContainsPoint(point2))
                    results.Add(point2);
            }
        }

        return results;
    }
}
