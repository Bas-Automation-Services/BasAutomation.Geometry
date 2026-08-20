namespace BasAutomation.Geometry.Curves;

/// <summary>
/// Represents a circle in 2D space defined by center and radius.
/// </summary>
public readonly struct Circle2D : IEquatable<Circle2D>, IGeometryComparable<Circle2D>, ICurve2D
{
    /// <summary>
    /// Gets or sets the center point of the circle.
    /// </summary>
    public Point2D Center { get; }

    /// <summary>
    /// Gets or sets the radius of the circle.
    /// </summary>
    public double Radius { get; }

    /// <summary>
    /// Initializes a new instance of Circle2D from center and radius.
    /// </summary>
    /// <param name="center">The center point of the circle.</param>
    /// <param name="radius">The radius of the circle.</param>
    public Circle2D(Point2D center, double radius)
    {
        if (radius < 0)
            throw new ArgumentException("Radius must be non-negative.", nameof(radius));

        Center = center;
        Radius = radius;
    }

    /// <summary>
    /// Initializes a new instance of Circle2D from center coordinates and radius.
    /// </summary>
    /// <param name="centerX">The X coordinate of the center.</param>
    /// <param name="centerY">The Y coordinate of the center.</param>
    /// <param name="radius">The radius of the circle.</param>
    public Circle2D(double centerX, double centerY, double radius)
        : this(new Point2D(centerX, centerY), radius)
    {
    }

    /// <summary>
    /// Creates a circle from three points on the circumference.
    /// </summary>
    /// <param name="p1">First point on the circle.</param>
    /// <param name="p2">Second point on the circle.</param>
    /// <param name="p3">Third point on the circle.</param>
    /// <returns>A new Circle2D instance.</returns>
    public static Circle2D FromThreePoints(Point2D p1, Point2D p2, Point2D p3)
    {
        // Calculate the center using the three points
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

        var center = new Point2D(ux, uy);
        var radius = center.DistanceTo(p1);

        return new Circle2D(center, radius);
    }

    /// <summary>
    /// Creates a circle from two points defining a diameter.
    /// </summary>
    /// <param name="p1">First endpoint of the diameter.</param>
    /// <param name="p2">Second endpoint of the diameter.</param>
    /// <returns>A new Circle2D instance.</returns>
    public static Circle2D FromDiameter(Point2D p1, Point2D p2)
    {
        var center = new Point2D((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
        var radius = p1.DistanceTo(p2) / 2;
        return new Circle2D(center, radius);
    }

    /// <summary>
    /// Gets the diameter of the circle.
    /// </summary>
    public double Diameter => 2 * Radius;

    /// <summary>
    /// Gets the circumference of the circle.
    /// </summary>
    public double Circumference => 2 * Math.PI * Radius;

    /// <summary>
    /// Gets the area of the circle.
    /// </summary>
    public double Area => Math.PI * Radius * Radius;

    // ICurve2D implementation
    public Point2D StartPoint => Center + new Vector2D(Radius, 0);
    public Point2D EndPoint => StartPoint;
    public double Length => Circumference;
    public Box2D BoundingBox => new(
        new Point2D(Center.X - Radius, Center.Y - Radius),
        new Point2D(Center.X + Radius, Center.Y + Radius)
    );

    /// <summary>
    /// Gets a point on the circle at the specified parameter.
    /// </summary>
    /// <param name="t">The parameter (0 to 1 for one full revolution).</param>
    /// <returns>The point at the specified parameter.</returns>
    public Point2D PointAt(double t)
    {
        var angle = t * 2 * Math.PI;
        return Center + new Vector2D(Radius * Math.Cos(angle), Radius * Math.Sin(angle));
    }

    /// <summary>
    /// Gets the tangent vector at the specified parameter.
    /// </summary>
    /// <param name="t">The parameter (0 to 1 for one full revolution).</param>
    /// <returns>The tangent vector at the specified parameter.</returns>
    public Vector2D TangentAt(double t)
    {
        var angle = t * 2 * Math.PI;
        return new Vector2D(-Math.Sin(angle), Math.Cos(angle));
    }

    public ICurve2D Clone()
    {
        return new Circle2D(Center, Radius);
    }

    /// <summary>
    /// Determines if a point lies on the circle within the specified tolerance.
    /// </summary>
    /// <param name="point">The point to test.</param>
    /// <param name="tolerance">The tolerance for the test.</param>
    /// <returns>true if the point is on the circle; otherwise, false.</returns>
    public bool ContainsPoint(Point2D point, double tolerance = Constants.Tolerance)
    {
        var distance = point.DistanceTo(Center);
        return Math.Abs(distance - Radius) < tolerance;
    }

    /// <summary>
    /// Determines if a point lies inside the circle.
    /// </summary>
    /// <param name="point">The point to test.</param>
    /// <returns>true if the point is inside the circle; otherwise, false.</returns>
    public bool IsPointInside(Point2D point)
    {
        return point.DistanceTo(Center) < Radius;
    }

    /// <summary>
    /// Transforms the circle by the specified matrix.
    /// </summary>
    /// <param name="matrix">The transformation matrix.</param>
    /// <returns>The transformed circle.</returns>
    public ICurve2D Transform(Matrix2D matrix)
    {
        // Transform center
        var transformedCenter = matrix.Transform(Center);

        // Transform two points on the circle to determine scale
        var p1 = Center + new Vector2D(Radius, 0);
        var p2 = Center + new Vector2D(0, Radius);
        var tp1 = matrix.Transform(p1);
        var tp2 = matrix.Transform(p2);

        // Calculate average radius (assumes uniform scaling)
        var r1 = transformedCenter.DistanceTo(tp1);
        var r2 = transformedCenter.DistanceTo(tp2);
        var transformedRadius = (r1 + r2) / 2;

        return new Circle2D(transformedCenter, transformedRadius);
    }

    /// <summary>
    /// Converts this circle to an arc representing the full circle.
    /// </summary>
    /// <returns>An Arc2D representing the full circle.</returns>
    public Arc2D ToArc()
    {
        // Use a small epsilon to ensure the end angle is not normalized to 0
        return new Arc2D(Center, Radius, 0, 2 * Math.PI - 1e-10, true);
    }

    /// <inheritdoc/>
    /// <exception cref="NotSupportedException">Circle2D does not support reversal as it is a closed curve.</exception>
    ICurve2D ICurve2D.Reverse()
    {
        throw new NotSupportedException("Circle2D does not support reversal as it is a closed curve with no defined start or end point.");
    }

    /// <summary>
    /// Calculates the intersection points of this circle with another circle.
    /// </summary>
    /// <param name="other">The other circle.</param>
    /// <param name="point1">The first intersection point if it exists.</param>
    /// <param name="point2">The second intersection point if it exists.</param>
    /// <returns>The number of intersection points (0, 1, or 2).</returns>
    public int IntersectCircle(Circle2D other, out Point2D point1, out Point2D point2)
    {
        point1 = new Point2D(0, 0);
        point2 = new Point2D(0, 0);

        var d = Center.DistanceTo(other.Center);

        // No intersection: circles are too far apart
        if (d > Radius + other.Radius + Constants.Tolerance)
            return 0;

        // No intersection: one circle is inside the other
        if (d < Math.Abs(Radius - other.Radius) - Constants.Tolerance)
            return 0;

        // Circles are coincident (infinite intersections)
        if (d < Constants.Tolerance && Math.Abs(Radius - other.Radius) < Constants.Tolerance)
            return -1; // Special case: infinite intersections

        // Calculate intersection points
        var a = (Radius * Radius - other.Radius * other.Radius + d * d) / (2 * d);
        var h = Math.Sqrt(Radius * Radius - a * a);

        var p2 = Center + a * (other.Center - Center) / d;
        var offset = h * new Vector2D(
            -(other.Center.Y - Center.Y) / d,
            (other.Center.X - Center.X) / d
        );

        point1 = p2 + offset;
        point2 = p2 - offset;

        // Check if it's a single point (tangent)
        if (h < Constants.Tolerance)
            return 1;

        return 2;
    }

    /// <summary>
    /// Calculates the intersection points of this circle with a line.
    /// </summary>
    /// <param name="line">The line to intersect with.</param>
    /// <param name="point1">The first intersection point if it exists.</param>
    /// <param name="point2">The second intersection point if it exists.</param>
    /// <returns>The number of intersection points (0, 1, or 2).</returns>
    public int IntersectLine(Line2D line, out Point2D point1, out Point2D point2)
    {
        point1 = new Point2D(0, 0);
        point2 = new Point2D(0, 0);

        var d = line.EndPoint - line.StartPoint;
        var f = line.StartPoint - Center;

        var a = d.Dot(d);
        var b = 2 * f.Dot(d);
        var c = f.Dot(f) - Radius * Radius;

        var discriminant = b * b - 4 * a * c;

        if (discriminant < -Constants.Tolerance)
            return 0;

        if (discriminant < Constants.Tolerance)
        {
            // One intersection (tangent)
            var t = -b / (2 * a);
            if (t >= 0 && t <= 1)
            {
                point1 = line.PointAt(t);
                return 1;
            }
            return 0;
        }

        // Two intersections
        discriminant = Math.Sqrt(discriminant);
        var t1 = (-b - discriminant) / (2 * a);
        var t2 = (-b + discriminant) / (2 * a);

        var count = 0;
        if (t1 >= 0 && t1 <= 1)
        {
            point1 = line.PointAt(t1);
            count++;
        }

        if (t2 >= 0 && t2 <= 1)
        {
            if (count == 0)
            {
                point1 = line.PointAt(t2);
            }
            else
            {
                point2 = line.PointAt(t2);
            }
            count++;
        }

        return count;
    }

    /// <summary>
    /// Determines whether this circle is equal to another circle within the specified tolerance.
    /// </summary>
    /// <param name="other">The other circle to compare.</param>
    /// <param name="tolerance">The tolerance for floating-point comparison.</param>
    /// <returns>true if the circles are equal within tolerance; otherwise, false.</returns>
    public bool IsEqualTo(Circle2D other, double tolerance = Constants.Tolerance)
    {
        return Center.IsEqualTo(other.Center, tolerance) &&
               Math.Abs(Radius - other.Radius) < tolerance;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj) => obj is Circle2D other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(Circle2D other) => IsEqualTo(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Center, Radius);

    /// <summary>
    /// Determines whether two circles are equal.
    /// </summary>
    public static bool operator ==(Circle2D left, Circle2D right) => left.Equals(right);

    /// <summary>
    /// Determines whether two circles are not equal.
    /// </summary>
    public static bool operator !=(Circle2D left, Circle2D right) => !left.Equals(right);

    /// <inheritdoc/>
    public override string ToString() => $"Circle2D(Center: {Center}, Radius: {Radius})";

    /// <summary>
    /// Finds intersection points between this circle and a line segment.
    /// </summary>
    /// <param name="line">The line segment to intersect with.</param>
    /// <returns>List of intersection points (0, 1, or 2 points).</returns>
    public List<Point2D> IntersectWithLine(Line2D line)
    {
        var results = new List<Point2D>();

        var d = line.EndPoint - line.StartPoint;
        var f = line.StartPoint - Center;

        var a = d.Dot(d);
        var b = 2 * f.Dot(d);
        var c = f.Dot(f) - Radius * Radius;

        var discriminant = b * b - 4 * a * c;

        if (discriminant < -Constants.Tolerance)
            return results;

        if (discriminant < Constants.Tolerance)
        {
            var t = -b / (2 * a);
            if (t >= -Constants.Tolerance && t <= 1 + Constants.Tolerance)
                results.Add(line.PointAt(Math.Max(0, Math.Min(1, t))));
            return results;
        }

        var sqrtD = Math.Sqrt(discriminant);
        var t1 = (-b - sqrtD) / (2 * a);
        var t2 = (-b + sqrtD) / (2 * a);

        if (t1 >= -Constants.Tolerance && t1 <= 1 + Constants.Tolerance)
            results.Add(line.PointAt(Math.Max(0, Math.Min(1, t1))));

        if (t2 >= -Constants.Tolerance && t2 <= 1 + Constants.Tolerance)
            results.Add(line.PointAt(Math.Max(0, Math.Min(1, t2))));

        return results;
    }

    /// <summary>
    /// Finds intersection points between this circle and a ray.
    /// </summary>
    /// <param name="origin">The ray origin point.</param>
    /// <param name="direction">The ray direction (extends infinitely in this direction).</param>
    /// <returns>List of intersection points (0, 1, or 2 points).</returns>
    public List<Point2D> IntersectWithRay(Point2D origin, Vector2D direction)
    {
        var results = new List<Point2D>();

        var f = origin - Center;

        var a = direction.Dot(direction);
        var b = 2 * f.Dot(direction);
        var c = f.Dot(f) - Radius * Radius;

        var discriminant = b * b - 4 * a * c;

        if (discriminant < -Constants.Tolerance)
            return results;

        if (discriminant < Constants.Tolerance)
        {
            var t = -b / (2 * a);
            if (t >= -Constants.Tolerance)
                results.Add(origin + Math.Max(0, t) * direction);
            return results;
        }

        var sqrtD = Math.Sqrt(discriminant);
        var t1 = (-b - sqrtD) / (2 * a);
        var t2 = (-b + sqrtD) / (2 * a);

        if (t1 >= -Constants.Tolerance)
            results.Add(origin + Math.Max(0, t1) * direction);

        if (t2 >= -Constants.Tolerance)
            results.Add(origin + Math.Max(0, t2) * direction);

        return results;
    }
}