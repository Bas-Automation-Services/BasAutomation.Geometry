namespace BasAutomation.Geometry;

/// <summary>
/// Represents a point in 3D space.
/// </summary>
/// <param name="x">The X coordinate.</param>
/// <param name="y">The Y coordinate.</param>
/// <param name="z">The Z coordinate.</param>
public readonly struct Point3D(double x, double y, double z) : IEquatable<Point3D>, IGeometryComparable<Point3D>
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
    /// Gets or sets the Z coordinate.
    /// </summary>
    public double Z { get; } = z;

    /// <summary>
    /// Adds a vector to a point.
    /// </summary>
    /// <param name="a">The point.</param>
    /// <param name="v">The vector to add.</param>
    /// <returns>The resulting point.</returns>
    public static Point3D operator +(Point3D a, Vector3D v) => new(a.X + v.X, a.Y + v.Y, a.Z + v.Z);

    /// <summary>
    /// Subtracts a vector from a point.
    /// </summary>
    /// <param name="a">The point.</param>
    /// <param name="v">The vector to subtract.</param>
    /// <returns>The resulting point.</returns>
    public static Point3D operator -(Point3D a, Vector3D v) => new(a.X - v.X, a.Y - v.Y, a.Z - v.Z);

    /// <summary>
    /// Subtracts one point from another, resulting in a vector.
    /// </summary>
    /// <param name="a">The first point.</param>
    /// <param name="b">The second point.</param>
    /// <returns>The vector from b to a.</returns>
    public static Vector3D operator -(Point3D a, Point3D b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

    /// <summary>
    /// Calculates the distance to another point.
    /// </summary>
    /// <param name="other">The other point.</param>
    /// <returns>The distance between the points.</returns>
    public double DistanceTo(Point3D other) => Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2) + Math.Pow(Z - other.Z, 2));

    /// <summary>
    /// Determines whether this point is equal to another point within the specified tolerance.
    /// </summary>
    /// <param name="other">The other point to compare.</param>
    /// <param name="tolerance">The tolerance for floating-point comparison. Default is 1e-10.</param>
    /// <returns>true if the points are equal within tolerance; otherwise, false.</returns>
    public bool IsEqualTo(Point3D other, double tolerance = 1e-10)
    {
        return Math.Abs(X - other.X) < tolerance &&
               Math.Abs(Y - other.Y) < tolerance &&
               Math.Abs(Z - other.Z) < tolerance;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj) => obj is Point3D other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(Point3D other) => IsEqualTo(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(X, Y, Z);

    /// <summary>
    /// Determines whether two points are equal.
    /// </summary>
    public static bool operator ==(Point3D left, Point3D right) => left.Equals(right);

    /// <summary>
    /// Determines whether two points are not equal.
    /// </summary>
    public static bool operator !=(Point3D left, Point3D right) => !left.Equals(right);

    /// <inheritdoc/>
    public override string ToString() => $"({X}, {Y}, {Z})";

    /// <summary>
    /// Determines if this point lies between two other points on a line segment.
    /// </summary>
    public bool IsBetween(Point3D p1, Point3D p2, double tolerance = Constants.Tolerance)
    {
        var v1 = this - p1;
        var v2 = p2 - p1;

        // Check if point is on the line (cross product should be zero)
        if (v1.Cross(v2).Length > tolerance)
            return false;

        // Check if point is within the segment bounds
        var dotProduct = v1.Dot(v2);
        var segmentLengthSquared = v2.LengthSquared;

        return dotProduct >= -tolerance && dotProduct <= segmentLengthSquared + tolerance;
    }

    /// <summary>
    /// Returns the point transformed by a transformation matrix.
    /// </summary>
    public Point3D Transform(Matrix3D matrix)
    {
        return matrix.Transform(this);
    }

    /// <summary>
    /// Gets the signed distance from this point to another point along a direction.
    /// </summary>
    public double GetSignedDistanceAlongDirection(Point3D to, Vector3D direction)
    {
        var delta = to - this;
        var unitDirection = direction.Normalized();
        return delta.Dot(unitDirection);
    }

    /// <summary>
    /// Finds the nearest point from a collection of points.
    /// </summary>
    public Point3D? GetNearestPoint(IEnumerable<Point3D> points)
    {
        Point3D? nearest = null;
        var minDistanceSquared = double.MaxValue;

        foreach (var p in points)
        {
            var distSquared = (p - this).LengthSquared;
            if (distSquared < minDistanceSquared)
            {
                minDistanceSquared = distSquared;
                nearest = p;
            }
        }

        return nearest;
    }

    /// <summary>
    /// Finds the furthest point from a collection of points.
    /// </summary>
    public Point3D? GetFurthestPoint(IEnumerable<Point3D> points)
    {
        Point3D? furthest = null;
        var maxDistanceSquared = 0.0;

        foreach (var p in points)
        {
            var distSquared = (p - this).LengthSquared;
            if (distSquared > maxDistanceSquared)
            {
                maxDistanceSquared = distSquared;
                furthest = p;
            }
        }

        return furthest;
    }

    /// <summary>
    /// Translates the point by a vector.
    /// </summary>
    public Point3D Translate(Vector3D vector)
    {
        return this + vector;
    }

    /// <summary>
    /// Translates the point by the specified amounts.
    /// </summary>
    public Point3D Translate(double dx, double dy, double dz)
    {
        return new Point3D(X + dx, Y + dy, Z + dz);
    }
    
    /// <summary>
    /// Creates a vector from this point to another point.
    /// </summary>
    public Vector3D VectorTo(Point3D other)
    {
        return other - this;
    }

    /// <summary>
    /// Returns a copy of this point.
    /// </summary>
    public Point3D Copy()
    {
        return new Point3D(X, Y, Z);
    }

    /// <summary>
    /// Converts this point to a vector from the origin.
    /// </summary>
    public Vector3D AsVector()
    {
        return new Vector3D(X, Y, Z);
    }

    /// <summary>
    /// Converts this point to a vector from the origin.
    /// Alias for AsVector().
    /// </summary>
    public Vector3D ToVector()
    {
        return new Vector3D(X, Y, Z);
    }
}