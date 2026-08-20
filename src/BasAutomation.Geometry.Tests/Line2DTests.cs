using BasAutomation.Geometry.Curves;

namespace BasAutomation.Geometry.Tests;

public class Line2DTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var start = new Point2D(0.0, 0.0);
        var end = new Point2D(3.0, 4.0);
        var line = new Line2D(start, end);
        Assert.Equal(start, line.StartPoint);
        Assert.Equal(end, line.EndPoint);
    }

    [Fact]
    public void Direction_ReturnsNormalizedVector()
    {
        var line = new Line2D(new Point2D(0.0, 0.0), new Point2D(3.0, 4.0));
        var dir = line.Direction;
        Assert.Equal(1.0, dir.Length, 10);
        Assert.Equal(0.6, dir.X, 10);
        Assert.Equal(0.8, dir.Y, 10);
    }

    [Fact]
    public void Length_CalculatesCorrectly()
    {
        var line = new Line2D(new Point2D(0.0, 0.0), new Point2D(3.0, 4.0));
        Assert.Equal(5.0, line.Length);
    }

    [Fact]
    public void PointAt_CalculatesCorrectly()
    {
        var line = new Line2D(new Point2D(0.0, 0.0), new Point2D(4.0, 4.0));
        var point = line.PointAt(0.5);
        Assert.Equal(2.0, point.X);
        Assert.Equal(2.0, point.Y);
    }

    [Fact]
    public void MidPoint_CalculatesCorrectly()
    {
        var line = new Line2D(new Point2D(0.0, 0.0), new Point2D(4.0, 4.0));
        var mid = line.MidPoint;
        Assert.Equal(2.0, mid.X);
        Assert.Equal(2.0, mid.Y);
    }

    [Fact]
    public void DistanceToPoint_OnLine_ReturnsZero()
    {
        var line = new Line2D(new Point2D(0.0, 0.0), new Point2D(4.0, 0.0));
        var point = new Point2D(2.0, 0.0);
        Assert.Equal(0.0, line.DistanceToPoint(point), 10);
    }

    [Fact]
    public void DistanceToPoint_Perpendicular_CalculatesCorrectly()
    {
        var line = new Line2D(new Point2D(0.0, 0.0), new Point2D(4.0, 0.0));
        var point = new Point2D(2.0, 3.0);
        Assert.Equal(3.0, line.DistanceToPoint(point), 10);
    }

    [Fact]
    public void DistanceToPoint_BeyondEnd_CalculatesCorrectly()
    {
        var line = new Line2D(new Point2D(0.0, 0.0), new Point2D(3.0, 0.0));
        var point = new Point2D(5.0, 0.0);
        Assert.Equal(2.0, line.DistanceToPoint(point), 10);
    }
}