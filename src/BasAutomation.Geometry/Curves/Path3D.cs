namespace BasAutomation.Geometry.Curves;

/// <summary>
/// Represents a connected sequence of 3D curves forming a continuous path.
/// </summary>
public sealed class Path3D : IEquatable<Path3D>, IGeometryComparable<Path3D>
{
    private readonly List<ICurve3D> _segments;
    private readonly bool _isLoop;
    private readonly double _length;
    private readonly bool _isCoplanar;
    private readonly Plane3D? _plane;
    private readonly Box3D _boundingBox;

    /// <summary>
    /// Gets the segments that make up this path.
    /// </summary>
    public IReadOnlyList<ICurve3D> Segments => _segments;

    /// <summary>
    /// Gets the number of segments in the path.
    /// </summary>
    public int SegmentCount => _segments.Count;

    /// <summary>
    /// Gets whether this path forms a closed loop.
    /// </summary>
    public bool IsLoop => _isLoop;

    /// <summary>
    /// Gets the total length of the path.
    /// </summary>
    public double Length => _length;

    /// <summary>
    /// Gets the start point of the path.
    /// </summary>
    public Point3D StartPoint => _segments.Count > 0 ? _segments[0].StartPoint : new Point3D(0, 0, 0);

    /// <summary>
    /// Gets the end point of the path.
    /// </summary>
    public Point3D EndPoint => _segments.Count > 0 ? _segments[^1].EndPoint : new Point3D(0, 0, 0);

    /// <summary>
    /// Gets whether all segments lie in the same plane.
    /// </summary>
    public bool IsCoplanar => _isCoplanar;

    /// <summary>
    /// Gets the plane containing all segments (if coplanar), otherwise null.
    /// </summary>
    public Plane3D? Plane => _plane;

    /// <summary>
    /// Gets the axis-aligned bounding box of the path.
    /// </summary>
    public Box3D BoundingBox => _boundingBox;

    /// <summary>
    /// Creates a path from an ordered collection of curves.
    /// Curves are expected to be already ordered and connected.
    /// </summary>
    /// <param name="segments">The ordered curve segments.</param>
    /// <param name="tolerance">The tolerance for checking connectivity and coplanarity.</param>
    public Path3D(IEnumerable<ICurve3D> segments, double tolerance = Constants.Tolerance)
    {
        _segments = segments.ToList();

        if (_segments.Count == 0)
        {
            _isLoop = false;
            _length = 0;
            _isCoplanar = true;
            _plane = null;
            _boundingBox = new Box3D(new Point3D(0, 0, 0), new Point3D(0, 0, 0));
            return;
        }

        // Calculate length
        _length = _segments.Sum(s => s.Length);

        // Check if loop
        _isLoop = StartPoint.IsEqualTo(EndPoint, tolerance);

        // Calculate bounding box
        _boundingBox = CalculateBoundingBox();

        // Check coplanarity
        (_isCoplanar, _plane) = CheckCoplanarity(tolerance);
    }

    /// <summary>
    /// Creates a path from unordered curves, automatically ordering them.
    /// </summary>
    /// <param name="curves">The unordered curves to form into a path.</param>
    /// <param name="tolerance">The tolerance for matching endpoints.</param>
    /// <returns>A new path with ordered curves.</returns>
    public static Path3D FromUnorderedCurves(IEnumerable<ICurve3D> curves, double tolerance = 1e-6)
    {
        var orderedCurves = CurveOrderHelper3D.OrderCurves(curves, tolerance);
        return new Path3D(orderedCurves, tolerance);
    }

    /// <summary>
    /// Creates a path from a collection of points (creates Line3D segments).
    /// </summary>
    /// <param name="points">The points defining the path vertices.</param>
    /// <param name="closePath">Whether to close the path by connecting the last point to the first.</param>
    /// <returns>A new path of line segments.</returns>
    public static Path3D FromPoints(IEnumerable<Point3D> points, bool closePath = false)
    {
        var pointList = points.ToList();
        if (pointList.Count < 2)
        {
            return new Path3D(Enumerable.Empty<ICurve3D>());
        }

        var segments = new List<ICurve3D>();
        for (int i = 0; i < pointList.Count - 1; i++)
        {
            segments.Add(new Line3D(pointList[i], pointList[i + 1]));
        }

        if (closePath && pointList.Count > 2)
        {
            segments.Add(new Line3D(pointList[^1], pointList[0]));
        }

        return new Path3D(segments);
    }

    /// <summary>
    /// Groups a collection of curves into connected sub-paths.
    /// Each group of curves that share endpoints becomes a separate Path3D.
    /// Curves within each path are ordered so they connect end-to-end.
    /// </summary>
    /// <param name="curves">The curves to group into paths.</param>
    /// <param name="tolerance">The tolerance for matching endpoints.</param>
    /// <returns>A list of paths, one per connected group of curves.</returns>
    public static List<Path3D> GroupIntoConnectedPaths(IEnumerable<ICurve3D> curves, double tolerance = Constants.Tolerance)
    {
        var curveList = curves.ToList();
        if (curveList.Count == 0)
            return new List<Path3D>();

        var paths = new List<Path3D>();
        var remaining = new HashSet<int>(Enumerable.Range(0, curveList.Count));

        while (remaining.Count > 0)
        {
            // Start a new group with the first remaining curve
            var startIndex = remaining.First();
            var group = FindConnectedCurves(curveList, startIndex, remaining, tolerance);

            // Order the curves within the group to form a continuous path
            var orderedCurves = OrderCurvesInGroup(group, tolerance);
            paths.Add(new Path3D(orderedCurves, tolerance));
        }

        return paths;
    }

    /// <summary>
    /// Finds all curves connected to the starting curve using breadth-first search.
    /// </summary>
    private static List<ICurve3D> FindConnectedCurves(
        List<ICurve3D> allCurves,
        int startIndex,
        HashSet<int> remaining,
        double tolerance)
    {
        var connected = new List<ICurve3D>();
        var queue = new Queue<int>();

        queue.Enqueue(startIndex);
        remaining.Remove(startIndex);
        connected.Add(allCurves[startIndex]);

        while (queue.Count > 0)
        {
            var currentIndex = queue.Dequeue();
            var current = allCurves[currentIndex];
            var currentStart = current.StartPoint;
            var currentEnd = current.EndPoint;

            // Find all remaining curves connected to this one
            foreach (var otherIndex in remaining.ToList())
            {
                var other = allCurves[otherIndex];
                var otherStart = other.StartPoint;
                var otherEnd = other.EndPoint;

                // Check if any endpoints connect
                if (otherStart.IsEqualTo(currentStart, tolerance) ||
                    otherStart.IsEqualTo(currentEnd, tolerance) ||
                    otherEnd.IsEqualTo(currentStart, tolerance) ||
                    otherEnd.IsEqualTo(currentEnd, tolerance))
                {
                    remaining.Remove(otherIndex);
                    queue.Enqueue(otherIndex);
                    connected.Add(other);
                }
            }
        }

        return connected;
    }

    /// <summary>
    /// Orders curves within a connected group so they form a continuous path.
    /// Reverses individual curves as needed.
    /// </summary>
    private static List<ICurve3D> OrderCurvesInGroup(List<ICurve3D> curves, double tolerance)
    {
        if (curves.Count == 0)
            return curves;

        if (curves.Count == 1)
        {
            // A lone curve still needs a deterministic direction, or it flips when the
            // resolver hands the same line back reversed. Orient it from its smaller endpoint.
            var only = curves[0];
            return [ComparePoints(only.StartPoint, only.EndPoint, tolerance) <= 0 ? only : only.Reverse()];
        }

        var ordered = new List<ICurve3D>();
        var remaining = new List<ICurve3D>(curves);

        // Find a starting curve (prefer one that has an endpoint not shared by two curves - i.e., an open end)
        var startCurve = FindStartCurve(remaining, tolerance);
        remaining.Remove(startCurve);

        // Determine orientation: check which end has fewer connections
        var startConnections = CountConnections(startCurve.StartPoint, remaining, tolerance);
        var endConnections = CountConnections(startCurve.EndPoint, remaining, tolerance);

        if (startConnections > endConnections)
        {
            // More connections at start, so start is actually in the middle - reverse the curve
            startCurve = startCurve.Reverse();
        }

        ordered.Add(startCurve);
        var currentEnd = startCurve.EndPoint;

        // Keep adding curves
        while (remaining.Count > 0)
        {
            bool found = false;

            for (int i = 0; i < remaining.Count; i++)
            {
                var candidate = remaining[i];

                if (candidate.StartPoint.IsEqualTo(currentEnd, tolerance))
                {
                    ordered.Add(candidate);
                    currentEnd = candidate.EndPoint;
                    remaining.RemoveAt(i);
                    found = true;
                    break;
                }
                else if (candidate.EndPoint.IsEqualTo(currentEnd, tolerance))
                {
                    var reversed = candidate.Reverse();
                    ordered.Add(reversed);
                    currentEnd = reversed.EndPoint;
                    remaining.RemoveAt(i);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                // Should not happen for connected curves, but break to avoid infinite loop
                break;
            }
        }

        return ordered;
    }

    /// <summary>
    /// Finds a good starting curve for ordering (one with an open end if path is not closed).
    /// </summary>
    private static ICurve3D FindStartCurve(List<ICurve3D> curves, double tolerance)
    {
        // Count how many times each endpoint appears, to detect open ends (count == 1).
        var endpointCounts = new Dictionary<string, int>();

        foreach (var curve in curves)
        {
            var startKey = GetPointKey(curve.StartPoint, tolerance);
            var endKey = GetPointKey(curve.EndPoint, tolerance);

            endpointCounts[startKey] = endpointCounts.TryGetValue(startKey, out int s) ? s + 1 : 1;
            endpointCounts[endKey] = endpointCounts.TryGetValue(endKey, out int e) ? e + 1 : 1;
        }

        // Pick the start curve DETERMINISTICALLY: among curves with an open end (a tip that
        // appears once), take the one whose open tip is geometrically smallest (X, then Y,
        // then Z). This makes the path orientation independent of input/enumeration order, so
        // the same source yields the same start tip on create and on every refresh — instead
        // of "first open-ended curve in input order", which flipped when a re-enumerated sketch
        // listed the other end first (a "wrong side on update" bug).
        ICurve3D startCurve = null;
        Point3D startTip = default;
        foreach (var curve in curves)
        {
            if (OpenEndpoint(curve, endpointCounts, tolerance) is not { } tip) continue;
            if (startCurve == null || ComparePoints(tip, startTip, tolerance) < 0)
            {
                startCurve = curve;
                startTip = tip;
            }
        }
        if (startCurve != null)
            return startCurve;

        // Closed loop (no open ends): pick by the smallest endpoint so even a loop is
        // enumeration-order independent, rather than "just return the first input curve".
        ICurve3D best = curves[0];
        foreach (var curve in curves)
            if (ComparePoints(SmallerEndpoint(curve, tolerance), SmallerEndpoint(best, tolerance), tolerance) < 0)
                best = curve;
        return best;
    }

    /// <summary>
    /// The curve's open tip — the endpoint appearing only once in the group (an exposed path
    /// end). A lone curve has both ends open, so its smaller endpoint is returned to keep its
    /// direction deterministic too. Null for an interior segment (both ends shared) / closed loop.
    /// </summary>
    private static Point3D? OpenEndpoint(ICurve3D curve, Dictionary<string, int> counts, double tolerance)
    {
        bool startOpen = counts[GetPointKey(curve.StartPoint, tolerance)] == 1;
        bool endOpen = counts[GetPointKey(curve.EndPoint, tolerance)] == 1;
        if (startOpen && endOpen) return SmallerEndpoint(curve, tolerance);
        if (startOpen) return curve.StartPoint;
        if (endOpen) return curve.EndPoint;
        return null;
    }

    private static Point3D SmallerEndpoint(ICurve3D curve, double tolerance)
        => ComparePoints(curve.StartPoint, curve.EndPoint, tolerance) <= 0 ? curve.StartPoint : curve.EndPoint;

    /// <summary>Deterministic 3D point ordering, rounded to <paramref name="tolerance"/> so float
    /// jitter can't reorder coincident tips. Lexicographic by X, then Y, then Z.</summary>
    private static int ComparePoints(Point3D a, Point3D b, double tolerance)
    {
        var scale = 1.0 / tolerance;
        int cx = Math.Round(a.X * scale).CompareTo(Math.Round(b.X * scale)); if (cx != 0) return cx;
        int cy = Math.Round(a.Y * scale).CompareTo(Math.Round(b.Y * scale)); if (cy != 0) return cy;
        return Math.Round(a.Z * scale).CompareTo(Math.Round(b.Z * scale));
    }

    private static int CountConnections(Point3D point, List<ICurve3D> curves, double tolerance)
    {
        int count = 0;
        foreach (var curve in curves)
        {
            if (curve.StartPoint.IsEqualTo(point, tolerance) || curve.EndPoint.IsEqualTo(point, tolerance))
                count++;
        }
        return count;
    }

    private static string GetPointKey(Point3D point, double tolerance)
    {
        var scale = 1.0 / tolerance;
        return $"{Math.Round(point.X * scale)}_{Math.Round(point.Y * scale)}_{Math.Round(point.Z * scale)}";
    }

    /// <summary>
    /// Gets a segment by index.
    /// </summary>
    /// <param name="index">The zero-based index of the segment.</param>
    /// <returns>The curve at the specified index.</returns>
    public ICurve3D GetSegment(int index) => _segments[index];

    /// <summary>
    /// Returns a new path with reversed direction.
    /// </summary>
    /// <returns>A new path with all segments reversed and in reverse order.</returns>
    public Path3D Reverse()
    {
        var reversed = new List<ICurve3D>();
        for (int i = _segments.Count - 1; i >= 0; i--)
        {
            reversed.Add(_segments[i].Reverse());
        }
        return new Path3D(reversed);
    }

    /// <summary>
    /// Gets a coordinate system representing the orientation at a specific segment.
    /// The Z-axis aligns with the segment tangent, X and Y form a perpendicular frame.
    /// </summary>
    /// <param name="segmentIndex">The index of the segment.</param>
    /// <param name="t">The parameter along the segment (0 = start, 1 = end). Default is 0.5 (midpoint).</param>
    /// <param name="upHint">Optional hint vector for the Y-axis direction.</param>
    /// <returns>A coordinate system at the specified position on the segment.</returns>
    public CoordinateSystem3D GetOrientationAtSegment(int segmentIndex, double t = 0.5, Vector3D? upHint = null)
    {
        if (segmentIndex < 0 || segmentIndex >= _segments.Count)
            throw new ArgumentOutOfRangeException(nameof(segmentIndex));

        var segment = _segments[segmentIndex];
        var point = segment.PointAt(t);
        var tangent = segment.TangentAt(t);

        return CreateOrientationFromTangent(point, tangent, upHint);
    }

    /// <summary>
    /// Gets orientations at all segment midpoints.
    /// </summary>
    /// <param name="upHint">Optional hint vector for consistent Y-axis direction across all segments.</param>
    /// <returns>A list of coordinate systems, one for each segment.</returns>
    public IReadOnlyList<CoordinateSystem3D> GetSegmentOrientations(Vector3D? upHint = null)
    {
        var orientations = new List<CoordinateSystem3D>();
        if (_segments.Count == 0) return orientations;

        // Segment 0: use global upHint
        orientations.Add(GetOrientationAtSegment(0, 0.5, upHint));

        // Segments 1..N: transport the previous frame through the rotation that takes
        // the previous tangent to the new tangent (rotation-minimizing frame).
        // This stays continuous even when prevY becomes parallel to the new tangent —
        // the case a simple projection cannot handle.
        for (int i = 1; i < _segments.Count; i++)
        {
            var segment = _segments[i];
            var point = segment.PointAt(0.5);
            var newTangent = segment.TangentAt(0.5).Normalized();
            var prevTangent = orientations[i - 1].ZAxis;
            var prevY = orientations[i - 1].YAxis;

            var rotation = Matrix3D.CreateRotationFromTo(prevTangent, newTangent);
            var newY = rotation.Transform(prevY);

            // Re-orthogonalize against the new tangent to kill numerical drift.
            var residual = newY - newTangent * newY.Dot(newTangent);
            if (residual.LengthSquared > Constants.GeometryTolerance * Constants.GeometryTolerance)
                newY = residual.Normalized();

            orientations.Add(CreateOrientationFromTangent(point, newTangent, newY));
        }

        return orientations;
    }

    /// <summary>
    /// Gets the segments with their miter planes at start and end.
    /// Miter planes are the bisector planes between adjacent segments.
    /// </summary>
    /// <returns>Enumeration of segments with their start and end miter planes.</returns>
    public IEnumerable<(ICurve3D Segment, Plane3D StartMiter, Plane3D EndMiter)> GetSegmentsWithMiters()
    {
        for (int i = 0; i < _segments.Count; i++)
        {
            var segment = _segments[i];
            var tangentStart = segment.TangentAt(0).Normalized();
            var tangentEnd = segment.TangentAt(1).Normalized();

            // Start miter: perpendicular to segment at start, or bisector with previous segment
            Plane3D startMiter;
            if (i == 0 && !_isLoop)
            {
                // Open path start: perpendicular plane with normal pointing opposite to beam direction
                startMiter = new Plane3D(segment.StartPoint, -tangentStart);
            }
            else if (i == 0 && _isLoop && _segments.Count > 1)
            {
                // Loop closure: miter with last segment (START plane - normal opposite to beam)
                startMiter = CalculateStartMiterPlane(_segments[^1], segment);
            }
            else
            {
                // Middle segment: miter with previous segment (START plane - normal opposite to beam)
                startMiter = CalculateStartMiterPlane(_segments[i - 1], segment);
            }

            // End miter: perpendicular to segment at end, or bisector with next segment
            Plane3D endMiter;
            if (i == _segments.Count - 1 && !_isLoop)
            {
                // Open path end: perpendicular plane with normal pointing same as beam direction
                endMiter = new Plane3D(segment.EndPoint, tangentEnd);
            }
            else if (i == _segments.Count - 1 && _isLoop && _segments.Count > 1)
            {
                // Loop closure: miter with first segment (END plane - normal same as beam)
                endMiter = CalculateEndMiterPlane(segment, _segments[0]);
            }
            else
            {
                // Middle segment: miter with next segment (END plane - normal same as beam)
                endMiter = CalculateEndMiterPlane(segment, _segments[i + 1]);
            }

            yield return (segment, startMiter, endMiter);
        }
    }

    /// <summary>
    /// Calculates intersections between this path and a line.
    /// </summary>
    /// <param name="line">The line to intersect with.</param>
    /// <returns>List of intersection points ordered along the line direction.</returns>
    public IReadOnlyList<PathIntersection3D> IntersectLine(Line3D line)
    {
        var intersections = new List<PathIntersection3D>();

        for (int i = 0; i < _segments.Count; i++)
        {
            var segment = _segments[i];

            // For Line3D segments, use direct line-line intersection
            if (segment is Line3D segmentLine)
            {
                if (TryIntersectLines(line, segmentLine, out var point, out var t))
                {
                    var tangent = segmentLine.TangentAt(t);
                    intersections.Add(new PathIntersection3D(point, i, t, tangent));
                }
            }
            else
            {
                // For other curve types, sample and find intersections
                var curveIntersections = IntersectCurveWithLine(segment, line, i);
                intersections.AddRange(curveIntersections);
            }
        }

        // Sort by parameter along the input line
        intersections.Sort((a, b) =>
        {
            var tA = (a.Point - line.StartPoint).Dot(line.Direction);
            var tB = (b.Point - line.StartPoint).Dot(line.Direction);
            return tA.CompareTo(tB);
        });

        return intersections;
    }

    /// <summary>
    /// Calculates intersections between this path and a ray.
    /// </summary>
    /// <param name="origin">The ray origin.</param>
    /// <param name="direction">The ray direction.</param>
    /// <returns>List of intersection points ordered along the ray.</returns>
    public IReadOnlyList<PathIntersection3D> IntersectRay(Point3D origin, Vector3D direction)
    {
        // Create a very long line in the ray direction
        var farPoint = origin + direction.Normalized() * 1e9;
        var line = new Line3D(origin, farPoint);

        var allIntersections = IntersectLine(line);

        // Filter to only include intersections in the positive direction
        return allIntersections
            .Where(i => (i.Point - origin).Dot(direction) >= 0)
            .ToList();
    }

    /// <summary>
    /// Checks if a point is inside a closed coplanar path.
    /// Uses the ray casting algorithm.
    /// </summary>
    /// <param name="point">The point to test.</param>
    /// <param name="tolerance">The tolerance for the test.</param>
    /// <returns>True if the point is inside the closed path.</returns>
    /// <exception cref="InvalidOperationException">If the path is not a closed loop or not coplanar.</exception>
    public bool ContainsPoint(Point3D point, double tolerance = Constants.Tolerance)
    {
        if (!_isLoop)
            throw new InvalidOperationException("ContainsPoint is only valid for closed paths.");
        if (!_isCoplanar || _plane == null)
            throw new InvalidOperationException("ContainsPoint is only valid for coplanar paths.");

        // Project point onto the plane
        var projectedPoint = _plane.Value.ProjectPoint(point);

        // Check if point is on the plane
        if (!projectedPoint.IsEqualTo(point, tolerance))
            return false;

        // Use ray casting in the plane
        // Cast a ray in an arbitrary direction within the plane
        var rayDirection = GetInPlaneRayDirection(_plane.Value);
        var intersections = IntersectRay(projectedPoint, rayDirection);

        // Odd number of intersections means inside
        return intersections.Count % 2 == 1;
    }

    /// <summary>
    /// Returns a new path transformed by the specified matrix.
    /// </summary>
    /// <param name="transformation">The transformation matrix.</param>
    /// <returns>A new transformed path.</returns>
    public Path3D Transform(Matrix3D transformation)
    {
        var transformedSegments = new List<ICurve3D>();
        foreach (var segment in _segments)
        {
            if (segment is Line3D line)
            {
                var newStart = transformation.Transform(line.StartPoint);
                var newEnd = transformation.Transform(line.EndPoint);
                transformedSegments.Add(new Line3D(newStart, newEnd));
            }
            else if (segment is Arc3D arc)
            {
                // Transform arc by transforming start, end, and center points
                var newStart = transformation.Transform(arc.StartPoint);
                var newEnd = transformation.Transform(arc.EndPoint);
                var newCenter = transformation.Transform(arc.Center);
                try
                {
                    transformedSegments.Add(new Arc3D(newStart, newEnd, newCenter, arc.IsCounterClockwise));
                }
                catch
                {
                    // Fall back to line if arc becomes degenerate after transformation
                    transformedSegments.Add(new Line3D(newStart, newEnd));
                }
            }
            else
            {
                // For other curve types, create a line approximation
                var newStart = transformation.Transform(segment.StartPoint);
                var newEnd = transformation.Transform(segment.EndPoint);
                transformedSegments.Add(new Line3D(newStart, newEnd));
            }
        }
        return new Path3D(transformedSegments);
    }

    /// <summary>
    /// Projects the path onto a plane.
    /// </summary>
    /// <param name="plane">The plane to project onto.</param>
    /// <returns>A new path projected onto the plane.</returns>
    public Path3D ProjectOntoPlane(Plane3D plane)
    {
        var projectedSegments = new List<ICurve3D>();
        foreach (var segment in _segments)
        {
            var projectedStart = plane.ProjectPoint(segment.StartPoint);
            var projectedEnd = plane.ProjectPoint(segment.EndPoint);

            // For simplicity, project all curves as lines
            // More sophisticated implementations could preserve arc curvature
            if (!projectedStart.IsEqualTo(projectedEnd))
            {
                projectedSegments.Add(new Line3D(projectedStart, projectedEnd));
            }
        }
        return new Path3D(projectedSegments);
    }

    /// <summary>
    /// Gets all vertex points of the path (segment endpoints without duplicates).
    /// </summary>
    /// <returns>List of unique vertex points.</returns>
    public IReadOnlyList<Point3D> GetVertices()
    {
        var vertices = new List<Point3D>();
        if (_segments.Count == 0) return vertices;

        vertices.Add(_segments[0].StartPoint);
        foreach (var segment in _segments)
        {
            if (!vertices[^1].IsEqualTo(segment.EndPoint))
            {
                vertices.Add(segment.EndPoint);
            }
        }

        // Remove duplicate at end if closed loop
        if (_isLoop && vertices.Count > 1 && vertices[0].IsEqualTo(vertices[^1]))
        {
            vertices.RemoveAt(vertices.Count - 1);
        }

        return vertices;
    }

    #region Private Helpers

    private Box3D CalculateBoundingBox()
    {
        if (_segments.Count == 0)
            return new Box3D(new Point3D(0, 0, 0), new Point3D(0, 0, 0));

        double minX = double.MaxValue, minY = double.MaxValue, minZ = double.MaxValue;
        double maxX = double.MinValue, maxY = double.MinValue, maxZ = double.MinValue;

        foreach (var segment in _segments)
        {
            // Sample curve for bounding box
            for (double t = 0; t <= 1; t += 0.1)
            {
                var p = segment.PointAt(t);
                minX = Math.Min(minX, p.X);
                minY = Math.Min(minY, p.Y);
                minZ = Math.Min(minZ, p.Z);
                maxX = Math.Max(maxX, p.X);
                maxY = Math.Max(maxY, p.Y);
                maxZ = Math.Max(maxZ, p.Z);
            }
        }

        return new Box3D(new Point3D(minX, minY, minZ), new Point3D(maxX, maxY, maxZ));
    }

    private (bool IsCoplanar, Plane3D? Plane) CheckCoplanarity(double tolerance)
    {
        if (_segments.Count < 2)
        {
            if (_segments.Count == 1)
            {
                // Single segment is always coplanar - create a plane through it
                var mid = _segments[0].PointAt(0.5);
                var tangent = _segments[0].TangentAt(0.5);
                var normal = tangent.Cross(Vector3D.ZAxis);
                if (normal.LengthSquared < tolerance)
                    normal = tangent.Cross(Vector3D.XAxis);
                return (true, new Plane3D(mid, normal.Normalized()));
            }
            return (true, null);
        }

        // Get three non-collinear points to define the plane
        var points = new List<Point3D>();
        foreach (var segment in _segments)
        {
            points.Add(segment.StartPoint);
            points.Add(segment.PointAt(0.5));
        }

        // Find three non-collinear points
        var p1 = points[0];
        Point3D p2 = default, p3 = default;
        bool foundP2 = false, foundP3 = false;

        for (int i = 1; i < points.Count && !foundP2; i++)
        {
            if (!points[i].IsEqualTo(p1, tolerance))
            {
                p2 = points[i];
                foundP2 = true;
            }
        }

        if (!foundP2) return (true, null); // All points are the same

        var v1 = (p2 - p1).Normalized();
        for (int i = 2; i < points.Count && !foundP3; i++)
        {
            var v2 = (points[i] - p1).Normalized();
            if (v1.Cross(v2).Length > tolerance)
            {
                p3 = points[i];
                foundP3 = true;
            }
        }

        if (!foundP3) return (true, null); // All points are collinear

        // Create plane from three points
        var plane = Plane3D.FromThreePoints(p1, p2, p3);

        // Check all points are on the plane
        foreach (var segment in _segments)
        {
            for (double t = 0; t <= 1; t += 0.25)
            {
                var p = segment.PointAt(t);
                if (plane.DistanceToPoint(p) > tolerance)
                {
                    return (false, null);
                }
            }
        }

        return (true, plane);
    }

    private CoordinateSystem3D CreateOrientationFromTangent(Point3D origin, Vector3D tangent, Vector3D? upHint)
    {
        var z = tangent.Normalized();
        var up = upHint ?? Vector3D.ZAxis;

        // If tangent is parallel to up, use alternative
        if (Math.Abs(z.Dot(up)) > 0.99)
        {
            up = Math.Abs(z.Dot(Vector3D.XAxis)) < 0.99 ? Vector3D.XAxis : Vector3D.YAxis;
        }

        var x = up.Cross(z).Normalized();
        var y = z.Cross(x).Normalized();

        return new CoordinateSystem3D(origin, x, y, z);
    }

    /// <summary>
    /// Calculates the miter plane between two segments.
    /// The plane is at segment2's start point with normal as the bisector of the two tangents.
    /// </summary>
    /// <param name="segment1">The first segment (its end connects to segment2's start).</param>
    /// <param name="segment2">The second segment.</param>
    /// <returns>The miter plane positioned at the connection point.</returns>
    private Plane3D CalculateMiterPlane(ICurve3D segment1, ICurve3D segment2)
    {
        var tangent1 = segment1.TangentAt(1).Normalized();
        var tangent2 = segment2.TangentAt(0).Normalized();

        // The miter plane normal is the bisector of the two tangent directions
        var sum = tangent1 + tangent2;

        Vector3D bisector;
        if (sum.LengthSquared < Constants.Tolerance)
        {
            // Segments are opposite direction (180° turn) - use perpendicular to tangent
            // For segment1's END, normal should point in tangent1 direction
            bisector = tangent1;
        }
        else
        {
            bisector = sum.Normalized();
        }

        var point = segment2.StartPoint;
        return new Plane3D(point, bisector);
    }

    /// <summary>
    /// Calculates miter plane for the END of a segment (normal points in beam direction).
    /// </summary>
    private Plane3D CalculateEndMiterPlane(ICurve3D segment, ICurve3D nextSegment)
    {
        var tangent1 = segment.TangentAt(1).Normalized();
        var tangent2 = nextSegment.TangentAt(0).Normalized();

        var sum = tangent1 + tangent2;
        Vector3D bisector;
        if (sum.LengthSquared < Constants.Tolerance)
        {
            bisector = tangent1;
        }
        else
        {
            bisector = sum.Normalized();
            // Ensure bisector points more toward segment's direction (for END plane)
            if (bisector.Dot(tangent1) < 0)
            {
                bisector = -bisector;
            }
        }

        return new Plane3D(segment.EndPoint, bisector);
    }

    /// <summary>
    /// Calculates miter plane for the START of a segment (normal points opposite to beam direction).
    /// </summary>
    private Plane3D CalculateStartMiterPlane(ICurve3D prevSegment, ICurve3D segment)
    {
        var tangent1 = prevSegment.TangentAt(1).Normalized();
        var tangent2 = segment.TangentAt(0).Normalized();

        var sum = tangent1 + tangent2;
        Vector3D bisector;
        if (sum.LengthSquared < Constants.Tolerance)
        {
            bisector = -tangent2;
        }
        else
        {
            bisector = sum.Normalized();
            // Ensure bisector points opposite to segment's direction (for START plane)
            if (bisector.Dot(tangent2) > 0)
            {
                bisector = -bisector;
            }
        }

        return new Plane3D(segment.StartPoint, bisector);
    }

    private bool TryIntersectLines(Line3D line1, Line3D line2, out Point3D intersection, out double t)
    {
        intersection = new Point3D(0, 0, 0);
        t = 0;

        var d1 = line1.Direction;
        var d2 = line2.Direction;
        var cross = d1.Cross(d2);

        if (cross.LengthSquared < Constants.Tolerance * Constants.Tolerance)
        {
            // Lines are parallel
            return false;
        }

        var diff = line2.StartPoint - line1.StartPoint;

        // Find parameters using the shortest distance between lines
        var n1 = d1.Cross(cross);
        var n2 = d2.Cross(cross);

        var t1 = diff.Dot(n2) / d1.Dot(n2);
        var t2 = -diff.Dot(n1) / d2.Dot(n1);

        // Check if intersection is within both line segments
        // t1 and t2 are in distance units (since Direction is normalized)
        if (t1 < 0 || t1 > line1.Length || t2 < 0 || t2 > line2.Length)
        {
            return false;
        }

        // PointAt expects normalized parameter (0-1), so divide by length
        var p1 = line1.PointAt(t1 / line1.Length);
        var p2 = line2.PointAt(t2 / line2.Length);

        // Check if the points are close enough (lines actually intersect)
        if (!p1.IsEqualTo(p2, Constants.Tolerance * 10))
        {
            return false;
        }

        intersection = p2;
        t = t2 / line2.Length;  // Normalize to 0-1 range
        return true;
    }

    private IEnumerable<PathIntersection3D> IntersectCurveWithLine(ICurve3D curve, Line3D line, int segmentIndex)
    {
        // Sample the curve and find approximate intersections
        const int samples = 20;
        var intersections = new List<PathIntersection3D>();

        for (int i = 0; i < samples; i++)
        {
            var t1 = (double)i / samples;
            var t2 = (double)(i + 1) / samples;

            var p1 = curve.PointAt(t1);
            var p2 = curve.PointAt(t2);

            var segmentLine = new Line3D(p1, p2);
            if (TryIntersectLines(line, segmentLine, out var intersection, out var localT))
            {
                var curveT = t1 + localT * (t2 - t1);
                var tangent = curve.TangentAt(curveT);
                intersections.Add(new PathIntersection3D(intersection, segmentIndex, curveT, tangent));
            }
        }

        return intersections;
    }

    private Vector3D GetInPlaneRayDirection(Plane3D plane)
    {
        // Get a direction that lies in the plane
        var normal = plane.Normal;
        var reference = Math.Abs(normal.Z) < 0.9 ? Vector3D.ZAxis : Vector3D.XAxis;
        return normal.Cross(reference).Normalized();
    }

    #endregion

    #region Equality

    /// <summary>
    /// Determines whether this path is equal to another within the specified tolerance.
    /// </summary>
    /// <param name="other">The other path to compare.</param>
    /// <param name="tolerance">The tolerance for comparison.</param>
    /// <returns>True if the paths are equal within tolerance.</returns>
    public bool IsEqualTo(Path3D? other, double tolerance = Constants.Tolerance)
    {
        if (other == null) return false;
        if (_segments.Count != other._segments.Count) return false;

        for (int i = 0; i < _segments.Count; i++)
        {
            if (!_segments[i].StartPoint.IsEqualTo(other._segments[i].StartPoint, tolerance) ||
                !_segments[i].EndPoint.IsEqualTo(other._segments[i].EndPoint, tolerance))
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc/>
    public bool Equals(Path3D? other) => IsEqualTo(other);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Path3D other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(_segments.Count, _isLoop, _length);

    /// <summary>
    /// Determines whether two paths are equal.
    /// </summary>
    public static bool operator ==(Path3D? left, Path3D? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two paths are not equal.
    /// </summary>
    public static bool operator !=(Path3D? left, Path3D? right) => !(left == right);

    #endregion

    /// <inheritdoc/>
    public override string ToString() => $"Path3D(Segments: {_segments.Count}, Length: {_length:F2}, IsLoop: {_isLoop})";
}
