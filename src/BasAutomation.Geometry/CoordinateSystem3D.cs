namespace BasAutomation.Geometry;

/// <summary>
/// Represents a 3D coordinate system with origin, axes, and transformation capabilities.
/// </summary>
public class CoordinateSystem3D : IEquatable<CoordinateSystem3D>, IGeometryComparable<CoordinateSystem3D>
{
    private Point3D origin;
    private Vector3D xAxis;
    private Vector3D yAxis;
    private Vector3D zAxis;
    private Matrix3D matrix;
    private Matrix3D inverseMatrix;
    private bool isDirty = true;

    /// <summary>
    /// Gets or sets the origin of the coordinate system.
    /// </summary>
    public Point3D Origin
    {
        get => origin;
        set
        {
            origin = value;
            isDirty = true;
        }
    }

    /// <summary>
    /// Gets or sets the X-axis direction vector.
    /// </summary>
    public Vector3D XAxis
    {
        get => xAxis;
        set
        {
            xAxis = value.Normalized();
            isDirty = true;
        }
    }

    /// <summary>
    /// Gets or sets the Y-axis direction vector.
    /// </summary>
    public Vector3D YAxis
    {
        get => yAxis;
        set
        {
            yAxis = value.Normalized();
            isDirty = true;
        }
    }

    /// <summary>
    /// Gets or sets the Z-axis direction vector.
    /// </summary>
    public Vector3D ZAxis
    {
        get => zAxis;
        set
        {
            zAxis = value.Normalized();
            isDirty = true;
        }
    }

    /// <summary>
    /// Gets the transformation matrix from world coordinates to this coordinate system.
    /// </summary>
    public Matrix3D Matrix
    {
        get
        {
            if (isDirty)
            {
                UpdateMatrices();
            }
            return matrix;
        }
    }

    /// <summary>
    /// Gets the inverse transformation matrix from this coordinate system to world coordinates.
    /// </summary>
    public Matrix3D InverseMatrix
    {
        get
        {
            if (isDirty)
            {
                UpdateMatrices();
            }
            return inverseMatrix;
        }
    }

    /// <summary>
    /// Initializes a new instance of the CoordinateSystem3D class with default values (identity).
    /// </summary>
    public CoordinateSystem3D() : this(
        new Point3D(0, 0, 0),
        new Vector3D(1, 0, 0),
        new Vector3D(0, 1, 0),
        new Vector3D(0, 0, 1))
    {
    }

    /// <summary>
    /// Initializes a new instance of the CoordinateSystem3D class.
    /// </summary>
    /// <param name="origin">The origin point.</param>
    /// <param name="xAxis">The X-axis direction (will be normalized).</param>
    /// <param name="yAxis">The Y-axis direction (will be normalized).</param>
    /// <param name="zAxis">The Z-axis direction (will be normalized).</param>
    public CoordinateSystem3D(Point3D origin, Vector3D xAxis, Vector3D yAxis, Vector3D zAxis)
    {
        this.origin = origin;
        this.xAxis = xAxis.Normalized();
        this.yAxis = yAxis.Normalized();
        this.zAxis = zAxis.Normalized();
        UpdateMatrices();
    }

    /// <summary>
    /// Initializes a new instance of the CoordinateSystem3D class from a transformation matrix.
    /// The matrix should be a local-to-world transformation (columns are X, Y, Z axes and translation).
    /// </summary>
    /// <param name="matrix">The transformation matrix.</param>
    public CoordinateSystem3D(Matrix3D matrix)
        : this(matrix.GetTranslation(), matrix.XAxis, matrix.YAxis, matrix.ZAxis)
    {
    }

    /// <summary>
    /// Creates a coordinate system from an origin and two axes (Z-axis is computed).
    /// </summary>
    /// <param name="origin">The origin point.</param>
    /// <param name="xAxis">The X-axis direction.</param>
    /// <param name="yAxis">The Y-axis direction.</param>
    /// <returns>A new coordinate system.</returns>
    public static CoordinateSystem3D FromOriginAndXy(Point3D origin, Vector3D xAxis, Vector3D yAxis)
    {
        var x = xAxis.Normalized();
        var y = yAxis.Normalized();
        var z = x.Cross(y).Normalized();

        // Recompute Y to ensure orthogonality
        y = z.Cross(x).Normalized();
        return new CoordinateSystem3D(origin, x, y, z);
    }

    /// <summary>
    /// Creates a coordinate system from an origin, Z-axis, and X-axis hint.
    /// </summary>
    /// <param name="origin">The origin point.</param>
    /// <param name="zAxis">The Z-axis direction.</param>
    /// <param name="xHint">A hint for the X-axis direction.</param>
    /// <returns>A new coordinate system.</returns>
    public static CoordinateSystem3D FromOriginAndZAxis(Point3D origin, Vector3D zAxis, Vector3D xHint)
    {
        var z = zAxis.Normalized();
        var y = z.Cross(xHint).Normalized();
        var x = y.Cross(z).Normalized();
        return new CoordinateSystem3D(origin, x, y, z);
    }

    /// <summary>
    /// Updates the transformation matrices based on current origin and axes.
    /// </summary>
    private void UpdateMatrices()
    {
        // Matrix to transform from world to local coordinates
        matrix = new Matrix3D(
            xAxis.X, xAxis.Y, xAxis.Z, -(xAxis.X * origin.X + xAxis.Y * origin.Y + xAxis.Z * origin.Z),
            yAxis.X, yAxis.Y, yAxis.Z, -(yAxis.X * origin.X + yAxis.Y * origin.Y + yAxis.Z * origin.Z),
            zAxis.X, zAxis.Y, zAxis.Z, -(zAxis.X * origin.X + zAxis.Y * origin.Y + zAxis.Z * origin.Z),
            0, 0, 0, 1
        );

        // Matrix to transform from local to world coordinates
        inverseMatrix = new Matrix3D(
            xAxis.X, yAxis.X, zAxis.X, origin.X,
            xAxis.Y, yAxis.Y, zAxis.Y, origin.Y,
            xAxis.Z, yAxis.Z, zAxis.Z, origin.Z,
            0, 0, 0, 1
        );

        isDirty = false;
    }

    /// <summary>
    /// Transforms a point from world coordinates to this coordinate system.
    /// </summary>
    /// <param name="worldPoint">The point in world coordinates.</param>
    /// <returns>The point in local coordinates.</returns>
    public Point3D ToLocal(Point3D worldPoint)
    {
        return Matrix.Transform(worldPoint);
    }

    /// <summary>
    /// Transforms a point from this coordinate system to world coordinates.
    /// </summary>
    /// <param name="localPoint">The point in local coordinates.</param>
    /// <returns>The point in world coordinates.</returns>
    public Point3D ToWorld(Point3D localPoint)
    {
        return InverseMatrix.Transform(localPoint);
    }

    /// <summary>
    /// Transforms a vector from world coordinates to this coordinate system.
    /// </summary>
    /// <param name="worldVector">The vector in world coordinates.</param>
    /// <returns>The vector in local coordinates.</returns>
    public Vector3D ToLocal(Vector3D worldVector)
    {
        return Matrix.Transform(worldVector);
    }

    /// <summary>
    /// Transforms a vector from this coordinate system to world coordinates.
    /// </summary>
    /// <param name="localVector">The vector in local coordinates.</param>
    /// <returns>The vector in world coordinates.</returns>
    public Vector3D ToWorld(Vector3D localVector)
    {
        return InverseMatrix.Transform(localVector);
    }

    /// <summary>
    /// Translates the coordinate system by the specified vector.
    /// </summary>
    /// <param name="translation">The translation vector.</param>
    public void Translate(Vector3D translation)
    {
        Origin = Origin + translation;
    }

    /// <summary>
    /// Rotates the coordinate system around the X-axis.
    /// </summary>
    /// <param name="angle">The rotation angle in radians.</param>
    public void RotateX(double angle)
    {
        var rotation = Matrix3D.RotationX(angle);
        XAxis = rotation.Transform(XAxis);
        YAxis = rotation.Transform(YAxis);
        ZAxis = rotation.Transform(ZAxis);
    }

    /// <summary>
    /// Rotates the coordinate system around the Y-axis.
    /// </summary>
    /// <param name="angle">The rotation angle in radians.</param>
    public void RotateY(double angle)
    {
        var rotation = Matrix3D.RotationY(angle);
        XAxis = rotation.Transform(XAxis);
        YAxis = rotation.Transform(YAxis);
        ZAxis = rotation.Transform(ZAxis);
    }

    /// <summary>
    /// Rotates the coordinate system around the Z-axis.
    /// </summary>
    /// <param name="angle">The rotation angle in radians.</param>
    public void RotateZ(double angle)
    {
        var rotation = Matrix3D.RotationZ(angle);
        XAxis = rotation.Transform(XAxis);
        YAxis = rotation.Transform(YAxis);
        ZAxis = rotation.Transform(ZAxis);
    }

    /// <summary>
    /// Rotates the coordinate system around an arbitrary axis passing through the origin.
    /// </summary>
    /// <param name="axis">The rotation axis (will be normalized).</param>
    /// <param name="angle">The rotation angle in radians.</param>
    public void RotateAroundAxis(Vector3D axis, double angle)
    {
        axis = axis.Normalized();
        var cos = Math.Cos(angle);
        var sin = Math.Sin(angle);
        var oneMinusCos = 1 - cos;

        // Rodrigues' rotation formula
        var rotation = new Matrix3D(
            cos + axis.X * axis.X * oneMinusCos,
            axis.X * axis.Y * oneMinusCos - axis.Z * sin,
            axis.X * axis.Z * oneMinusCos + axis.Y * sin,
            0,
            axis.Y * axis.X * oneMinusCos + axis.Z * sin,
            cos + axis.Y * axis.Y * oneMinusCos,
            axis.Y * axis.Z * oneMinusCos - axis.X * sin,
            0,
            axis.Z * axis.X * oneMinusCos - axis.Y * sin,
            axis.Z * axis.Y * oneMinusCos + axis.X * sin,
            cos + axis.Z * axis.Z * oneMinusCos,
            0,
            0, 0, 0, 1
        );

        XAxis = rotation.Transform(XAxis);
        YAxis = rotation.Transform(YAxis);
        ZAxis = rotation.Transform(ZAxis);
    }

    /// <summary>
    /// Rotates the coordinate system around an arbitrary axis passing through a point.
    /// </summary>
    /// <param name="point">A point on the rotation axis.</param>
    /// <param name="axis">The rotation axis direction.</param>
    /// <param name="angle">The rotation angle in radians.</param>
    public void RotateAroundAxis(Point3D point, Vector3D axis, double angle)
    {
        // Translate to make point the origin
        var toPoint = Origin - point;

        // Create rotation matrix
        axis = axis.Normalized();
        var cos = Math.Cos(angle);
        var sin = Math.Sin(angle);
        var oneMinusCos = 1 - cos;

        var rotation = new Matrix3D(
            cos + axis.X * axis.X * oneMinusCos,
            axis.X * axis.Y * oneMinusCos - axis.Z * sin,
            axis.X * axis.Z * oneMinusCos + axis.Y * sin,
            0,
            axis.Y * axis.X * oneMinusCos + axis.Z * sin,
            cos + axis.Y * axis.Y * oneMinusCos,
            axis.Y * axis.Z * oneMinusCos - axis.X * sin,
            0,
            axis.Z * axis.X * oneMinusCos - axis.Y * sin,
            axis.Z * axis.Y * oneMinusCos + axis.X * sin,
            cos + axis.Z * axis.Z * oneMinusCos,
            0,
            0, 0, 0, 1
        );

        // Rotate origin position
        var rotatedOffset = rotation.Transform(toPoint);
        Origin = point + rotatedOffset;

        // Rotate axes
        XAxis = rotation.Transform(XAxis);
        YAxis = rotation.Transform(YAxis);
        ZAxis = rotation.Transform(ZAxis);
    }

    /// <summary>
    /// Rotates the coordinate system around a point.
    /// </summary>
    /// <param name="center">The center of rotation.</param>
    /// <param name="rotationX">Rotation around X-axis in radians.</param>
    /// <param name="rotationY">Rotation around Y-axis in radians.</param>
    /// <param name="rotationZ">Rotation around Z-axis in radians.</param>
    public void RotateAroundPoint(Point3D center, double rotationX, double rotationY, double rotationZ)
    {
        // Translate origin
        var toCenter = Origin - center;

        // Apply rotations
        var rotation = Matrix3D.RotationZ(rotationZ) * Matrix3D.RotationY(rotationY) * Matrix3D.RotationX(rotationX);

        // Transform origin
        var rotatedOffset = rotation.Transform(toCenter);
        Origin = center + rotatedOffset;

        // Transform axes
        XAxis = rotation.Transform(XAxis);
        YAxis = rotation.Transform(YAxis);
        ZAxis = rotation.Transform(ZAxis);
    }

    /// <summary>
    /// Scales the coordinate system uniformly.
    /// </summary>
    /// <param name="scale">The scale factor.</param>
    public void Scale(double scale)
    {
        Scale(scale, scale, scale);
    }

    /// <summary>
    /// Scales the coordinate system non-uniformly.
    /// </summary>
    /// <param name="scaleX">The X scale factor.</param>
    /// <param name="scaleY">The Y scale factor.</param>
    public void Scale(double scaleX, double scaleY, double scaleZ)
    {
        // Scale the axes by their respective scale factors
        xAxis *= scaleX;
        yAxis *= scaleY;
        zAxis *= scaleZ;
        isDirty = true;
    }

    /// <summary>
    /// Ensures the coordinate system axes are orthonormal.
    /// </summary>
    public void Orthonormalize()
    {
        // Gram-Schmidt process
        xAxis = xAxis.Normalized();
        yAxis = (yAxis - yAxis.Dot(xAxis) * xAxis).Normalized();
        zAxis = xAxis.Cross(yAxis).Normalized();
        isDirty = true;
    }

    /// <summary>
    /// Creates a copy of this coordinate system.
    /// </summary>
    /// <returns>A new coordinate system with the same properties.</returns>
    public CoordinateSystem3D Clone()
    {
        return new CoordinateSystem3D(Origin, XAxis, YAxis, ZAxis);
    }

    /// <summary>
    /// Determines whether this coordinate system is equal to another within the specified tolerance.
    /// </summary>
    /// <param name="other">The other coordinate system to compare.</param>
    /// <param name="tolerance">The tolerance for floating-point comparison.</param>
    /// <returns>true if the coordinate systems are equal within tolerance; otherwise, false.</returns>
    public bool IsEqualTo(CoordinateSystem3D? other, double tolerance = 1e-6)
    {
        if (other == null) return false;
        return Origin.IsEqualTo(other.Origin, tolerance) &&
               XAxis.IsEqualTo(other.XAxis, tolerance) &&
               YAxis.IsEqualTo(other.YAxis, tolerance) &&
               ZAxis.IsEqualTo(other.ZAxis, tolerance);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is CoordinateSystem3D other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(CoordinateSystem3D? other) => IsEqualTo(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Origin, XAxis, YAxis, ZAxis);
}