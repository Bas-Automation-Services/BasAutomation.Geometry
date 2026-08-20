using BasAutomation.Geometry.Curves;
using BasAutomation.Geometry.Surfaces.SweptSurfaces;

namespace BasAutomation.Geometry.Tests.SweptSurfaces;

public class LineSweptSurfaceClassifierTests
{
    private static Arc3D StandardArc(Vector3D? normal = null) => new(
        center: new Point3D(0, 0, 0),
        normal: normal ?? new Vector3D(0, 0, 1),
        xAxis: new Vector3D(1, 0, 0),
        radius: 500,
        startAngle: 0,
        endAngle: Math.PI / 2,
        isCounterClockwise: true);

    [Fact]
    public void Classify_RadialLineAtConstantHeight_IsRadialPlane()
    {
        var arc = StandardArc();
        // Line at arc start, radial (X-direction), at height 0
        var line = new Line3D(new Point3D(450, 0, 50), new Point3D(550, 0, 50));

        Assert.Equal(LineSweptSurfaceKind.RadialPlane, LineSweptSurfaceClassifier.Classify(line, arc));
        Assert.True(LineSweptSurfaceClassifier.IsRadial(line, arc));
    }

    [Fact]
    public void Classify_TangentialLineAtSameRadius_IsCylinder()
    {
        var arc = StandardArc();
        // Line parallel to arc axis (Z), at radius 500, no radial component
        var line = new Line3D(new Point3D(500, 0, 0), new Point3D(500, 0, 100));

        Assert.Equal(LineSweptSurfaceKind.Cylinder, LineSweptSurfaceClassifier.Classify(line, arc));
        Assert.False(LineSweptSurfaceClassifier.IsRadial(line, arc));
    }

    [Fact]
    public void Classify_TiltedLineWithDifferentRadii_IsCone()
    {
        var arc = StandardArc();
        // Line from (450,0,0) to (550,0,100): radius changes 450→550, height changes 0→100
        var line = new Line3D(new Point3D(450, 0, 0), new Point3D(550, 0, 100));

        Assert.Equal(LineSweptSurfaceKind.Cone, LineSweptSurfaceClassifier.Classify(line, arc));
    }

    [Fact]
    public void GetRadialDistanceAndHeight_PointAtArcRadius_ReturnsRadiusAndHeight()
    {
        var arc = StandardArc();
        var (radial, height) = LineSweptSurfaceClassifier.GetRadialDistanceAndHeight(new Point3D(500, 0, 42), arc);

        Assert.Equal(500, radial, 9);
        Assert.Equal(42, height, 9);
    }

    [Fact]
    public void GetRadialDistanceAndHeight_RotatedFrame_ProjectsAlongActualNormal()
    {
        // Arc whose normal is the X-axis, lying in the YZ plane
        var arc = new Arc3D(
            center: new Point3D(0, 0, 0),
            normal: new Vector3D(1, 0, 0),
            xAxis: new Vector3D(0, 1, 0),
            radius: 500,
            startAngle: 0,
            endAngle: Math.PI / 2,
            isCounterClockwise: true);

        // Point at radius 500 in the YZ plane, offset 42 along X (= arc normal)
        var (radial, height) = LineSweptSurfaceClassifier.GetRadialDistanceAndHeight(new Point3D(42, 0, 500), arc);

        Assert.Equal(500, radial, 9);
        Assert.Equal(42, height, 9);
    }

    [Fact]
    public void IsRadial_RadialDirectionButSlopedHeights_IsFalse()
    {
        var arc = StandardArc();
        // Line midpoint is radial-ish but endpoints are at different heights → conical, not planar
        var line = new Line3D(new Point3D(450, 0, 0), new Point3D(550, 0, 100));

        Assert.False(LineSweptSurfaceClassifier.IsRadial(line, arc));
    }
}
