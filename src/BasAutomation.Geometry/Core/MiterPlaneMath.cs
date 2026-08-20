namespace BasAutomation.Geometry;

/// <summary>
/// Shared math for the miter (bisector) cut plane formed where two straight segments
/// meet at a joint. Keep all mitre cuts on this one helper so they always produce the same
/// plane for the same geometry.
/// </summary>
public static class MiterPlaneMath
{
    /// <summary>
    /// Computes the normal of the miter plane that bisects the corner between two segments
    /// meeting at a joint. The plane contains the angle bisector of the two directions and
    /// is perpendicular to the plane they span, i.e. <c>normal = bisector × (d1 × d2)</c>.
    /// </summary>
    /// <param name="awayDirection1">
    /// Direction of the first segment pointing AWAY from the joint (need not be unit length).
    /// </param>
    /// <param name="awayDirection2">
    /// Direction of the second segment pointing AWAY from the joint (need not be unit length).
    /// </param>
    /// <param name="normal">The unit-length miter-plane normal when the method succeeds.</param>
    /// <returns>
    /// <c>false</c> when the directions are degenerate, (anti)parallel, or exactly opposite —
    /// in those cases there is no well-defined corner to miter.
    /// </returns>
    public static bool TryGetMiterPlaneNormal(Vector3D awayDirection1, Vector3D awayDirection2, out Vector3D normal)
    {
        normal = default;

        if (awayDirection1.LengthSquared < Constants.Tolerance ||
            awayDirection2.LengthSquared < Constants.Tolerance)
        {
            return false;
        }

        var u1 = awayDirection1.Normalized();
        var u2 = awayDirection2.Normalized();

        // Parallel directions do not define a corner.
        if (u1.IsParallel(u2))
        {
            return false;
        }

        // Angle bisector of the corner; lies in the plane spanned by the two segments.
        var bisector = u1 + u2;
        if (bisector.LengthSquared < Constants.Tolerance)
        {
            // Exactly opposite directions (straight-through 180°): bisector is undefined.
            return false;
        }
        bisector = bisector.Normalized();

        // Normal of the plane the two segments span.
        var spanNormal = u1.Cross(u2);
        if (spanNormal.LengthSquared < Constants.Tolerance)
        {
            return false;
        }

        // Miter-plane normal: perpendicular to the bisector, lying in the segment plane.
        normal = bisector.Cross(spanNormal.Normalized()).Normalized();
        return normal.LengthSquared > Constants.Tolerance;
    }
}
