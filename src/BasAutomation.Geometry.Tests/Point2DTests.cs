namespace BasAutomation.Geometry.Tests;

public class Point2DTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var p = new Point2D(3.0, 4.0);
        Assert.Equal(3.0, p.X);
        Assert.Equal(4.0, p.Y);
    }

    [Fact]
    public void AddVector_ReturnsCorrectPoint()
    {
        var p = new Point2D(1.0, 2.0);
        var v = new Vector2D(3.0, 4.0);
        var result = p + v;
        Assert.Equal(4.0, result.X);
        Assert.Equal(6.0, result.Y);
    }

    [Fact]
    public void SubtractVector_ReturnsCorrectPoint()
    {
        var p = new Point2D(5.0, 7.0);
        var v = new Vector2D(2.0, 3.0);
        var result = p - v;
        Assert.Equal(3.0, result.X);
        Assert.Equal(4.0, result.Y);
    }

    [Fact]
    public void SubtractPoint_ReturnsCorrectVector()
    {
        var p1 = new Point2D(5.0, 7.0);
        var p2 = new Point2D(2.0, 3.0);
        var result = p1 - p2;
        Assert.Equal(3.0, result.X);
        Assert.Equal(4.0, result.Y);
    }

    [Fact]
    public void DistanceTo_CalculatesCorrectly()
    {
        var p1 = new Point2D(0.0, 0.0);
        var p2 = new Point2D(3.0, 4.0);
        Assert.Equal(5.0, p1.DistanceTo(p2));
    }

    [Fact]
    public void IsEqualTo_WithinTolerance_ReturnsTrue()
    {
        var p1 = new Point2D(1.0, 2.0);
        var p2 = new Point2D(1.00001, 2.00001);
        Assert.True(p1.IsEqualTo(p2, 0.001));
    }

    [Fact]
    public void IsEqualTo_OutsideTolerance_ReturnsFalse()
    {
        var p1 = new Point2D(1.0, 2.0);
        var p2 = new Point2D(1.1, 2.1);
        Assert.False(p1.IsEqualTo(p2, 0.001));
    }

    [Fact]
    public void Equals_SameValues_ReturnsTrue()
    {
        var p1 = new Point2D(1.0, 2.0);
        var p2 = new Point2D(1.0, 2.0);
        Assert.True(p1.Equals(p2));
        Assert.True(p1 == p2);
        Assert.False(p1 != p2);
    }

    [Fact]
    public void GetHashCode_SameValues_SameHash()
    {
        var p1 = new Point2D(1.0, 2.0);
        var p2 = new Point2D(1.0, 2.0);
        Assert.Equal(p1.GetHashCode(), p2.GetHashCode());
    }

    [Fact]
    public void IsBetween_PointOnSegment_ReturnsTrue()
    {
        var p1 = new Point2D(0, 0);
        var p2 = new Point2D(10, 0);
        var point = new Point2D(5, 0);

        Assert.True(point.IsBetween(p1, p2));
    }

    [Fact]
    public void IsBetween_PointAtEndpoint_ReturnsTrue()
    {
        var p1 = new Point2D(0, 0);
        var p2 = new Point2D(10, 0);
        var point = new Point2D(0, 0);

        Assert.True(point.IsBetween(p1, p2));
    }

    [Fact]
    public void IsBetween_PointOutsideSegment_ReturnsFalse()
    {
        var p1 = new Point2D(0, 0);
        var p2 = new Point2D(10, 0);
        var point = new Point2D(15, 0);

        Assert.False(point.IsBetween(p1, p2));
    }

    [Fact]
    public void IsBetween_PointNotCollinear_ReturnsFalse()
    {
        var p1 = new Point2D(0, 0);
        var p2 = new Point2D(10, 0);
        var point = new Point2D(5, 5);

        Assert.False(point.IsBetween(p1, p2));
    }

    [Fact]
    public void RotateAround_90DegreesAroundOrigin_RotatesCorrectly()
    {
        var point = new Point2D(1, 0);
        var rotated = point.RotateAround(Math.PI / 2);

        Assert.Equal(0, rotated.X, 10);
        Assert.Equal(1, rotated.Y, 10);
    }

    [Fact]
    public void RotateAround_90DegreesAroundCustomCenter_RotatesCorrectly()
    {
        var point = new Point2D(2, 1);
        var center = new Point2D(1, 1);
        var rotated = point.RotateAround(Math.PI / 2, center);

        Assert.Equal(1, rotated.X, 10);
        Assert.Equal(2, rotated.Y, 10);
    }

    [Fact]
    public void ToPoint3D_TransformsCorrectly()
    {
        var point2d = new Point2D(2, 3);
        var origin = new Point3D(1, 1, 1);
        var xAxis = new Vector3D(1, 0, 0);
        var yAxis = new Vector3D(0, 1, 0);

        var point3d = point2d.ToPoint3D(origin, xAxis, yAxis);

        Assert.Equal(3, point3d.X); // 1 + 2*1
        Assert.Equal(4, point3d.Y); // 1 + 3*1
        Assert.Equal(1, point3d.Z);
    }

    [Fact]
    public void GetDirectionalDistance_PositiveDirection_ReturnsPositiveDistance()
    {
        var from = new Point2D(0, 0);
        var to = new Point2D(3, 4);
        var direction = new Vector2D(3, 4); // Same direction

        var distance = from.GetDirectionalDistance(to, direction);

        Assert.Equal(5, distance, 10); // Distance is 5
    }

    [Fact]
    public void GetDirectionalDistance_OppositeDirection_ReturnsNegativeDistance()
    {
        var from = new Point2D(3, 4);
        var to = new Point2D(0, 0);
        var direction = new Vector2D(3, 4);

        var distance = from.GetDirectionalDistance(to, direction);

        Assert.Equal(-5, distance, 10);
    }
}