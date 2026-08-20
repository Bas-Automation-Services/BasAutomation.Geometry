namespace BasAutomation.Geometry;

/// <summary>
/// Represents a 4x4 transformation matrix for 3D transformations.
/// </summary>
public readonly struct Matrix3D : IEquatable<Matrix3D>, IGeometryComparable<Matrix3D>, IGeometryCopyable<Matrix3D>
{
    private readonly double[,] m;

    /// <summary>
    /// Initializes a new instance of the Matrix3D struct with the specified values.
    /// </summary>
    /// <param name="m00">Element at row 0, column 0.</param>
    /// <param name="m01">Element at row 0, column 1.</param>
    /// <param name="m02">Element at row 0, column 2.</param>
    /// <param name="m03">Element at row 0, column 3.</param>
    /// <param name="m10">Element at row 1, column 0.</param>
    /// <param name="m11">Element at row 1, column 1.</param>
    /// <param name="m12">Element at row 1, column 2.</param>
    /// <param name="m13">Element at row 1, column 3.</param>
    /// <param name="m20">Element at row 2, column 0.</param>
    /// <param name="m21">Element at row 2, column 1.</param>
    /// <param name="m22">Element at row 2, column 2.</param>
    /// <param name="m23">Element at row 2, column 3.</param>
    /// <param name="m30">Element at row 3, column 0.</param>
    /// <param name="m31">Element at row 3, column 1.</param>
    /// <param name="m32">Element at row 3, column 2.</param>
    /// <param name="m33">Element at row 3, column 3.</param>
    public Matrix3D(double m00, double m01, double m02, double m03,
        double m10, double m11, double m12, double m13,
        double m20, double m21, double m22, double m23,
        double m30, double m31, double m32, double m33)
    {
        m = new double[4, 4];
        m[0, 0] = m00; m[0, 1] = m01; m[0, 2] = m02; m[0, 3] = m03;
        m[1, 0] = m10; m[1, 1] = m11; m[1, 2] = m12; m[1, 3] = m13;
        m[2, 0] = m20; m[2, 1] = m21; m[2, 2] = m22; m[2, 3] = m23;
        m[3, 0] = m30; m[3, 1] = m31; m[3, 2] = m32; m[3, 3] = m33;
    }

    /// <summary>
    /// Gets the matrix element at the specified row and column.
    /// </summary>
    /// <param name="row">The row index (0-3).</param>
    /// <param name="col">The column index (0-3).</param>
    public double this[int row, int col] => m != null ? m[row, col] : (row == col ? 1.0 : 0.0);

    /// <summary>
    /// Returns a new matrix with the element at the specified position changed.
    /// </summary>
    /// <param name="row">The row index (0-3).</param>
    /// <param name="col">The column index (0-3).</param>
    /// <param name="value">The new value.</param>
    /// <returns>A new matrix with the modified element.</returns>
    public Matrix3D WithElement(int row, int col, double value)
    {
        var newM = new double[4, 4];
        for (var i = 0; i < 4; i++)
            for (var j = 0; j < 4; j++)
                newM[i, j] = this[i, j];
        newM[row, col] = value;

        return new Matrix3D(
            newM[0, 0], newM[0, 1], newM[0, 2], newM[0, 3],
            newM[1, 0], newM[1, 1], newM[1, 2], newM[1, 3],
            newM[2, 0], newM[2, 1], newM[2, 2], newM[2, 3],
            newM[3, 0], newM[3, 1], newM[3, 2], newM[3, 3]
        );
    }

    /// <summary>
    /// Gets the identity matrix.
    /// </summary>
    public static Matrix3D Identity => new(
        1, 0, 0, 0,
        0, 1, 0, 0,
        0, 0, 1, 0,
        0, 0, 0, 1
    );

    /// <summary>
    /// Creates a translation matrix.
    /// </summary>
    /// <param name="x">The X translation.</param>
    /// <param name="y">The Y translation.</param>
    /// <param name="z">The Z translation.</param>
    /// <returns>A translation matrix.</returns>
    public static Matrix3D Translation(double x, double y, double z) => new(
        1, 0, 0, x,
        0, 1, 0, y,
        0, 0, 1, z,
        0, 0, 0, 1
    );

    /// <summary>
    /// Creates a rotation matrix around the X axis.
    /// </summary>
    /// <param name="angle">The rotation angle in radians.</param>
    /// <returns>A rotation matrix.</returns>
    public static Matrix3D RotationX(double angle)
    {
        var cos = Math.Cos(angle);
        var sin = Math.Sin(angle);
        return new Matrix3D(
            1, 0, 0, 0,
            0, cos, -sin, 0,
            0, sin, cos, 0,
            0, 0, 0, 1
        );
    }

    /// <summary>
    /// Creates a rotation matrix around the Y axis.
    /// </summary>
    /// <param name="angle">The rotation angle in radians.</param>
    /// <returns>A rotation matrix.</returns>
    public static Matrix3D RotationY(double angle)
    {
        var cos = Math.Cos(angle);
        var sin = Math.Sin(angle);
        return new Matrix3D(
            cos, 0, sin, 0,
            0, 1, 0, 0,
            -sin, 0, cos, 0,
            0, 0, 0, 1
        );
    }

    /// <summary>
    /// Creates a rotation matrix around the Z axis.
    /// </summary>
    /// <param name="angle">The rotation angle in radians.</param>
    /// <returns>A rotation matrix.</returns>
    public static Matrix3D RotationZ(double angle)
    {
        var cos = Math.Cos(angle);
        var sin = Math.Sin(angle);
        return new Matrix3D(
            cos, -sin, 0, 0,
            sin, cos, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1
        );
    }

    /// <summary>
    /// Creates a scale matrix.
    /// </summary>
    /// <param name="sx">The X scale factor.</param>
    /// <param name="sy">The Y scale factor.</param>
    /// <param name="sz">The Z scale factor.</param>
    /// <returns>A scale matrix.</returns>
    public static Matrix3D Scale(double sx, double sy, double sz) => new(
        sx, 0, 0, 0,
        0, sy, 0, 0,
        0, 0, sz, 0,
        0, 0, 0, 1
    );

    /// <summary>
    /// Creates a uniform scale matrix.
    /// </summary>
    /// <param name="s">The uniform scale factor.</param>
    /// <returns>A uniform scale matrix.</returns>
    public static Matrix3D Scale(double s) => Scale(s, s, s);

    /// <summary>
    /// Multiplies two matrices.
    /// </summary>
    /// <param name="a">The first matrix.</param>
    /// <param name="b">The second matrix.</param>
    /// <returns>The product of the two matrices.</returns>
    public static Matrix3D operator *(Matrix3D a, Matrix3D b)
    {
        var result = new double[4, 4];
        for (var i = 0; i < 4; i++)
            for (var j = 0; j < 4; j++)
                for (var k = 0; k < 4; k++)
                    result[i, j] += a[i, k] * b[k, j];

        return new Matrix3D(
            result[0, 0], result[0, 1], result[0, 2], result[0, 3],
            result[1, 0], result[1, 1], result[1, 2], result[1, 3],
            result[2, 0], result[2, 1], result[2, 2], result[2, 3],
            result[3, 0], result[3, 1], result[3, 2], result[3, 3]
        );
    }

    /// <summary>
    /// Transforms a point by this matrix.
    /// </summary>
    /// <param name="p">The point to transform.</param>
    /// <returns>The transformed point.</returns>
    public Point3D Transform(Point3D p)
    {
        var x = this[0, 0] * p.X + this[0, 1] * p.Y + this[0, 2] * p.Z + this[0, 3];
        var y = this[1, 0] * p.X + this[1, 1] * p.Y + this[1, 2] * p.Z + this[1, 3];
        var z = this[2, 0] * p.X + this[2, 1] * p.Y + this[2, 2] * p.Z + this[2, 3];
        var w = this[3, 0] * p.X + this[3, 1] * p.Y + this[3, 2] * p.Z + this[3, 3];
        return new Point3D(x / w, y / w, z / w);
    }

    /// <summary>
    /// Transforms a vector by this matrix.
    /// </summary>
    /// <param name="v">The vector to transform.</param>
    /// <returns>The transformed vector.</returns>
    public Vector3D Transform(Vector3D v)
    {
        var x = this[0, 0] * v.X + this[0, 1] * v.Y + this[0, 2] * v.Z;
        var y = this[1, 0] * v.X + this[1, 1] * v.Y + this[1, 2] * v.Z;
        var z = this[2, 0] * v.X + this[2, 1] * v.Y + this[2, 2] * v.Z;
        return new Vector3D(x, y, z);
    }

    /// <summary>
    /// Determines whether this matrix is equal to another matrix within the specified tolerance.
    /// </summary>
    /// <param name="other">The other matrix to compare.</param>
    /// <param name="tolerance">The tolerance for floating-point comparison. Default is 1e-10.</param>
    /// <returns>true if the matrices are equal within tolerance; otherwise, false.</returns>
    public bool IsEqualTo(Matrix3D other, double tolerance = 1e-10)
    {
        for (var i = 0; i < 4; i++)
            for (var j = 0; j < 4; j++)
                if (Math.Abs(this[i, j] - other[i, j]) >= tolerance)
                    return false;
        return true;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj) => obj is Matrix3D other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(Matrix3D other) => IsEqualTo(other);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        for (var i = 0; i < 4; i++)
            for (var j = 0; j < 4; j++)
                hash.Add(this[i, j]);
        return hash.ToHashCode();
    }

    /// <summary>
    /// Determines whether two matrices are equal.
    /// </summary>
    public static bool operator ==(Matrix3D left, Matrix3D right) => left.Equals(right);

    /// <summary>
    /// Determines whether two matrices are not equal.
    /// </summary>
    public static bool operator !=(Matrix3D left, Matrix3D right) => !left.Equals(right);

    /// <summary>
    /// Copies a matrix.
    /// </summary>
    /// <returns>A copy of this matrix.</returns>
    public Matrix3D Copy()
    {
        return new Matrix3D(
            this[0, 0], this[0, 1], this[0, 2], this[0, 3],
            this[1, 0], this[1, 1], this[1, 2], this[1, 3],
            this[2, 0], this[2, 1], this[2, 2], this[2, 3],
            this[3, 0], this[3, 1], this[3, 2], this[3, 3]
        );
    }

    /// <summary>
    /// Gets the translation component as a Point3D.
    /// </summary>
    public Point3D GetTranslation() => new Point3D(this[0, 3], this[1, 3], this[2, 3]);

    /// <summary>
    /// Gets the X axis of the transformation.
    /// </summary>
    public Vector3D XAxis => new Vector3D(this[0, 0], this[1, 0], this[2, 0]);

    /// <summary>
    /// Gets the Y axis of the transformation.
    /// </summary>
    public Vector3D YAxis => new Vector3D(this[0, 1], this[1, 1], this[2, 1]);

    /// <summary>
    /// Gets the Z axis of the transformation.
    /// </summary>
    public Vector3D ZAxis => new Vector3D(this[0, 2], this[1, 2], this[2, 2]);

    /// <summary>
    /// Gets the determinant of the matrix.
    /// </summary>
    public double Determinant =>
        this[0, 0] * (this[1, 1] * (this[2, 2] * this[3, 3] - this[2, 3] * this[3, 2]) -
                      this[1, 2] * (this[2, 1] * this[3, 3] - this[2, 3] * this[3, 1]) +
                      this[1, 3] * (this[2, 1] * this[3, 2] - this[2, 2] * this[3, 1])) -
        this[0, 1] * (this[1, 0] * (this[2, 2] * this[3, 3] - this[2, 3] * this[3, 2]) -
                      this[1, 2] * (this[2, 0] * this[3, 3] - this[2, 3] * this[3, 0]) +
                      this[1, 3] * (this[2, 0] * this[3, 2] - this[2, 2] * this[3, 0])) +
        this[0, 2] * (this[1, 0] * (this[2, 1] * this[3, 3] - this[2, 3] * this[3, 1]) -
                      this[1, 1] * (this[2, 0] * this[3, 3] - this[2, 3] * this[3, 0]) +
                      this[1, 3] * (this[2, 0] * this[3, 1] - this[2, 1] * this[3, 0])) -
        this[0, 3] * (this[1, 0] * (this[2, 1] * this[3, 2] - this[2, 2] * this[3, 1]) -
                      this[1, 1] * (this[2, 0] * this[3, 2] - this[2, 2] * this[3, 0]) +
                      this[1, 2] * (this[2, 0] * this[3, 1] - this[2, 1] * this[3, 0]));

    /// <summary>
    /// Gets whether this is an identity matrix.
    /// </summary>
    public bool IsIdentity =>
        Math.Abs(this[0, 0] - 1) < Constants.Tolerance && Math.Abs(this[0, 1]) < Constants.Tolerance &&
        Math.Abs(this[0, 2]) < Constants.Tolerance && Math.Abs(this[0, 3]) < Constants.Tolerance &&
        Math.Abs(this[1, 0]) < Constants.Tolerance && Math.Abs(this[1, 1] - 1) < Constants.Tolerance &&
        Math.Abs(this[1, 2]) < Constants.Tolerance && Math.Abs(this[1, 3]) < Constants.Tolerance &&
        Math.Abs(this[2, 0]) < Constants.Tolerance && Math.Abs(this[2, 1]) < Constants.Tolerance &&
        Math.Abs(this[2, 2] - 1) < Constants.Tolerance && Math.Abs(this[2, 3]) < Constants.Tolerance &&
        Math.Abs(this[3, 0]) < Constants.Tolerance && Math.Abs(this[3, 1]) < Constants.Tolerance &&
        Math.Abs(this[3, 2]) < Constants.Tolerance && Math.Abs(this[3, 3] - 1) < Constants.Tolerance;

    /// <summary>
    /// Returns a matrix with modified translation.
    /// </summary>
    public Matrix3D SetTranslation(Point3D translation)
    {
        return new Matrix3D(
            this[0, 0], this[0, 1], this[0, 2], translation.X,
            this[1, 0], this[1, 1], this[1, 2], translation.Y,
            this[2, 0], this[2, 1], this[2, 2], translation.Z,
            this[3, 0], this[3, 1], this[3, 2], this[3, 3]
        );
    }

    /// <summary>
    /// Returns a matrix with modified translation.
    /// </summary>
    public Matrix3D SetTranslation(double x, double y, double z)
    {
        return SetTranslation(new Point3D(x, y, z));
    }

    /// <summary>
    /// Returns a matrix with the translation component scaled by the specified factor.
    /// </summary>
    /// <param name="scale">The scale factor to apply to the translation.</param>
    /// <returns>A new matrix with scaled translation.</returns>
    public Matrix3D ScaleTranslation(double scale)
    {
        var t = GetTranslation();
        return SetTranslation(new Point3D(t.X * scale, t.Y * scale, t.Z * scale));
    }

    /// <summary>
    /// Returns a matrix with the specified orientation axes.
    /// </summary>
    public Matrix3D SetAxes(Vector3D xAxis, Vector3D yAxis, Vector3D zAxis)
    {
        var translation = GetTranslation();
        return new Matrix3D(
            xAxis.X, yAxis.X, zAxis.X, translation.X,
            xAxis.Y, yAxis.Y, zAxis.Y, translation.Y,
            xAxis.Z, yAxis.Z, zAxis.Z, translation.Z,
            0, 0, 0, 1
        );
    }

    /// <summary>
    /// Returns the inverse of the matrix.
    /// </summary>
    public Matrix3D Inverse()
    {
        var det = Determinant;

        if (Math.Abs(det) < Constants.Tolerance)
            throw new InvalidOperationException("Matrix is singular and cannot be inverted.");

        // For efficiency with 4x4 matrices, we can use the adjugate method
        // This is the cofactor matrix transposed, divided by determinant

        return new Matrix3D(
            // Row 0
            (this[1, 1] * (this[2, 2] * this[3, 3] - this[2, 3] * this[3, 2]) -
             this[1, 2] * (this[2, 1] * this[3, 3] - this[2, 3] * this[3, 1]) +
             this[1, 3] * (this[2, 1] * this[3, 2] - this[2, 2] * this[3, 1])) / det,

            -(this[0, 1] * (this[2, 2] * this[3, 3] - this[2, 3] * this[3, 2]) -
              this[0, 2] * (this[2, 1] * this[3, 3] - this[2, 3] * this[3, 1]) +
              this[0, 3] * (this[2, 1] * this[3, 2] - this[2, 2] * this[3, 1])) / det,

            (this[0, 1] * (this[1, 2] * this[3, 3] - this[1, 3] * this[3, 2]) -
             this[0, 2] * (this[1, 1] * this[3, 3] - this[1, 3] * this[3, 1]) +
             this[0, 3] * (this[1, 1] * this[3, 2] - this[1, 2] * this[3, 1])) / det,

            -(this[0, 1] * (this[1, 2] * this[2, 3] - this[1, 3] * this[2, 2]) -
              this[0, 2] * (this[1, 1] * this[2, 3] - this[1, 3] * this[2, 1]) +
              this[0, 3] * (this[1, 1] * this[2, 2] - this[1, 2] * this[2, 1])) / det,

            // Row 1
            -(this[1, 0] * (this[2, 2] * this[3, 3] - this[2, 3] * this[3, 2]) -
              this[1, 2] * (this[2, 0] * this[3, 3] - this[2, 3] * this[3, 0]) +
              this[1, 3] * (this[2, 0] * this[3, 2] - this[2, 2] * this[3, 0])) / det,

            (this[0, 0] * (this[2, 2] * this[3, 3] - this[2, 3] * this[3, 2]) -
             this[0, 2] * (this[2, 0] * this[3, 3] - this[2, 3] * this[3, 0]) +
             this[0, 3] * (this[2, 0] * this[3, 2] - this[2, 2] * this[3, 0])) / det,

            -(this[0, 0] * (this[1, 2] * this[3, 3] - this[1, 3] * this[3, 2]) -
              this[0, 2] * (this[1, 0] * this[3, 3] - this[1, 3] * this[3, 0]) +
              this[0, 3] * (this[1, 0] * this[3, 2] - this[1, 2] * this[3, 0])) / det,

            (this[0, 0] * (this[1, 2] * this[2, 3] - this[1, 3] * this[2, 2]) -
             this[0, 2] * (this[1, 0] * this[2, 3] - this[1, 3] * this[2, 0]) +
             this[0, 3] * (this[1, 0] * this[2, 2] - this[1, 2] * this[2, 0])) / det,

            // Row 2
            (this[1, 0] * (this[2, 1] * this[3, 3] - this[2, 3] * this[3, 1]) -
             this[1, 1] * (this[2, 0] * this[3, 3] - this[2, 3] * this[3, 0]) +
             this[1, 3] * (this[2, 0] * this[3, 1] - this[2, 1] * this[3, 0])) / det,

            -(this[0, 0] * (this[2, 1] * this[3, 3] - this[2, 3] * this[3, 1]) -
              this[0, 1] * (this[2, 0] * this[3, 3] - this[2, 3] * this[3, 0]) +
              this[0, 3] * (this[2, 0] * this[3, 1] - this[2, 1] * this[3, 0])) / det,

            (this[0, 0] * (this[1, 1] * this[3, 3] - this[1, 3] * this[3, 1]) -
             this[0, 1] * (this[1, 0] * this[3, 3] - this[1, 3] * this[3, 0]) +
             this[0, 3] * (this[1, 0] * this[3, 1] - this[1, 1] * this[3, 0])) / det,

            -(this[0, 0] * (this[1, 1] * this[2, 3] - this[1, 3] * this[2, 1]) -
              this[0, 1] * (this[1, 0] * this[2, 3] - this[1, 3] * this[2, 0]) +
              this[0, 3] * (this[1, 0] * this[2, 1] - this[1, 1] * this[2, 0])) / det,

            // Row 3
            -(this[1, 0] * (this[2, 1] * this[3, 2] - this[2, 2] * this[3, 1]) -
              this[1, 1] * (this[2, 0] * this[3, 2] - this[2, 2] * this[3, 0]) +
              this[1, 2] * (this[2, 0] * this[3, 1] - this[2, 1] * this[3, 0])) / det,

            (this[0, 0] * (this[2, 1] * this[3, 2] - this[2, 2] * this[3, 1]) -
             this[0, 1] * (this[2, 0] * this[3, 2] - this[2, 2] * this[3, 0]) +
             this[0, 2] * (this[2, 0] * this[3, 1] - this[2, 1] * this[3, 0])) / det,

            -(this[0, 0] * (this[1, 1] * this[3, 2] - this[1, 2] * this[3, 1]) -
              this[0, 1] * (this[1, 0] * this[3, 2] - this[1, 2] * this[3, 0]) +
              this[0, 2] * (this[1, 0] * this[3, 1] - this[1, 1] * this[3, 0])) / det,

            (this[0, 0] * (this[1, 1] * this[2, 2] - this[1, 2] * this[2, 1]) -
             this[0, 1] * (this[1, 0] * this[2, 2] - this[1, 2] * this[2, 0]) +
             this[0, 2] * (this[1, 0] * this[2, 1] - this[1, 1] * this[2, 0])) / det
        );
    }

    /// <summary>
    /// Creates a rotation matrix around an arbitrary axis.
    /// </summary>
    public static Matrix3D CreateRotationAroundAxis(Vector3D axis, double angleRadians)
    {
        var k = axis.Normalized();
        var cos = Math.Cos(angleRadians);
        var sin = Math.Sin(angleRadians);
        var oneMinusCos = 1 - cos;

        return new Matrix3D(
            cos + k.X * k.X * oneMinusCos,
            k.X * k.Y * oneMinusCos - k.Z * sin,
            k.X * k.Z * oneMinusCos + k.Y * sin,
            0,

            k.Y * k.X * oneMinusCos + k.Z * sin,
            cos + k.Y * k.Y * oneMinusCos,
            k.Y * k.Z * oneMinusCos - k.X * sin,
            0,

            k.Z * k.X * oneMinusCos - k.Y * sin,
            k.Z * k.Y * oneMinusCos + k.X * sin,
            cos + k.Z * k.Z * oneMinusCos,
            0,

            0, 0, 0, 1
        );
    }

    /// <summary>
    /// Creates a rotation matrix that rotates around an axis through a point.
    /// </summary>
    public static Matrix3D CreateRotationAroundPoint(Vector3D axis, double angleRadians, Point3D point)
    {
        var toOrigin = Translation(-point.X, -point.Y, -point.Z);
        var rotation = CreateRotationAroundAxis(axis, angleRadians);
        var fromOrigin = Translation(point.X, point.Y, point.Z);

        return fromOrigin * rotation * toOrigin;
    }

    /// <summary>
    /// Creates a matrix that rotates one vector to align with another.
    /// </summary>
    public static Matrix3D CreateRotationFromTo(Vector3D from, Vector3D to)
    {
        var fromNorm = from.Normalized();
        var toNorm = to.Normalized();

        var dot = fromNorm.Dot(toNorm);

        // Check if vectors are already aligned
        if (Math.Abs(dot - 1.0) < Constants.Tolerance)
            return Identity;

        // Check if vectors are opposite
        if (Math.Abs(dot + 1.0) < Constants.Tolerance)
        {
            // Find a perpendicular vector
            var perpendicular = Math.Abs(fromNorm.X) < 0.9
                ? fromNorm.Cross(Vector3D.XAxis)
                : fromNorm.Cross(Vector3D.YAxis);
            return CreateRotationAroundAxis(perpendicular.Normalized(), Math.PI);
        }

        // General case
        var axis = fromNorm.Cross(toNorm);
        var angle = Math.Acos(Math.Clamp(dot, -1.0, 1.0));
        return CreateRotationAroundAxis(axis.Normalized(), angle);
    }

    /// <summary>
    /// Creates a reflection matrix across a plane.
    /// </summary>
    public static Matrix3D CreateReflection(Plane3D plane)
    {
        var n = plane.Normal;
        var d = plane.Normal.Dot(new Vector3D(plane.Origin.X, plane.Origin.Y, plane.Origin.Z));

        return new Matrix3D(
            1 - 2 * n.X * n.X, -2 * n.X * n.Y, -2 * n.X * n.Z, 2 * n.X * d,
            -2 * n.Y * n.X, 1 - 2 * n.Y * n.Y, -2 * n.Y * n.Z, 2 * n.Y * d,
            -2 * n.Z * n.X, -2 * n.Z * n.Y, 1 - 2 * n.Z * n.Z, 2 * n.Z * d,
            0, 0, 0, 1
        );
    }

    /// <summary>
    /// Returns an orthonormalized version of the rotation part of the matrix.
    /// </summary>
    public Matrix3D OrthonormalizeRotation()
    {
        var xAxis = XAxis;
        var yAxis = YAxis;
        var zAxis = ZAxis;

        // Gram-Schmidt orthogonalization
        xAxis = xAxis.Normalized();
        yAxis = (yAxis - yAxis.Dot(xAxis) * xAxis).Normalized();
        zAxis = xAxis.Cross(yAxis).Normalized();

        return SetAxes(xAxis, yAxis, zAxis);
    }

    /// <summary>
    /// Checks if the matrix represents a valid rotation (orthonormal).
    /// </summary>
    public bool IsRotationMatrix(double tolerance = Constants.Tolerance)
    {
        var x = XAxis;
        var y = YAxis;
        var z = ZAxis;

        // Check if axes are unit length
        if (Math.Abs(x.Length - 1.0) > tolerance ||
            Math.Abs(y.Length - 1.0) > tolerance ||
            Math.Abs(z.Length - 1.0) > tolerance)
            return false;

        // Check if axes are orthogonal
        if (Math.Abs(x.Dot(y)) > tolerance ||
            Math.Abs(y.Dot(z)) > tolerance ||
            Math.Abs(z.Dot(x)) > tolerance)
            return false;

        // Check if determinant is 1 (proper rotation, not reflection)
        // For rotation part only
        var rotDet = x.X * (y.Y * z.Z - y.Z * z.Y) -
                     x.Y * (y.X * z.Z - y.Z * z.X) +
                     x.Z * (y.X * z.Y - y.Y * z.X);

        return Math.Abs(rotDet - 1.0) < tolerance;
    }

    /// <summary>
    /// Decomposes the matrix into translation, rotation, and scale components.
    /// Note: This assumes the matrix represents translation * rotation * scale.
    /// </summary>
    public void Decompose(out Point3D translation, out Vector3D scale, out Matrix3D rotation)
    {
        translation = GetTranslation();

        // Calculate scale from axis lengths
        var scaleX = XAxis.Length;
        var scaleY = YAxis.Length;
        var scaleZ = ZAxis.Length;
        scale = new Vector3D(scaleX, scaleY, scaleZ);

        // Extract rotation by normalizing axes
        if (scaleX > Constants.Tolerance && scaleY > Constants.Tolerance && scaleZ > Constants.Tolerance)
        {
            rotation = new Matrix3D(
                XAxis.X / scaleX, YAxis.X / scaleY, ZAxis.X / scaleZ, 0,
                XAxis.Y / scaleX, YAxis.Y / scaleY, ZAxis.Y / scaleZ, 0,
                XAxis.Z / scaleX, YAxis.Z / scaleY, ZAxis.Z / scaleZ, 0,
                0, 0, 0, 1
            );
        }
        else
        {
            rotation = Identity;
        }
    }

    /// <summary>
    /// Returns a string representation of the matrix.
    /// </summary>
    public override string ToString()
    {
        return $"[{this[0, 0]:F3}, {this[0, 1]:F3}, {this[0, 2]:F3}, {this[0, 3]:F3}]\n" +
               $"[{this[1, 0]:F3}, {this[1, 1]:F3}, {this[1, 2]:F3}, {this[1, 3]:F3}]\n" +
               $"[{this[2, 0]:F3}, {this[2, 1]:F3}, {this[2, 2]:F3}, {this[2, 3]:F3}]\n" +
               $"[{this[3, 0]:F3}, {this[3, 1]:F3}, {this[3, 2]:F3}, {this[3, 3]:F3}]";
    }
}