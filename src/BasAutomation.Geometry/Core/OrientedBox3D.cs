namespace BasAutomation.Geometry;

/// <summary>
/// Represents a 3D oriented bounding box defined by a center point,
/// orthonormal axes, and half-extents along each axis.
/// </summary>
public readonly struct OrientedBox3D : IEquatable<OrientedBox3D>, IGeometryComparable<OrientedBox3D>
{
    /// <summary>
    /// Gets the center point of the box.
    /// </summary>
    public Point3D Center { get; }

    /// <summary>
    /// Gets the X-axis direction (normalized).
    /// </summary>
    public Vector3D XAxis { get; }

    /// <summary>
    /// Gets the Y-axis direction (normalized).
    /// </summary>
    public Vector3D YAxis { get; }

    /// <summary>
    /// Gets the Z-axis direction (normalized).
    /// </summary>
    public Vector3D ZAxis { get; }

    /// <summary>
    /// Gets the half-extent along the X-axis.
    /// </summary>
    public double HalfExtentX { get; }

    /// <summary>
    /// Gets the half-extent along the Y-axis.
    /// </summary>
    public double HalfExtentY { get; }

    /// <summary>
    /// Gets the half-extent along the Z-axis.
    /// </summary>
    public double HalfExtentZ { get; }

    /// <summary>
    /// Gets the full width (X dimension).
    /// </summary>
    public double Width => HalfExtentX * 2;

    /// <summary>
    /// Gets the full height (Y dimension).
    /// </summary>
    public double Height => HalfExtentY * 2;

    /// <summary>
    /// Gets the full depth (Z dimension).
    /// </summary>
    public double Depth => HalfExtentZ * 2;

    /// <summary>
    /// Gets the volume of the box.
    /// </summary>
    public double Volume => Width * Height * Depth;

    #region Face Planes

    /// <summary>
    /// Gets the +X face plane (right side).
    /// </summary>
    public Plane3D FaceXPos => new(Center + XAxis * HalfExtentX, XAxis);

    /// <summary>
    /// Gets the -X face plane (left side).
    /// </summary>
    public Plane3D FaceXNeg => new(Center - XAxis * HalfExtentX, -XAxis);

    /// <summary>
    /// Gets the +Y face plane (top).
    /// </summary>
    public Plane3D FaceYPos => new(Center + YAxis * HalfExtentY, YAxis);

    /// <summary>
    /// Gets the -Y face plane (bottom).
    /// </summary>
    public Plane3D FaceYNeg => new(Center - YAxis * HalfExtentY, -YAxis);

    /// <summary>
    /// Gets the +Z face plane (front/end).
    /// </summary>
    public Plane3D FaceZPos => new(Center + ZAxis * HalfExtentZ, ZAxis);

    /// <summary>
    /// Gets the -Z face plane (back/start).
    /// </summary>
    public Plane3D FaceZNeg => new(Center - ZAxis * HalfExtentZ, -ZAxis);

    /// <summary>
    /// Gets all 6 face planes.
    /// </summary>
    public IEnumerable<Plane3D> Faces
    {
        get
        {
            yield return FaceXPos;
            yield return FaceXNeg;
            yield return FaceYPos;
            yield return FaceYNeg;
            yield return FaceZPos;
            yield return FaceZNeg;
        }
    }

    /// <summary>
    /// Gets the 4 side faces (excludes Z+ and Z- faces).
    /// For a beam, these are the faces along the length, not the end caps.
    /// </summary>
    public IEnumerable<Plane3D> SideFaces
    {
        get
        {
            yield return FaceXPos;
            yield return FaceXNeg;
            yield return FaceYPos;
            yield return FaceYNeg;
        }
    }

    #endregion

    /// <summary>
    /// Creates an oriented box from center, axes, and half-extents.
    /// </summary>
    /// <param name="center">The center point of the box.</param>
    /// <param name="xAxis">The X-axis direction (will be normalized).</param>
    /// <param name="yAxis">The Y-axis direction (will be normalized).</param>
    /// <param name="zAxis">The Z-axis direction (will be normalized).</param>
    /// <param name="halfExtentX">The half-extent along the X-axis.</param>
    /// <param name="halfExtentY">The half-extent along the Y-axis.</param>
    /// <param name="halfExtentZ">The half-extent along the Z-axis.</param>
    public OrientedBox3D(
        Point3D center,
        Vector3D xAxis,
        Vector3D yAxis,
        Vector3D zAxis,
        double halfExtentX,
        double halfExtentY,
        double halfExtentZ)
    {
        Center = center;
        XAxis = xAxis.Normalized();
        YAxis = yAxis.Normalized();
        ZAxis = zAxis.Normalized();
        HalfExtentX = halfExtentX;
        HalfExtentY = halfExtentY;
        HalfExtentZ = halfExtentZ;
    }

    /// <summary>
    /// Creates an oriented box from a coordinate system and half-extents.
    /// </summary>
    /// <param name="coordinateSystem">The coordinate system defining center and orientation.</param>
    /// <param name="halfExtentX">The half-extent along the X-axis.</param>
    /// <param name="halfExtentY">The half-extent along the Y-axis.</param>
    /// <param name="halfExtentZ">The half-extent along the Z-axis.</param>
    public OrientedBox3D(
        CoordinateSystem3D coordinateSystem,
        double halfExtentX,
        double halfExtentY,
        double halfExtentZ)
        : this(
            coordinateSystem.Origin,
            coordinateSystem.XAxis,
            coordinateSystem.YAxis,
            coordinateSystem.ZAxis,
            halfExtentX,
            halfExtentY,
            halfExtentZ)
    {
    }

    /// <summary>
    /// Determines whether the box contains the specified point.
    /// </summary>
    /// <param name="point">The point to test.</param>
    /// <param name="tolerance">Tolerance for the containment test.</param>
    /// <returns>True if the point is inside or on the box surface.</returns>
    public bool Contains(Point3D point, double tolerance = Constants.Tolerance)
    {
        // Transform point to local coordinates relative to center
        var toPoint = point - Center;

        // Project onto each axis and check against half-extents
        var projX = Math.Abs(toPoint.Dot(XAxis));
        var projY = Math.Abs(toPoint.Dot(YAxis));
        var projZ = Math.Abs(toPoint.Dot(ZAxis));

        return projX <= HalfExtentX + tolerance &&
               projY <= HalfExtentY + tolerance &&
               projZ <= HalfExtentZ + tolerance;
    }

    /// <summary>
    /// Finds the intersection of a ray with this box.
    /// Returns the nearest intersection point and the face that was hit.
    /// </summary>
    /// <param name="rayOrigin">The origin of the ray.</param>
    /// <param name="rayDirection">The direction of the ray.</param>
    /// <param name="hitPoint">The intersection point (if found).</param>
    /// <param name="hitFace">The face plane that was hit (if found).</param>
    /// <returns>True if the ray intersects the box.</returns>
    public bool IntersectRay(
        Point3D rayOrigin,
        Vector3D rayDirection,
        out Point3D hitPoint,
        out Plane3D hitFace)
    {
        hitPoint = default;
        hitFace = default;

        double nearestDistance = double.MaxValue;
        bool foundHit = false;

        foreach (var face in Faces)
        {
            if (face.IntersectRay(rayOrigin, rayDirection, out var intersection))
            {
                // Check if intersection point is within the face bounds
                if (IsPointOnFace(intersection, face))
                {
                    var distance = (intersection - rayOrigin).Length;
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        hitPoint = intersection;
                        hitFace = face;
                        foundHit = true;
                    }
                }
            }
        }

        return foundHit;
    }

    /// <summary>
    /// Finds the intersection of a ray with the side faces only (excludes Z+/Z- faces).
    /// This is useful for beam-to-beam intersection where you want to hit the sides, not the ends.
    /// Only returns hits within the actual face bounds.
    /// </summary>
    /// <param name="rayOrigin">The origin of the ray.</param>
    /// <param name="rayDirection">The direction of the ray.</param>
    /// <param name="hitPoint">The intersection point (if found).</param>
    /// <param name="hitFace">The face plane that was hit (if found).</param>
    /// <returns>True if the ray intersects a side face.</returns>
    public bool IntersectRaySideFaces(
        Point3D rayOrigin,
        Vector3D rayDirection,
        out Point3D hitPoint,
        out Plane3D hitFace)
    {
        hitPoint = default;
        hitFace = default;

        double nearestDistance = double.MaxValue;
        bool foundHit = false;

        foreach (var face in SideFaces)
        {
            if (face.IntersectRay(rayOrigin, rayDirection, out var intersection))
            {
                // Check if intersection point is within the face bounds
                if (IsPointOnFace(intersection, face))
                {
                    var distance = (intersection - rayOrigin).Length;
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        hitPoint = intersection;
                        hitFace = face;
                        foundHit = true;
                    }
                }
            }
        }

        return foundHit;
    }

    /// <summary>
    /// Finds intersections of a line with the infinite side planes (not bounded by face edges).
    /// This works even when beams don't physically overlap - it finds where the line
    /// would intersect the extended planes of the beam's sides.
    /// </summary>
    /// <param name="lineOrigin">A point on the line.</param>
    /// <param name="lineDirection">The direction of the line.</param>
    /// <returns>List of (point, plane, t) tuples for all plane intersections, ordered by t (signed distance along line direction).</returns>
    public List<(Point3D Point, Plane3D Plane, double T)> IntersectLineSidePlanes(
        Point3D lineOrigin,
        Vector3D lineDirection)
    {
        var hits = new List<(Point3D Point, Plane3D Plane, double T)>();
        lineDirection = lineDirection.Normalized();

        foreach (var plane in SideFaces)
        {
            var denominator = lineDirection.Dot(plane.Normal);

            // Skip if line is parallel to plane
            if (Math.Abs(denominator) < Constants.Tolerance)
                continue;

            // Find intersection point (works in both directions, unlike ray)
            // t is signed: positive = forward along direction, negative = backward
            var t = (plane.Origin - lineOrigin).Dot(plane.Normal) / denominator;
            var intersection = lineOrigin + t * lineDirection;

            hits.Add((intersection, plane, t));
        }

        // Order by t (signed distance along line direction)
        // Smallest t = entry point, largest t = exit point
        return hits
            .OrderBy(h => h.T)
            .ToList();
    }

    /// <summary>
    /// Finds the entry point (first intersection in direction of travel) of a line with the infinite side planes.
    /// </summary>
    /// <param name="lineOrigin">A point on the line.</param>
    /// <param name="lineDirection">The direction of travel along the line.</param>
    /// <param name="hitPoint">The entry intersection point (smallest t).</param>
    /// <param name="hitPlane">The plane that was hit.</param>
    /// <returns>True if any intersection was found.</returns>
    public bool IntersectLineSidePlanesEntry(
        Point3D lineOrigin,
        Vector3D lineDirection,
        out Point3D hitPoint,
        out Plane3D hitPlane)
    {
        hitPoint = default;
        hitPlane = default;

        var hits = IntersectLineSidePlanes(lineOrigin, lineDirection);
        if (hits.Count == 0)
            return false;

        // First hit (smallest t) = entry point
        hitPoint = hits[0].Point;
        hitPlane = hits[0].Plane;
        return true;
    }

    /// <summary>
    /// Finds the exit point (last intersection in direction of travel) of a line with the infinite side planes.
    /// </summary>
    /// <param name="lineOrigin">A point on the line.</param>
    /// <param name="lineDirection">The direction of travel along the line.</param>
    /// <param name="hitPoint">The exit intersection point (largest t).</param>
    /// <param name="hitPlane">The plane that was hit.</param>
    /// <returns>True if any intersection was found.</returns>
    public bool IntersectLineSidePlanesExit(
        Point3D lineOrigin,
        Vector3D lineDirection,
        out Point3D hitPoint,
        out Plane3D hitPlane)
    {
        hitPoint = default;
        hitPlane = default;

        var hits = IntersectLineSidePlanes(lineOrigin, lineDirection);
        if (hits.Count == 0)
            return false;

        // Last hit (largest t) = exit point
        var last = hits[^1];
        hitPoint = last.Point;
        hitPlane = last.Plane;
        return true;
    }

    /// <summary>
    /// Finds all intersections of a ray with this box (entry and exit points).
    /// </summary>
    /// <param name="rayOrigin">The origin of the ray.</param>
    /// <param name="rayDirection">The direction of the ray.</param>
    /// <returns>List of (point, face) tuples for all intersections, ordered by distance.</returns>
    public List<(Point3D Point, Plane3D Face)> IntersectRayAll(
        Point3D rayOrigin,
        Vector3D rayDirection)
    {
        var hits = new List<(Point3D Point, Plane3D Face, double Distance)>();

        foreach (var face in Faces)
        {
            if (face.IntersectRay(rayOrigin, rayDirection, out var intersection))
            {
                if (IsPointOnFace(intersection, face))
                {
                    var distance = (intersection - rayOrigin).Length;
                    hits.Add((intersection, face, distance));
                }
            }
        }

        return hits
            .OrderBy(h => h.Distance)
            .Select(h => (h.Point, h.Face))
            .ToList();
    }

    /// <summary>
    /// Checks if a point lies on a specific face of the box (within bounds).
    /// </summary>
    private bool IsPointOnFace(Point3D point, Plane3D face, double tolerance = Constants.Tolerance)
    {
        // Get the two axes that define the face (perpendicular to face normal)
        Vector3D axis1, axis2;
        double halfExtent1, halfExtent2;

        if (face.Normal.IsParallel(XAxis))
        {
            axis1 = YAxis;
            axis2 = ZAxis;
            halfExtent1 = HalfExtentY;
            halfExtent2 = HalfExtentZ;
        }
        else if (face.Normal.IsParallel(YAxis))
        {
            axis1 = XAxis;
            axis2 = ZAxis;
            halfExtent1 = HalfExtentX;
            halfExtent2 = HalfExtentZ;
        }
        else // Z face
        {
            axis1 = XAxis;
            axis2 = YAxis;
            halfExtent1 = HalfExtentX;
            halfExtent2 = HalfExtentY;
        }

        // Project point onto face axes relative to center
        var toPoint = point - Center;
        var proj1 = Math.Abs(toPoint.Dot(axis1));
        var proj2 = Math.Abs(toPoint.Dot(axis2));

        return proj1 <= halfExtent1 + tolerance && proj2 <= halfExtent2 + tolerance;
    }

    /// <summary>
    /// Gets the 8 corner points of the box.
    /// </summary>
    public Point3D[] GetCorners()
    {
        var dx = XAxis * HalfExtentX;
        var dy = YAxis * HalfExtentY;
        var dz = ZAxis * HalfExtentZ;

        return
        [
            Center - dx - dy - dz,
            Center + dx - dy - dz,
            Center + dx + dy - dz,
            Center - dx + dy - dz,
            Center - dx - dy + dz,
            Center + dx - dy + dz,
            Center + dx + dy + dz,
            Center - dx + dy + dz
        ];
    }

    /// <summary>
    /// Determines whether this box is equal to another box within the specified tolerance.
    /// </summary>
    public bool IsEqualTo(OrientedBox3D other, double tolerance = Constants.Tolerance)
    {
        return Center.IsEqualTo(other.Center, tolerance) &&
               XAxis.IsEqualTo(other.XAxis, tolerance) &&
               YAxis.IsEqualTo(other.YAxis, tolerance) &&
               ZAxis.IsEqualTo(other.ZAxis, tolerance) &&
               Math.Abs(HalfExtentX - other.HalfExtentX) < tolerance &&
               Math.Abs(HalfExtentY - other.HalfExtentY) < tolerance &&
               Math.Abs(HalfExtentZ - other.HalfExtentZ) < tolerance;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is OrientedBox3D other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(OrientedBox3D other) => IsEqualTo(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Center, XAxis, YAxis, ZAxis, HalfExtentX, HalfExtentY, HalfExtentZ);

    /// <summary>
    /// Determines whether two boxes are equal.
    /// </summary>
    public static bool operator ==(OrientedBox3D left, OrientedBox3D right) => left.Equals(right);

    /// <summary>
    /// Determines whether two boxes are not equal.
    /// </summary>
    public static bool operator !=(OrientedBox3D left, OrientedBox3D right) => !left.Equals(right);

    /// <inheritdoc/>
    public override string ToString() =>
        $"OrientedBox3D(Center: {Center}, Size: {Width:F2} x {Height:F2} x {Depth:F2})";
}
