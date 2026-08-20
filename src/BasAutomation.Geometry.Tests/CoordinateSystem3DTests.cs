namespace BasAutomation.Geometry.Tests;

public class CoordinateSystem3DTests
{
    [Fact]
    public void Constructor_Default_CreatesIdentity()
    {
        var cs = new CoordinateSystem3D();

        Assert.Equal(0.0, cs.Origin.X);
        Assert.Equal(0.0, cs.Origin.Y);
        Assert.Equal(0.0, cs.Origin.Z);
        Assert.Equal(1.0, cs.XAxis.X);
        Assert.Equal(0.0, cs.XAxis.Y);
        Assert.Equal(0.0, cs.XAxis.Z);
        Assert.Equal(0.0, cs.YAxis.X);
        Assert.Equal(1.0, cs.YAxis.Y);
        Assert.Equal(0.0, cs.YAxis.Z);
        Assert.Equal(0.0, cs.ZAxis.X);
        Assert.Equal(0.0, cs.ZAxis.Y);
        Assert.Equal(1.0, cs.ZAxis.Z);
    }

    [Fact]
    public void Constructor_WithParameters_SetsPropertiesAndNormalizes()
    {
        var origin = new Point3D(5, 10, 15);
        var xAxis = new Vector3D(2, 0, 0);
        var yAxis = new Vector3D(0, 3, 0);
        var zAxis = new Vector3D(0, 0, 4);

        var cs = new CoordinateSystem3D(origin, xAxis, yAxis, zAxis);

        Assert.Equal(5.0, cs.Origin.X);
        Assert.Equal(10.0, cs.Origin.Y);
        Assert.Equal(15.0, cs.Origin.Z);
        Assert.Equal(1.0, cs.XAxis.Length, Constants.Tolerance);
        Assert.Equal(1.0, cs.YAxis.Length, Constants.Tolerance);
        Assert.Equal(1.0, cs.ZAxis.Length, Constants.Tolerance);
    }

    [Fact]
    public void FromOriginAndXY_CreatesOrthogonalSystem()
    {
        var origin = new Point3D(0, 0, 0);
        var xAxis = new Vector3D(1, 0, 0);
        var yAxis = new Vector3D(0, 1, 0);

        var cs = CoordinateSystem3D.FromOriginAndXy(origin, xAxis, yAxis);

        Assert.Equal(0.0, cs.XAxis.Dot(cs.YAxis), Constants.Tolerance);
        Assert.Equal(0.0, cs.XAxis.Dot(cs.ZAxis), Constants.Tolerance);
        Assert.Equal(0.0, cs.YAxis.Dot(cs.ZAxis), Constants.Tolerance);
        Assert.Equal(1.0, cs.ZAxis.Z, Constants.Tolerance);
    }

    [Fact]
    public void FromOriginAndZAxis_CreatesOrthogonalSystem()
    {
        var origin = new Point3D(0, 0, 0);
        var zAxis = new Vector3D(0, 0, 1);
        var xHint = new Vector3D(1, 0, 0);

        var cs = CoordinateSystem3D.FromOriginAndZAxis(origin, zAxis, xHint);

        Assert.Equal(0.0, cs.XAxis.Dot(cs.YAxis), Constants.Tolerance);
        Assert.Equal(0.0, cs.XAxis.Dot(cs.ZAxis), Constants.Tolerance);
        Assert.Equal(0.0, cs.YAxis.Dot(cs.ZAxis), Constants.Tolerance);
    }

    [Fact]
    public void ToLocal_Point_TransformsCorrectly()
    {
        var cs = new CoordinateSystem3D(
            new Point3D(10, 20, 30),
            new Vector3D(1, 0, 0),
            new Vector3D(0, 1, 0),
            new Vector3D(0, 0, 1)
        );

        var worldPoint = new Point3D(15, 25, 35);
        var localPoint = cs.ToLocal(worldPoint);

        Assert.Equal(5.0, localPoint.X, Constants.Tolerance);
        Assert.Equal(5.0, localPoint.Y, Constants.Tolerance);
        Assert.Equal(5.0, localPoint.Z, Constants.Tolerance);
    }

    [Fact]
    public void ToWorld_Point_TransformsCorrectly()
    {
        var cs = new CoordinateSystem3D(
            new Point3D(10, 20, 30),
            new Vector3D(1, 0, 0),
            new Vector3D(0, 1, 0),
            new Vector3D(0, 0, 1)
        );

        var localPoint = new Point3D(5, 5, 5);
        var worldPoint = cs.ToWorld(localPoint);

        Assert.Equal(15.0, worldPoint.X, Constants.Tolerance);
        Assert.Equal(25.0, worldPoint.Y, Constants.Tolerance);
        Assert.Equal(35.0, worldPoint.Z, Constants.Tolerance);
    }

    [Fact]
    public void Translate_MovesOrigin()
    {
        var cs = new CoordinateSystem3D();
        var translation = new Vector3D(5, 10, 15);

        cs.Translate(translation);

        Assert.Equal(5.0, cs.Origin.X);
        Assert.Equal(10.0, cs.Origin.Y);
        Assert.Equal(15.0, cs.Origin.Z);
    }

    [Fact]
    public void RotateX_RotatesCorrectly()
    {
        var cs = new CoordinateSystem3D();

        cs.RotateX(Math.PI / 2);

        Assert.Equal(1.0, cs.XAxis.X, Constants.Tolerance);
        Assert.Equal(0.0, cs.XAxis.Y, Constants.Tolerance);
        Assert.Equal(0.0, cs.XAxis.Z, Constants.Tolerance);
        Assert.Equal(0.0, cs.YAxis.X, Constants.Tolerance);
        Assert.Equal(0.0, cs.YAxis.Y, Constants.Tolerance);
        Assert.Equal(1.0, cs.YAxis.Z, Constants.Tolerance);
        Assert.Equal(0.0, cs.ZAxis.X, Constants.Tolerance);
        Assert.Equal(-1.0, cs.ZAxis.Y, Constants.Tolerance);
        Assert.Equal(0.0, cs.ZAxis.Z, Constants.Tolerance);
    }

    [Fact]
    public void RotateY_RotatesCorrectly()
    {
        var cs = new CoordinateSystem3D();

        cs.RotateY(Math.PI / 2);

        Assert.Equal(0.0, cs.XAxis.X, Constants.Tolerance);
        Assert.Equal(0.0, cs.XAxis.Y, Constants.Tolerance);
        Assert.Equal(-1.0, cs.XAxis.Z, Constants.Tolerance);
        Assert.Equal(0.0, cs.YAxis.X, Constants.Tolerance);
        Assert.Equal(1.0, cs.YAxis.Y, Constants.Tolerance);
        Assert.Equal(0.0, cs.YAxis.Z, Constants.Tolerance);
        Assert.Equal(1.0, cs.ZAxis.X, Constants.Tolerance);
        Assert.Equal(0.0, cs.ZAxis.Y, Constants.Tolerance);
        Assert.Equal(0.0, cs.ZAxis.Z, Constants.Tolerance);
    }

    [Fact]
    public void RotateZ_RotatesCorrectly()
    {
        var cs = new CoordinateSystem3D();

        cs.RotateZ(Math.PI / 2);

        Assert.Equal(0.0, cs.XAxis.X, Constants.Tolerance);
        Assert.Equal(1.0, cs.XAxis.Y, Constants.Tolerance);
        Assert.Equal(0.0, cs.XAxis.Z, Constants.Tolerance);
        Assert.Equal(-1.0, cs.YAxis.X, Constants.Tolerance);
        Assert.Equal(0.0, cs.YAxis.Y, Constants.Tolerance);
        Assert.Equal(0.0, cs.YAxis.Z, Constants.Tolerance);
        Assert.Equal(0.0, cs.ZAxis.X, Constants.Tolerance);
        Assert.Equal(0.0, cs.ZAxis.Y, Constants.Tolerance);
        Assert.Equal(1.0, cs.ZAxis.Z, Constants.Tolerance);
    }

    [Fact]
    public void RotateAroundAxis_ArbitraryAxis_RotatesCorrectly()
    {
        var cs = new CoordinateSystem3D();
        var axis = new Vector3D(1, 1, 1).Normalized();

        cs.RotateAroundAxis(axis, Math.PI * 2 / 3); // 120 degrees

        // After 120 degree rotation around (1,1,1), X axis should go to Y position
        Assert.Equal(0.0, cs.XAxis.X, Constants.Tolerance);
        Assert.Equal(1.0, cs.XAxis.Y, Constants.Tolerance);
        Assert.Equal(0.0, cs.XAxis.Z, Constants.Tolerance);
    }

    [Fact]
    public void RotateAroundAxis_WithPoint_TransformsCorrectly()
    {
        var cs = new CoordinateSystem3D(
            new Point3D(1, 0, 0),
            new Vector3D(1, 0, 0),
            new Vector3D(0, 1, 0),
            new Vector3D(0, 0, 1)
        );

        cs.RotateAroundAxis(
            new Point3D(0, 0, 0),
            new Vector3D(0, 0, 1),
            Math.PI / 2
        );

        Assert.Equal(0.0, cs.Origin.X, Constants.Tolerance);
        Assert.Equal(1.0, cs.Origin.Y, Constants.Tolerance);
        Assert.Equal(0.0, cs.Origin.Z, Constants.Tolerance);
    }

    [Fact]
    public void RotateAroundPoint_CombinesRotations()
    {
        var cs = new CoordinateSystem3D(
            new Point3D(1, 0, 0),
            new Vector3D(1, 0, 0),
            new Vector3D(0, 1, 0),
            new Vector3D(0, 0, 1)
        );

        cs.RotateAroundPoint(
            new Point3D(0, 0, 0),
            0, 0, Math.PI / 2
        );

        Assert.Equal(0.0, cs.Origin.X, Constants.Tolerance);
        Assert.Equal(1.0, cs.Origin.Y, Constants.Tolerance);
        Assert.Equal(0.0, cs.Origin.Z, Constants.Tolerance);
    }

    [Fact]
    public void Scale_Uniform_ScalesAllAxes()
    {
        var cs = new CoordinateSystem3D();

        cs.Scale(2.0);

        Assert.Equal(2.0, cs.XAxis.Length, Constants.Tolerance);
        Assert.Equal(2.0, cs.YAxis.Length, Constants.Tolerance);
        Assert.Equal(2.0, cs.ZAxis.Length, Constants.Tolerance);
    }

    [Fact]
    public void Scale_NonUniform_ScalesAxesIndependently()
    {
        var cs = new CoordinateSystem3D();

        cs.Scale(2.0, 3.0, 4.0);

        Assert.Equal(2.0, cs.XAxis.Length, Constants.Tolerance);
        Assert.Equal(3.0, cs.YAxis.Length, Constants.Tolerance);
        Assert.Equal(4.0, cs.ZAxis.Length, Constants.Tolerance);
    }

    [Fact]
    public void Orthonormalize_CorrectsBasis()
    {
        // Create a slightly skewed coordinate system
        var cs = new CoordinateSystem3D(
            new Point3D(0, 0, 0),
            new Vector3D(1, 0.1, 0),
            new Vector3D(0.1, 1, 0),
            new Vector3D(0, 0, 1)
        );

        cs.Orthonormalize();

        // Check orthogonality
        Assert.Equal(0.0, cs.XAxis.Dot(cs.YAxis), Constants.Tolerance);
        Assert.Equal(0.0, cs.XAxis.Dot(cs.ZAxis), Constants.Tolerance);
        Assert.Equal(0.0, cs.YAxis.Dot(cs.ZAxis), Constants.Tolerance);

        // Check normalization
        Assert.Equal(1.0, cs.XAxis.Length, Constants.Tolerance);
        Assert.Equal(1.0, cs.YAxis.Length, Constants.Tolerance);
        Assert.Equal(1.0, cs.ZAxis.Length, Constants.Tolerance);
    }

    [Fact]
    public void Clone_CreatesIndependentCopy()
    {
        var cs1 = new CoordinateSystem3D(
            new Point3D(5, 10, 15),
            new Vector3D(1, 0, 0),
            new Vector3D(0, 1, 0),
            new Vector3D(0, 0, 1)
        );

        var cs2 = cs1.Clone();
        cs1.Translate(new Vector3D(10, 10, 10));

        Assert.Equal(5.0, cs2.Origin.X);
        Assert.Equal(10.0, cs2.Origin.Y);
        Assert.Equal(15.0, cs2.Origin.Z);
    }

    [Fact]
    public void RoundTripTransformation_PreservesPoint()
    {
        var cs = CoordinateSystem3D.FromOriginAndXy(
            new Point3D(5, 10, 15),
            new Vector3D(1, 1, 0).Normalized(),
            new Vector3D(-1, 1, 0).Normalized()
        );

        var originalPoint = new Point3D(20, 30, 40);
        var localPoint = cs.ToLocal(originalPoint);
        var worldPoint = cs.ToWorld(localPoint);

        Assert.True(originalPoint.IsEqualTo(worldPoint, Constants.Tolerance));
    }

    [Fact]
    public void RoundTripTransformation_PreservesVector()
    {
        var cs = new CoordinateSystem3D(
            new Point3D(5, 10, 15),
            new Vector3D(0, 1, 0),
            new Vector3D(0, 0, 1),
            new Vector3D(1, 0, 0)
        );

        var originalVector = new Vector3D(1, 2, 3);
        var localVector = cs.ToLocal(originalVector);
        var worldVector = cs.ToWorld(localVector);

        Assert.True(originalVector.IsEqualTo(worldVector, Constants.Tolerance));
    }

    [Fact]
    public void MatrixCaching_UpdatesOnChange()
    {
        var cs = new CoordinateSystem3D();

        // Get initial matrix
        var matrix1 = cs.Matrix;

        // Change origin
        cs.Origin = new Point3D(10, 0, 0);

        // Get new matrix
        var matrix2 = cs.Matrix;

        // Matrices should be different
        Assert.False(matrix1.IsEqualTo(matrix2));
    }
}