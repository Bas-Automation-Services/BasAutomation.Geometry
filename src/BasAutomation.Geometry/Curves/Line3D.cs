namespace BasAutomation.Geometry.Curves;

/// <summary>
/// Represents a 3D line segment defined by start and end points.
/// </summary>
/// <param name="startPoint">The start point of the line.</param>
/// <param name="endPoint">The end point of the line.</param>
public readonly struct Line3D(Point3D startPoint, Point3D endPoint)
    : IEquatable<Line3D>, IGeometryComparable<Line3D>, ICurve3D
{
    /// <summary>
    /// Gets or sets the start point of the line.
    /// </summary>
    public Point3D StartPoint { get; } = startPoint;

    /// <summary>
    /// Gets or sets the end point of the line.
    /// </summary>
    public Point3D EndPoint { get; } = endPoint;

    /// <summary>
    /// Gets the normalized direction vector of the line.
    /// </summary>
    public Vector3D Direction => (EndPoint - StartPoint).Normalized();

    /// <summary>
    /// Gets the length of the line.
    /// </summary>
    public double Length => StartPoint.DistanceTo(EndPoint);

    /// <summary>
    /// Gets a point along the line at the specified parameter.
    /// </summary>
    /// <param name="t">The parameter (0 = start, 1 = end).</param>
    /// <returns>The point at the specified parameter.</returns>
    public Point3D PointAt(double t) => StartPoint + t * (EndPoint - StartPoint);

    /// <summary>
    /// Gets the midpoint of the line.
    /// </summary>
    public Point3D MidPoint => PointAt(0.5);

    /// <summary>
    /// Determines whether this line is equal to another line within the specified tolerance.
    /// </summary>
    /// <param name="other">The other line to compare.</param>
    /// <param name="tolerance">The tolerance for floating-point comparison. Default is 1e-10.</param>
    /// <returns>true if the lines are equal within tolerance; otherwise, false.</returns>
    public bool IsEqualTo(Line3D other, double tolerance = 1e-10)
    {
        return StartPoint.IsEqualTo(other.StartPoint, tolerance) && EndPoint.IsEqualTo(other.EndPoint, tolerance);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj) => obj is Line3D other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(Line3D other) => IsEqualTo(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(StartPoint, EndPoint);

    /// <summary>
    /// Determines whether two lines are equal.
    /// </summary>
    public static bool operator ==(Line3D left, Line3D right) => left.Equals(right);

    /// <summary>
    /// Determines whether two lines are not equal.
    /// </summary>
    public static bool operator !=(Line3D left, Line3D right) => !left.Equals(right);

    public Vector3D TangentAt(double t)
    {
        return Direction;
    }

    public Vector3D NormalAt(double t, Vector3D upHint)
    {
        var tangent = TangentAt(t);
        var cross = tangent.Cross(upHint);

        // If parallel, try alternative up vector
        if (cross.LengthSquared < Constants.Tolerance)
        {
            throw new InvalidOperationException($"up hint {upHint} is parallel to direction");
        }

        return cross.Normalized();
    }

    /// <summary>
    /// Returns a normal to the line, but uses a default reference vector.
    /// Uses the z-axis if the tangent is not close to vertical, otherwise uses the x-axis.
    /// </summary>
    /// <param name="t">The parameter value between 0 and 1</param>
    /// <returns></returns>
    public Vector3D NormalAt(double t)
    {
        var tangent = TangentAt(t);
        var reference = Math.Abs(tangent.Z) < 0.9 ? Vector3D.ZAxis : Vector3D.XAxis;
        var cross = tangent.Cross(reference);
        return cross.Normalized();
    }

    public double ParameterAtDistance(double distance)
    {
        return distance / Length;
    }

    public ICurve3D Clone()
    {
        return new Line3D(StartPoint, EndPoint);
    }

    /// <inheritdoc/>
    public override string ToString() => $"Line3D(Start: {StartPoint}, End: {EndPoint}, Direction: {Direction})";
    
    /// <summary>
    /// Creates a plane perpendicular to the curve at the specified parameter.
    /// </summary>
    /// <param name="t">The parameter along the curve (0 = start, 1 = end).</param>
    /// <returns>A plane positioned at the curve point with normal aligned to the curve tangent.</returns>
    public Plane3D CreatePlaneAt(double t)
    {
        var point = PointAt(t);
        var tangent = TangentAt(t);

        return new Plane3D(point, tangent);
    }

    /// <summary>
    /// Reverses the direction of the line.
    /// </summary>
    /// <returns>A new line with reversed direction.</returns>
    public Line3D Reverse()
    {
        return new Line3D(EndPoint, StartPoint);
    }

    /// <inheritdoc/>
    ICurve3D ICurve3D.Reverse()
    {
        return Reverse();
    }

    /// <summary>
    /// Returns a copy of this line with its start and/or end point snapped onto the given planes
    /// by extending/trimming along the line's direction. If a plane is null or parallel to the
    /// line direction (within tolerance), that endpoint is left unchanged.
    /// </summary>
    /// <remarks>
    /// Useful to keep a centerline's endpoints on their miter planes after an offset has shifted
    /// the centerline off its path line.
    /// </remarks>
    public Line3D SnapToPlanes(Plane3D? startPlane, Plane3D? endPlane)
    {
        var direction = Direction;
        var newStart = StartPoint;
        var newEnd = EndPoint;

        if (startPlane is { } sp)
        {
            var denom = direction.Dot(sp.Normal);
            if (Math.Abs(denom) > Constants.Tolerance)
            {
                var t = (sp.Origin - StartPoint).Dot(sp.Normal) / denom;
                newStart = StartPoint + t * direction;
            }
        }

        if (endPlane is { } ep)
        {
            var denom = direction.Dot(ep.Normal);
            if (Math.Abs(denom) > Constants.Tolerance)
            {
                var t = (ep.Origin - EndPoint).Dot(ep.Normal) / denom;
                newEnd = EndPoint + t * direction;
            }
        }

        return new Line3D(newStart, newEnd);
    }

    /// <summary>
    /// Finds the point on this line that is closest to another line.
    /// Works for skew lines (lines not in the same plane).
    /// </summary>
    /// <param name="other">The other line.</param>
    /// <returns>The point on this line closest to the other line.</returns>
    public Point3D ClosestPointTo(Line3D other)
    {
        var p1 = StartPoint;
        var d1 = Direction;
        var p2 = other.StartPoint;
        var d2 = other.Direction;

        var w0 = p1 - p2;

        var a = d1.Dot(d1); // always >= 0
        var b = d1.Dot(d2);
        var c = d2.Dot(d2); // always >= 0
        var d = d1.Dot(w0);
        var e = d2.Dot(w0);

        var denom = a * c - b * b;

        // If lines are parallel, return projection of other's start onto this line
        if (Math.Abs(denom) < Constants.Tolerance)
        {
            return p1 + (-d / a) * d1;
        }

        // Parameter on this line for closest point
        var t = (b * e - c * d) / denom;

        return p1 + t * d1;
    }
}
