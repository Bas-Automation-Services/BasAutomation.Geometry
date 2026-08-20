namespace BasAutomation.Geometry.Curves;

/// <summary>
/// Represents a 2D ellipse or elliptical arc defined by center, radii, rotation, and angular parameters.
///
/// Ellipse parametric equation (axis-aligned):
///   x = center.X + majorRadius * cos(θ)
///   y = center.Y + minorRadius * sin(θ)
///
/// With rotation:
///   x = center.X + majorRadius * cos(θ) * cos(rotation) - minorRadius * sin(θ) * sin(rotation)
///   y = center.Y + majorRadius * cos(θ) * sin(rotation) + minorRadius * sin(θ) * cos(rotation)
///
/// Where θ is the eccentric anomaly parameter (0 to 2π for full ellipse).
/// </summary>
public readonly struct Ellipse2D : IEquatable<Ellipse2D>, IGeometryComparable<Ellipse2D>, ICurve2D
{
    /// <summary>
    /// Gets the center point of the ellipse.
    /// </summary>
    public Point2D Center { get; }

    /// <summary>
    /// Gets the semi-major axis length (larger radius).
    /// </summary>
    public double MajorRadius { get; }

    /// <summary>
    /// Gets the semi-minor axis length (smaller radius).
    /// </summary>
    public double MinorRadius { get; }

    /// <summary>
    /// Gets the rotation angle of the major axis from the positive X-axis (in radians).
    /// </summary>
    public double Rotation { get; }

    /// <summary>
    /// Gets the start parameter (eccentric anomaly) in radians.
    /// </summary>
    public double StartParameter { get; }

    /// <summary>
    /// Gets the end parameter (eccentric anomaly) in radians.
    /// </summary>
    public double EndParameter { get; }

    /// <summary>
    /// Gets whether the ellipse arc is traversed counterclockwise.
    /// </summary>
    public bool IsCounterClockwise { get; }

    /// <summary>
    /// Creates a new Ellipse2D with the specified parameters.
    /// </summary>
    /// <param name="center">Center point of the ellipse.</param>
    /// <param name="majorRadius">Semi-major axis length.</param>
    /// <param name="minorRadius">Semi-minor axis length.</param>
    /// <param name="rotation">Rotation angle of major axis from X-axis (radians).</param>
    /// <param name="startParameter">Start parameter (radians).</param>
    /// <param name="endParameter">End parameter (radians).</param>
    /// <param name="isCounterClockwise">Whether arc is traversed counterclockwise.</param>
    public Ellipse2D(Point2D center, double majorRadius, double minorRadius, double rotation,
        double startParameter, double endParameter, bool isCounterClockwise = true)
    {
        Center = center;
        MajorRadius = majorRadius;
        MinorRadius = minorRadius;
        Rotation = NormalizeAngle(rotation);
        StartParameter = NormalizeAngle(startParameter);
        EndParameter = NormalizeAngle(endParameter);
        IsCounterClockwise = isCounterClockwise;
    }

    /// <summary>
    /// Gets whether this is a circular ellipse (major ≈ minor radius).
    /// </summary>
    public bool IsCircular => Math.Abs(MajorRadius - MinorRadius) < Constants.Tolerance;

    /// <summary>
    /// Gets whether this represents a full ellipse (start to end spans 2π).
    /// </summary>
    public bool IsFullEllipse
    {
        get
        {
            var sweep = Math.Abs(GetSweepAngle());
            return Math.Abs(sweep - 2 * Math.PI) < Constants.Tolerance;
        }
    }

    /// <summary>
    /// Gets the sweep angle (parameter range) in radians.
    /// </summary>
    private double GetSweepAngle()
    {
        var sweep = EndParameter - StartParameter;
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

    /// <summary>
    /// Gets the start point of the ellipse arc.
    /// </summary>
    public Point2D StartPoint => PointAtParameter(StartParameter);

    /// <summary>
    /// Gets the end point of the ellipse arc.
    /// </summary>
    public Point2D EndPoint => PointAtParameter(EndParameter);

    /// <summary>
    /// Gets the arc length of the ellipse.
    /// Uses Ramanujan's approximation for full ellipse perimeter,
    /// scaled for partial arcs.
    /// </summary>
    public double Length
    {
        get
        {
            if (IsCircular)
            {
                // Circular: use exact arc length formula
                return Math.Abs(GetSweepAngle()) * MajorRadius;
            }

            // Ellipse: use Ramanujan's approximation scaled by sweep fraction
            var a = MajorRadius;
            var b = MinorRadius;
            var fullPerimeter = Math.PI * (3 * (a + b) - Math.Sqrt((3 * a + b) * (a + 3 * b)));
            var sweepFraction = Math.Abs(GetSweepAngle()) / (2 * Math.PI);
            return fullPerimeter * sweepFraction;
        }
    }

    /// <summary>
    /// Gets a point on the ellipse at the specified normalized parameter.
    /// </summary>
    /// <param name="t">Parameter from 0 (start) to 1 (end).</param>
    public Point2D PointAt(double t)
    {
        var theta = StartParameter + t * GetSweepAngle();
        return PointAtParameter(theta);
    }

    /// <summary>
    /// Gets a point on the ellipse at the specified eccentric anomaly parameter.
    /// </summary>
    /// <param name="theta">Eccentric anomaly in radians.</param>
    private Point2D PointAtParameter(double theta)
    {
        var cosTheta = Math.Cos(theta);
        var sinTheta = Math.Sin(theta);
        var cosRot = Math.Cos(Rotation);
        var sinRot = Math.Sin(Rotation);

        var x = Center.X + MajorRadius * cosTheta * cosRot - MinorRadius * sinTheta * sinRot;
        var y = Center.Y + MajorRadius * cosTheta * sinRot + MinorRadius * sinTheta * cosRot;

        return new Point2D(x, y);
    }

    /// <summary>
    /// Gets the tangent vector at the specified normalized parameter.
    /// </summary>
    /// <param name="t">Parameter from 0 (start) to 1 (end).</param>
    public Vector2D TangentAt(double t)
    {
        var theta = StartParameter + t * GetSweepAngle();

        var cosTheta = Math.Cos(theta);
        var sinTheta = Math.Sin(theta);
        var cosRot = Math.Cos(Rotation);
        var sinRot = Math.Sin(Rotation);

        // Derivative of parametric equation with respect to theta
        var dx = -MajorRadius * sinTheta * cosRot - MinorRadius * cosTheta * sinRot;
        var dy = -MajorRadius * sinTheta * sinRot + MinorRadius * cosTheta * cosRot;

        var tangent = new Vector2D(dx, dy).Normalized();

        // Reverse direction for clockwise arc
        return IsCounterClockwise ? tangent : -tangent;
    }

    /// <summary>
    /// Gets the bounding box of the ellipse arc.
    /// </summary>
    public Box2D BoundingBox => GetBoundingBox();

    /// <summary>
    /// Calculates the bounding box of the ellipse arc.
    /// </summary>
    public Box2D GetBoundingBox()
    {
        var points = new List<Point2D> { StartPoint, EndPoint };

        // For a full or partial ellipse, check extrema points
        // Extrema occur where the tangent is horizontal or vertical
        // For rotated ellipse, these are at specific theta values

        // Check cardinal parameter values (0, π/2, π, 3π/2) if they're in range
        var cardinalParams = new[] { 0.0, Math.PI / 2, Math.PI, 3 * Math.PI / 2 };
        foreach (var param in cardinalParams)
        {
            if (IsParameterInRange(param))
            {
                points.Add(PointAtParameter(param));
            }
        }

        // Also check the rotated extrema (where dx/dθ = 0 or dy/dθ = 0)
        // For rotated ellipse extrema, we need to solve:
        // dx/dθ = 0 => tan(θ) = -MinorRadius * tan(Rotation) / MajorRadius (for x extrema)
        // dy/dθ = 0 => tan(θ) = MinorRadius * cot(Rotation) / MajorRadius (for y extrema)
        var extremaParams = CalculateExtremaParameters();
        foreach (var param in extremaParams)
        {
            if (IsParameterInRange(param))
            {
                points.Add(PointAtParameter(param));
            }
        }

        var minX = points.Min(p => p.X);
        var minY = points.Min(p => p.Y);
        var maxX = points.Max(p => p.X);
        var maxY = points.Max(p => p.Y);

        return new Box2D(new Point2D(minX, minY), new Point2D(maxX, maxY));
    }

    /// <summary>
    /// Calculates the parameters where the ellipse has extrema in X and Y.
    /// </summary>
    private double[] CalculateExtremaParameters()
    {
        var results = new List<double>();

        // X extrema: dx/dθ = 0
        // -MajorRadius * sin(θ) * cos(Rotation) - MinorRadius * cos(θ) * sin(Rotation) = 0
        // tan(θ) = -MinorRadius * tan(Rotation) / MajorRadius
        if (Math.Abs(Math.Cos(Rotation)) > Constants.Tolerance)
        {
            var tanRot = Math.Tan(Rotation);
            var thetaX = Math.Atan2(-MinorRadius * tanRot, MajorRadius);
            results.Add(NormalizeAngle(thetaX));
            results.Add(NormalizeAngle(thetaX + Math.PI));
        }
        else
        {
            // Rotation is ±90°, X extrema at θ = 0 and π
            results.Add(0);
            results.Add(Math.PI);
        }

        // Y extrema: dy/dθ = 0
        // -MajorRadius * sin(θ) * sin(Rotation) + MinorRadius * cos(θ) * cos(Rotation) = 0
        // tan(θ) = MinorRadius * cot(Rotation) / MajorRadius = MinorRadius / (MajorRadius * tan(Rotation))
        if (Math.Abs(Math.Sin(Rotation)) > Constants.Tolerance)
        {
            var cotRot = 1.0 / Math.Tan(Rotation);
            var thetaY = Math.Atan2(MinorRadius * cotRot, MajorRadius);
            results.Add(NormalizeAngle(thetaY));
            results.Add(NormalizeAngle(thetaY + Math.PI));
        }
        else
        {
            // Rotation is 0 or 180°, Y extrema at θ = π/2 and 3π/2
            results.Add(Math.PI / 2);
            results.Add(3 * Math.PI / 2);
        }

        return results.ToArray();
    }

    /// <summary>
    /// Checks if a parameter value is within the arc's range.
    /// </summary>
    private bool IsParameterInRange(double param)
    {
        // For full ellipse, all parameters are in range
        if (IsFullEllipse)
            return true;

        param = NormalizeAngle(param);
        var start = NormalizeAngle(StartParameter);
        var end = NormalizeAngle(EndParameter);

        if (IsCounterClockwise)
        {
            if (start < end)
                return param >= start && param <= end;
            else
                return param >= start || param <= end;
        }
        else
        {
            if (start > end)
                return param <= start && param >= end;
            else
                return param <= start || param >= end;
        }
    }

    /// <summary>
    /// Creates a clone of this ellipse.
    /// </summary>
    public ICurve2D Clone()
    {
        return new Ellipse2D(Center, MajorRadius, MinorRadius, Rotation, StartParameter, EndParameter, IsCounterClockwise);
    }

    /// <summary>
    /// Transforms the ellipse by the specified matrix.
    /// Note: Non-uniform scaling may produce incorrect results.
    /// </summary>
    public ICurve2D Transform(Matrix2D matrix)
    {
        var newCenter = matrix.Transform(Center);

        // For translation-only transforms, preserve radii and rotation
        // For more complex transforms, this is an approximation
        return new Ellipse2D(newCenter, MajorRadius, MinorRadius, Rotation, StartParameter, EndParameter, IsCounterClockwise);
    }

    /// <inheritdoc/>
    public ICurve2D Reverse()
    {
        return new Ellipse2D(Center, MajorRadius, MinorRadius, Rotation, EndParameter, StartParameter, !IsCounterClockwise);
    }

    /// <summary>
    /// Normalizes an angle to the range [0, 2π).
    /// </summary>
    private static double NormalizeAngle(double angle)
    {
        angle = angle % (2 * Math.PI);
        if (angle < 0) angle += 2 * Math.PI;
        return angle;
    }

    /// <summary>
    /// Determines whether this ellipse is equal to another within tolerance.
    /// </summary>
    public bool IsEqualTo(Ellipse2D other, double tolerance = Constants.Tolerance)
    {
        return Center.IsEqualTo(other.Center, tolerance) &&
               Math.Abs(MajorRadius - other.MajorRadius) < tolerance &&
               Math.Abs(MinorRadius - other.MinorRadius) < tolerance &&
               Math.Abs(NormalizeAngle(Rotation) - NormalizeAngle(other.Rotation)) < tolerance &&
               Math.Abs(NormalizeAngle(StartParameter) - NormalizeAngle(other.StartParameter)) < tolerance &&
               Math.Abs(NormalizeAngle(EndParameter) - NormalizeAngle(other.EndParameter)) < tolerance &&
               IsCounterClockwise == other.IsCounterClockwise;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Ellipse2D other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(Ellipse2D other) => IsEqualTo(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Center, MajorRadius, MinorRadius, Rotation, StartParameter, EndParameter, IsCounterClockwise);

    /// <summary>
    /// Determines whether two ellipses are equal.
    /// </summary>
    public static bool operator ==(Ellipse2D left, Ellipse2D right) => left.Equals(right);

    /// <summary>
    /// Determines whether two ellipses are not equal.
    /// </summary>
    public static bool operator !=(Ellipse2D left, Ellipse2D right) => !left.Equals(right);

    /// <inheritdoc/>
    public override string ToString() =>
        $"Ellipse2D(Center: {Center}, MajorRadius: {MajorRadius:F2}, MinorRadius: {MinorRadius:F2}, Rotation: {Rotation:F2}, Start: {StartParameter:F2}, End: {EndParameter:F2}, CCW: {IsCounterClockwise})";

    /// <summary>
    /// Finds intersection points between this ellipse and a line segment.
    /// </summary>
    /// <param name="line">The line segment to intersect with.</param>
    /// <returns>List of intersection points.</returns>
    public List<Point2D> IntersectWithLine(Line2D line)
    {
        // For circular ellipses, delegate to Arc2D logic
        if (IsCircular)
        {
            var arc = new Arc2D(Center, MajorRadius, StartParameter, EndParameter, IsCounterClockwise);
            return arc.IntersectWithLine(line);
        }

        // For true ellipses, use numerical approach
        // Transform line to ellipse-aligned coordinate system, solve quadratic
        var results = new List<Point2D>();

        // Transform line endpoints to ellipse-local coordinates (centered, rotated)
        var cosRot = Math.Cos(-Rotation);
        var sinRot = Math.Sin(-Rotation);

        var p1Local = TransformToLocal(line.StartPoint, cosRot, sinRot);
        var p2Local = TransformToLocal(line.EndPoint, cosRot, sinRot);

        // Line in local coords: P = p1Local + t * (p2Local - p1Local)
        var d = p2Local - p1Local;

        // Ellipse equation: (x/a)^2 + (y/b)^2 = 1
        // Substituting: ((p1.x + t*d.x)/a)^2 + ((p1.y + t*d.y)/b)^2 = 1
        var a = MajorRadius;
        var b = MinorRadius;

        var A = (d.X * d.X) / (a * a) + (d.Y * d.Y) / (b * b);
        var B = 2 * ((p1Local.X * d.X) / (a * a) + (p1Local.Y * d.Y) / (b * b));
        var C = (p1Local.X * p1Local.X) / (a * a) + (p1Local.Y * p1Local.Y) / (b * b) - 1;

        var discriminant = B * B - 4 * A * C;

        if (discriminant < -Constants.Tolerance)
            return results;

        if (Math.Abs(discriminant) < Constants.Tolerance)
        {
            var t = -B / (2 * A);
            if (t >= -Constants.Tolerance && t <= 1 + Constants.Tolerance)
            {
                var point = line.PointAt(Math.Max(0, Math.Min(1, t)));
                if (IsPointOnEllipseArc(point))
                    results.Add(point);
            }
        }
        else
        {
            var sqrtD = Math.Sqrt(discriminant);
            var t1 = (-B - sqrtD) / (2 * A);
            var t2 = (-B + sqrtD) / (2 * A);

            if (t1 >= -Constants.Tolerance && t1 <= 1 + Constants.Tolerance)
            {
                var point = line.PointAt(Math.Max(0, Math.Min(1, t1)));
                if (IsPointOnEllipseArc(point))
                    results.Add(point);
            }

            if (t2 >= -Constants.Tolerance && t2 <= 1 + Constants.Tolerance)
            {
                var point = line.PointAt(Math.Max(0, Math.Min(1, t2)));
                if (IsPointOnEllipseArc(point))
                    results.Add(point);
            }
        }

        return results;
    }

    /// <summary>
    /// Finds intersection points between this ellipse and a ray.
    /// </summary>
    /// <param name="origin">The ray origin point.</param>
    /// <param name="direction">The ray direction.</param>
    /// <returns>List of intersection points.</returns>
    public List<Point2D> IntersectWithRay(Point2D origin, Vector2D direction)
    {
        // For circular ellipses, delegate to Arc2D logic
        if (IsCircular)
        {
            var arc = new Arc2D(Center, MajorRadius, StartParameter, EndParameter, IsCounterClockwise);
            return arc.IntersectWithRay(origin, direction);
        }

        var results = new List<Point2D>();

        // Transform to ellipse-local coordinates
        var cosRot = Math.Cos(-Rotation);
        var sinRot = Math.Sin(-Rotation);

        var originLocal = TransformToLocal(origin, cosRot, sinRot);
        var dirLocal = new Vector2D(
            direction.X * cosRot - direction.Y * sinRot,
            direction.X * sinRot + direction.Y * cosRot);

        var a = MajorRadius;
        var b = MinorRadius;

        var A = (dirLocal.X * dirLocal.X) / (a * a) + (dirLocal.Y * dirLocal.Y) / (b * b);
        var B = 2 * ((originLocal.X * dirLocal.X) / (a * a) + (originLocal.Y * dirLocal.Y) / (b * b));
        var C = (originLocal.X * originLocal.X) / (a * a) + (originLocal.Y * originLocal.Y) / (b * b) - 1;

        var discriminant = B * B - 4 * A * C;

        if (discriminant < -Constants.Tolerance)
            return results;

        if (Math.Abs(discriminant) < Constants.Tolerance)
        {
            var t = -B / (2 * A);
            if (t >= -Constants.Tolerance)
            {
                var point = origin + Math.Max(0, t) * direction;
                if (IsPointOnEllipseArc(point))
                    results.Add(point);
            }
        }
        else
        {
            var sqrtD = Math.Sqrt(discriminant);
            var t1 = (-B - sqrtD) / (2 * A);
            var t2 = (-B + sqrtD) / (2 * A);

            if (t1 >= -Constants.Tolerance)
            {
                var point = origin + Math.Max(0, t1) * direction;
                if (IsPointOnEllipseArc(point))
                    results.Add(point);
            }

            if (t2 >= -Constants.Tolerance)
            {
                var point = origin + Math.Max(0, t2) * direction;
                if (IsPointOnEllipseArc(point))
                    results.Add(point);
            }
        }

        return results;
    }

    private Point2D TransformToLocal(Point2D point, double cosRot, double sinRot)
    {
        var dx = point.X - Center.X;
        var dy = point.Y - Center.Y;
        return new Point2D(dx * cosRot - dy * sinRot, dx * sinRot + dy * cosRot);
    }

    private bool IsPointOnEllipseArc(Point2D point)
    {
        // Get the parameter for this point on the ellipse
        var dx = point.X - Center.X;
        var dy = point.Y - Center.Y;
        var cosRot = Math.Cos(-Rotation);
        var sinRot = Math.Sin(-Rotation);
        var localX = dx * cosRot - dy * sinRot;
        var localY = dx * sinRot + dy * cosRot;

        // Calculate the parameter (eccentric anomaly)
        var param = Math.Atan2(localY / MinorRadius, localX / MajorRadius);
        return IsParameterInRange(param);
    }
}
