namespace BasAutomation.Geometry;


/// <summary>
/// Represents a vector in 3D space.
/// </summary>
/// <param name="x">The X component.</param>
/// <param name="y">The Y component.</param>
/// <param name="z">The Z component.</param>
public readonly struct Vector3D(double x, double y, double z) : IEquatable<Vector3D>, IGeometryComparable<Vector3D>
{
    /// <summary>
    /// Gets or sets the X component.
    /// </summary>
    public double X { get; } = x;

    /// <summary>
    /// Gets or sets the Y component.
    /// </summary>
    public double Y { get; } = y;

    /// <summary>
    /// Gets or sets the Z component.
    /// </summary>
    public double Z { get; } = z;

    /// <summary>
    /// Gets the length (magnitude) of the vector.
    /// </summary>
    public double Length => Math.Sqrt(X * X + Y * Y + Z * Z);

    /// <summary>
    /// Gets the squared length of the vector (avoids square root calculation).
    /// </summary>
    public double LengthSquared => X * X + Y * Y + Z * Z;

    public static Vector3D ZAxis => new Vector3D(0, 0, 1);
    public static Vector3D YAxis => new Vector3D(0, 1, 0);
    public static Vector3D XAxis => new Vector3D(1, 0, 0);
    public static Vector3D Zero => new Vector3D(0, 0, 0);

    /// <summary>
    /// Returns a normalized (unit) vector in the same direction.
    /// </summary>
    /// <returns>A normalized vector, or zero vector if the original length is zero.</returns>
    public Vector3D Normalized()
    {
        var len = Length;
        return len > 0 ? new Vector3D(X / len, Y / len, Z / len) : new Vector3D(0, 0, 0);
    }

    /// <summary>
    /// Adds two vectors.
    /// </summary>
    /// <param name="a">The first vector.</param>
    /// <param name="b">The second vector.</param>
    /// <returns>The sum of the vectors.</returns>
    public static Vector3D operator +(Vector3D a, Vector3D b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

    /// <summary>
    /// Subtracts one vector from another.
    /// </summary>
    /// <param name="a">The first vector.</param>
    /// <param name="b">The second vector.</param>
    /// <returns>The difference of the vectors.</returns>
    public static Vector3D operator -(Vector3D a, Vector3D b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

    /// <summary>
    /// Multiplies a vector by a scalar.
    /// </summary>
    /// <param name="v">The vector.</param>
    /// <param name="s">The scalar.</param>
    /// <returns>The scaled vector.</returns>
    public static Vector3D operator *(Vector3D v, double s) => new(v.X * s, v.Y * s, v.Z * s);

    /// <summary>
    /// Multiplies a scalar by a vector.
    /// </summary>
    /// <param name="s">The scalar.</param>
    /// <param name="v">The vector.</param>
    /// <returns>The scaled vector.</returns>
    public static Vector3D operator *(double s, Vector3D v) => v * s;

    /// <summary>
    /// Divides a vector by a scalar.
    /// </summary>
    /// <param name="v">The vector.</param>
    /// <param name="s">The scalar.</param>
    /// <returns>The scaled vector.</returns>
    public static Vector3D operator /(Vector3D v, double s) => new(v.X / s, v.Y / s, v.Z / s);

    /// <summary>
    /// Negates a vector.
    /// </summary>
    /// <param name="v">The vector to negate.</param>
    /// <returns>The negated vector.</returns>
    public static Vector3D operator -(Vector3D v) => new(-v.X, -v.Y, -v.Z);

    /// <summary>
    /// Calculates the dot product of two vectors.
    /// </summary>
    /// <param name="other">The other vector.</param>
    /// <returns>The dot product.</returns>
    public double Dot(Vector3D other) => X * other.X + Y * other.Y + Z * other.Z;

    /// <summary>
    /// Calculates the cross product of two vectors.
    /// </summary>
    /// <param name="other">The other vector.</param>
    /// <returns>The cross product vector.</returns>
    public Vector3D Cross(Vector3D other) => new(
        Y * other.Z - Z * other.Y,
        Z * other.X - X * other.Z,
        X * other.Y - Y * other.X
    );

    /// <summary>
    /// Calculates the angle between two vectors in radians.
    /// </summary>
    /// <param name="other">The other vector.</param>
    /// <returns>The angle in radians.</returns>
    public double AngleTo(Vector3D other) => Math.Acos(Dot(other) / (Length * other.Length));

    /// <summary>
    /// Determines whether this vector is equal to another vector within the specified tolerance.
    /// </summary>
    /// <param name="other">The other vector to compare.</param>
    /// <param name="tolerance">The tolerance for floating-point comparison. Default is 1e-10.</param>
    /// <returns>true if the vectors are equal within tolerance; otherwise, false.</returns>
    public bool IsEqualTo(Vector3D other, double tolerance = 1e-10)
    {
        return Math.Abs(X - other.X) < tolerance &&
               Math.Abs(Y - other.Y) < tolerance &&
               Math.Abs(Z - other.Z) < tolerance;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj) => obj is Vector3D other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(Vector3D other) => IsEqualTo(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(X, Y, Z);

    /// <summary>
    /// Determines whether two vectors are equal.
    /// </summary>
    public static bool operator ==(Vector3D left, Vector3D right) => left.Equals(right);

    /// <summary>
    /// Determines whether two vectors are not equal.
    /// </summary>
    public static bool operator !=(Vector3D left, Vector3D right) => !left.Equals(right);

    /// <inheritdoc/>
    public override string ToString() => $"[{X}, {Y}, {Z}]";

    /// <summary>
    /// Returns true only when the two vectors point in the same direction (normalized dot product ≈ 1).
    /// For the parallel-or-anti-parallel test, use <see cref="IsParallel"/>; for the strict opposite test, use <see cref="IsOppositeDirection"/>.
    /// Uses the dot product instead of <c>AngleTo</c> to avoid NaN from <c>Math.Acos</c> floating-point drift.
    /// Returns false if either vector is zero-length.
    /// </summary>
    public bool IsSameDirection(Vector3D other)
    {
        var lenA = Length;
        var lenB = other.Length;
        if (lenA < Constants.Tolerance || lenB < Constants.Tolerance)
            return false;
        var dot = Math.Clamp(Dot(other) / (lenA * lenB), -1.0, 1.0);
        return Math.Abs(dot - 1.0) < Constants.Tolerance;
    }

    /// <summary>
    /// Determines if two vectors are parallel within tolerance.
    /// </summary>
    public bool IsParallel(Vector3D other, double tolerance = Constants.Tolerance)
    {
        // Two vectors are parallel if their cross product magnitude is zero
        return Cross(other).Length < tolerance;
    }

    /// <summary>
    /// Determines if two vectors are perpendicular within tolerance.
    /// </summary>
    public bool IsPerpendicular(Vector3D other, double tolerance = Constants.Tolerance)
    {
        // Two vectors are perpendicular if their dot product is zero
        return Math.Abs(Dot(other)) < tolerance;
    }

    /// <summary>
    /// Determines if two vectors point in opposite directions.
    /// </summary>
    public bool IsOppositeDirection(Vector3D other, double tolerance = Constants.Tolerance)
    {
        var n1 = Normalized();
        var n2 = other.Normalized();
        return Math.Abs(n1.Dot(n2) + 1.0) < tolerance;
    }

    /// <summary>
    /// Returns a vector rotated around an axis by the specified angle.
    /// </summary>
    /// <param name="axis">The rotation axis (will be normalized).</param>
    /// <param name="angleRadians">The rotation angle in radians.</param>
    /// <returns>The rotated vector.</returns>
    public Vector3D RotateAroundAxis(Vector3D axis, double angleRadians)
    {
        var k = axis.Normalized();
        var cosTheta = Math.Cos(angleRadians);
        var sinTheta = Math.Sin(angleRadians);

        // Rodrigues' rotation formula
        var dotProduct = Dot(k);
        var crossProduct = k.Cross(this);

        return this * cosTheta + crossProduct * sinTheta + k * dotProduct * (1 - cosTheta);
    }

    /// <summary>
    /// Projects a point onto a line defined by this vector direction and a point on the line.
    /// </summary>
    /// <param name="linePoint">A point on the line.</param>
    /// <param name="pointToProject">The point to project.</param>
    /// <returns>The projected point on the line.</returns>
    public Point3D ProjectPointOntoLine(Point3D linePoint, Point3D pointToProject)
    {
        var dir = Normalized();
        var toPoint = pointToProject - linePoint;
        var projection = toPoint.Dot(dir) * dir;
        return linePoint + projection;
    }

    /// <summary>
    /// Returns the vector transformed by a matrix (rotation only, no translation).
    /// </summary>
    public Vector3D GetTransformed(Matrix3D matrix)
    {
        return matrix.Transform(this);
    }

    /// <summary>
    /// Returns the angle between this vector and another in radians (0 to π).
    /// </summary>
    public double AngleBetween(Vector3D other)
    {
        return AngleTo(other);
    }

    /// <summary>
    /// Returns a negated vector.
    /// </summary>
    public Vector3D Negate()
    {
        return new Vector3D(-X, -Y, -Z);
    }

    /// <summary>
    /// Returns a vector with each component raised to the specified power.
    /// </summary>
    public Vector3D ElementWiseExponentiation(double exponent)
    {
        return new Vector3D(
            Math.Pow(X, exponent),
            Math.Pow(Y, exponent),
            Math.Pow(Z, exponent)
        );
    }

    /// <summary>
    /// Calculates the bisector vector at a point between this vector and another.
    /// </summary>
    public Vector3D GetBisectorAtPoint(Point3D point, Vector3D other)
    {
        var p1 = point + Normalized();
        var p2 = point + other.Normalized();
        var midpoint = new Point3D(
            (p1.X + p2.X) / 2,
            (p1.Y + p2.Y) / 2,
            (p1.Z + p2.Z) / 2
        );
        return (midpoint - point).Normalized();
    }
}