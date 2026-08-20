namespace BasAutomation.Geometry;

/// <summary>
/// Represents a point in 2D space.
/// </summary>
/// <param name="x">The X coordinate.</param>
/// <param name="y">The Y coordinate.</param>
public readonly struct Point2D(double x, double y) : IEquatable<Point2D>, IGeometryComparable<Point2D>
{
    /// <summary>
    /// Gets or sets the X coordinate.
    /// </summary>
    public double X { get; } = x;

    /// <summary>
    /// Gets or sets the Y coordinate.
    /// </summary>
    public double Y { get; } = y;

    /// <summary>
    /// Adds a vector to a point.
    /// </summary>
    /// <param name="a">The point.</param>
    /// <param name="v">The vector to add.</param>
    /// <returns>The resulting point.</returns>
    public static Point2D operator +(Point2D a, Vector2D v) => new(a.X + v.X, a.Y + v.Y);

    /// <summary>
    /// Subtracts a vector from a point.
    /// </summary>
    /// <param name="a">The point.</param>
    /// <param name="v">The vector to subtract.</param>
    /// <returns>The resulting point.</returns>
    public static Point2D operator -(Point2D a, Vector2D v) => new(a.X - v.X, a.Y - v.Y);

    /// <summary>
    /// Subtracts one point from another, resulting in a vector.
    /// </summary>
    /// <param name="a">The first point.</param>
    /// <param name="b">The second point.</param>
    /// <returns>The vector from b to a.</returns>
    public static Vector2D operator -(Point2D a, Point2D b) => new(a.X - b.X, a.Y - b.Y);

    /// <summary>
    /// Calculates the distance to another point.
    /// </summary>
    /// <param name="other">The other point.</param>
    /// <returns>The distance between the points.</returns>
    public double DistanceTo(Point2D other) => Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));

    /// <summary>
    /// Determines whether this point is equal to another point within the specified tolerance.
    /// </summary>
    /// <param name="other">The other point to compare.</param>
    /// <param name="tolerance">The tolerance for floating-point comparison. Default is 1e-10.</param>
    /// <returns>true if the points are equal within tolerance; otherwise, false.</returns>
    public bool IsEqualTo(Point2D other, double tolerance = 1e-10)
    {
        return Math.Abs(X - other.X) < tolerance && Math.Abs(Y - other.Y) < tolerance;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj) => obj is Point2D other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(Point2D other) => IsEqualTo(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(X, Y);

    /// <summary>
    /// Determines whether two points are equal.
    /// </summary>
    public static bool operator ==(Point2D left, Point2D right) => left.Equals(right);

    /// <summary>
    /// Determines whether two points are not equal.
    /// </summary>
    public static bool operator !=(Point2D left, Point2D right) => !left.Equals(right);

    /// <inheritdoc/>
    public override string ToString() => $"({X}, {Y})";

    /// <summary>
    /// Determines if this point lies between two other points on a line segment.
    /// </summary>
    public bool IsBetween(Point2D p1, Point2D p2, double tolerance = Constants.Tolerance)
    {
        // Check if point is collinear with p1 and p2 using cross product
        var v1 = this - p1;
        var v2 = p2 - p1;

        // Check collinearity
        if (Math.Abs(v1.Cross(v2)) > tolerance)
            return false;

        // Check if point is within the segment bounds
        var dotProduct = v1.Dot(v2);
        var segmentLengthSquared = v2.LengthSquared;

        return dotProduct >= -tolerance && dotProduct <= segmentLengthSquared + tolerance;
    }

    /// <summary>
    /// Returns a point rotated around a center point by the specified angle.
    /// </summary>
    /// <param name="angleRadians">The rotation angle in radians.</param>
    /// <param name="center">The center of rotation (default is origin).</param>
    /// <returns>The rotated point.</returns>
    public Point2D RotateAround(double angleRadians, Point2D center = default)
    {
        var translated = this - center;
        var rotated = new Vector2D(translated.X, translated.Y).Rotate(angleRadians);
        return center + rotated;
    }

    /// <summary>
    /// Transforms this 2D point to a 3D point in a specified plane coordinate system.
    /// </summary>
    /// <param name="origin">The origin of the 3D coordinate system.</param>
    /// <param name="xAxis">The X axis of the 3D coordinate system (will be normalized).</param>
    /// <param name="yAxis">The Y axis of the 3D coordinate system (will be normalized).</param>
    /// <returns>The 3D point.</returns>
    public Point3D ToPoint3D(Point3D origin, Vector3D xAxis, Vector3D yAxis)
    {
        var x = xAxis.Normalized();
        var y = yAxis.Normalized();
        return origin + X * x + Y * y;
    }

    /// <summary>
    /// Gets the signed distance from this point to another point along a direction.
    /// </summary>
    public double GetDirectionalDistance(Point2D to, Vector2D direction)
    {
        var delta = to - this;
        var unitDirection = direction.Normalized();
        return delta.Dot(unitDirection);
    }

    /// <summary>
    /// Translates the point by a vector.
    /// </summary>
    public Point2D Translate(Vector2D vector)
    {
        return this + vector;
    }


    /// <summary>
    /// Translates the point by a vector.
    /// </summary>
    public Point2D Translate(double dx, double dy)
    {
        return new (X + dx, Y + dy);
    }

    /// <summary>
    /// Converts this point to a vector from the origin.
    /// </summary>
    public Vector2D AsVector()
    {
        return new Vector2D(X, Y);
    }
}