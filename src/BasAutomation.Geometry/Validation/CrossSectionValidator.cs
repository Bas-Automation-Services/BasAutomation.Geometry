using BasAutomation.Geometry.Curves;

namespace BasAutomation.Geometry.Validation;

/// <summary>
/// Validates 2D cross-sections for suitability for extrusion operations.
/// </summary>
public class CrossSectionValidator
{
    private const double NearZeroLengthThreshold = 0.01; // Curves shorter than this trigger a warning
    private const double NearDiscontinuityThreshold = 0.1; // Gaps smaller than this trigger a warning
    private readonly double tolerance;

    /// <summary>
    /// Initializes a new instance of the CrossSectionValidator class.
    /// </summary>
    /// <param name="tolerance">The tolerance for geometric comparisons. Defaults to Constants.Tolerance.</param>
    public CrossSectionValidator(double tolerance = Constants.Tolerance)
    {
        this.tolerance = tolerance;
    }

    /// <summary>
    /// Validates a cross-section defined by outer curves.
    /// </summary>
    /// <param name="outerCurves">The outer boundary curves.</param>
    /// <returns>A validation result containing any issues found.</returns>
    public ValidationResult Validate(IEnumerable<ICurve2D> outerCurves)
    {
        return Validate(outerCurves, Array.Empty<List<ICurve2D>>());
    }

    /// <summary>
    /// Validates a cross-section defined by outer curves and inner contours.
    /// </summary>
    /// <param name="outerCurves">The outer boundary curves.</param>
    /// <param name="innerContours">The inner contours (holes).</param>
    /// <returns>A validation result containing any issues found.</returns>
    public ValidationResult Validate(
        IEnumerable<ICurve2D> outerCurves,
        IEnumerable<IEnumerable<ICurve2D>> innerContours)
    {
        var result = new ValidationResult();

        var outerList = outerCurves as List<ICurve2D> ?? outerCurves.ToList();
        var innerList = innerContours.Select(c => c as List<ICurve2D> ?? c.ToList()).ToList();

        // Validate outer contour
        ValidateContour(outerList, 0, true, result);

        // Validate inner contours
        for (int i = 0; i < innerList.Count; i++)
        {
            ValidateContour(innerList[i], i + 1, false, result);
        }

        // Check for intersections between contours
        if (!result.HasErrors) // Only check if individual contours are valid
        {
            ValidateContourIntersections(outerList, innerList, result);
        }

        return result;
    }

    /// <summary>
    /// Validates a single contour (outer or inner).
    /// </summary>
    private void ValidateContour(
        List<ICurve2D> curves,
        int contourIndex,
        bool isOuter,
        ValidationResult result)
    {
        if (curves.Count == 0)
        {
            result.AddIssue(new ValidationIssue(
                ValidationSeverity.Error,
                ValidationIssueType.NotClosed,
                "Contour has no curves",
                contourIndex));
            return;
        }

        // Check for zero-length and near-zero length curves
        for (int i = 0; i < curves.Count; i++)
        {
            var curve = curves[i];
            var length = curve.Length;

            if (length < tolerance)
            {
                result.AddIssue(new ValidationIssue(
                    ValidationSeverity.Error,
                    ValidationIssueType.ZeroLengthCurve,
                    $"Curve has zero length ({length:E2})",
                    contourIndex,
                    curveIndex: i,
                    curve: curve));
            }
            else if (length < NearZeroLengthThreshold)
            {
                result.AddIssue(new ValidationIssue(
                    ValidationSeverity.Warning,
                    ValidationIssueType.NearZeroLengthCurve,
                    $"Curve has very small length ({length:F6})",
                    contourIndex,
                    curveIndex: i,
                    curve: curve));
            }
        }

        // Check for duplicate/coincident points
        for (int i = 0; i < curves.Count; i++)
        {
            var curve = curves[i];
            var startDist = curve.StartPoint.DistanceTo(curve.EndPoint);

            if (startDist < tolerance && !(curve is Circle2D))
            {
                result.AddIssue(new ValidationIssue(
                    ValidationSeverity.Warning,
                    ValidationIssueType.DuplicatePoint,
                    "Curve start and end points are nearly identical",
                    contourIndex,
                    curveIndex: i,
                    curve: curve));
            }
        }

        // Check continuity between consecutive curves
        for (int i = 0; i < curves.Count; i++)
        {
            var currentCurve = curves[i];
            var nextCurve = curves[(i + 1) % curves.Count];

            var gap = currentCurve.EndPoint.DistanceTo(nextCurve.StartPoint);

            if (gap > tolerance)
            {
                if (gap < NearDiscontinuityThreshold)
                {
                    result.AddIssue(new ValidationIssue(
                        ValidationSeverity.Warning,
                        ValidationIssueType.NearDiscontinuity,
                        $"Small gap between curves ({gap:F6})",
                        contourIndex,
                        curveIndex: i,
                        relatedCurveIndex: (i + 1) % curves.Count));
                }
                else
                {
                    result.AddIssue(new ValidationIssue(
                        ValidationSeverity.Error,
                        ValidationIssueType.Discontinuity,
                        $"Gap between curves ({gap:F6})",
                        contourIndex,
                        curveIndex: i,
                        relatedCurveIndex: (i + 1) % curves.Count));
                }
            }
        }

        // Check if contour is closed
        if (curves.Count > 0)
        {
            var firstStart = curves[0].StartPoint;
            var lastEnd = curves[^1].EndPoint;
            var closureGap = firstStart.DistanceTo(lastEnd);

            if (closureGap > tolerance)
            {
                result.AddIssue(new ValidationIssue(
                    ValidationSeverity.Error,
                    ValidationIssueType.NotClosed,
                    $"Contour is not closed (gap: {closureGap:F6})",
                    contourIndex));
            }
        }

        // Check for self-intersections within the contour
        CheckSelfIntersections(curves, contourIndex, result);

        // Check winding order
        CheckWindingOrder(curves, contourIndex, isOuter, result);
    }

    /// <summary>
    /// Checks for self-intersections within a contour.
    /// </summary>
    private void CheckSelfIntersections(
        List<ICurve2D> curves,
        int contourIndex,
        ValidationResult result)
    {
        // Check all pairs of curves for intersections
        for (int i = 0; i < curves.Count; i++)
        {
            for (int j = i + 1; j < curves.Count; j++)
            {
                var curve1 = curves[i];
                var curve2 = curves[j];

                // Quick rejection test using bounding boxes
                if (!curve1.BoundingBox.Intersects(curve2.BoundingBox))
                    continue;

                // Check for intersections using extension method
                var intersections = curve1.Intersect(curve2, tolerance);

                // Filter out valid touches:
                // 1. Shared endpoints (adjacent curves naturally share these)
                // 2. Tangent touches (curves touch but don't cross - valid for rounded corners)
                intersections = intersections
                    .Where(p => !IsSharedEndpoint(curve1, curve2, p) &&
                                !IsTangentTouch(curve1, curve2, p))
                    .ToList();

                if (intersections.Count > 0)
                {
                    result.AddIssue(new ValidationIssue(
                        ValidationSeverity.Error,
                        ValidationIssueType.SelfIntersection,
                        $"Curves intersect at {intersections.Count} point(s)",
                        contourIndex,
                        curveIndex: i,
                        relatedCurveIndex: j));
                }
            }
        }
    }

    /// <summary>
    /// Checks if two curves touch tangentially at a point (touch but don't cross).
    /// </summary>
    private bool IsTangentTouch(ICurve2D curve1, ICurve2D curve2, Point2D point)
    {
        // Get the parameter t for the point on each curve
        var t1 = GetParameterAtPoint(curve1, point);
        var t2 = GetParameterAtPoint(curve2, point);

        if (t1 < 0 || t2 < 0)
            return false; // Point not on one of the curves

        // Get tangent vectors at the intersection point
        var tangent1 = curve1.TangentAt(t1);
        var tangent2 = curve2.TangentAt(t2);

        // Normalize tangents manually
        var len1 = Math.Sqrt(tangent1.X * tangent1.X + tangent1.Y * tangent1.Y);
        var len2 = Math.Sqrt(tangent2.X * tangent2.X + tangent2.Y * tangent2.Y);

        if (len1 < tolerance || len2 < tolerance)
            return false; // Degenerate tangent

        var nx1 = tangent1.X / len1;
        var ny1 = tangent1.Y / len1;
        var nx2 = tangent2.X / len2;
        var ny2 = tangent2.Y / len2;

        // Check if tangents are parallel (same or opposite direction)
        // Dot product of normalized vectors is ±1 for parallel vectors
        var dot = Math.Abs(nx1 * nx2 + ny1 * ny2);
        return dot > 1 - tolerance; // Parallel within tolerance
    }

    /// <summary>
    /// Gets the parameter t (0 to 1) for a point on a curve.
    /// </summary>
    private double GetParameterAtPoint(ICurve2D curve, Point2D point)
    {
        // Binary search or analytical solution depending on curve type
        if (curve is Line2D line)
        {
            var dir = new Vector2D(line.EndPoint.X - line.StartPoint.X,
                                   line.EndPoint.Y - line.StartPoint.Y);
            var toPoint = new Vector2D(point.X - line.StartPoint.X,
                                       point.Y - line.StartPoint.Y);
            var length = Math.Sqrt(dir.X * dir.X + dir.Y * dir.Y);
            if (length < tolerance) return 0;
            return (dir.X * toPoint.X + dir.Y * toPoint.Y) / (length * length);
        }
        else if (curve is Arc2D arc)
        {
            var angle = Math.Atan2(point.Y - arc.Center.Y, point.X - arc.Center.X);
            // Normalize and convert to parameter
            var startAngle = arc.StartAngle;
            var sweepAngle = arc.SweepAngle;
            var deltaAngle = angle - startAngle;

            // Normalize delta to [0, 2π) or (-2π, 0] depending on direction
            while (deltaAngle < 0) deltaAngle += 2 * Math.PI;
            while (deltaAngle >= 2 * Math.PI) deltaAngle -= 2 * Math.PI;

            if (!arc.IsCounterClockwise)
            {
                deltaAngle = 2 * Math.PI - deltaAngle;
                if (deltaAngle >= 2 * Math.PI) deltaAngle -= 2 * Math.PI;
            }

            return deltaAngle / Math.Abs(sweepAngle);
        }

        return -1; // Unknown curve type
    }

    /// <summary>
    /// Checks the winding order of a contour.
    /// Outer contours should be counter-clockwise (CCW), inner contours should be clockwise (CW).
    /// </summary>
    private void CheckWindingOrder(
        List<ICurve2D> curves,
        int contourIndex,
        bool isOuter,
        ValidationResult result)
    {
        if (curves.Count == 0)
            return;

        // Calculate signed area using the shoelace formula
        double signedArea = 0;

        foreach (var curve in curves)
        {
            // Sample points along the curve
            int samples = Math.Max(3, (int)Math.Ceiling(curve.Length / 10)); // At least 3 samples
            for (int i = 0; i < samples; i++)
            {
                double t1 = (double)i / samples;
                double t2 = (double)(i + 1) / samples;

                var p1 = curve.PointAt(t1);
                var p2 = curve.PointAt(t2);

                signedArea += (p2.X - p1.X) * (p2.Y + p1.Y);
            }
        }

        bool isCounterClockwise = signedArea < 0;

        if (isOuter && !isCounterClockwise)
        {
            result.AddIssue(new ValidationIssue(
                ValidationSeverity.Warning,
                ValidationIssueType.IncorrectWindingOrder,
                "Outer contour should be counter-clockwise (CCW)",
                contourIndex));
        }
        else if (!isOuter && isCounterClockwise)
        {
            result.AddIssue(new ValidationIssue(
                ValidationSeverity.Warning,
                ValidationIssueType.IncorrectWindingOrder,
                "Inner contour should be clockwise (CW)",
                contourIndex));
        }
    }

    /// <summary>
    /// Validates intersections between contours (outer-inner and inner-inner).
    /// </summary>
    private void ValidateContourIntersections(
        List<ICurve2D> outerCurves,
        List<List<ICurve2D>> innerContours,
        ValidationResult result)
    {
        // Check outer-inner intersections
        for (int innerIdx = 0; innerIdx < innerContours.Count; innerIdx++)
        {
            var innerCurves = innerContours[innerIdx];

            // Check if inner curves intersect outer curves
            for (int i = 0; i < outerCurves.Count; i++)
            {
                for (int j = 0; j < innerCurves.Count; j++)
                {
                    if (!outerCurves[i].BoundingBox.Intersects(innerCurves[j].BoundingBox))
                        continue;

                    var intersections = outerCurves[i].Intersect(innerCurves[j], tolerance);

                    if (intersections.Count > 0)
                    {
                        result.AddIssue(new ValidationIssue(
                            ValidationSeverity.Error,
                            ValidationIssueType.InnerOuterIntersection,
                            $"Inner contour {innerIdx} intersects outer boundary",
                            0,
                            curveIndex: i,
                            relatedContourIndex: innerIdx + 1,
                            relatedCurveIndex: j));
                    }
                }
            }

            // Check if inner contour is contained within outer boundary (simplified check using bounding boxes)
            var innerBox = innerCurves.GetBoundingBox();
            var outerBox = outerCurves.GetBoundingBox();

            if (!outerBox.Contains(innerBox.Min) || !outerBox.Contains(innerBox.Max))
            {
                result.AddIssue(new ValidationIssue(
                    ValidationSeverity.Error,
                    ValidationIssueType.HoleOutsideBoundary,
                    $"Inner contour {innerIdx} may extend outside outer boundary",
                    innerIdx + 1));
            }
        }

        // Check inner-inner intersections
        for (int i = 0; i < innerContours.Count; i++)
        {
            for (int j = i + 1; j < innerContours.Count; j++)
            {
                var inner1 = innerContours[i];
                var inner2 = innerContours[j];

                // Quick rejection using bounding boxes
                var box1 = inner1.GetBoundingBox();
                var box2 = inner2.GetBoundingBox();

                if (!box1.Intersects(box2))
                    continue;

                // Check all curve pairs
                for (int ci = 0; ci < inner1.Count; ci++)
                {
                    for (int cj = 0; cj < inner2.Count; cj++)
                    {
                        if (!inner1[ci].BoundingBox.Intersects(inner2[cj].BoundingBox))
                            continue;

                        var intersections = inner1[ci].Intersect(inner2[cj], tolerance);

                        if (intersections.Count > 0)
                        {
                            result.AddIssue(new ValidationIssue(
                                ValidationSeverity.Error,
                                ValidationIssueType.InnerInnerIntersection,
                                $"Inner contour {i} intersects inner contour {j}",
                                i + 1,
                                relatedContourIndex: j + 1));
                            goto NextInnerPair; // Found intersection, move to next pair
                        }
                    }
                }
                NextInnerPair:;
            }
        }
    }

    /// <summary>
    /// Checks if a point is a shared endpoint between two curves.
    /// </summary>
    private bool IsSharedEndpoint(ICurve2D curve1, ICurve2D curve2, Point2D point)
    {
        bool isEndpoint1 = point.IsEqualTo(curve1.StartPoint, tolerance) ||
                          point.IsEqualTo(curve1.EndPoint, tolerance);
        bool isEndpoint2 = point.IsEqualTo(curve2.StartPoint, tolerance) ||
                          point.IsEqualTo(curve2.EndPoint, tolerance);

        return isEndpoint1 && isEndpoint2;
    }
}
