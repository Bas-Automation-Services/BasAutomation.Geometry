namespace BasAutomation.Geometry.Tests;

public class Point3DTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var p = new Point3D(3.0, 4.0, 5.0);
        Assert.Equal(3.0, p.X);
        Assert.Equal(4.0, p.Y);
        Assert.Equal(5.0, p.Z);
    }

    [Fact]
    public void AddVector_ReturnsCorrectPoint()
    {
        var p = new Point3D(1.0, 2.0, 3.0);
        var v = new Vector3D(3.0, 4.0, 5.0);
        var result = p + v;
        Assert.Equal(4.0, result.X);
        Assert.Equal(6.0, result.Y);
        Assert.Equal(8.0, result.Z);
    }

    [Fact]
    public void SubtractVector_ReturnsCorrectPoint()
    {
        var p = new Point3D(5.0, 7.0, 9.0);
        var v = new Vector3D(2.0, 3.0, 4.0);
        var result = p - v;
        Assert.Equal(3.0, result.X);
        Assert.Equal(4.0, result.Y);
        Assert.Equal(5.0, result.Z);
    }

    [Fact]
    public void SubtractPoint_ReturnsCorrectVector()
    {
        var p1 = new Point3D(5.0, 7.0, 9.0);
        var p2 = new Point3D(2.0, 3.0, 4.0);
        var result = p1 - p2;
        Assert.Equal(3.0, result.X);
        Assert.Equal(4.0, result.Y);
        Assert.Equal(5.0, result.Z);
    }

    [Fact]
    public void DistanceTo_CalculatesCorrectly()
    {
        var p1 = new Point3D(0.0, 0.0, 0.0);
        var p2 = new Point3D(2.0, 3.0, 6.0);
        Assert.Equal(7.0, p1.DistanceTo(p2));
    }

    [Fact]
    public void IsEqualTo_WithinTolerance_ReturnsTrue()
    {
        var p1 = new Point3D(1.0, 2.0, 3.0);
        var p2 = new Point3D(1.00001, 2.00001, 3.00001);
        Assert.True(p1.IsEqualTo(p2, 0.001));
    }

    [Fact]
    public void IsBetween_PointOnSegment_ReturnsTrue()
    {
        var p1 = new Point3D(0, 0, 0);
        var p2 = new Point3D(10, 0, 0);
        var point = new Point3D(5, 0, 0);

        Assert.True(point.IsBetween(p1, p2));
    }

    [Fact]
    public void IsBetween_PointOnDiagonalSegment_ReturnsTrue()
    {
        var p1 = new Point3D(0, 0, 0);
        var p2 = new Point3D(2, 2, 2);
        var point = new Point3D(1, 1, 1);

        Assert.True(point.IsBetween(p1, p2));
    }

    [Fact]
    public void IsBetween_PointOutsideSegment_ReturnsFalse()
    {
        var p1 = new Point3D(0, 0, 0);
        var p2 = new Point3D(10, 0, 0);
        var point = new Point3D(15, 0, 0);

        Assert.False(point.IsBetween(p1, p2));
    }

    [Fact]
    public void Transform_WithIdentityMatrix_ReturnsOriginalPoint()
    {
        var point = new Point3D(1, 2, 3);
        var transformed = point.Transform(Matrix3D.Identity);

        Assert.Equal(point, transformed);
    }

    [Fact]
    public void Transform_WithTranslationMatrix_TranslatesCorrectly()
    {
        var point = new Point3D(1, 2, 3);
        var matrix = Matrix3D.Translation(10, 20, 30);
        var transformed = point.Transform(matrix);

        Assert.Equal(11, transformed.X);
        Assert.Equal(22, transformed.Y);
        Assert.Equal(33, transformed.Z);
    }

    [Fact]
    public void GetSignedDistanceAlongDirection_PositiveDirection_ReturnsPositiveDistance()
    {
        var from = new Point3D(0, 0, 0);
        var to = new Point3D(3, 4, 0);
        var direction = new Vector3D(1, 0, 0);

        var distance = from.GetSignedDistanceAlongDirection(to, direction);

        Assert.Equal(3, distance); // Only X component
    }

    [Fact]
    public void GetNearestPoint_ReturnsClosestPoint()
    {
        var point = new Point3D(0, 0, 0);
        var points = new List<Point3D>
        {
            new(10, 0, 0),
            new(0, 5, 0),
            new(0, 0, 3)
        };

        var nearest = point.GetNearestPoint(points);

        Assert.Equal(new Point3D(0, 0, 3), nearest);
    }

    [Fact]
    public void GetNearestPoint_EmptyList_ReturnsNull()
    {
        var point = new Point3D(0, 0, 0);
        var points = new List<Point3D>();

        var nearest = point.GetNearestPoint(points);

        Assert.Null(nearest);
    }

    [Fact]
    public void GetFurthestPoint_ReturnsFurthestPoint()
    {
        var point = new Point3D(0, 0, 0);
        var points = new List<Point3D>
        {
            new(10, 0, 0),
            new(0, 5, 0),
            new(0, 0, 3)
        };

        var furthest = point.GetFurthestPoint(points);

        Assert.Equal(new Point3D(10, 0, 0), furthest);
    }
}