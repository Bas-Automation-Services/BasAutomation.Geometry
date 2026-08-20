using BasAutomation.Geometry.Curves;
using BasAutomation.Geometry.Surfaces.SweptSurfaces;

namespace BasAutomation.Geometry.Tests.SweptSurfaces;

public class ConeFromArcSweepTests
{
    private static Arc3D StandardArc() => new(
        center: new Point3D(0, 0, 0),
        normal: new Vector3D(0, 0, 1),
        xAxis: new Vector3D(1, 0, 0),
        radius: 500,
        startAngle: 0,
        endAngle: Math.PI / 2,
        isCounterClockwise: true);

    [Fact]
    public void Compute_AscendingCone_RootAtSmallerRadiusEnd_AxisAlongNormal()
    {
        var arc = StandardArc();
        // Radius grows from 450 to 550 as height grows from 0 to 100
        var line = new Line3D(new Point3D(450, 0, 0), new Point3D(550, 0, 100));

        var (root, axis, rootRadius, halfAngle) = ConeFromArcSweep.Compute(line, arc);

        Assert.Equal(new Point3D(0, 0, 0), root);
        Assert.Equal(arc.Normal, axis);
        Assert.Equal(450, rootRadius, 9);
        Assert.Equal(Math.Atan2(100, 100), halfAngle, 9);
    }

    [Fact]
    public void Compute_DescendingCone_RootAtSmallerRadiusEnd_AxisOpposesNormal()
    {
        var arc = StandardArc();
        // Radius shrinks from 550 to 450 as height grows from 0 to 100
        var line = new Line3D(new Point3D(550, 0, 0), new Point3D(450, 0, 100));

        var (root, axis, rootRadius, _) = ConeFromArcSweep.Compute(line, arc);

        // Smaller-radius end is at height 100; axis points back toward height 0 (= -Normal)
        Assert.Equal(new Point3D(0, 0, 100), root);
        Assert.Equal(-arc.Normal, axis);
        Assert.Equal(450, rootRadius, 9);
    }

    [Fact]
    public void Compute_HalfAngle_45Degrees_WhenRadiusEqualsHeightDelta()
    {
        var arc = StandardArc();
        var line = new Line3D(new Point3D(450, 0, 0), new Point3D(550, 0, 100));

        var (_, _, _, halfAngle) = ConeFromArcSweep.Compute(line, arc);

        Assert.Equal(Math.PI / 4, halfAngle, 9);
    }
}
