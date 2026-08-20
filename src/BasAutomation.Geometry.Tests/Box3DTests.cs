namespace BasAutomation.Geometry.Tests;

public class Box3DTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var min = new Point3D(0.0, 0.0, 0.0);
        var max = new Point3D(5.0, 10.0, 15.0);
        var box = new Box3D(min, max);
        Assert.Equal(min, box.Min);
        Assert.Equal(max, box.Max);
    }

    [Fact]
    public void Width_CalculatesCorrectly()
    {
        var box = new Box3D(new Point3D(1.0, 2.0, 3.0), new Point3D(6.0, 8.0, 10.0));
        Assert.Equal(5.0, box.Width);
    }

    [Fact]
    public void Height_CalculatesCorrectly()
    {
        var box = new Box3D(new Point3D(1.0, 2.0, 3.0), new Point3D(6.0, 8.0, 10.0));
        Assert.Equal(6.0, box.Height);
    }

    [Fact]
    public void Depth_CalculatesCorrectly()
    {
        var box = new Box3D(new Point3D(1.0, 2.0, 3.0), new Point3D(6.0, 8.0, 10.0));
        Assert.Equal(7.0, box.Depth);
    }

    [Fact]
    public void Center_CalculatesCorrectly()
    {
        var box = new Box3D(new Point3D(0.0, 0.0, 0.0), new Point3D(4.0, 6.0, 8.0));
        var center = box.Center;
        Assert.Equal(2.0, center.X);
        Assert.Equal(3.0, center.Y);
        Assert.Equal(4.0, center.Z);
    }

    [Fact]
    public void Volume_CalculatesCorrectly()
    {
        var box = new Box3D(new Point3D(0.0, 0.0, 0.0), new Point3D(4.0, 5.0, 6.0));
        Assert.Equal(120.0, box.Volume);
    }

    [Fact]
    public void Contains_PointInside_ReturnsTrue()
    {
        var box = new Box3D(new Point3D(0.0, 0.0, 0.0), new Point3D(5.0, 5.0, 5.0));
        Assert.True(box.Contains(new Point3D(2.5, 2.5, 2.5)));
    }

    [Fact]
    public void Contains_PointOnBoundary_ReturnsTrue()
    {
        var box = new Box3D(new Point3D(0.0, 0.0, 0.0), new Point3D(5.0, 5.0, 5.0));
        Assert.True(box.Contains(new Point3D(0.0, 0.0, 0.0)));
        Assert.True(box.Contains(new Point3D(5.0, 5.0, 5.0)));
    }

    [Fact]
    public void Contains_PointOutside_ReturnsFalse()
    {
        var box = new Box3D(new Point3D(0.0, 0.0, 0.0), new Point3D(5.0, 5.0, 5.0));
        Assert.False(box.Contains(new Point3D(-1.0, 2.5, 2.5)));
        Assert.False(box.Contains(new Point3D(6.0, 2.5, 2.5)));
    }

    [Fact]
    public void Intersects_Overlapping_ReturnsTrue()
    {
        var box1 = new Box3D(new Point3D(0.0, 0.0, 0.0), new Point3D(5.0, 5.0, 5.0));
        var box2 = new Box3D(new Point3D(2.0, 2.0, 2.0), new Point3D(7.0, 7.0, 7.0));
        Assert.True(box1.Intersects(box2));
    }

    [Fact]
    public void Intersects_NonOverlapping_ReturnsFalse()
    {
        var box1 = new Box3D(new Point3D(0.0, 0.0, 0.0), new Point3D(2.0, 2.0, 2.0));
        var box2 = new Box3D(new Point3D(3.0, 3.0, 3.0), new Point3D(5.0, 5.0, 5.0));
        Assert.False(box1.Intersects(box2));
    }

    [Fact]
    public void Union_CalculatesCorrectly()
    {
        var box1 = new Box3D(new Point3D(0.0, 0.0, 0.0), new Point3D(2.0, 2.0, 2.0));
        var box2 = new Box3D(new Point3D(1.0, 1.0, 1.0), new Point3D(3.0, 3.0, 3.0));
        var union = box1.Union(box2);
        Assert.Equal(0.0, union.Min.X);
        Assert.Equal(0.0, union.Min.Y);
        Assert.Equal(0.0, union.Min.Z);
        Assert.Equal(3.0, union.Max.X);
        Assert.Equal(3.0, union.Max.Y);
        Assert.Equal(3.0, union.Max.Z);
    }
}