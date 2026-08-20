namespace BasAutomation.Geometry.Tests;

public class MiterPlaneMathTests
{
    private const double Tolerance = Constants.Tolerance;

    [Fact]
    public void RightAngleCorner_InXYPlane_ProducesFortyFiveDegreeNormal()
    {
        // +X and +Y rays meet at a 90° corner; the miter cuts at 45°, so the plane
        // normal points along (1, -1) in the XY plane.
        var ok = MiterPlaneMath.TryGetMiterPlaneNormal(new Vector3D(1, 0, 0), new Vector3D(0, 1, 0), out var normal);

        Assert.True(ok);
        Assert.Equal(1.0, normal.Length, Tolerance);
        AssertVectorEqual(new Vector3D(Math.Sqrt(0.5), -Math.Sqrt(0.5), 0), normal);
    }

    [Fact]
    public void MatchesBeamTrimReferenceValues_SlopedJoint()
    {
        // Directions and expected normal captured from a real sloped mitre joint.
        var awayDir1 = new Vector3D(0, -0.9071599274033648, -0.420786009883316);
        var awayDir2 = new Vector3D(0, 1, 0);

        var ok = MiterPlaneMath.TryGetMiterPlaneNormal(awayDir1, awayDir2, out var normal);

        Assert.True(ok);
        AssertVectorEqual(new Vector3D(0, -0.976514190220338, -0.2154530953556193), normal);
    }

    [Fact]
    public void Normal_IsPerpendicularToCornerBisector()
    {
        // Regression guard: the original bug used the corner bisector itself as the plane
        // normal. The correct miter normal is perpendicular to the bisector.
        var awayDir1 = new Vector3D(0, -0.9071599274033648, -0.420786009883316);
        var awayDir2 = new Vector3D(0, 1, 0);

        Assert.True(MiterPlaneMath.TryGetMiterPlaneNormal(awayDir1, awayDir2, out var normal));

        var bisector = (awayDir1.Normalized() + awayDir2.Normalized()).Normalized();
        Assert.Equal(0.0, normal.Dot(bisector), Tolerance);
    }

    [Fact]
    public void Normal_LiesInPlaneSpannedByDirections()
    {
        var awayDir1 = new Vector3D(0, -0.9071599274033648, -0.420786009883316);
        var awayDir2 = new Vector3D(0, 1, 0);

        Assert.True(MiterPlaneMath.TryGetMiterPlaneNormal(awayDir1, awayDir2, out var normal));

        var spanNormal = awayDir1.Cross(awayDir2).Normalized();
        Assert.Equal(0.0, normal.Dot(spanNormal), Tolerance);
    }

    [Fact]
    public void SwappingDirections_GivesSamePlane()
    {
        var awayDir1 = new Vector3D(0, -0.9071599274033648, -0.420786009883316);
        var awayDir2 = new Vector3D(0, 1, 0);

        Assert.True(MiterPlaneMath.TryGetMiterPlaneNormal(awayDir1, awayDir2, out var n1));
        Assert.True(MiterPlaneMath.TryGetMiterPlaneNormal(awayDir2, awayDir1, out var n2));

        // Same plane: normals are equal or exactly opposite (|dot| == 1).
        Assert.Equal(1.0, Math.Abs(n1.Dot(n2)), Tolerance);
    }

    [Fact]
    public void IsScaleInvariant()
    {
        Assert.True(MiterPlaneMath.TryGetMiterPlaneNormal(new Vector3D(3, 0, 0), new Vector3D(0, 5, 0), out var scaled));
        Assert.True(MiterPlaneMath.TryGetMiterPlaneNormal(new Vector3D(1, 0, 0), new Vector3D(0, 1, 0), out var unit));

        AssertVectorEqual(unit, scaled);
    }

    [Fact]
    public void ParallelDirections_ReturnsFalse()
    {
        Assert.False(MiterPlaneMath.TryGetMiterPlaneNormal(new Vector3D(0, 1, 0), new Vector3D(0, 2, 0), out _));
    }

    [Fact]
    public void AntiParallelDirections_ReturnsFalse()
    {
        // 180° "corner" — no well-defined bisector / cut plane.
        Assert.False(MiterPlaneMath.TryGetMiterPlaneNormal(new Vector3D(0, 1, 0), new Vector3D(0, -1, 0), out _));
    }

    [Fact]
    public void ZeroLengthDirection_ReturnsFalse()
    {
        Assert.False(MiterPlaneMath.TryGetMiterPlaneNormal(new Vector3D(0, 0, 0), new Vector3D(0, 1, 0), out _));
    }

    [Fact]
    public void HorizontalPlaneJoint_MatchesBeamTrimReference()
    {
        // Y-up / horizontal-plane run: the same corner angle as the sloped case but the
        // joint lies in a horizontal plane (span normal along Z), so the miter normal is
        // horizontal. Values from the beam mitre trim at joint (-70, 2298.79, 1181.5).
        var awayDir1 = new Vector3D(0, -1, 0);
        var awayDir2 = new Vector3D(-0.4207860098833156, 0.9071599274033648, 0);

        Assert.True(MiterPlaneMath.TryGetMiterPlaneNormal(awayDir1, awayDir2, out var normal));
        AssertVectorEqual(new Vector3D(0.21545309535561913, -0.976514190220338, 0), normal);
    }

    [Fact]
    public void HorizontalPlaneSecondJoint_MatchesBeamTrimReference()
    {
        // Second joint of the horizontal run (-1091.34, 4500.68, 1181.5); a different
        // corner angle, confirming the helper is not tied to any particular orientation.
        var awayDir1 = new Vector3D(0.4207860098833156, -0.9071599274033648, 0);
        var awayDir2 = new Vector3D(0.17662406738288375, 0.9842783848186074, 0);

        Assert.True(MiterPlaneMath.TryGetMiterPlaneNormal(awayDir1, awayDir2, out var normal));
        AssertVectorEqual(new Vector3D(0.12802569140575917, -0.991770851729409, 0), normal);
    }

    [Fact]
    public void IsRotationEquivariant()
    {
        // Rotating both input directions rotates the resulting normal the same way:
        // the same physical corner produces the same physical plane regardless of how the
        // model is oriented (e.g. Z-up vs Y-up). Uses the cyclic axis rotation
        // (x -> y -> z -> x), which is a proper rotation (determinant +1).
        var awayDir1 = new Vector3D(0, -0.9071599274033648, -0.420786009883316);
        var awayDir2 = new Vector3D(0, 1, 0);

        Assert.True(MiterPlaneMath.TryGetMiterPlaneNormal(awayDir1, awayDir2, out var normal));
        Assert.True(MiterPlaneMath.TryGetMiterPlaneNormal(Rotate(awayDir1), Rotate(awayDir2), out var rotatedNormal));

        AssertVectorEqual(Rotate(normal), rotatedNormal);

        static Vector3D Rotate(Vector3D v) => new(v.Z, v.X, v.Y);
    }

    private static void AssertVectorEqual(Vector3D expected, Vector3D actual)
    {
        Assert.Equal(expected.X, actual.X, Tolerance);
        Assert.Equal(expected.Y, actual.Y, Tolerance);
        Assert.Equal(expected.Z, actual.Z, Tolerance);
    }
}
