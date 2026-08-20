namespace BasAutomation.Geometry;

/// <summary>
/// Finds the proper rigid motion (rotation with det = +1, then translation) that maps
/// one point set onto another with exact index correspondence — or proves there is
/// none. This is a <i>verification-grade</i> fit, not a least-squares Kabsch: the
/// candidate motion is constructed from centroids + a frame built on the two most
/// informative points, then EVERY pair must map within tolerance, so a reflection of
/// a non-planar set, a reshape, or a wrong correspondence is rejected rather than
/// approximated. (A planar set mirrored within its own plane is reachable by a 180°
/// out-of-plane rotation — that is a genuine rigid motion and correctly accepted.)
/// Intended use: confirm "the whole group moved rigidly" before applying the motion,
/// falling back to regeneration otherwise.
/// </summary>
public static class RigidMotionFit
{
    /// <summary>
    /// Tries to find the proper rigid motion with <c>motion.Transform(from[i]) ≈ to[i]</c>
    /// for every i. Returns false when the sets are not one rigid motion apart (within
    /// <paramref name="tolerance"/>, same units as the points) — including reflections
    /// of non-planar sets — or when the lists differ in length or are empty.
    /// </summary>
    public static bool TryFit(
        IReadOnlyList<Point3D> from, IReadOnlyList<Point3D> to, double tolerance, out Matrix3D motion)
    {
        motion = Matrix3D.Identity;
        if (from.Count == 0 || from.Count != to.Count) return false;

        var centroidFrom = Centroid(from);
        var centroidTo = Centroid(to);

        var rotation = FitRotation(from, to, centroidFrom, centroidTo, tolerance);

        var candidate =
            Matrix3D.Translation(centroidTo.X, centroidTo.Y, centroidTo.Z)
            * rotation
            * Matrix3D.Translation(-centroidFrom.X, -centroidFrom.Y, -centroidFrom.Z);

        for (var i = 0; i < from.Count; i++)
            if (candidate.Transform(from[i]).DistanceTo(to[i]) > tolerance)
                return false;

        motion = candidate;
        return true;
    }

    /// <summary>
    /// Builds the candidate rotation from the same two index points in both sets: the
    /// point farthest from the centroid gives the primary axis, the point with the
    /// largest rejection from it the secondary. Right-handed frames on both sides make
    /// the result a proper rotation by construction; degenerate sets (single point,
    /// collinear) fall back to fewer axes — any slack the frame leaves is caught by the
    /// caller's full-residual check.
    /// </summary>
    private static Matrix3D FitRotation(
        IReadOnlyList<Point3D> from, IReadOnlyList<Point3D> to,
        Point3D centroidFrom, Point3D centroidTo, double tolerance)
    {
        var primary = IndexOfMaxDistance(from, centroidFrom);
        var primaryFrom = from[primary] - centroidFrom;
        if (primaryFrom.Length <= tolerance) return Matrix3D.Identity;   // all points coincide

        var primaryTo = to[primary] - centroidTo;
        if (primaryTo.Length <= tolerance) return Matrix3D.Identity;     // cannot be rigid; residual check fails

        var secondary = IndexOfMaxRejection(from, centroidFrom, primaryFrom.Normalized());
        var secondaryFrom = Reject(from[secondary] - centroidFrom, primaryFrom.Normalized());
        if (secondaryFrom.Length <= tolerance)
        {
            // Collinear set: rotation about the line is free, aligning the line suffices.
            return Matrix3D.CreateRotationFromTo(primaryFrom, primaryTo);
        }
        var secondaryTo = Reject(to[secondary] - centroidTo, primaryTo.Normalized());
        if (secondaryTo.Length <= tolerance)
            return Matrix3D.CreateRotationFromTo(primaryFrom, primaryTo); // degenerate target; residual decides

        return FrameOf(primaryTo, secondaryTo) * Transposed(FrameOf(primaryFrom, secondaryFrom));
    }

    private static Point3D Centroid(IReadOnlyList<Point3D> points)
    {
        double x = 0, y = 0, z = 0;
        foreach (var p in points) { x += p.X; y += p.Y; z += p.Z; }
        return new Point3D(x / points.Count, y / points.Count, z / points.Count);
    }

    private static int IndexOfMaxDistance(IReadOnlyList<Point3D> points, Point3D origin)
    {
        var best = 0;
        var bestDistance = double.MinValue;
        for (var i = 0; i < points.Count; i++)
        {
            var d = points[i].DistanceTo(origin);
            if (d > bestDistance) { bestDistance = d; best = i; }
        }
        return best;
    }

    private static int IndexOfMaxRejection(IReadOnlyList<Point3D> points, Point3D origin, Vector3D axis)
    {
        var best = 0;
        var bestLength = double.MinValue;
        for (var i = 0; i < points.Count; i++)
        {
            var l = Reject(points[i] - origin, axis).Length;
            if (l > bestLength) { bestLength = l; best = i; }
        }
        return best;
    }

    /// <summary>The component of <paramref name="v"/> perpendicular to the unit <paramref name="axis"/>.</summary>
    private static Vector3D Reject(Vector3D v, Vector3D axis) => v - v.Dot(axis) * axis;

    /// <summary>Right-handed orthonormal frame: X along primary, Z = X × secondary, Y = Z × X.</summary>
    private static Matrix3D FrameOf(Vector3D primary, Vector3D secondary)
    {
        var x = primary.Normalized();
        var z = x.Cross(secondary).Normalized();
        var y = z.Cross(x);
        return Matrix3D.Identity.SetAxes(x, y, z);
    }

    /// <summary>Transpose of a rotation-only matrix = its inverse.</summary>
    private static Matrix3D Transposed(Matrix3D r) => new(
        r[0, 0], r[1, 0], r[2, 0], 0,
        r[0, 1], r[1, 1], r[2, 1], 0,
        r[0, 2], r[1, 2], r[2, 2], 0,
        0, 0, 0, 1);
}
