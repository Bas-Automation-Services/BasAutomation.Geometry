namespace BasAutomation.Geometry.Tests;

public class FaceAlignedFrameTests
{
    private const double Tol = 1e-9;

    #region BuildAxisAlignedFallback

    [Fact]
    public void BuildAxisAlignedFallback_AlignToX_ProducesIdentityRotation()
    {
        var m = FaceAlignedFrame.BuildAxisAlignedFallback(alignReferenceToX: true);

        AssertVectorEqual(Vector3D.XAxis, m.XAxis);
        AssertVectorEqual(Vector3D.YAxis, m.YAxis);
        AssertVectorEqual(Vector3D.ZAxis, m.ZAxis);
        Assert.True(m.IsRotationMatrix(Tol));
    }

    [Fact]
    public void BuildAxisAlignedFallback_AlignToY_SwapsXAndY()
    {
        var m = FaceAlignedFrame.BuildAxisAlignedFallback(alignReferenceToX: false);

        AssertVectorEqual(Vector3D.YAxis, m.XAxis);
        AssertVectorEqual(Vector3D.XAxis, m.YAxis);
        AssertVectorEqual(Vector3D.ZAxis, m.ZAxis);
    }

    [Fact]
    public void BuildModelMinusXViewFrame_ModelZIsVerticalOnSheet()
    {
        var m = FaceAlignedFrame.BuildModelMinusXViewFrame();

        AssertVectorEqual(Vector3D.YAxis, m.XAxis);
        AssertVectorEqual(Vector3D.ZAxis, m.YAxis);
        AssertVectorEqual(Vector3D.XAxis, m.ZAxis);
        Assert.True(m.IsRotationMatrix(Tol));
    }

    #endregion

    #region PickPerpendicularAxis

    [Theory]
    [MemberData(nameof(PrincipalAxes))]
    public void PickPerpendicularAxis_PrincipalAxis_ReturnsUnitPerpendicular(Vector3D axis)
    {
        var perp = FaceAlignedFrame.PickPerpendicularAxis(axis);

        Assert.Equal(1.0, perp.Length, 9);
        Assert.Equal(0.0, perp.Dot(axis), 9);
    }

    [Fact]
    public void PickPerpendicularAxis_Diagonal_ReturnsUnitPerpendicular()
    {
        var diagonal = new Vector3D(1, 1, 1).Normalized();

        var perp = FaceAlignedFrame.PickPerpendicularAxis(diagonal);

        Assert.Equal(1.0, perp.Length, 9);
        Assert.Equal(0.0, perp.Dot(diagonal), 9);
    }

    [Fact]
    public void PickPerpendicularAxis_NonUnitInput_StillReturnsUnitPerpendicular()
    {
        var perp = FaceAlignedFrame.PickPerpendicularAxis(new Vector3D(0, 0, 5));

        Assert.Equal(1.0, perp.Length, 9);
        Assert.Equal(0.0, perp.Dot(Vector3D.ZAxis), 9);
    }

    // The exact return for each principal axis is part of the contract: downstream
    // Build() relies on these specific outputs to keep the legacy axis selection
    // deterministic. Any change here is a behavioural change.
    [Fact]
    public void PickPerpendicularAxis_X_ReturnsZ()
    {
        AssertVectorEqual(Vector3D.ZAxis, FaceAlignedFrame.PickPerpendicularAxis(Vector3D.XAxis));
    }

    [Fact]
    public void PickPerpendicularAxis_Y_ReturnsNegativeZ()
    {
        AssertVectorEqual(new Vector3D(0, 0, -1), FaceAlignedFrame.PickPerpendicularAxis(Vector3D.YAxis));
    }

    [Fact]
    public void PickPerpendicularAxis_Z_ReturnsY()
    {
        AssertVectorEqual(Vector3D.YAxis, FaceAlignedFrame.PickPerpendicularAxis(Vector3D.ZAxis));
    }

    public static IEnumerable<object[]> PrincipalAxes() => new[]
    {
        new object[] { Vector3D.XAxis },
        new object[] { Vector3D.YAxis },
        new object[] { Vector3D.ZAxis },
    };

    #endregion

    #region Build - standard cases

    [Fact]
    public void Build_NormalZ_ReferenceY_AlignToY_ProducesIdentity()
    {
        // alignReferenceToX = false means the reference becomes the Y axis.
        // With normal=Z and reference=Y, the cross Y × Z = X reproduces a right-handed identity.
        var m = FaceAlignedFrame.Build(
            faceNormal: Vector3D.ZAxis,
            inPlaneReference: Vector3D.YAxis,
            alignReferenceToX: false);

        AssertVectorEqual(Vector3D.XAxis, m.XAxis);
        AssertVectorEqual(Vector3D.YAxis, m.YAxis);
        AssertVectorEqual(Vector3D.ZAxis, m.ZAxis);
        Assert.True(m.IsRotationMatrix(Tol));
    }

    [Fact]
    public void Build_NormalZ_ReferenceX_AlignToX_PreservesHistoricalHandedness()
    {
        // Documented existing behaviour: x = reference = X, y = X × Z = -Y, z = Z.
        // The resulting matrix has determinant -1 (left-handed rotation block) — the
        // CAD pipeline relies on this combined with a flipped camera Eye sign.
        var m = FaceAlignedFrame.Build(
            faceNormal: Vector3D.ZAxis,
            inPlaneReference: Vector3D.XAxis,
            alignReferenceToX: true);

        AssertVectorEqual(Vector3D.XAxis, m.XAxis);
        AssertVectorEqual(new Vector3D(0, -1, 0), m.YAxis);
        AssertVectorEqual(Vector3D.ZAxis, m.ZAxis);
    }

    [Fact]
    public void Build_TiltedFace_AlignToX_PinsXAndZ()
    {
        var normal = new Vector3D(1, 1, 1).Normalized();
        var reference = new Vector3D(1, -1, 0).Normalized();

        var m = FaceAlignedFrame.Build(normal, reference, alignReferenceToX: true);

        AssertOrthonormal(m);
        // Pin the algorithm: when alignReferenceToX = true the reference IS the X axis verbatim,
        // and Z is the (already unit) face normal — no implicit re-orientation.
        AssertVectorEqual(reference, m.XAxis);
        AssertVectorEqual(normal, m.ZAxis);
    }

    [Fact]
    public void Build_TiltedFace_AlignToY_PinsYAndZ()
    {
        var normal = new Vector3D(1, 1, 1).Normalized();
        var reference = new Vector3D(1, -1, 0).Normalized();

        var m = FaceAlignedFrame.Build(normal, reference, alignReferenceToX: false);

        AssertOrthonormal(m);
        AssertVectorEqual(reference, m.YAxis);
        AssertVectorEqual(normal, m.ZAxis);
    }

    [Fact]
    public void Build_NonUnitInputs_NormalisesInternally()
    {
        var m = FaceAlignedFrame.Build(
            faceNormal: new Vector3D(0, 0, 7),
            inPlaneReference: new Vector3D(3, 0, 0),
            alignReferenceToX: false);

        AssertOrthonormal(m);
        AssertVectorEqual(Vector3D.ZAxis, m.ZAxis);
    }

    #endregion

    #region Build - parallel-reference fallback

    [Fact]
    public void Build_ReferenceParallelToNormal_FallsBackToPerpendicular()
    {
        // Reference parallel to normal would produce a zero cross product; the builder must
        // substitute a perpendicular axis and still return an orthonormal matrix.
        var m = FaceAlignedFrame.Build(
            faceNormal: Vector3D.ZAxis,
            inPlaneReference: Vector3D.ZAxis,
            alignReferenceToX: true);

        AssertOrthonormal(m);
        AssertVectorEqual(Vector3D.ZAxis, m.ZAxis);
    }

    [Fact]
    public void Build_ReferenceAntiparallelToNormal_FallsBackToPerpendicular()
    {
        var m = FaceAlignedFrame.Build(
            faceNormal: Vector3D.ZAxis,
            inPlaneReference: -Vector3D.ZAxis,
            alignReferenceToX: false);

        AssertOrthonormal(m);
        AssertVectorEqual(Vector3D.ZAxis, m.ZAxis);
    }

    [Fact]
    public void Build_ReferenceNearlyParallelToNormal_TriggersFallback()
    {
        // Dot product 0.999 is above ParallelThreshold (0.99) → fallback expected.
        var nearlyParallelToZ = new Vector3D(0.04472, 0, 0.999).Normalized();

        var m = FaceAlignedFrame.Build(
            faceNormal: Vector3D.ZAxis,
            inPlaneReference: nearlyParallelToZ,
            alignReferenceToX: true);

        AssertOrthonormal(m);
    }

    #endregion

    #region Build - degenerate input

    [Fact]
    public void Build_ZeroNormal_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            FaceAlignedFrame.Build(
                faceNormal: Vector3D.Zero,
                inPlaneReference: Vector3D.XAxis,
                alignReferenceToX: true));
    }

    [Fact]
    public void Build_ZeroReference_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            FaceAlignedFrame.Build(
                faceNormal: Vector3D.ZAxis,
                inPlaneReference: Vector3D.Zero,
                alignReferenceToX: true));
    }

    #endregion

    #region Helpers

    private static void AssertVectorEqual(Vector3D expected, Vector3D actual)
    {
        Assert.Equal(expected.X, actual.X, 9);
        Assert.Equal(expected.Y, actual.Y, 9);
        Assert.Equal(expected.Z, actual.Z, 9);
    }

    private static void AssertOrthonormal(Matrix3D m)
    {
        Assert.Equal(1.0, m.XAxis.Length, 9);
        Assert.Equal(1.0, m.YAxis.Length, 9);
        Assert.Equal(1.0, m.ZAxis.Length, 9);

        Assert.Equal(0.0, m.XAxis.Dot(m.YAxis), 9);
        Assert.Equal(0.0, m.YAxis.Dot(m.ZAxis), 9);
        Assert.Equal(0.0, m.ZAxis.Dot(m.XAxis), 9);
    }

    #endregion
}
