namespace BasAutomation.Geometry.Tests;

public class Box2DTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var min = new Point2D(0.0, 0.0);
        var max = new Point2D(5.0, 10.0);
        var box = new Box2D(min, max);
        Assert.Equal(min, box.Min);
        Assert.Equal(max, box.Max);
    }

    [Fact]
    public void Width_CalculatesCorrectly()
    {
        var box = new Box2D(new Point2D(1.0, 2.0), new Point2D(6.0, 8.0));
        Assert.Equal(5.0, box.Width);
    }

    [Fact]
    public void Height_CalculatesCorrectly()
    {
        var box = new Box2D(new Point2D(1.0, 2.0), new Point2D(6.0, 8.0));
        Assert.Equal(6.0, box.Height);
    }

    [Fact]
    public void Center_CalculatesCorrectly()
    {
        var box = new Box2D(new Point2D(0.0, 0.0), new Point2D(4.0, 6.0));
        var center = box.Center;
        Assert.Equal(2.0, center.X);
        Assert.Equal(3.0, center.Y);
    }

    [Fact]
    public void Area_CalculatesCorrectly()
    {
        var box = new Box2D(new Point2D(0.0, 0.0), new Point2D(4.0, 5.0));
        Assert.Equal(20.0, box.Area);
    }

    [Fact]
    public void Contains_PointInside_ReturnsTrue()
    {
        var box = new Box2D(new Point2D(0.0, 0.0), new Point2D(5.0, 5.0));
        Assert.True(box.Contains(new Point2D(2.5, 2.5)));
    }

    [Fact]
    public void Contains_PointOnBoundary_ReturnsTrue()
    {
        var box = new Box2D(new Point2D(0.0, 0.0), new Point2D(5.0, 5.0));
        Assert.True(box.Contains(new Point2D(0.0, 0.0)));
        Assert.True(box.Contains(new Point2D(5.0, 5.0)));
    }

    [Fact]
    public void Contains_PointOutside_ReturnsFalse()
    {
        var box = new Box2D(new Point2D(0.0, 0.0), new Point2D(5.0, 5.0));
        Assert.False(box.Contains(new Point2D(-1.0, 2.5)));
        Assert.False(box.Contains(new Point2D(6.0, 2.5)));
    }

    [Fact]
    public void Intersects_Overlapping_ReturnsTrue()
    {
        var box1 = new Box2D(new Point2D(0.0, 0.0), new Point2D(5.0, 5.0));
        var box2 = new Box2D(new Point2D(2.0, 2.0), new Point2D(7.0, 7.0));
        Assert.True(box1.Intersects(box2));
    }

    [Fact]
    public void Intersects_NonOverlapping_ReturnsFalse()
    {
        var box1 = new Box2D(new Point2D(0.0, 0.0), new Point2D(2.0, 2.0));
        var box2 = new Box2D(new Point2D(3.0, 3.0), new Point2D(5.0, 5.0));
        Assert.False(box1.Intersects(box2));
    }

    [Fact]
    public void Union_CalculatesCorrectly()
    {
        var box1 = new Box2D(new Point2D(0.0, 0.0), new Point2D(2.0, 2.0));
        var box2 = new Box2D(new Point2D(1.0, 1.0), new Point2D(3.0, 3.0));
        var union = box1.Union(box2);
        Assert.Equal(0.0, union.Min.X);
        Assert.Equal(0.0, union.Min.Y);
        Assert.Equal(3.0, union.Max.X);
        Assert.Equal(3.0, union.Max.Y);
    }
}