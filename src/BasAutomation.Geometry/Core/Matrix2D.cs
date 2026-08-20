namespace BasAutomation.Geometry;

/// <summary>
/// Represents a 3x3 transformation matrix for 2D transformations.
/// </summary>
public readonly struct Matrix2D : IEquatable<Matrix2D>, IGeometryComparable<Matrix2D>, IGeometryCopyable<Matrix2D>
{
    private readonly double[,] m;

    /// <summary>
    /// Initializes a new instance of the Matrix2D struct with the specified values.
    /// </summary>
    /// <param name="m00">Element at row 0, column 0.</param>
    /// <param name="m01">Element at row 0, column 1.</param>
    /// <param name="m02">Element at row 0, column 2.</param>
    /// <param name="m10">Element at row 1, column 0.</param>
    /// <param name="m11">Element at row 1, column 1.</param>
    /// <param name="m12">Element at row 1, column 2.</param>
    /// <param name="m20">Element at row 2, column 0.</param>
    /// <param name="m21">Element at row 2, column 1.</param>
    /// <param name="m22">Element at row 2, column 2.</param>
    public Matrix2D(double m00, double m01, double m02,
        double m10, double m11, double m12,
        double m20, double m21, double m22)
    {
        m = new double[3, 3];
        m[0, 0] = m00; m[0, 1] = m01; m[0, 2] = m02;
        m[1, 0] = m10; m[1, 1] = m11; m[1, 2] = m12;
        m[2, 0] = m20; m[2, 1] = m21; m[2, 2] = m22;
    }

    /// <summary>
    /// Gets or sets the matrix element at the specified row and column.
    /// </summary>
    /// <param name="row">The row index (0-2).</param>
    /// <param name="col">The column index (0-2).</param>
    public double this[int row, int col] => m[row, col];

    /// <summary>
    /// Gets the identity matrix.
    /// </summary>
    public static Matrix2D Identity => new(
        1, 0, 0,
        0, 1, 0,
        0, 0, 1
    );

    /// <summary>
    /// Creates a translation matrix.
    /// </summary>
    /// <param name="x">The X translation.</param>
    /// <param name="y">The Y translation.</param>
    /// <returns>A translation matrix.</returns>
    public static Matrix2D Translation(double x, double y) => new(
        1, 0, x,
        0, 1, y,
        0, 0, 1
    );

    /// <summary>
    /// Creates a rotation matrix.
    /// </summary>
    /// <param name="angle">The rotation angle in radians.</param>
    /// <returns>A rotation matrix.</returns>
    public static Matrix2D Rotation(double angle)
    {
        var cos = Math.Cos(angle);
        var sin = Math.Sin(angle);
        return new Matrix2D(
            cos, -sin, 0,
            sin, cos, 0,
            0, 0, 1
        );
    }

    /// <summary>
    /// Creates a scale matrix.
    /// </summary>
    /// <param name="sx">The X scale factor.</param>
    /// <param name="sy">The Y scale factor.</param>
    /// <returns>A scale matrix.</returns>
    public static Matrix2D Scale(double sx, double sy) => new(
        sx, 0, 0,
        0, sy, 0,
        0, 0, 1
    );

    /// <summary>
    /// Multiplies two matrices.
    /// </summary>
    /// <param name="a">The first matrix.</param>
    /// <param name="b">The second matrix.</param>
    /// <returns>The product of the two matrices.</returns>
    public static Matrix2D operator *(Matrix2D a, Matrix2D b)
    {
        var result = new double[3, 3];
        for (var i = 0; i < 3; i++)
            for (var j = 0; j < 3; j++)
                for (var k = 0; k < 3; k++)
                    result[i, j] += a[i, k] * b[k, j];

        return new Matrix2D(
            result[0, 0], result[0, 1], result[0, 2],
            result[1, 0], result[1, 1], result[1, 2],
            result[2, 0], result[2, 1], result[2, 2]
        );
    }

    /// <summary>
    /// Returns a new matrix with the element at the specified position changed.
    /// </summary>
    public Matrix2D WithElement(int row, int col, double value)
    {
        var result = Copy();
        // This creates a new instance, so we can modify the new array
        var newM = new double[3, 3];
        Array.Copy(m, newM, 9);
        newM[row, col] = value;

        return new Matrix2D(
            newM[0, 0], newM[0, 1], newM[0, 2],
            newM[1, 0], newM[1, 1], newM[1, 2],
            newM[2, 0], newM[2, 1], newM[2, 2]
        );
    }

    /// <summary>
    /// Transforms a point by this matrix.
    /// </summary>
    /// <param name="p">The point to transform.</param>
    /// <returns>The transformed point.</returns>
    public Point2D Transform(Point2D p)
    {
        var x = m[0, 0] * p.X + m[0, 1] * p.Y + m[0, 2];
        var y = m[1, 0] * p.X + m[1, 1] * p.Y + m[1, 2];
        var w = m[2, 0] * p.X + m[2, 1] * p.Y + m[2, 2];
        return new Point2D(x / w, y / w);
    }

    /// <summary>
    /// Transforms a vector by this matrix.
    /// </summary>
    /// <param name="v">The vector to transform.</param>
    /// <returns>The transformed vector.</returns>
    public Vector2D Transform(Vector2D v)
    {
        var x = m[0, 0] * v.X + m[0, 1] * v.Y;
        var y = m[1, 0] * v.X + m[1, 1] * v.Y;
        return new Vector2D(x, y);
    }

    /// <summary>
    /// Determines whether this matrix is equal to another matrix within the specified tolerance.
    /// </summary>
    /// <param name="other">The other matrix to compare.</param>
    /// <param name="tolerance">The tolerance for floating-point comparison. Default is 1e-10.</param>
    /// <returns>true if the matrices are equal within tolerance; otherwise, false.</returns>
    public bool IsEqualTo(Matrix2D other, double tolerance = 1e-10)
    {
        for (var i = 0; i < 3; i++)
            for (var j = 0; j < 3; j++)
                if (Math.Abs(m[i, j] - other[i, j]) >= tolerance)
                    return false;
        return true;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj) => obj is Matrix2D other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(Matrix2D other) => IsEqualTo(other);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        for (var i = 0; i < 3; i++)
            for (var j = 0; j < 3; j++)
                hash.Add(m[i, j]);
        return hash.ToHashCode();
    }

    /// <summary>
    /// Determines whether two matrices are equal.
    /// </summary>
    public static bool operator ==(Matrix2D left, Matrix2D right) => left.Equals(right);

    /// <summary>
    /// Determines whether two matrices are not equal.
    /// </summary>
    public static bool operator !=(Matrix2D left, Matrix2D right) => !left.Equals(right);

    /// <summary>
    /// Gets the translation component as a Point2D.
    /// </summary>
    public Point2D GetTranslation() => new Point2D(this[0, 2], this[1, 2]);

    /// <summary>
    /// Gets the X axis of the transformation.
    /// </summary>
    public Vector2D XAxis => new Vector2D(this[0, 0], this[1, 0]);

    /// <summary>
    /// Gets the Y axis of the transformation.
    /// </summary>
    public Vector2D YAxis => new Vector2D(this[0, 1], this[1, 1]);

    /// <summary>
    /// Gets the determinant of the matrix.
    /// </summary>
    public double Determinant =>
        this[0, 0] * (this[1, 1] * this[2, 2] - this[1, 2] * this[2, 1]) -
        this[0, 1] * (this[1, 0] * this[2, 2] - this[1, 2] * this[2, 0]) +
        this[0, 2] * (this[1, 0] * this[2, 1] - this[1, 1] * this[2, 0]);

    /// <summary>
    /// Gets whether this is an identity matrix.
    /// </summary>
    public bool IsIdentity =>
        Math.Abs(this[0, 0] - 1) < Constants.Tolerance &&
        Math.Abs(this[0, 1]) < Constants.Tolerance &&
        Math.Abs(this[0, 2]) < Constants.Tolerance &&
        Math.Abs(this[1, 0]) < Constants.Tolerance &&
        Math.Abs(this[1, 1] - 1) < Constants.Tolerance &&
        Math.Abs(this[1, 2]) < Constants.Tolerance &&
        Math.Abs(this[2, 0]) < Constants.Tolerance &&
        Math.Abs(this[2, 1]) < Constants.Tolerance &&
        Math.Abs(this[2, 2] - 1) < Constants.Tolerance;

    /// <summary>
    /// Returns a matrix with modified translation.
    /// </summary>
    public Matrix2D SetTranslation(Point2D translation)
    {
        return new Matrix2D(
            this[0, 0], this[0, 1], translation.X,
            this[1, 0], this[1, 1], translation.Y,
            this[2, 0], this[2, 1], this[2, 2]
        );
    }

    /// <summary>
    /// Returns a matrix with modified translation.
    /// </summary>
    public Matrix2D SetTranslation(double x, double y)
    {
        return SetTranslation(new Point2D(x, y));
    }

    /// <summary>
    /// Returns a matrix with the specified axes.
    /// </summary>
    public Matrix2D SetAxes(Vector2D xAxis, Vector2D yAxis)
    {
        var translation = GetTranslation();
        return new Matrix2D(
            xAxis.X, yAxis.X, translation.X,
            xAxis.Y, yAxis.Y, translation.Y,
            0, 0, 1
        );
    }

    /// <summary>
    /// Returns the inverse of the matrix.
    /// </summary>
    public Matrix2D Inverse()
    {
        var det = Determinant;

        if (Math.Abs(det) < Constants.Tolerance)
            throw new InvalidOperationException("Matrix is singular and cannot be inverted.");

        return new Matrix2D(
            (this[1, 1] * this[2, 2] - this[1, 2] * this[2, 1]) / det,
            (this[0, 2] * this[2, 1] - this[0, 1] * this[2, 2]) / det,
            (this[0, 1] * this[1, 2] - this[0, 2] * this[1, 1]) / det,

            (this[1, 2] * this[2, 0] - this[1, 0] * this[2, 2]) / det,
            (this[0, 0] * this[2, 2] - this[0, 2] * this[2, 0]) / det,
            (this[0, 2] * this[1, 0] - this[0, 0] * this[1, 2]) / det,

            (this[1, 0] * this[2, 1] - this[1, 1] * this[2, 0]) / det,
            (this[0, 1] * this[2, 0] - this[0, 0] * this[2, 1]) / det,
            (this[0, 0] * this[1, 1] - this[0, 1] * this[1, 0]) / det
        );
    }

    /// <summary>
    /// Creates a rotation matrix from one vector to another.
    /// </summary>
    public static Matrix2D CreateRotationFromTo(Vector2D from, Vector2D to)
    {
        var fromNorm = from.Normalized();
        var toNorm = to.Normalized();

        // Calculate the rotation using the dot and cross products
        var dot = fromNorm.Dot(toNorm);
        var cross = fromNorm.Cross(toNorm);

        // Build rotation matrix
        return new Matrix2D(
            dot, -cross, 0,
            cross, dot, 0,
            0, 0, 1
        );
    }

    /// <summary>
    /// Creates a rotation matrix around a point.
    /// </summary>
    public static Matrix2D CreateRotationAroundPoint(double angleRadians, Point2D center)
    {
        var toOrigin = Translation(-center.X, -center.Y);
        var rotation = Rotation(angleRadians);
        var fromOrigin = Translation(center.X, center.Y);

        return fromOrigin * rotation * toOrigin;
    }

    /// <summary>
    /// Creates a reflection matrix across a line through the origin.
    /// The line is defined by its <b>normal vector</b>.
    ///
    /// IMPORTANT:
    /// - The <paramref name="lineNormal"/> must point perpendicular to the line you want to reflect across.
    /// - Example: 
    ///     Normal (0,1) → mirrors across the X-axis.
    ///     Normal (1,0) → mirrors across the Y-axis.
    /// </summary>
    /// <param name="lineNormal">
    /// A vector perpendicular to the line of reflection.
    /// Only the direction matters; the method normalizes it.
    /// </param>
    public static Matrix2D CreateReflectionFromNormal(Vector2D lineNormal)
    {
        var n = lineNormal.Normalized();
        return new Matrix2D(
            1 - 2 * n.X * n.X, -2 * n.X * n.Y, 0,
            -2 * n.Y * n.X, 1 - 2 * n.Y * n.Y, 0,
            0, 0, 1
        );
    }

    /// <summary>
    /// Creates a reflection matrix across a line through the origin.
    /// The line is defined by its <b>direction vector</b>.
    ///
    /// IMPORTANT:
    /// - The <paramref name="lineDirection"/> must point along the line you want to reflect across.
    /// - Example:
    ///     Direction (1,0) → mirrors across the X-axis.
    ///     Direction (0,1) → mirrors across the Y-axis.
    /// </summary>
    /// <param name="lineDirection">
    /// A vector parallel to the line of reflection.
    /// Only the direction matters; the method normalizes it.
    /// </param>
    public static Matrix2D CreateReflectionFromDirection(Vector2D lineDirection)
    {
        var d = lineDirection.Normalized();
        return new Matrix2D(
            2 * d.X * d.X - 1, 2 * d.X * d.Y, 0,
            2 * d.Y * d.X, 2 * d.Y * d.Y - 1, 0,
            0, 0, 1
        );
    }


    /// <summary>
    /// Decomposes the matrix into translation, rotation, and scale components.
    /// </summary>
    public void Decompose(out Point2D translation, out double rotation, out Vector2D scale)
    {
        translation = GetTranslation();

        // Calculate scale
        var scaleX = XAxis.Length;
        var scaleY = YAxis.Length;
        scale = new Vector2D(scaleX, scaleY);

        // Calculate rotation (angle of X axis from positive X)
        if (scaleX > Constants.Tolerance)
        {
            rotation = Math.Atan2(XAxis.Y, XAxis.X);
        }
        else
        {
            rotation = 0;
        }
    }

    /// <summary>
    /// Copies a matrix.
    /// </summary>
    /// <returns></returns>
    public Matrix2D Copy()
    {
        return new Matrix2D(
            m[0, 0], m[0, 1], m[0, 2],
            m[1, 0], m[1, 1], m[1, 2],
            m[2, 0], m[2, 1], m[2, 2]
        );
    }
}