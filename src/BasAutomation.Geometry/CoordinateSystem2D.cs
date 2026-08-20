namespace BasAutomation.Geometry;

/// <summary>
/// Represents a 2D coordinate system with origin, axes, and transformation capabilities.
/// </summary>
public class CoordinateSystem2D : IEquatable<CoordinateSystem2D>, IGeometryComparable<CoordinateSystem2D>
{
    private Point2D origin;
    private Vector2D xAxis;
    private Vector2D yAxis;
    private Matrix2D matrix;
    private Matrix2D inverseMatrix;
    private bool isDirty = true;

    /// <summary>
    /// Gets or sets the origin of the coordinate system.
    /// </summary>
    public Point2D Origin
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
    public Vector2D XAxis
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
    public Vector2D YAxis
    {
        get => yAxis;
        set
        {
            yAxis = value.Normalized();
            isDirty = true;
        }
    }

    /// <summary>
    /// Gets the transformation matrix from world coordinates to this coordinate system.
    /// </summary>
    public Matrix2D Matrix
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
    public Matrix2D InverseMatrix
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
    /// Initializes a new instance of the CoordinateSystem2D class with default values (identity).
    /// </summary>
    public CoordinateSystem2D() : this(new Point2D(0, 0), new Vector2D(1, 0), new Vector2D(0, 1))
    {
    }

    /// <summary>
    /// Initializes a new instance of the CoordinateSystem2D class.
    /// </summary>
    /// <param name="origin">The origin point.</param>
    /// <param name="xAxis">The X-axis direction (will be normalized).</param>
    /// <param name="yAxis">The Y-axis direction (will be normalized).</param>
    public CoordinateSystem2D(Point2D origin, Vector2D xAxis, Vector2D yAxis)
    {
        this.origin = origin;
        this.xAxis = xAxis.Normalized();
        this.yAxis = yAxis.Normalized();
        UpdateMatrices();
    }

    /// <summary>
    /// Creates a coordinate system from an origin and rotation angle.
    /// </summary>
    /// <param name="origin">The origin point.</param>
    /// <param name="angle">The rotation angle in radians.</param>
    /// <returns>A new coordinate system.</returns>
    public static CoordinateSystem2D FromOriginAndAngle(Point2D origin, double angle)
    {
        var cos = Math.Cos(angle);
        var sin = Math.Sin(angle);
        return new CoordinateSystem2D(origin, new Vector2D(cos, sin), new Vector2D(-sin, cos));
    }

    /// <summary>
    /// Updates the transformation matrices based on current origin and axes.
    /// </summary>
    private void UpdateMatrices()
    {
        // Matrix to transform from world to local coordinates
        matrix = new Matrix2D(
            xAxis.X, xAxis.Y, -(xAxis.X * origin.X + xAxis.Y * origin.Y),
            yAxis.X, yAxis.Y, -(yAxis.X * origin.X + yAxis.Y * origin.Y),
            0, 0, 1
        );

        // Matrix to transform from local to world coordinates
        inverseMatrix = new Matrix2D(
            xAxis.X, yAxis.X, origin.X,
            xAxis.Y, yAxis.Y, origin.Y,
            0, 0, 1
        );

        isDirty = false;
    }

    /// <summary>
    /// Transforms a point from world coordinates to this coordinate system.
    /// </summary>
    /// <param name="worldPoint">The point in world coordinates.</param>
    /// <returns>The point in local coordinates.</returns>
    public Point2D ToLocal(Point2D worldPoint)
    {
        return Matrix.Transform(worldPoint);
    }

    /// <summary>
    /// Transforms a point from this coordinate system to world coordinates.
    /// </summary>
    /// <param name="localPoint">The point in local coordinates.</param>
    /// <returns>The point in world coordinates.</returns>
    public Point2D ToWorld(Point2D localPoint)
    {
        return InverseMatrix.Transform(localPoint);
    }

    /// <summary>
    /// Transforms a vector from world coordinates to this coordinate system.
    /// </summary>
    /// <param name="worldVector">The vector in world coordinates.</param>
    /// <returns>The vector in local coordinates.</returns>
    public Vector2D ToLocal(Vector2D worldVector)
    {
        return Matrix.Transform(worldVector);
    }

    /// <summary>
    /// Transforms a vector from this coordinate system to world coordinates.
    /// </summary>
    /// <param name="localVector">The vector in local coordinates.</param>
    /// <returns>The vector in world coordinates.</returns>
    public Vector2D ToWorld(Vector2D localVector)
    {
        return InverseMatrix.Transform(localVector);
    }

    /// <summary>
    /// Translates the coordinate system by the specified vector.
    /// </summary>
    /// <param name="translation">The translation vector.</param>
    public void Translate(Vector2D translation)
    {
        Origin += translation;
    }

    /// <summary>
    /// Rotates the coordinate system around its origin.
    /// </summary>
    /// <param name="angle">The rotation angle in radians.</param>
    public void Rotate(double angle)
    {
        var rotation = Matrix2D.Rotation(angle);
        XAxis = rotation.Transform(XAxis);
        YAxis = rotation.Transform(YAxis);
    }

    /// <summary>
    /// Rotates the coordinate system around a specified point.
    /// </summary>
    /// <param name="center">The center of rotation.</param>
    /// <param name="angle">The rotation angle in radians.</param>
    public void RotateAroundPoint(Point2D center, double angle)
    {
        // Translate origin
        var toCenter = Origin - center;
        var cos = Math.Cos(angle);
        var sin = Math.Sin(angle);

        // Rotate the position
        var newX = toCenter.X * cos - toCenter.Y * sin;
        var newY = toCenter.X * sin + toCenter.Y * cos;
        Origin = new Point2D(center.X + newX, center.Y + newY);

        // Rotate the axes
        Rotate(angle);
    }

    /// <summary>
    /// Scales the coordinate system uniformly.
    /// </summary>
    /// <param name="scale">The scale factor.</param>
    public void Scale(double scale)
    {
        Scale(scale, scale);
    }

    /// <summary>
    /// Scales the coordinate system non-uniformly.
    /// </summary>
    /// <param name="scaleX">The X scale factor.</param>
    /// <param name="scaleY">The Y scale factor.</param>
    public void Scale(double scaleX, double scaleY)
    {
        // Scale the axes by their respective scale factors
        xAxis *= scaleX;
        yAxis *= scaleY;
        isDirty = true;
    }

    /// <summary>
    /// Creates a copy of this coordinate system.
    /// </summary>
    /// <returns>A new coordinate system with the same properties.</returns>
    public CoordinateSystem2D Clone()
    {
        return new CoordinateSystem2D(Origin, XAxis, YAxis);
    }

    /// <summary>
    /// Determines whether this coordinate system is equal to another within the specified tolerance.
    /// </summary>
    /// <param name="other">The other coordinate system to compare.</param>
    /// <param name="tolerance">The tolerance for floating-point comparison.</param>
    /// <returns>true if the coordinate systems are equal within tolerance; otherwise, false.</returns>
    public bool IsEqualTo(CoordinateSystem2D? other, double tolerance = 1e-6)
    {
        if (other == null) return false;

        return Origin.IsEqualTo(other.Origin, tolerance) &&
               XAxis.IsEqualTo(other.XAxis, tolerance) &&
               YAxis.IsEqualTo(other.YAxis, tolerance);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is CoordinateSystem2D other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(CoordinateSystem2D? other) => IsEqualTo(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Origin, XAxis, YAxis);
}