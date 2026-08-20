namespace BasAutomation.Geometry;

/// <summary>
/// Pure-math builder for an orthonormal frame anchored to a planar face.
/// <para>
/// Given a face normal (Z) and an in-plane reference direction (the projection of which
/// becomes either X or Y depending on <c>alignReferenceToX</c>), produces a coordinate-system
/// matrix whose Z axis equals the normal and whose remaining axis is derived by
/// <see cref="Vector3D.Cross"/> + normalisation.
/// </para>
/// <para>
/// Stateless and pure — no COM, no I/O, no logging, no domain knowledge. Callers in the CAD
/// layer are responsible for
/// translating their domain types (Inventor <c>UnitVector</c>, drawing rotation enums, etc.)
/// to <see cref="Vector3D"/> + <see cref="bool"/> on the way in and to their target matrix
/// type on the way out.
/// </para>
/// </summary>
/// <remarks>
/// <para>
/// <b>Handedness warning.</b> The output matrix is not guaranteed to be a proper rotation
/// (det = +1). With <c>alignReferenceToX = true</c> the construction
/// <c>Y = X × Z</c> yields det = −1 (left-handed) for inputs in the standard X/Y/Z basis.
/// Do not feed the result into transforms that assume a proper rotation; consumers must
/// compensate (e.g. a camera consumer can flip the sign of its eye vector).
/// </para>
/// </remarks>
public static class FaceAlignedFrame
{
    /// <summary>
    /// Above this absolute dot product (with both inputs unit vectors) the in-plane reference
    /// direction is treated as parallel to the face normal and replaced by a perpendicular
    /// axis chosen by <see cref="PickPerpendicularAxis"/>.
    /// </summary>
    public const double ParallelThreshold = 0.99;

    /// <summary>
    /// Above this absolute dot product a candidate "perpendicular" basis vector is rejected
    /// in favour of the next principal axis. Picked loosely to avoid near-zero cross products.
    /// </summary>
    public const double PerpendicularThreshold = 0.9;

    /// <summary>
    /// Tolerance below which a cross-product magnitude is treated as zero and the frame is
    /// rejected as degenerate. Algorithm-local, deliberately decoupled from
    /// <see cref="Constants.Tolerance"/> (which is a length-in-mm tolerance).
    /// </summary>
    private const double DegenerateCrossMagnitude = 1e-6;

    /// <summary>
    /// Builds an orthonormal frame whose Z axis equals <paramref name="faceNormal"/>.
    /// The X/Y assignment depends on <paramref name="alignReferenceToX"/>:
    /// <list type="bullet">
    ///   <item><c>true</c>: X = reference direction, Y = X × normal.</item>
    ///   <item><c>false</c>: Y = reference direction, X = Y × normal.</item>
    /// </list>
    /// When <paramref name="inPlaneReference"/> is parallel to <paramref name="faceNormal"/>
    /// (within <see cref="ParallelThreshold"/>), the reference is replaced by
    /// <see cref="PickPerpendicularAxis"/> of the normal so the cross product cannot collapse.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the chosen cross-product axis is shorter than
    /// <see cref="DegenerateCrossMagnitude"/> (would yield a degenerate matrix). The exception
    /// message includes the offending axis and normal so the caller can wrap it with context.
    /// </exception>
    public static Matrix3D Build(Vector3D faceNormal, Vector3D inPlaneReference, bool alignReferenceToX)
    {
        var normal = faceNormal.Normalized();
        var direction = inPlaneReference.Normalized();

        if (Math.Abs(direction.Dot(normal)) > ParallelThreshold)
        {
            direction = PickPerpendicularAxis(normal);
        }

        Vector3D xAxis, yAxis;
        if (alignReferenceToX)
        {
            xAxis = direction;
            yAxis = SafeCross(xAxis, normal, missingAxis: "Y");
        }
        else
        {
            yAxis = direction;
            xAxis = SafeCross(yAxis, normal, missingAxis: "X");
        }

        return Matrix3D.Identity.SetAxes(xAxis, yAxis, normal);
    }

    /// <summary>
    /// Builds the global-axis fallback frame used when a face / reference direction cannot be
    /// derived. Z is always world-Z; X and Y are either the world X/Y identity
    /// (<paramref name="alignReferenceToX"/> = <see langword="true"/>) or swapped
    /// (det = −1, 90° around Z).
    /// </summary>
    public static Matrix3D BuildAxisAlignedFallback(bool alignReferenceToX)
    {
        var xAxis = alignReferenceToX ? Vector3D.XAxis : Vector3D.YAxis;
        var yAxis = alignReferenceToX ? Vector3D.YAxis : Vector3D.XAxis;
        return Matrix3D.Identity.SetAxes(xAxis, yAxis, Vector3D.ZAxis);
    }

    /// <summary>
    /// Builds a fixed view frame where model +Z is vertical on the sheet and the
    /// camera looks from −X toward the origin. Independent of session world-up conventions.
    /// </summary>
    public static Matrix3D BuildModelMinusXViewFrame()
        => Matrix3D.Identity.SetAxes(Vector3D.YAxis, Vector3D.ZAxis, Vector3D.XAxis);

    /// <summary>
    /// Returns a unit vector that is reasonably perpendicular to <paramref name="vector"/> by
    /// trying the principal axes (X → Y → Z) and crossing with the first one whose absolute
    /// dot product with <paramref name="vector"/> is below <see cref="PerpendicularThreshold"/>.
    /// </summary>
    /// <remarks>
    /// Used as the parallel-edge fallback inside <see cref="Build"/>; exposed for direct
    /// testing and for any future caller that needs the same primitive.
    /// </remarks>
    public static Vector3D PickPerpendicularAxis(Vector3D vector)
    {
        var v = vector.Normalized();

        var test = Vector3D.XAxis;
        if (Math.Abs(v.Dot(test)) > PerpendicularThreshold)
        {
            test = Vector3D.YAxis;
        }
        if (Math.Abs(v.Dot(test)) > PerpendicularThreshold)
        {
            test = Vector3D.ZAxis;
        }

        return v.Cross(test).Normalized();
    }

    /// <summary>
    /// Cross-product variant that normalises the result and rejects near-zero output. Mirrors
    /// the post-condition of Inventor's <c>UnitVector.CrossProduct</c> (returns a unit-length
    /// vector) but adds an explicit guard so the caller never hands a zero axis to
    /// <see cref="Matrix3D.SetAxes"/>.
    /// </summary>
    private static Vector3D SafeCross(Vector3D a, Vector3D b, string missingAxis)
    {
        var cross = a.Cross(b);
        if (cross.Length < DegenerateCrossMagnitude)
        {
            throw new InvalidOperationException(
                $"Cross product produced a zero {missingAxis} axis. " +
                $"a=[{a.X:F3},{a.Y:F3},{a.Z:F3}], b=[{b.X:F3},{b.Y:F3},{b.Z:F3}].");
        }
        return cross.Normalized();
    }
}
