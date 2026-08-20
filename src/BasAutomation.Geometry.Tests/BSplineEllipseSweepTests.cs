using BasAutomation.Geometry.Curves;
using BasAutomation.Geometry.Surfaces;

namespace BasAutomation.Geometry.Tests;

public class BSplineEllipseSweepTests
{
    private const double Tolerance = 1e-10;

    /// <summary>
    /// Creates a standard test case: ellipse at (500,0,0) swept along a 90-degree arc of radius 500.
    /// </summary>
    private static (Ellipse3D ellipse, Arc3D arc) CreateTestCase90()
    {
        var arc = new Arc3D(
            center: new Point3D(0, 0, 0),
            normal: new Vector3D(0, 0, 1),
            xAxis: new Vector3D(1, 0, 0),
            radius: 500,
            startAngle: 0,
            endAngle: Math.PI / 2,
            isCounterClockwise: true);

        // Ellipse centered at arc start point, in the plane perpendicular to the arc tangent
        var ellipse = new Ellipse3D(
            center: arc.StartPoint,
            semiMajorAxis: 60,
            semiMinorAxis: 30,
            normal: arc.TangentAt(0),
            majorAxisDirection: new Vector3D(0, 0, 1));

        return (ellipse, arc);
    }

    [Fact]
    public void ComputeSurface_Returns_Order3InBothDirections()
    {
        var (ellipse, arc) = CreateTestCase90();

        var (order, _, _, _, _, _) = BSplineEllipseSweep.ComputeSurface(ellipse, arc);

        Assert.Equal(2, order.Length);
        Assert.Equal(3, order[0]); // degree 2 + 1
        Assert.Equal(3, order[1]);
    }

    [Fact]
    public void ComputeSurface_Returns_NonPeriodic()
    {
        var (ellipse, arc) = CreateTestCase90();

        var (_, _, _, _, _, isPeriodic) = BSplineEllipseSweep.ComputeSurface(ellipse, arc);

        Assert.Equal(2, isPeriodic.Length);
        Assert.False(isPeriodic[0]);
        Assert.False(isPeriodic[1]);
    }

    [Fact]
    public void ComputeSurface_90DegreeArc_Has9x3ControlPoints()
    {
        var (ellipse, arc) = CreateTestCase90();

        var (_, poles, _, _, weights, _) = BSplineEllipseSweep.ComputeSurface(ellipse, arc);

        // 9 ellipse CPs * (2*1+1)=3 arc CPs (1 segment for 90 degrees)
        var numU = 9;
        var numV = 3;
        Assert.Equal(numU * numV * 3, poles.Length);
        Assert.Equal(numU * numV, weights.Length);
    }

    [Fact]
    public void ComputeSurface_180DegreeArc_Has9x5ControlPoints()
    {
        var arc = new Arc3D(
            center: new Point3D(0, 0, 0),
            normal: new Vector3D(0, 0, 1),
            xAxis: new Vector3D(1, 0, 0),
            radius: 500,
            startAngle: 0,
            endAngle: Math.PI,
            isCounterClockwise: true);

        var ellipse = new Ellipse3D(
            center: arc.StartPoint,
            semiMajorAxis: 60,
            semiMinorAxis: 30,
            normal: arc.TangentAt(0),
            majorAxisDirection: new Vector3D(0, 0, 1));

        var (_, poles, _, _, weights, _) = BSplineEllipseSweep.ComputeSurface(ellipse, arc);

        // 9 ellipse CPs * (2*2+1)=5 arc CPs (2 segments for 180 degrees)
        Assert.Equal(9 * 5 * 3, poles.Length);
        Assert.Equal(9 * 5, weights.Length);
    }

    [Fact]
    public void ComputeSurface_KnotsU_HasCorrectStructureFor4Spans()
    {
        var (ellipse, arc) = CreateTestCase90();

        var (_, _, knotsU, _, _, _) = BSplineEllipseSweep.ComputeSurface(ellipse, arc);

        // 4 spans, order 3: knot count = 2*4+4 = 12
        Assert.Equal(12, knotsU.Length);

        // Clamped: first 3 are 0, last 3 are 4
        Assert.Equal(0, knotsU[0]);
        Assert.Equal(0, knotsU[1]);
        Assert.Equal(0, knotsU[2]);
        Assert.Equal(4, knotsU[9]);
        Assert.Equal(4, knotsU[10]);
        Assert.Equal(4, knotsU[11]);

        // Internal double knots at 1, 2, 3
        Assert.Equal(1, knotsU[3]);
        Assert.Equal(1, knotsU[4]);
        Assert.Equal(2, knotsU[5]);
        Assert.Equal(2, knotsU[6]);
        Assert.Equal(3, knotsU[7]);
        Assert.Equal(3, knotsU[8]);
    }

    [Fact]
    public void ComputeSurface_KnotsV_90Degree_HasCorrectStructureFor1Span()
    {
        var (ellipse, arc) = CreateTestCase90();

        var (_, _, _, knotsV, _, _) = BSplineEllipseSweep.ComputeSurface(ellipse, arc);

        // 1 span, order 3: knot count = 2*1+4 = 6
        Assert.Equal(6, knotsV.Length);

        // Clamped: [0, 0, 0, 1, 1, 1]
        Assert.Equal(0, knotsV[0]);
        Assert.Equal(0, knotsV[1]);
        Assert.Equal(0, knotsV[2]);
        Assert.Equal(1, knotsV[3]);
        Assert.Equal(1, knotsV[4]);
        Assert.Equal(1, knotsV[5]);
    }

    [Fact]
    public void ComputeSurface_Weights_AreProductOfEllipseAndArcWeights()
    {
        var (ellipse, arc) = CreateTestCase90();

        var (_, _, _, _, weights, _) = BSplineEllipseSweep.ComputeSurface(ellipse, arc);

        var w = 1.0 / Math.Sqrt(2.0);
        var arcMidWeight = Math.Cos(Math.PI / 4); // cos(45°) = 1/sqrt(2)

        // V=0 row (on-arc, arc weight=1): ellipse weights directly
        Assert.Equal(1.0, weights[0], Tolerance); // U=0: 1*1
        Assert.Equal(w, weights[1], Tolerance);    // U=1: w*1
        Assert.Equal(1.0, weights[2], Tolerance);  // U=2: 1*1
        Assert.Equal(w, weights[3], Tolerance);    // U=3: w*1

        // V=1 row (tangent-intersection, arc weight=cos(45°)=w): ellipse_w * arc_w
        Assert.Equal(1.0 * arcMidWeight, weights[9 + 0], Tolerance);  // U=0: 1*w
        Assert.Equal(w * arcMidWeight, weights[9 + 1], Tolerance);     // U=1: w*w
        Assert.Equal(1.0 * arcMidWeight, weights[9 + 2], Tolerance);  // U=2: 1*w

        // V=2 row (on-arc end, arc weight=1): ellipse weights directly
        Assert.Equal(1.0, weights[18 + 0], Tolerance);
        Assert.Equal(w, weights[18 + 1], Tolerance);
    }

    [Fact]
    public void ComputeSurface_AllWeightsArePositive()
    {
        var (ellipse, arc) = CreateTestCase90();

        var (_, _, _, _, weights, _) = BSplineEllipseSweep.ComputeSurface(ellipse, arc);

        foreach (var w in weights)
        {
            Assert.True(w > 0, $"Weight {w} should be positive");
        }
    }

    [Fact]
    public void ComputeSurface_OnArcPoles_MatchEllipsePointAtPositions()
    {
        var (ellipse, arc) = CreateTestCase90();

        var (_, poles, _, _, _, _) = BSplineEllipseSweep.ComputeSurface(ellipse, arc);

        var numU = 9;

        // V=0 row, U=0: should be ellipse point at t=0 at arc start
        // Ellipse.PointAt(0) = Center + SemiMajorAxis * MajorAxisDirection
        var expectedStart = ellipse.Center + ellipse.SemiMajorAxis * ellipse.MajorAxisDirection;
        Assert.Equal(expectedStart.X, poles[0], Tolerance);
        Assert.Equal(expectedStart.Y, poles[1], Tolerance);
        Assert.Equal(expectedStart.Z, poles[2], Tolerance);

        // V=0 row, U=2: should be ellipse point at t=0.25 (top of ellipse)
        // = Center + SemiMinorAxis * MinorAxisDirection
        var expectedTop = ellipse.Center + ellipse.SemiMinorAxis * ellipse.MinorAxisDirection;
        var idx2 = 2; // U=2 in V=0 row
        Assert.Equal(expectedTop.X, poles[idx2 * 3 + 0], Tolerance);
        Assert.Equal(expectedTop.Y, poles[idx2 * 3 + 1], Tolerance);
        Assert.Equal(expectedTop.Z, poles[idx2 * 3 + 2], Tolerance);

        // V=0 row, U=4: should be at (-SemiMajorAxis, 0) = Center - SemiMajorAxis * MajorAxisDirection
        var expectedLeft = ellipse.Center - ellipse.SemiMajorAxis * ellipse.MajorAxisDirection;
        var idx4 = 4;
        Assert.Equal(expectedLeft.X, poles[idx4 * 3 + 0], Tolerance);
        Assert.Equal(expectedLeft.Y, poles[idx4 * 3 + 1], Tolerance);
        Assert.Equal(expectedLeft.Z, poles[idx4 * 3 + 2], Tolerance);

        // V=2 (end of arc) row, U=0: should be the same ellipse point swept to arc end
        // The concentric arc from the start point should end at the arc-end position
        var startPoint = expectedStart;
        var concentricArc = arc.CreateConcentricArc(startPoint);
        var expectedEnd = concentricArc.PointAt(1.0);
        var endRowOffset = 2 * numU; // V=2 row starts at index 2*numU
        Assert.Equal(expectedEnd.X, poles[endRowOffset * 3 + 0], Tolerance);
        Assert.Equal(expectedEnd.Y, poles[endRowOffset * 3 + 1], Tolerance);
        Assert.Equal(expectedEnd.Z, poles[endRowOffset * 3 + 2], Tolerance);
    }

    [Fact]
    public void ComputeSurface_FirstAndLastUPoles_AreCoincident_ClosingEllipse()
    {
        var (ellipse, arc) = CreateTestCase90();

        var (_, poles, _, _, _, _) = BSplineEllipseSweep.ComputeSurface(ellipse, arc);

        var numU = 9;
        var numV = 3;

        // For each V row, U=0 and U=8 should be the same point (closure)
        for (var j = 0; j < numV; j++)
        {
            var idx0 = (j * numU + 0) * 3;
            var idx8 = (j * numU + 8) * 3;

            Assert.Equal(poles[idx0 + 0], poles[idx8 + 0], Tolerance);
            Assert.Equal(poles[idx0 + 1], poles[idx8 + 1], Tolerance);
            Assert.Equal(poles[idx0 + 2], poles[idx8 + 2], Tolerance);
        }
    }

    [Fact]
    public void ComputeSurface_PolesAreInInputUnits_NotConverted()
    {
        var (ellipse, arc) = CreateTestCase90();

        var (_, poles, _, _, _, _) = BSplineEllipseSweep.ComputeSurface(ellipse, arc);

        // V=0, U=0: ellipse point at (Center + SemiMajorAxis * MajorAxisDirection)
        // Arc center is at (0,0,0), arc start is at (500,0,0), ellipse center is at (500,0,0)
        // MajorAxisDirection is (0,0,1), so this point should be at (500, 0, 60)
        var expectedX = 500.0;
        var expectedZ = 60.0;
        Assert.Equal(expectedX, poles[0], Tolerance);
        Assert.Equal(expectedZ, poles[2], Tolerance);
    }

    [Fact]
    public void ComputeSurface_SmallArc_30Degrees_StillProduces1Segment()
    {
        var arc = new Arc3D(
            center: new Point3D(0, 0, 0),
            normal: new Vector3D(0, 0, 1),
            xAxis: new Vector3D(1, 0, 0),
            radius: 500,
            startAngle: 0,
            endAngle: Math.PI / 6, // 30 degrees
            isCounterClockwise: true);

        var ellipse = new Ellipse3D(
            center: arc.StartPoint,
            semiMajorAxis: 60,
            semiMinorAxis: 30,
            normal: arc.TangentAt(0),
            majorAxisDirection: new Vector3D(0, 0, 1));

        var (_, poles, _, knotsV, weights, _) = BSplineEllipseSweep.ComputeSurface(ellipse, arc);

        // 1 segment: 3 V control points
        Assert.Equal(9 * 3 * 3, poles.Length);
        Assert.Equal(6, knotsV.Length); // [0,0,0,1,1,1]
    }

    [Fact]
    public void ComputeSurface_270DegreeArc_Has3Segments()
    {
        var arc = new Arc3D(
            center: new Point3D(0, 0, 0),
            normal: new Vector3D(0, 0, 1),
            xAxis: new Vector3D(1, 0, 0),
            radius: 500,
            startAngle: 0,
            endAngle: 3 * Math.PI / 2,
            isCounterClockwise: true);

        var ellipse = new Ellipse3D(
            center: arc.StartPoint,
            semiMajorAxis: 60,
            semiMinorAxis: 30,
            normal: arc.TangentAt(0),
            majorAxisDirection: new Vector3D(0, 0, 1));

        var (_, poles, _, knotsV, weights, _) = BSplineEllipseSweep.ComputeSurface(ellipse, arc);

        // 3 segments: 7 V control points
        var numV = 7;
        Assert.Equal(9 * numV * 3, poles.Length);
        Assert.Equal(9 * numV, weights.Length);

        // KnotsV: [0,0,0, 1,1, 2,2, 3,3,3] = 10 knots
        Assert.Equal(10, knotsV.Length);
    }

    [Fact]
    public void ComputeSurface_ClockwiseArc_ProducesValidOutput()
    {
        var arc = new Arc3D(
            center: new Point3D(0, 0, 0),
            normal: new Vector3D(0, 0, 1),
            xAxis: new Vector3D(1, 0, 0),
            radius: 500,
            startAngle: Math.PI / 2,
            endAngle: 0,
            isCounterClockwise: false);

        var ellipse = new Ellipse3D(
            center: arc.StartPoint,
            semiMajorAxis: 60,
            semiMinorAxis: 30,
            normal: arc.TangentAt(0),
            majorAxisDirection: new Vector3D(0, 0, 1));

        var (order, poles, knotsU, knotsV, weights, isPeriodic) =
            BSplineEllipseSweep.ComputeSurface(ellipse, arc);

        // Should still produce valid output
        Assert.Equal(9 * 3 * 3, poles.Length);
        Assert.All(weights, w => Assert.True(w > 0));
    }

    [Fact]
    public void ComputeSurface_CircularEllipse_MatchesExpectedSymmetry()
    {
        // When semi-major == semi-minor, the ellipse is a circle
        // The result should be equivalent to a torus
        var arc = new Arc3D(
            center: new Point3D(0, 0, 0),
            normal: new Vector3D(0, 0, 1),
            xAxis: new Vector3D(1, 0, 0),
            radius: 500,
            startAngle: 0,
            endAngle: Math.PI / 2,
            isCounterClockwise: true);

        var circularEllipse = new Ellipse3D(
            center: arc.StartPoint,
            semiMajorAxis: 40,
            semiMinorAxis: 40,
            normal: arc.TangentAt(0),
            majorAxisDirection: new Vector3D(0, 0, 1));

        var (_, poles, _, _, weights, _) = BSplineEllipseSweep.ComputeSurface(circularEllipse, arc);

        // For a circular cross-section, U=0 and U=2 should be equidistant from center
        // (both at radius 40 from the ellipse center)
        var p0 = new Point3D(poles[0], poles[1], poles[2]);
        var p2 = new Point3D(poles[2 * 3], poles[2 * 3 + 1], poles[2 * 3 + 2]);
        var dist0 = (p0 - circularEllipse.Center).Length;
        var dist2 = (p2 - circularEllipse.Center).Length;

        Assert.Equal(40.0, dist0, 1e-6);
        Assert.Equal(40.0, dist2, 1e-6);
    }
}
