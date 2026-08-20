namespace BasAutomation.Geometry.Curves;

/// <summary>
/// Represents a circular arc curve in 3D space.
/// </summary>
public readonly struct Arc3D : ICurve3D
{
    public Arc3D(Point3D center, Vector3D normal, Vector3D xAxis, double radius,
        double startAngle, double endAngle, bool isCounterClockwise = true)
    {
        Center = center;
        Normal = normal.Normalized();
        XAxis = xAxis.Normalized();
        Radius = radius;
        StartAngle = startAngle;
        EndAngle = endAngle;
        IsCounterClockwise = isCounterClockwise;
    }

    // TODO: Test
    /// <summary>
    /// Creates an arc from start point, end point, and center.
    /// The normal is calculated from the three points.
    /// </summary>
    /// <param name="startPoint">The start point of the arc</param>
    /// <param name="endPoint">The end point of the arc</param>
    /// <param name="center">The center point of the arc</param>
    /// <param name="isCounterClockwise">Whether the arc follows counter-clockwise direction when viewed along the calculated normal</param>
    public Arc3D(Point3D startPoint, Point3D endPoint, Point3D center, bool isCounterClockwise = true)
    {
        Center = center;
        IsCounterClockwise = isCounterClockwise;

        // Calculate vectors from center to start and end
        var toStart = startPoint - center;
        var toEnd = endPoint - center;

        // Calculate radius from center to start point
        Radius = toStart.Length;

        // Validate that end point is also at the same radius (within tolerance)
        var endRadius = toEnd.Length;
        if (Math.Abs(endRadius - Radius) > 1e-10)
        {
            throw new ArgumentException($"Start and end points must be equidistant from center. Start radius: {Radius}, End radius: {endRadius}");
        }

        // Check if points are collinear (degenerate case)
        var cross = toStart.Cross(toEnd);
        if (cross.Length < 1e-10)
        {
            throw new ArgumentException("Start point, end point, and center are collinear. Cannot form an arc.");
        }

        // Calculate normal from the cross product
        // The direction depends on whether we want CCW or CW
        Normal = isCounterClockwise ? cross.Normalized() : (-cross).Normalized();

        // Normalize the vectors
        toStart = toStart.Normalized();
        toEnd = toEnd.Normalized();

        // Set XAxis as the direction to the start point
        XAxis = toStart;

        // Calculate YAxis perpendicular to both normal and XAxis
        var yAxis = Normal.Cross(XAxis).Normalized();

        // Calculate start angle (should be 0 since XAxis points to start)
        StartAngle = 0;

        // Calculate end angle
        var cosAngle = toEnd.Dot(XAxis);
        var sinAngle = toEnd.Dot(yAxis);
        EndAngle = Math.Atan2(sinAngle, cosAngle);

        // For the calculated normal, the angle should always be positive for CCW
        // If it's negative, we need to add 2π
        if (EndAngle < 0)
        {
            EndAngle = EndAngle + 2 * Math.PI;
        }
    }

    /// <summary>
    /// Creates an arc from three points: start, a point on the arc, and end.
    /// </summary>
    /// <param name="startPoint">The start point of the arc</param>
    /// <param name="midPoint">A point on the arc between start and end</param>
    /// <param name="endPoint">The end point of the arc</param>
    public Arc3D(Point3D startPoint, Point3D midPoint, Point3D endPoint)
    {
        // Calculate the center using the three points
        // The center is equidistant from all three points

        // Vectors from start to mid and start to end
        var v1 = midPoint - startPoint;
        var v2 = endPoint - startPoint;

        // Calculate normal (perpendicular to the plane containing the three points)
        var cross = v1.Cross(v2);
        if (cross.LengthSquared < 1e-20)
        {
            throw new ArgumentException("The three points are collinear and do not define an arc.");
        }
        Normal = cross.Normalized();

        // Find center using perpendicular bisectors
        // Midpoint of start-mid chord
        var mid1 = startPoint + v1 * 0.5;

        // Midpoint of start-end chord  
        var mid2 = startPoint + v2 * 0.5;

        // Direction perpendicular to start-mid chord in the plane
        var perp1 = Normal.Cross(v1).Normalized();
        // Direction perpendicular to start-end chord in the plane
        var perp2 = Normal.Cross(v2).Normalized();

        // Find intersection of the two perpendicular bisectors
        // Line 1: mid1 + t * perp1
        // Line 2: mid2 + s * perp2
        // Solve: mid1 + t * perp1 = mid2 + s * perp2

        var w = mid2 - mid1;
        var denom = perp1.Cross(perp2).Dot(Normal);
        
        if (Math.Abs(denom) < 1e-10)
        {
            throw new ArgumentException("Cannot find unique center for the arc.");
        }

        var t = w.Cross(perp2).Dot(Normal) / denom;
        Center = mid1 + perp1 * t;

        // Calculate radius
        Radius = (startPoint - Center).Length;

        // Validate all three points are at the same distance
        var radiusMid = (midPoint - Center).Length;
        var radiusEnd = (endPoint - Center).Length;
        if (Math.Abs(radiusMid - Radius) > 1e-6 || Math.Abs(radiusEnd - Radius) > 1e-6)
        {
            throw new ArgumentException($"Points are not on a circular arc. Radii: {Radius:F6}, {radiusMid:F6}, {radiusEnd:F6}");
        }

        // Set up coordinate system
        var toStart = (startPoint - Center).Normalized();
        XAxis = toStart;
        var yAxis = Normal.Cross(XAxis).Normalized();

        // Calculate angles
        StartAngle = 0; // XAxis points to start

        var toMid = (midPoint - Center).Normalized();
        var midAngle = Math.Atan2(toMid.Dot(yAxis), toMid.Dot(XAxis));
        if (midAngle < 0) midAngle += 2 * Math.PI;

        var toEnd = (endPoint - Center).Normalized();
        EndAngle = Math.Atan2(toEnd.Dot(yAxis), toEnd.Dot(XAxis));
        if (EndAngle < 0) EndAngle += 2 * Math.PI;

        // Determine direction based on whether mid is between start and end
        // If going CCW from start to end passes through mid, then it's CCW
        if (midAngle > 0 && midAngle < EndAngle)
        {
            IsCounterClockwise = true;
        }
        else if (EndAngle < midAngle && midAngle < 2 * Math.PI)
        {
            IsCounterClockwise = false;
        }
        else
        {
            // Mid is past end, so we're going the long way
            IsCounterClockwise = EndAngle > Math.PI;
        }
    }

    public Point3D Center
    {
        get;
    }

    /// <summary>
    /// Gets the normal vector perpendicular to the arc plane.
    /// </summary>
    public Vector3D Normal
    {
        get;
    }

    public Vector3D XAxis
    {
        get;
    }

    public double Radius
    {
        get;
    }

    public double StartAngle
    {
        get;
    }

    public double EndAngle
    {
        get;
    }

    public bool IsCounterClockwise
    {
        get;
    }

    public Point3D StartPoint => PointAt(0);

    public Point3D EndPoint => PointAt(1);

    public Point3D MidPoint => PointAt(0.5);

    public double Length => Math.Abs(SweepAngle) * Radius;


    public Point3D PointAt(double t)
    {
        var sweep = SweepAngle;
        double angle = StartAngle + t * sweep;

        var yAxis = Normal.Cross(XAxis).Normalized();
        var localPoint = Radius * Math.Cos(angle) * XAxis + Radius * Math.Sin(angle) * yAxis;
        return Center + localPoint;
    }

    /// <summary>
    /// Gets the sweep angle of the arc in radians.
    /// Positive for CCW arcs, negative for CW arcs.
    /// </summary>
    public double SweepAngle
    {
        get
        {
            var sweep = EndAngle - StartAngle;
            if (IsCounterClockwise)
            {
                if (sweep <= 0) sweep += 2 * Math.PI;
            }
            else
            {
                if (sweep >= 0) sweep -= 2 * Math.PI;
            }
            return sweep;
        }
    }

    public Vector3D TangentAt(double t)
    {
        var sweep = SweepAngle;
        double angle = StartAngle + t * sweep;

        // Calculate tangent vector
        var yAxis = Normal.Cross(XAxis).Normalized();

        // The position vector is: P(t) = Center + R*cos(angle)*XAxis + R*sin(angle)*YAxis
        // where angle = StartAngle + t*sweep
        // 
        // The tangent is dP/dt = dP/d(angle) * d(angle)/dt
        //                      = R*[-sin(angle)*XAxis + cos(angle)*YAxis] * sweep
        //
        // Since we want a unit tangent, we just need the direction:
        // For positive sweep: -sin(angle)*XAxis + cos(angle)*YAxis
        // For negative sweep: sin(angle)*XAxis - cos(angle)*YAxis

        var tangent = sweep * (-Math.Sin(angle) * XAxis + Math.Cos(angle) * yAxis);

        return tangent.Normalized();
    }

    public Vector3D NormalAt(double t)
    {
        // Get the point on the arc
        var point = PointAt(t);

        // The normal points from the point toward the center
        var toCenter = Center - point;

        // Normalize to get unit normal vector
        return toCenter.Normalized();
    }

    public double ParameterAtDistance(double distance)
    {
        return distance / Length;
    }

    public ICurve3D Clone()
    {
        return new Arc3D(Center, Normal, XAxis, Radius, StartAngle, EndAngle, IsCounterClockwise);
    }

    /// <summary>
    /// Creates a plane perpendicular to the curve at the specified parameter.
    /// </summary>
    /// <param name="curve">The curve to create the plane from.</param>
    /// <param name="t">The parameter along the curve (0 = start, 1 = end).</param>
    /// <returns>A plane positioned at the curve point with normal aligned to the curve tangent.</returns>
    public Plane3D CreatePlaneAt(double t)
    {
        var point = PointAt(t);
        var tangent = TangentAt(t);

        return new Plane3D(point, tangent);
    }

    /// <inheritdoc/>
    public override string ToString() => $"Arc3D(Start: {StartPoint}, Mid: {MidPoint}, End: {EndPoint}, Center: {Center}, Radius: {Radius}, StartAngle: {StartAngle:F2}, EndAngle: {EndAngle:F2}, CCW: {IsCounterClockwise})";

    /// <summary>
    /// Reverses the direction of the arc.
    /// </summary>
    /// <returns>A new arc with reversed direction.</returns>
    public Arc3D Reverse()
    {
        return new Arc3D(EndPoint, MidPoint, StartPoint);
    }

    /// <inheritdoc/>
    ICurve3D ICurve3D.Reverse()
    {
        return Reverse();
    }

    /// <summary>
    /// Transforms this arc by the given transformation matrix.
    /// </summary>
    /// <param name="matrix">The transformation matrix to apply.</param>
    /// <returns>A new arc with the transformation applied.</returns>
    public Arc3D Transform(Matrix3D matrix)
    {
        // Transform the three defining points and reconstruct the arc
        // This preserves the arc's direction and sweep correctly
        var newStart = StartPoint.Transform(matrix);
        var newMid = MidPoint.Transform(matrix);
        var newEnd = EndPoint.Transform(matrix);

        return new Arc3D(newStart, newMid, newEnd);
    }

    /// <summary>
    /// Creates a concentric arc that passes through the given start point.
    /// The new arc shares the same center axis, angular sweep, and orientation as the original.
    /// The radius is derived from the radial distance of <paramref name="startPoint"/> from the center axis.
    /// </summary>
    /// <param name="startPoint">The point the new arc should pass through at t=0</param>
    /// <returns>A new concentric arc passing through the given start point</returns>
    public Arc3D CreateConcentricArc(Point3D startPoint)
    {
        // Vector from original center to the new start point
        var toNewStart = startPoint - Center;

        // Decompose into height (along normal) and radial (in arc plane) components
        var heightAlongAxis = toNewStart.Dot(Normal);
        var radialComponent = toNewStart - Normal * heightAlongAxis;
        var newRadius = radialComponent.Length;

        // Translate center along the normal by the height component
        var newCenter = Center + Normal * heightAlongAxis;

        // Use direct constructor preserving the same angular sweep and orientation
        return new Arc3D(newCenter, Normal, XAxis, newRadius, StartAngle, EndAngle, IsCounterClockwise);
    }

    /// <summary>
    /// Creates a new arc offset perpendicular to the tangent at each point.
    /// The offset is applied in the arc's natural perpendicular frame:
    /// X component along the radial inward direction (toward the center, in the arc plane),
    /// Y component along the arc's Normal direction (perpendicular to the arc plane).
    /// </summary>
    /// <param name="offset">
    /// The 2D offset: X = along radial inward (toward the center, in the arc plane),
    /// Y = along the arc Normal (perpendicular to the arc plane).
    /// </param>
    /// <returns>A new arc with the offset applied</returns>
    /// <remarks>
    /// Preserves the arc's parametrization: the t=0 point of the result is the offset of
    /// the t=0 point of the original. Implementation note: rebuilding via
    /// <c>new Arc3D(start, mid, end)</c> would flip the parametrization when
    /// <c>offset.X > Radius</c> (the offset crosses through the arc center), because the
    /// 3-point constructor recomputes the sweep direction from scratch. We instead reuse
    /// the original (Center, Normal, XAxis, StartAngle, EndAngle, CCW) and feed in a
    /// signed radius — <c>PointAt</c>'s <c>R * cos(angle) * X + R * sin(angle) * Y</c>
    /// formula handles negative R as "the point on the opposite side of the center", which
    /// is exactly the offset semantics we want when the offset crosses the center.
    /// </remarks>
    public Arc3D Offset(Vector2D offset)
    {
        var newCenter = Center + offset.Y * Normal;
        var newRadius = Radius - offset.X;

        return new Arc3D(newCenter, Normal, XAxis, newRadius, StartAngle, EndAngle, IsCounterClockwise);
    }

    /// <summary>
    /// Returns a copy of this arc with its start and/or end point snapped onto the given planes
    /// by sliding them along the arc's circle. If a plane is null, parallel to the arc plane, or
    /// doesn't intersect the arc circle, that endpoint is left unchanged.
    /// </summary>
    /// <remarks>
    /// Useful to restore an "endpoint lies on plane" invariant after an offset has shifted a
    /// curved centerline off its path arc.
    /// </remarks>
    public Arc3D SnapToPlanes(Plane3D? startPlane, Plane3D? endPlane)
    {
        var newStart = SnapEndpointToPlane(StartPoint, startPlane) ?? StartPoint;
        var newEnd = SnapEndpointToPlane(EndPoint, endPlane) ?? EndPoint;

        if (newStart.IsEqualTo(StartPoint, Constants.Tolerance)
            && newEnd.IsEqualTo(EndPoint, Constants.Tolerance))
        {
            return this;
        }

        // Rebuild preserving center / normal / radius / sweep direction. Recompute start/end
        // angles in the arc's own frame (XAxis, Normal × XAxis).
        var yAxis = Normal.Cross(XAxis).Normalized();
        var newStartAngle = AngleInArcFrame(newStart, yAxis);
        var newEndAngle = AngleInArcFrame(newEnd, yAxis);

        return new Arc3D(Center, Normal, XAxis, Radius, newStartAngle, newEndAngle, IsCounterClockwise);
    }

    private Point3D? SnapEndpointToPlane(Point3D endpoint, Plane3D? plane)
    {
        if (plane is not { } p) return null;

        // Already on the plane → nothing to do.
        if (p.DistanceToPoint(endpoint) <= Constants.Tolerance)
            return endpoint;

        // Intersect the target plane with the arc's plane to get a line in the arc plane.
        var arcPlane = new Plane3D(Center, Normal);
        if (!p.IntersectPlane(arcPlane, out var lineOrigin, out var lineDir))
            return null; // target plane parallel to arc plane

        lineDir = lineDir.Normalized();

        // Intersect the line with the arc's circle: solve |lineOrigin + t*lineDir - Center|² = R².
        var oc = lineOrigin - Center;
        var b = 2.0 * oc.Dot(lineDir);
        var c = oc.Dot(oc) - Radius * Radius;
        var discriminant = b * b - 4.0 * c;

        if (discriminant < -Constants.Tolerance)
            return null; // line misses the circle

        var sqrtD = Math.Sqrt(Math.Max(0, discriminant));
        var t1 = (-b + sqrtD) / 2.0;
        var t2 = (-b - sqrtD) / 2.0;
        var p1 = lineOrigin + t1 * lineDir;
        var p2 = lineOrigin + t2 * lineDir;

        // Pick the root closer to the original endpoint so we trim/extend minimally.
        return (p1 - endpoint).Length <= (p2 - endpoint).Length ? p1 : p2;
    }

    private double AngleInArcFrame(Point3D point, Vector3D yAxis)
    {
        var toPoint = point - Center;
        var angle = Math.Atan2(toPoint.Dot(yAxis), toPoint.Dot(XAxis));
        if (angle < 0) angle += 2 * Math.PI;
        return angle;
    }
}