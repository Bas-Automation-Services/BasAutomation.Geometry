using BasAutomation.Geometry.Curves;

namespace BasAutomation.Geometry.Tests;

public class Path3DTests
{
    private const double Tolerance = 1e-6;

    #region Constructor Tests

    [Fact]
    public void Constructor_EmptySegments_CreatesEmptyPath()
    {
        var path = new Path3D(Array.Empty<ICurve3D>());

        Assert.Equal(0, path.SegmentCount);
        Assert.Equal(0, path.Length);
        Assert.False(path.IsLoop);
    }

    [Fact]
    public void Constructor_SingleSegment_SetsProperties()
    {
        var line = new Line3D(new Point3D(0, 0, 0), new Point3D(100, 0, 0));
        var path = new Path3D(new ICurve3D[] { line });

        Assert.Equal(1, path.SegmentCount);
        Assert.Equal(100, path.Length, 6);
        Assert.False(path.IsLoop);
        Assert.Equal(new Point3D(0, 0, 0), path.StartPoint);
        Assert.Equal(new Point3D(100, 0, 0), path.EndPoint);
    }

    [Fact]
    public void Constructor_MultipleConnectedSegments_CalculatesLength()
    {
        var line1 = new Line3D(new Point3D(0, 0, 0), new Point3D(100, 0, 0));
        var line2 = new Line3D(new Point3D(100, 0, 0), new Point3D(100, 100, 0));
        var path = new Path3D(new ICurve3D[] { line1, line2 });

        Assert.Equal(2, path.SegmentCount);
        Assert.Equal(200, path.Length, 6);
        Assert.False(path.IsLoop);
    }

    [Fact]
    public void Constructor_ClosedPath_DetectsLoop()
    {
        var line1 = new Line3D(new Point3D(0, 0, 0), new Point3D(100, 0, 0));
        var line2 = new Line3D(new Point3D(100, 0, 0), new Point3D(100, 100, 0));
        var line3 = new Line3D(new Point3D(100, 100, 0), new Point3D(0, 0, 0));
        var path = new Path3D(new ICurve3D[] { line1, line2, line3 });

        Assert.Equal(3, path.SegmentCount);
        Assert.True(path.IsLoop);
    }

    #endregion

    #region FromPoints Tests

    [Fact]
    public void FromPoints_CreatesLineSegments()
    {
        var points = new[]
        {
            new Point3D(0, 0, 0),
            new Point3D(100, 0, 0),
            new Point3D(100, 100, 0)
        };

        var path = Path3D.FromPoints(points);

        Assert.Equal(2, path.SegmentCount);
        Assert.False(path.IsLoop);
    }

    [Fact]
    public void FromPoints_WithClosePath_CreatesClosedLoop()
    {
        var points = new[]
        {
            new Point3D(0, 0, 0),
            new Point3D(100, 0, 0),
            new Point3D(100, 100, 0)
        };

        var path = Path3D.FromPoints(points, closePath: true);

        Assert.Equal(3, path.SegmentCount);
        Assert.True(path.IsLoop);
    }

    #endregion

    #region GroupIntoConnectedPaths Tests

    [Fact]
    public void GroupIntoConnectedPaths_SingleConnectedPath_ReturnsSinglePath()
    {
        var curves = new ICurve3D[]
        {
            new Line3D(new Point3D(0, 0, 0), new Point3D(100, 0, 0)),
            new Line3D(new Point3D(100, 0, 0), new Point3D(200, 0, 0))
        };

        var paths = Path3D.GroupIntoConnectedPaths(curves);

        Assert.Single(paths);
        Assert.Equal(2, paths[0].SegmentCount);
    }

    [Fact]
    public void GroupIntoConnectedPaths_TwoDisconnectedPaths_ReturnsTwoPaths()
    {
        var curves = new ICurve3D[]
        {
            // Path 1
            new Line3D(new Point3D(0, 0, 0), new Point3D(100, 0, 0)),
            new Line3D(new Point3D(100, 0, 0), new Point3D(200, 0, 0)),
            // Path 2 (disconnected)
            new Line3D(new Point3D(0, 500, 0), new Point3D(100, 500, 0)),
            new Line3D(new Point3D(100, 500, 0), new Point3D(200, 500, 0))
        };

        var paths = Path3D.GroupIntoConnectedPaths(curves);

        Assert.Equal(2, paths.Count);
        Assert.Equal(2, paths[0].SegmentCount);
        Assert.Equal(2, paths[1].SegmentCount);
    }

    [Fact]
    public void GroupIntoConnectedPaths_ThreeDisconnectedPaths_ReturnsThreePaths()
    {
        var curves = new ICurve3D[]
        {
            // Path 1
            new Line3D(new Point3D(0, 0, 0), new Point3D(100, 0, 0)),
            // Path 2
            new Line3D(new Point3D(0, 200, 0), new Point3D(100, 200, 0)),
            new Line3D(new Point3D(100, 200, 0), new Point3D(100, 300, 0)),
            // Path 3
            new Line3D(new Point3D(500, 0, 0), new Point3D(600, 0, 0))
        };

        var paths = Path3D.GroupIntoConnectedPaths(curves);

        Assert.Equal(3, paths.Count);
    }

    [Fact]
    public void GroupIntoConnectedPaths_UnorderedCurves_OrdersThemCorrectly()
    {
        // Curves given in wrong order
        var curves = new ICurve3D[]
        {
            new Line3D(new Point3D(200, 0, 0), new Point3D(300, 0, 0)), // Third
            new Line3D(new Point3D(0, 0, 0), new Point3D(100, 0, 0)),   // First
            new Line3D(new Point3D(100, 0, 0), new Point3D(200, 0, 0))  // Second
        };

        var paths = Path3D.GroupIntoConnectedPaths(curves);

        Assert.Single(paths);
        var path = paths[0];
        Assert.Equal(3, path.SegmentCount);

        // Verify the path is continuous
        Assert.True(path.StartPoint.IsEqualTo(new Point3D(0, 0, 0), Tolerance) ||
                    path.StartPoint.IsEqualTo(new Point3D(300, 0, 0), Tolerance));
    }

    [Fact]
    public void GroupIntoConnectedPaths_CurvesNeedingReversal_ReversesCorrectly()
    {
        // Curves with wrong direction
        var curves = new ICurve3D[]
        {
            new Line3D(new Point3D(0, 0, 0), new Point3D(100, 0, 0)),
            new Line3D(new Point3D(200, 0, 0), new Point3D(100, 0, 0)) // Reversed
        };

        var paths = Path3D.GroupIntoConnectedPaths(curves);

        Assert.Single(paths);
        var path = paths[0];
        Assert.Equal(2, path.SegmentCount);

        // Verify continuity
        var seg1 = path.GetSegment(0);
        var seg2 = path.GetSegment(1);
        Assert.True(seg1.EndPoint.IsEqualTo(seg2.StartPoint, Tolerance));
    }

    [Fact]
    public void GroupIntoConnectedPaths_ClosedLoop_DetectsLoop()
    {
        var curves = new ICurve3D[]
        {
            new Line3D(new Point3D(0, 0, 0), new Point3D(100, 0, 0)),
            new Line3D(new Point3D(100, 0, 0), new Point3D(100, 100, 0)),
            new Line3D(new Point3D(100, 100, 0), new Point3D(0, 0, 0))
        };

        var paths = Path3D.GroupIntoConnectedPaths(curves);

        Assert.Single(paths);
        Assert.True(paths[0].IsLoop);
    }

    [Fact]
    public void GroupIntoConnectedPaths_EmptyInput_ReturnsEmptyList()
    {
        var paths = Path3D.GroupIntoConnectedPaths(Array.Empty<ICurve3D>());

        Assert.Empty(paths);
    }

    #endregion

    #region GetSegmentsWithMiters Tests - Open Paths

    [Fact]
    public void GetSegmentsWithMiters_OpenPath_FirstSegmentStartIsPerpendicular()
    {
        var line1 = new Line3D(new Point3D(0, 0, 0), new Point3D(100, 0, 0));
        var line2 = new Line3D(new Point3D(100, 0, 0), new Point3D(100, 100, 0));
        var path = new Path3D(new ICurve3D[] { line1, line2 });

        var miters = path.GetSegmentsWithMiters().ToList();

        // First segment's start miter should be perpendicular (normal = -tangent)
        var (_, startMiter, _) = miters[0];
        var expectedNormal = new Vector3D(-1, 0, 0); // Opposite to beam direction
        Assert.True(startMiter.Normal.IsEqualTo(expectedNormal, Tolerance),
            $"Expected normal {expectedNormal}, got {startMiter.Normal}");
    }

    [Fact]
    public void GetSegmentsWithMiters_OpenPath_LastSegmentEndIsPerpendicular()
    {
        var line1 = new Line3D(new Point3D(0, 0, 0), new Point3D(100, 0, 0));
        var line2 = new Line3D(new Point3D(100, 0, 0), new Point3D(100, 100, 0));
        var path = new Path3D(new ICurve3D[] { line1, line2 });

        var miters = path.GetSegmentsWithMiters().ToList();

        // Last segment's end miter should be perpendicular (normal = tangent)
        var (_, _, endMiter) = miters[1];
        var expectedNormal = new Vector3D(0, 1, 0); // Same as beam direction
        Assert.True(endMiter.Normal.IsEqualTo(expectedNormal, Tolerance),
            $"Expected normal {expectedNormal}, got {endMiter.Normal}");
    }

    [Fact]
    public void GetSegmentsWithMiters_OpenPath_MiddleMiterIsBisector()
    {
        var line1 = new Line3D(new Point3D(0, 0, 0), new Point3D(100, 0, 0));
        var line2 = new Line3D(new Point3D(100, 0, 0), new Point3D(100, 100, 0));
        var path = new Path3D(new ICurve3D[] { line1, line2 });

        var miters = path.GetSegmentsWithMiters().ToList();

        // First segment's end miter (at corner)
        var (_, _, endMiter1) = miters[0];
        // Second segment's start miter (at same corner)
        var (_, startMiter2, _) = miters[1];

        // Both should be at the same point
        Assert.True(endMiter1.Origin.IsEqualTo(new Point3D(100, 0, 0), Tolerance));
        Assert.True(startMiter2.Origin.IsEqualTo(new Point3D(100, 0, 0), Tolerance));

        // The bisector of (1,0,0) and (0,1,0) is (1,1,0) normalized
        var expectedBisector = new Vector3D(1, 1, 0).Normalized();

        // End miter normal should point in beam direction (positive)
        Assert.True(endMiter1.Normal.Dot(new Vector3D(1, 0, 0)) > 0,
            $"End miter normal should point toward beam direction");

        // Start miter normal should point opposite to beam direction (negative)
        Assert.True(startMiter2.Normal.Dot(new Vector3D(0, 1, 0)) < 0,
            $"Start miter normal should point opposite to beam direction");
    }

    #endregion

    #region GetSegmentsWithMiters Tests - Closed Loops

    [Fact]
    public void GetSegmentsWithMiters_ClosedLoop_AllMitersAreBisectors()
    {
        // Square loop
        var line1 = new Line3D(new Point3D(0, 0, 0), new Point3D(100, 0, 0));
        var line2 = new Line3D(new Point3D(100, 0, 0), new Point3D(100, 100, 0));
        var line3 = new Line3D(new Point3D(100, 100, 0), new Point3D(0, 100, 0));
        var line4 = new Line3D(new Point3D(0, 100, 0), new Point3D(0, 0, 0));
        var path = new Path3D(new ICurve3D[] { line1, line2, line3, line4 });

        Assert.True(path.IsLoop);

        var miters = path.GetSegmentsWithMiters().ToList();
        Assert.Equal(4, miters.Count);

        // All corners should have miter planes (not perpendicular)
        // At each 90-degree corner, the bisector is at 45 degrees
        foreach (var (segment, startMiter, endMiter) in miters)
        {
            // Check that miter origins are at segment endpoints
            Assert.True(startMiter.Origin.IsEqualTo(segment.StartPoint, Tolerance));
            Assert.True(endMiter.Origin.IsEqualTo(segment.EndPoint, Tolerance));
        }
    }

    [Fact]
    public void GetSegmentsWithMiters_ClosedLoop_ClosureMiterIsCorrect()
    {
        // Triangle loop
        var line1 = new Line3D(new Point3D(0, 0, 0), new Point3D(100, 0, 0));
        var line2 = new Line3D(new Point3D(100, 0, 0), new Point3D(50, 86.6, 0)); // ~60 degree angle
        var line3 = new Line3D(new Point3D(50, 86.6, 0), new Point3D(0, 0, 0));
        var path = new Path3D(new ICurve3D[] { line1, line2, line3 });

        Assert.True(path.IsLoop);

        var miters = path.GetSegmentsWithMiters().ToList();

        // First segment's start miter should connect to last segment
        var (_, firstStart, _) = miters[0];
        var (_, _, lastEnd) = miters[2];

        // Both should be at origin (0,0,0)
        Assert.True(firstStart.Origin.IsEqualTo(new Point3D(0, 0, 0), Tolerance));
        Assert.True(lastEnd.Origin.IsEqualTo(new Point3D(0, 0, 0), Tolerance));
    }

    #endregion

    #region GetSegmentsWithMiters Tests - Edge Cases

    [Fact]
    public void GetSegmentsWithMiters_SingleSegment_BothEndsArePerpendicular()
    {
        var line = new Line3D(new Point3D(0, 0, 0), new Point3D(100, 0, 0));
        var path = new Path3D(new ICurve3D[] { line });

        var miters = path.GetSegmentsWithMiters().ToList();
        Assert.Single(miters);

        var (_, startMiter, endMiter) = miters[0];

        // Start should have normal pointing opposite to beam (-X)
        Assert.True(startMiter.Normal.IsEqualTo(new Vector3D(-1, 0, 0), Tolerance));

        // End should have normal pointing same as beam (+X)
        Assert.True(endMiter.Normal.IsEqualTo(new Vector3D(1, 0, 0), Tolerance));
    }

    [Fact]
    public void GetSegmentsWithMiters_StraightPath_MitersArePerpendicular()
    {
        // Three collinear segments (no angle between them)
        var line1 = new Line3D(new Point3D(0, 0, 0), new Point3D(100, 0, 0));
        var line2 = new Line3D(new Point3D(100, 0, 0), new Point3D(200, 0, 0));
        var line3 = new Line3D(new Point3D(200, 0, 0), new Point3D(300, 0, 0));
        var path = new Path3D(new ICurve3D[] { line1, line2, line3 });

        var miters = path.GetSegmentsWithMiters().ToList();

        // Middle miters should still be perpendicular since segments are collinear
        // (bisector of two parallel vectors is perpendicular)
        foreach (var (segment, startMiter, endMiter) in miters)
        {
            var tangent = segment.TangentAt(0.5).Normalized();

            // All miter normals should be parallel to tangent (perpendicular cut)
            var startDot = Math.Abs(startMiter.Normal.Dot(tangent));
            var endDot = Math.Abs(endMiter.Normal.Dot(tangent));

            Assert.True(startDot > 0.99, $"Start miter should be perpendicular, dot={startDot}");
            Assert.True(endDot > 0.99, $"End miter should be perpendicular, dot={endDot}");
        }
    }

    [Fact]
    public void GetSegmentsWithMiters_3DPath_HandlesNonPlanarCorrectly()
    {
        // Path going in 3D
        var line1 = new Line3D(new Point3D(0, 0, 0), new Point3D(100, 0, 0));
        var line2 = new Line3D(new Point3D(100, 0, 0), new Point3D(100, 100, 0));
        var line3 = new Line3D(new Point3D(100, 100, 0), new Point3D(100, 100, 100));
        var path = new Path3D(new ICurve3D[] { line1, line2, line3 });

        var miters = path.GetSegmentsWithMiters().ToList();
        Assert.Equal(3, miters.Count);

        // Verify all miter planes have unit normals
        foreach (var (_, startMiter, endMiter) in miters)
        {
            Assert.Equal(1.0, startMiter.Normal.Length, 6);
            Assert.Equal(1.0, endMiter.Normal.Length, 6);
        }
    }

    #endregion

    #region Reverse Tests

    [Fact]
    public void Reverse_ReversesPathDirection()
    {
        var line1 = new Line3D(new Point3D(0, 0, 0), new Point3D(100, 0, 0));
        var line2 = new Line3D(new Point3D(100, 0, 0), new Point3D(100, 100, 0));
        var path = new Path3D(new ICurve3D[] { line1, line2 });

        var reversed = path.Reverse();

        Assert.Equal(path.StartPoint, reversed.EndPoint);
        Assert.Equal(path.EndPoint, reversed.StartPoint);
        Assert.Equal(path.Length, reversed.Length, 6);
    }

    #endregion

    #region GetSegmentOrientations Tests

    [Fact]
    public void GetSegmentOrientations_ReturnsCorrectCount()
    {
        var line1 = new Line3D(new Point3D(0, 0, 0), new Point3D(100, 0, 0));
        var line2 = new Line3D(new Point3D(100, 0, 0), new Point3D(100, 100, 0));
        var path = new Path3D(new ICurve3D[] { line1, line2 });

        var orientations = path.GetSegmentOrientations();

        Assert.Equal(2, orientations.Count);
    }

    [Fact]
    public void GetSegmentOrientations_ZAxisAlignedWithTangent()
    {
        var line = new Line3D(new Point3D(0, 0, 0), new Point3D(100, 0, 0));
        var path = new Path3D(new ICurve3D[] { line });

        var orientations = path.GetSegmentOrientations();
        var orientation = orientations[0];

        // Z-axis should align with tangent (X direction)
        Assert.True(orientation.ZAxis.IsEqualTo(new Vector3D(1, 0, 0), Tolerance));
    }

    [Fact]
    public void GetSegmentOrientations_WithUpHint_UsesHint()
    {
        var line = new Line3D(new Point3D(0, 0, 0), new Point3D(100, 0, 0));
        var path = new Path3D(new ICurve3D[] { line });

        var orientations = path.GetSegmentOrientations(new Vector3D(0, 0, 1));
        var orientation = orientations[0];

        // Y-axis should be influenced by up hint (Z direction)
        // For a beam along X with up hint Z, Y should be approximately Z
        Assert.True(Math.Abs(orientation.YAxis.Dot(new Vector3D(0, 0, 1))) > 0.99);
    }

    [Fact]
    public void GetSegmentOrientations_NonCoplanarPath_PropagatesYAxis()
    {
        // Two segments in XY plane, third exits upward
        var line1 = new Line3D(new Point3D(0, 0, 0), new Point3D(100, 0, 0));
        var line2 = new Line3D(new Point3D(100, 0, 0), new Point3D(100, 100, 0));
        var line3 = new Line3D(new Point3D(100, 100, 0), new Point3D(100, 100, 100));
        var path = new Path3D(new ICurve3D[] { line1, line2, line3 });

        var orientations = path.GetSegmentOrientations(new Vector3D(0, 0, 1));

        // Segments 0 and 1 are in XY plane with upHint Z — both should have Y ≈ Z
        Assert.True(Math.Abs(orientations[0].YAxis.Dot(new Vector3D(0, 0, 1))) > 0.99);
        Assert.True(Math.Abs(orientations[1].YAxis.Dot(new Vector3D(0, 0, 1))) > 0.99);

        // Segment 2 goes along Z. Previous Y was Z, which is parallel to new tangent (degenerate).
        // Falls back to upHint-based computation. Frame should still be valid.
        Assert.True(orientations[2].YAxis.Length > 0.99);
        Assert.True(Math.Abs(orientations[2].ZAxis.Dot(new Vector3D(0, 0, 1))) > 0.99);
    }

    [Fact]
    public void GetSegmentOrientations_NonCoplanarPath_FirstSegmentStable()
    {
        // 2-segment coplanar path
        var line1 = new Line3D(new Point3D(0, 0, 0), new Point3D(100, 0, 0));
        var line2 = new Line3D(new Point3D(100, 0, 0), new Point3D(100, 100, 0));
        var path2 = new Path3D(new ICurve3D[] { line1, line2 });
        var orientations2 = path2.GetSegmentOrientations(new Vector3D(0, 0, 1));

        // Same path + a 3rd non-coplanar segment
        var line3 = new Line3D(new Point3D(100, 100, 0), new Point3D(50, 100, 80));
        var path3 = new Path3D(new ICurve3D[] { line1, line2, line3 });
        var orientations3 = path3.GetSegmentOrientations(new Vector3D(0, 0, 1));

        // Segment 0 and 1 orientations must be identical regardless of the 3rd segment
        Assert.True(orientations2[0].YAxis.IsEqualTo(orientations3[0].YAxis, Tolerance));
        Assert.True(orientations2[1].YAxis.IsEqualTo(orientations3[1].YAxis, Tolerance));
        Assert.True(orientations2[0].XAxis.IsEqualTo(orientations3[0].XAxis, Tolerance));
        Assert.True(orientations2[1].XAxis.IsEqualTo(orientations3[1].XAxis, Tolerance));
    }

    [Fact]
    public void GetSegmentOrientations_DegenerateProjection_ProducesValidFrame()
    {
        // L-shape in XY then vertical — prevY (Z) is parallel to new tangent (Z)
        var line1 = new Line3D(new Point3D(0, 0, 0), new Point3D(100, 0, 0));
        var line2 = new Line3D(new Point3D(100, 0, 0), new Point3D(100, 0, 100));
        var path = new Path3D(new ICurve3D[] { line1, line2 });

        var orientations = path.GetSegmentOrientations(new Vector3D(0, 0, 1));

        // Both frames must be orthonormal (no NaN, no zero vectors)
        foreach (var o in orientations)
        {
            Assert.True(Math.Abs(o.XAxis.Length - 1.0) < Tolerance);
            Assert.True(Math.Abs(o.YAxis.Length - 1.0) < Tolerance);
            Assert.True(Math.Abs(o.ZAxis.Length - 1.0) < Tolerance);
            Assert.True(Math.Abs(o.XAxis.Dot(o.YAxis)) < Tolerance);
            Assert.True(Math.Abs(o.XAxis.Dot(o.ZAxis)) < Tolerance);
            Assert.True(Math.Abs(o.YAxis.Dot(o.ZAxis)) < Tolerance);
        }
    }

    [Fact]
    public void GetSegmentOrientations_CoplanarPath_AllYAxesMatchPlaneNormal()
    {
        // 3-segment L-shape in XY plane
        var line1 = new Line3D(new Point3D(0, 0, 0), new Point3D(100, 0, 0));
        var line2 = new Line3D(new Point3D(100, 0, 0), new Point3D(100, 100, 0));
        var line3 = new Line3D(new Point3D(100, 100, 0), new Point3D(0, 100, 0));
        var path = new Path3D(new ICurve3D[] { line1, line2, line3 });

        var upHint = new Vector3D(0, 0, 1);
        var orientations = path.GetSegmentOrientations(upHint);

        // All Y-axes should be Z (plane normal) — propagation preserves this for coplanar paths
        foreach (var o in orientations)
        {
            Assert.True(Math.Abs(o.YAxis.Dot(new Vector3D(0, 0, 1))) > 0.99);
        }
    }

    [Fact]
    public void GetSegmentOrientations_NonCoplanar_ModerateAngle_MinimalTwist()
    {
        // Segment 1 along X, segment 2 along Y, segment 3 angled out of XY plane at 45°
        var line1 = new Line3D(new Point3D(0, 0, 0), new Point3D(100, 0, 0));
        var line2 = new Line3D(new Point3D(100, 0, 0), new Point3D(100, 100, 0));
        var line3 = new Line3D(new Point3D(100, 100, 0), new Point3D(100, 200, 100));
        var path = new Path3D(new ICurve3D[] { line1, line2, line3 });

        var orientations = path.GetSegmentOrientations(new Vector3D(0, 0, 1));

        // Segment 2's Y-axis should be the parallel transport of segment 1's Y-axis
        // Segment 1 Y ≈ Z. Segment 2 tangent is along Y, so prevY (Z) projected onto
        // plane perp to Y-tangent is still Z. So segment 2 Y ≈ Z.
        Assert.True(Math.Abs(orientations[1].YAxis.Dot(new Vector3D(0, 0, 1))) > 0.99);

        // Segment 3 tangent is (0, 1, 1)/sqrt(2). PrevY = Z = (0,0,1).
        // projected = (0,0,1) - (0,1,1)/sqrt(2) * ((0,0,1)·(0,1,1)/sqrt(2))
        //           = (0,0,1) - (0,1,1)/sqrt(2) * 1/sqrt(2)
        //           = (0,0,1) - (0, 0.5, 0.5) = (0, -0.5, 0.5)
        // Normalized: (0, -1, 1)/sqrt(2)
        var expectedY = new Vector3D(0, -1, 1).Normalized();
        Assert.True(Math.Abs(orientations[2].YAxis.Dot(expectedY)) > 0.99);
    }

    [Fact]
    public void GetSegmentOrientations_UShapePath_UpHintParallelToMiddleSegment_StaysContinuous()
    {
        // Regression: U-shaped path whose middle segment is parallel to the up hint.
        // Middle segment tangent (0,0,-1) is parallel to the caller-supplied upHint (0,0,-1).
        // The old projection-based transport degenerated here and jumped Y to an arbitrary axis,
        // causing a 90° flat-bar rotation between segments. With rotation-minimizing transport,
        // each segment's Y must equal the previous Y rotated by the tangent delta — no jumps.
        var line1 = new Line3D(new Point3D(0, 1860, 700), new Point3D(0,   60, 700));
        var line2 = new Line3D(new Point3D(0,   60, 700), new Point3D(0,   60, 160));
        var line3 = new Line3D(new Point3D(0,   60, 160), new Point3D(0, 1860, 160));
        var path = new Path3D(new ICurve3D[] { line1, line2, line3 });

        var upHint = new Vector3D(0, 0, -1);
        var orientations = path.GetSegmentOrientations(upHint);

        Assert.Equal(3, orientations.Count);

        // Segment 0 Y should equal upHint (perpendicular to tangent (0,-1,0)).
        Assert.True(orientations[0].YAxis.IsEqualTo(new Vector3D(0, 0, -1), Tolerance));

        // For each adjacent pair, the Y-axis delta must match the tangent delta
        // (continuous parallel transport — the defining property of an RMF).
        for (int i = 1; i < orientations.Count; i++)
        {
            var prev = orientations[i - 1];
            var curr = orientations[i];
            var tangentRotation = Matrix3D.CreateRotationFromTo(prev.ZAxis, curr.ZAxis);
            var expectedY = tangentRotation.Transform(prev.YAxis);
            Assert.True(curr.YAxis.IsEqualTo(expectedY, Tolerance),
                $"Segment {i}: Y axis {curr.YAxis} does not match transported previous Y {expectedY}");
        }

        // Concretely: Feature 1 flat-bar flange walks -Z → +Y → +Z as the path bends around.
        Assert.True(orientations[1].YAxis.IsEqualTo(new Vector3D(0, 1, 0), Tolerance));
        Assert.True(orientations[2].YAxis.IsEqualTo(new Vector3D(0, 0, 1), Tolerance));
    }

    #endregion

    #region Coplanarity Tests

    [Fact]
    public void IsCoplanar_PlanarPath_ReturnsTrue()
    {
        var line1 = new Line3D(new Point3D(0, 0, 0), new Point3D(100, 0, 0));
        var line2 = new Line3D(new Point3D(100, 0, 0), new Point3D(100, 100, 0));
        var line3 = new Line3D(new Point3D(100, 100, 0), new Point3D(0, 100, 0));
        var path = new Path3D(new ICurve3D[] { line1, line2, line3 });

        Assert.True(path.IsCoplanar);
        Assert.NotNull(path.Plane);
    }

    [Fact]
    public void IsCoplanar_NonPlanarPath_ReturnsFalse()
    {
        var line1 = new Line3D(new Point3D(0, 0, 0), new Point3D(100, 0, 0));
        var line2 = new Line3D(new Point3D(100, 0, 0), new Point3D(100, 100, 0));
        var line3 = new Line3D(new Point3D(100, 100, 0), new Point3D(100, 100, 100));
        var line4 = new Line3D(new Point3D(100, 100, 100), new Point3D(0, 50, 50));
        var path = new Path3D(new ICurve3D[] { line1, line2, line3, line4 });

        Assert.False(path.IsCoplanar);
        Assert.Null(path.Plane);
    }

    #endregion
}
