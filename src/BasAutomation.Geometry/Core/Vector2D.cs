namespace BasAutomation.Geometry;

/// <summary>
/// Represents a vector in 2D space.
/// </summary>
/// <param name="x">The X component.</param>
/// <param name="y">The Y component.</param>
public readonly struct Vector2D(double x, double y) : IEquatable<Vector2D>, IGeometryComparable<Vector2D>
{
    public static Vector2D Zero => new Vector2D(0, 0);
    public static Vector2D XAxis => new Vector2D(1, 0);
    public static Vector2D YAxis => new Vector2D(0, 1);

    /// <summary>
    /// Gets or sets the X component.
    /// </summary>
    public double X { get; } = x;

    /// <summary>
    /// Gets or sets the Y component.
    /// </summary>
    public double Y { get; } = y;

    /// <summary>
    /// Gets the length (magnitude) of the vector.
    /// </summary>
    public double Length => Math.Sqrt(X * X + Y * Y);

    /// <summary>
    /// Gets the squared length of the vector (avoids square root calculation).
    /// </summary>
    public double LengthSquared => X * X + Y * Y;

    /// <summary>
    /// Returns a normalized (unit) vector in the same direction.
    /// </summary>
    /// <returns>A normalized vector, or zero vector if the original length is zero.</returns>
    public Vector2D Normalized()
    {
        var len = Length;
        return len > 0 ? new Vector2D(X / len, Y / len) : new Vector2D(0, 0);
    }

    /// <summary>
    /// Adds two vectors.
    /// </summary>
    /// <param name="a">The first vector.</param>
    /// <param name="b">The second vector.</param>
    /// <returns>The sum of the vectors.</returns>
    public static Vector2D operator +(Vector2D a, Vector2D b) => new(a.X + b.X, a.Y + b.Y);

    /// <summary>
    /// Subtracts one vector from another.
    /// </summary>
    /// <param name="a">The first vector.</param>
    /// <param name="b">The second vector.</param>
    /// <returns>The difference of the vectors.</returns>
    public static Vector2D operator -(Vector2D a, Vector2D b) => new(a.X - b.X, a.Y - b.Y);

    /// <summary>
    /// Multiplies a vector by a scalar.
    /// </summary>
    /// <param name="v">The vector.</param>
    /// <param name="s">The scalar.</param>
    /// <returns>The scaled vector.</returns>
    public static Vector2D operator *(Vector2D v, double s) => new(v.X * s, v.Y * s);

    /// <summary>
    /// Multiplies a scalar by a vector.
    /// </summary>
    /// <param name="s">The scalar.</param>
    /// <param name="v">The vector.</param>
    /// <returns>The scaled vector.</returns>
    public static Vector2D operator *(double s, Vector2D v) => v * s;

    /// <summary>
    /// Divides a vector by a scalar.
    /// </summary>
    /// <param name="v">The vector.</param>
    /// <param name="s">The scalar.</param>
    /// <returns>The scaled vector.</returns>
    public static Vector2D operator /(Vector2D v, double s) => new(v.X / s, v.Y / s);

    /// <summary>
    /// Negates a vector.
    /// </summary>
    /// <param name="v">The vector to negate.</param>
    /// <returns>The negated vector.</returns>
    public static Vector2D operator -(Vector2D v) => new(-v.X, -v.Y);

    /// <summary>
    /// Calculates the dot product of two vectors.
    /// </summary>
    /// <param name="other">The other vector.</param>
    /// <returns>The dot product.</returns>
    public double Dot(Vector2D other) => X * other.X + Y * other.Y;

    /// <summary>
    /// Calculates the 2D cross product (scalar) of two vectors.
    /// </summary>
    /// <param name="other">The other vector.</param>
    /// <returns>The cross product scalar.</returns>
    public double Cross(Vector2D other) => X * other.Y - Y * other.X;

    /// <summary>
    /// Calculates the angle between two vectors in radians.
    /// </summary>
    /// <param name="other">The other vector.</param>
    /// <returns>The angle in radians.</returns>
    public double AngleTo(Vector2D other) => Math.Acos(Dot(other) / (Length * other.Length));

    /// <summary>
    /// Determines whether this vector is equal to another vector within the specified tolerance.
    /// </summary>
    /// <param name="other">The other vector to compare.</param>
    /// <param name="tolerance">The tolerance for floating-point comparison. Default is 1e-10.</param>
    /// <returns>true if the vectors are equal within tolerance; otherwise, false.</returns>
    public bool IsEqualTo(Vector2D other, double tolerance = 1e-10)
    {
        return Math.Abs(X - other.X) < tolerance && Math.Abs(Y - other.Y) < tolerance;
    }
    
    /// <inheritdoc/>
    public override bool Equals(object obj) => obj is Vector2D other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(Vector2D other) => IsEqualTo(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(X, Y);

    /// <summary>
    /// Determines whether two vectors are equal.
    /// </summary>
    public static bool operator ==(Vector2D left, Vector2D right) => left.Equals(right);

    /// <summary>
    /// Determines whether two vectors are not equal.
    /// </summary>
    public static bool operator !=(Vector2D left, Vector2D right) => !left.Equals(right);

    /// <inheritdoc/>
    public override string ToString() => $"[{X}, {Y}]";

    /// <summary>
    /// Determines if two vectors are parallel within tolerance.
    /// </summary>
    public bool IsParallel(Vector2D other, double tolerance = Constants.Tolerance)
    {
        // Two vectors are parallel if their cross product is zero
        return Math.Abs(Cross(other)) < tolerance;
    }

    /// <summary>
    /// Determines if two vectors are perpendicular within tolerance.
    /// </summary>
    public bool IsPerpendicular(Vector2D other, double tolerance = Constants.Tolerance)
    {
        // Two vectors are perpendicular if their dot product is zero
        return Math.Abs(Dot(other)) < tolerance;
    }

    /// <summary>
    /// Determines if two vectors point in the same direction.
    /// </summary>
    public bool IsSameDirection(Vector2D other, double tolerance = Constants.Tolerance)
    {
        var n1 = Normalized();
        var n2 = other.Normalized();
        return Math.Abs(n1.Dot(n2) - 1.0) < tolerance;
    }

    /// <summary>
    /// Determines if two vectors point in opposite directions.
    /// </summary>
    public bool IsOppositeDirection(Vector2D other, double tolerance = Constants.Tolerance)
    {
        var n1 = Normalized();
        var n2 = other.Normalized();
        return Math.Abs(n1.Dot(n2) + 1.0) < tolerance;
    }

    /// <summary>
    /// Gets a vector perpendicular to this vector (rotated 90 degrees counter-clockwise).
    /// </summary>
    public Vector2D GetPerpendicular()
    {
        return new Vector2D(-Y, X);
    }

    /// <summary>
    /// Returns a rotated vector by the specified angle in radians.
    /// </summary>
    public Vector2D Rotate(double angleRadians)
    {
        var cos = Math.Cos(angleRadians);
        var sin = Math.Sin(angleRadians);
        return new Vector2D(
            X * cos - Y * sin,
            X * sin + Y * cos
        );
    }

    /// <summary>
    /// Transforms this 2D vector to 3D space using a coordinate system.
    /// </summary>
    public Vector3D TransformTo3DSpace(Point3D origin, Vector3D xAxis, Vector3D yAxis)
    {
        var x = xAxis.Normalized();
        var y = yAxis.Normalized();
        return X * x + Y * y;
    }
}
