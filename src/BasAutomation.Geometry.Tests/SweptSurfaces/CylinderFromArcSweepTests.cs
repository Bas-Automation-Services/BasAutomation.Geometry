using BasAutomation.Geometry.Curves;
using BasAutomation.Geometry.Surfaces.SweptSurfaces;

namespace BasAutomation.Geometry.Tests.SweptSurfaces;

public class CylinderFromArcSweepTests
{
    [Fact]
    public void Compute_TangentialLineAtArcRadius_ReturnsArcCenterAxisAndRadius()
    {
        var arc = new Arc3D(
            center: new Point3D(0, 0, 0),
            normal: new Vector3D(0, 0, 1),
            xAxis: new Vector3D(1, 0, 0),
            radius: 500,
            startAngle: 0,
            endAngle: Math.PI / 2,
            isCounterClockwise: true);

        // Line tangent to arc-axis at start, height 0..100
        var line = new Line3D(new Point3D(500, 0, 0), new Point3D(500, 0, 100));

        var (center, axis, radius) = CylinderFromArcSweep.Compute(line, arc);

        // Cylinder is centered on the arc axis at the line's midpoint height
        Assert.Equal(new Point3D(0, 0, 50), center);
        Assert.Equal(arc.Normal, axis);
        Assert.Equal(500, radius, 9);
    }

    [Fact]
    public void Compute_TangentialLineInsideCenterline_ReturnsSmallerRadius()
    {
        var arc = new Arc3D(
            center: new Point3D(0, 0, 0),
            normal: new Vector3D(0, 0, 1),
            xAxis: new Vector3D(1, 0, 0),
            radius: 500,
            startAngle: 0,
            endAngle: Math.PI / 2,
            isCounterClockwise: true);

        var line = new Line3D(new Point3D(450, 0, 0), new Point3D(450, 0, 100));

        var (_, _, radius) = CylinderFromArcSweep.Compute(line, arc);

        Assert.Equal(450, radius, 9);
    }

    [Fact]
    public void Compute_RotatedFrame_AxisIsArcNormal()
    {
        var arc = new Arc3D(
            center: new Point3D(10, 20, 30),
            normal: new Vector3D(1, 0, 0),
            xAxis: new Vector3D(0, 1, 0),
            radius: 500,
            startAngle: 0,
            endAngle: Math.PI / 2,
            isCounterClockwise: true);

        // Tangential line in the YZ plane, parallel to arc normal (X)
        var line = new Line3D(new Point3D(0, 520, 30), new Point3D(100, 520, 30));

        var (center, axis, radius) = CylinderFromArcSweep.Compute(line, arc);

        Assert.Equal(arc.Normal, axis);
        Assert.Equal(500, radius, 9);
        // Cylinder center: arc center moved along arc normal by line-midpoint height
        Assert.Equal(new Point3D(50, 20, 30), center);
    }
}
