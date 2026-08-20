using BasAutomation.Geometry.Curves;
using BasAutomation.Geometry.Surfaces.SweptSurfaces;

namespace BasAutomation.Geometry.Tests.SweptSurfaces;

public class TorusFromArcSweepTests
{
    private static Arc3D Centerline() => new(
        center: new Point3D(0, 0, 0),
        normal: new Vector3D(0, 0, 1),
        xAxis: new Vector3D(1, 0, 0),
        radius: 500,
        startAngle: 0,
        endAngle: Math.PI / 2,
        isCounterClockwise: true);

    [Fact]
    public void Compute_FromArc_MajorEqualsRadialDistance_MinorEqualsProfileRadius()
    {
        var centerline = Centerline();
        // Profile arc centered at (500,0,30) with radius 25 — radial distance to centerline is 500
        var profile = new Arc3D(
            center: new Point3D(500, 0, 30),
            normal: new Vector3D(0, 1, 0),
            xAxis: new Vector3D(1, 0, 0),
            radius: 25,
            startAngle: 0,
            endAngle: Math.PI,
            isCounterClockwise: true);

        var (center, axis, major, minor) = TorusFromArcSweep.Compute(profile, centerline);

        Assert.Equal(new Point3D(0, 0, 30), center);
        Assert.Equal(centerline.Normal, axis);
        Assert.Equal(500, major, 9);
        Assert.Equal(25, minor, 9);
    }

    [Fact]
    public void Compute_FromCircle_MatchesArcOverload()
    {
        var centerline = Centerline();
        var circleCenter = new Point3D(450, 0, 10);
        var arc = new Arc3D(
            center: circleCenter,
            normal: new Vector3D(0, 1, 0),
            xAxis: new Vector3D(1, 0, 0),
            radius: 17,
            startAngle: 0,
            endAngle: Math.PI,
            isCounterClockwise: true);
        var circle = new Circle3D(circleCenter, 17, new Vector3D(0, 1, 0));

        var fromArc = TorusFromArcSweep.Compute(arc, centerline);
        var fromCircle = TorusFromArcSweep.Compute(circle, centerline);

        Assert.Equal(fromArc, fromCircle);
    }

    [Fact]
    public void Compute_RotatedFrame_TorusCenterOnArcAxis()
    {
        var centerline = new Arc3D(
            center: new Point3D(0, 0, 0),
            normal: new Vector3D(1, 0, 0),
            xAxis: new Vector3D(0, 1, 0),
            radius: 500,
            startAngle: 0,
            endAngle: Math.PI / 2,
            isCounterClockwise: true);

        // Profile circle in YZ plane, offset 42 along the arc normal (X)
        var profile = new Circle3D(new Point3D(42, 500, 0), 30, new Vector3D(0, 0, 1));

        var (center, _, major, minor) = TorusFromArcSweep.Compute(profile, centerline);

        Assert.Equal(new Point3D(42, 0, 0), center);
        Assert.Equal(500, major, 9);
        Assert.Equal(30, minor, 9);
    }
}
