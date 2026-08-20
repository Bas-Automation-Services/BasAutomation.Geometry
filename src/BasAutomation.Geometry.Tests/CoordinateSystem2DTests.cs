namespace BasAutomation.Geometry.Tests;

public class CoordinateSystem2DTests
{
    [Fact]
    public void Constructor_Default_CreatesIdentity()
    {
        var cs = new CoordinateSystem2D();

        Assert.Equal(0.0, cs.Origin.X);
        Assert.Equal(0.0, cs.Origin.Y);
        Assert.Equal(1.0, cs.XAxis.X);
        Assert.Equal(0.0, cs.XAxis.Y);
        Assert.Equal(0.0, cs.YAxis.X);
        Assert.Equal(1.0, cs.YAxis.Y);
    }

    [Fact]
    public void Constructor_WithParameters_SetsPropertiesAndNormalizes()
    {
        var origin = new Point2D(5, 10);
        var xAxis = new Vector2D(2, 0); // Will be normalized
        var yAxis = new Vector2D(0, 3); // Will be normalized

        var cs = new CoordinateSystem2D(origin, xAxis, yAxis);

        Assert.Equal(5.0, cs.Origin.X);
        Assert.Equal(10.0, cs.Origin.Y);
        Assert.Equal(1.0, cs.XAxis.X);
        Assert.Equal(0.0, cs.XAxis.Y);
        Assert.Equal(0.0, cs.YAxis.X);
        Assert.Equal(1.0, cs.YAxis.Y);
    }

    [Fact]
    public void FromOriginAndAngle_CreatesCorrectSystem()
    {
        var origin = new Point2D(5, 5);
        var angle = Math.PI / 4; // 45 degrees

        var cs = CoordinateSystem2D.FromOriginAndAngle(origin, angle);

        Assert.Equal(5.0, cs.Origin.X);
        Assert.Equal(5.0, cs.Origin.Y);
        Assert.Equal(Math.Sqrt(2) / 2, cs.XAxis.X, Constants.Tolerance);
        Assert.Equal(Math.Sqrt(2) / 2, cs.XAxis.Y, Constants.Tolerance);
        Assert.Equal(-Math.Sqrt(2) / 2, cs.YAxis.X, Constants.Tolerance);
        Assert.Equal(Math.Sqrt(2) / 2, cs.YAxis.Y, Constants.Tolerance);
    }

    [Fact]
    public void ToLocal_Point_TransformsCorrectly()
    {
        var cs = new CoordinateSystem2D(
            new Point2D(10, 20),
            new Vector2D(1, 0),
            new Vector2D(0, 1)
        );

        var worldPoint = new Point2D(15, 25);
        var localPoint = cs.ToLocal(worldPoint);

        Assert.Equal(5.0, localPoint.X, Constants.Tolerance);
        Assert.Equal(5.0, localPoint.Y, Constants.Tolerance);
    }

    [Fact]
    public void ToWorld_Point_TransformsCorrectly()
    {
        var cs = new CoordinateSystem2D(
            new Point2D(10, 20),
            new Vector2D(1, 0),
            new Vector2D(0, 1)
        );

        var localPoint = new Point2D(5, 5);
        var worldPoint = cs.ToWorld(localPoint);

        Assert.Equal(15.0, worldPoint.X, Constants.Tolerance);
        Assert.Equal(25.0, worldPoint.Y, Constants.Tolerance);
    }

    [Fact]
    public void ToLocal_Vector_TransformsCorrectly()
    {
        var cs = CoordinateSystem2D.FromOriginAndAngle(new Point2D(0, 0), Math.PI / 2);

        var worldVector = new Vector2D(1, 0);
        var localVector = cs.ToLocal(worldVector);

        Assert.Equal(0.0, localVector.X, Constants.Tolerance);
        Assert.Equal(-1.0, localVector.Y, Constants.Tolerance);
    }

    [Fact]
    public void ToWorld_Vector_TransformsCorrectly()
    {
        var cs = CoordinateSystem2D.FromOriginAndAngle(new Point2D(0, 0), Math.PI / 2);

        var localVector = new Vector2D(1, 0);
        var worldVector = cs.ToWorld(localVector);

        Assert.Equal(0.0, worldVector.X, Constants.Tolerance);
        Assert.Equal(1.0, worldVector.Y, Constants.Tolerance);
    }

    [Fact]
    public void Translate_MovesOrigin()
    {
        var cs = new CoordinateSystem2D();
        var translation = new Vector2D(5, 10);

        cs.Translate(translation);

        Assert.Equal(5.0, cs.Origin.X);
        Assert.Equal(10.0, cs.Origin.Y);
    }

    [Fact]
    public void Rotate_RotatesAxes()
    {
        var cs = new CoordinateSystem2D();

        cs.Rotate(Math.PI / 2); // 90 degrees

        Assert.Equal(0.0, cs.XAxis.X, Constants.Tolerance);
        Assert.Equal(1.0, cs.XAxis.Y, Constants.Tolerance);
        Assert.Equal(-1.0, cs.YAxis.X, Constants.Tolerance);
        Assert.Equal(0.0, cs.YAxis.Y, Constants.Tolerance);
    }

    [Fact]
    public void RotateAroundPoint_TransformsCorrectly()
    {
        var cs = new CoordinateSystem2D(
            new Point2D(1, 0),
            new Vector2D(1, 0),
            new Vector2D(0, 1)
        );

        cs.RotateAroundPoint(new Point2D(0, 0), Math.PI / 2);

        Assert.Equal(0.0, cs.Origin.X, Constants.Tolerance);
        Assert.Equal(1.0, cs.Origin.Y, Constants.Tolerance);
        Assert.Equal(0.0, cs.XAxis.X, Constants.Tolerance);
        Assert.Equal(1.0, cs.XAxis.Y, Constants.Tolerance);
    }

    [Fact]
    public void Scale_Uniform_ScalesAxes()
    {
        var cs = new CoordinateSystem2D();

        cs.Scale(2.0);

        Assert.Equal(2.0, cs.XAxis.Length, Constants.Tolerance);
        Assert.Equal(2.0, cs.YAxis.Length, Constants.Tolerance);
    }

    [Fact]
    public void Scale_NonUniform_ScalesAxesIndependently()
    {
        var cs = new CoordinateSystem2D();

        cs.Scale(2.0, 3.0);

        Assert.Equal(2.0, cs.XAxis.Length, Constants.Tolerance);
        Assert.Equal(3.0, cs.YAxis.Length, Constants.Tolerance);
    }

    [Fact]
    public void Clone_CreatesIndependentCopy()
    {
        var cs1 = new CoordinateSystem2D(
            new Point2D(5, 10),
            new Vector2D(1, 1),
            new Vector2D(-1, 1)
        );

        var cs2 = cs1.Clone();
        cs1.Translate(new Vector2D(10, 10));

        Assert.Equal(5.0, cs2.Origin.X);
        Assert.Equal(10.0, cs2.Origin.Y);
    }

    [Fact]
    public void IsEqualTo_SameValues_ReturnsTrue()
    {
        var cs1 = new CoordinateSystem2D(
            new Point2D(1, 2),
            new Vector2D(1, 0),
            new Vector2D(0, 1)
        );
        var cs2 = new CoordinateSystem2D(
            new Point2D(1, 2),
            new Vector2D(1, 0),
            new Vector2D(0, 1)
        );

        Assert.True(cs1.IsEqualTo(cs2));
    }

    [Fact]
    public void IsEqualTo_DifferentValues_ReturnsFalse()
    {
        var cs1 = new CoordinateSystem2D();
        var cs2 = new CoordinateSystem2D(
            new Point2D(1, 0),
            new Vector2D(1, 0),
            new Vector2D(0, 1)
        );

        Assert.False(cs1.IsEqualTo(cs2));
    }

    [Fact]
    public void RoundTripTransformation_PreservesPoint()
    {
        var cs = CoordinateSystem2D.FromOriginAndAngle(
            new Point2D(5, 10),
            Math.PI / 3
        );

        var originalPoint = new Point2D(20, 30);
        var localPoint = cs.ToLocal(originalPoint);
        var worldPoint = cs.ToWorld(localPoint);

        Assert.True(originalPoint.IsEqualTo(worldPoint, Constants.Tolerance));
    }
}