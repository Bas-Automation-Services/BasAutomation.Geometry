namespace BasAutomation.Geometry.RayCasting;

/// <summary>
/// Utility methods for ray-geometry intersection operations.
/// </summary>
public static class RayIntersectionUtilities
{
    /// <summary>
    /// Intersects a ray with a plane.
    /// </summary>
    /// <param name="ray">The ray to intersect.</param>
    /// <param name="plane">The plane to intersect with.</param>
    /// <param name="intersection">The intersection point if found.</param>
    /// <param name="distance">The distance along the ray to the intersection point.</param>
    /// <returns>true if the ray intersects the plane; otherwise, false.</returns>
    public static bool IntersectRayWithPlane(Ray3D ray, Plane3D plane, out Point3D intersection, out double distance)
    {
        intersection = new Point3D(0, 0, 0);
        distance = 0;

        var denominator = ray.Direction.Dot(plane.Normal);

        // Check if ray is parallel to plane
        if (Math.Abs(denominator) < Constants.Tolerance)
            return false;

        var t = (plane.Origin - ray.Origin).Dot(plane.Normal) / denominator;

        // Check if intersection is in the positive direction of the ray
        if (t < 0)
            return false;

        distance = t;
        intersection = ray.PointAt(t);
        return true;
    }

    /// <summary>
    /// Casts a ray from a point on a plane and finds the distance to another parallel plane.
    /// </summary>
    /// <param name="startPoint">The starting point on the first plane.</param>
    /// <param name="rayDirection">The direction to cast the ray (typically the plane normal).</param>
    /// <param name="targetPlane">The target plane to intersect.</param>
    /// <param name="intersection">The intersection point on the target plane.</param>
    /// <param name="distance">The distance from start point to intersection.</param>
    /// <returns>true if intersection found; otherwise, false.</returns>
    public static bool CastRayToPlane(Point3D startPoint, Vector3D rayDirection, Plane3D targetPlane,
        out Point3D intersection, out double distance)
    {
        var ray = new Ray3D(startPoint, rayDirection);
        return IntersectRayWithPlane(ray, targetPlane, out intersection, out distance);
    }
}
