using BasAutomation.Geometry.Curves;

namespace BasAutomation.Geometry.Validation;

/// <summary>
/// Represents the severity level of a validation issue.
/// </summary>
public enum ValidationSeverity
{
    /// <summary>
    /// Warning that may cause issues but won't necessarily fail extrusion.
    /// </summary>
    Warning,

    /// <summary>
    /// Critical error that will likely cause extrusion to fail.
    /// </summary>
    Error
}

/// <summary>
/// Represents the type of validation issue found in a cross-section.
/// </summary>
public enum ValidationIssueType
{
    /// <summary>
    /// A curve has zero length.
    /// </summary>
    ZeroLengthCurve,

    /// <summary>
    /// A curve has near-zero length (below warning threshold).
    /// </summary>
    NearZeroLengthCurve,

    /// <summary>
    /// Curves in a contour intersect each other at non-endpoint locations.
    /// </summary>
    SelfIntersection,

    /// <summary>
    /// Gap between consecutive curves in a contour.
    /// </summary>
    Discontinuity,

    /// <summary>
    /// Near-gap between consecutive curves (within warning threshold).
    /// </summary>
    NearDiscontinuity,

    /// <summary>
    /// Contour is not closed (first and last curve endpoints don't meet).
    /// </summary>
    NotClosed,

    /// <summary>
    /// Contour has incorrect winding order (outer should be CCW, inner should be CW).
    /// </summary>
    IncorrectWindingOrder,

    /// <summary>
    /// Consecutive points are identical or nearly identical.
    /// </summary>
    DuplicatePoint,

    /// <summary>
    /// Inner contour intersects the outer boundary.
    /// </summary>
    InnerOuterIntersection,

    /// <summary>
    /// Inner contours intersect each other.
    /// </summary>
    InnerInnerIntersection,

    /// <summary>
    /// Inner contour (hole) is not fully contained within the outer boundary.
    /// </summary>
    HoleOutsideBoundary
}

/// <summary>
/// Represents a single validation issue found in a cross-section.
/// </summary>
public class ValidationIssue
{
    /// <summary>
    /// Gets the severity of the issue.
    /// </summary>
    public ValidationSeverity Severity { get; }

    /// <summary>
    /// Gets the type of validation issue.
    /// </summary>
    public ValidationIssueType Type { get; }

    /// <summary>
    /// Gets a human-readable description of the issue.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the contour index where the issue was found (0 = outer, 1+ = inner contours).
    /// </summary>
    public int ContourIndex { get; }

    /// <summary>
    /// Gets the curve index within the contour where the issue was found (if applicable).
    /// </summary>
    public int? CurveIndex { get; }

    /// <summary>
    /// Gets the related curve index for issues involving multiple curves (if applicable).
    /// </summary>
    public int? RelatedCurveIndex { get; }

    /// <summary>
    /// Gets the related contour index for issues involving multiple contours (if applicable).
    /// </summary>
    public int? RelatedContourIndex { get; }

    /// <summary>
    /// Gets the curve involved in the issue (if applicable).
    /// </summary>
    public ICurve2D? Curve { get; }

    /// <summary>
    /// Initializes a new instance of the ValidationIssue class.
    /// </summary>
    public ValidationIssue(
        ValidationSeverity severity,
        ValidationIssueType type,
        string description,
        int contourIndex,
        int? curveIndex = null,
        int? relatedCurveIndex = null,
        int? relatedContourIndex = null,
        ICurve2D? curve = null)
    {
        Severity = severity;
        Type = type;
        Description = description;
        ContourIndex = contourIndex;
        CurveIndex = curveIndex;
        RelatedCurveIndex = relatedCurveIndex;
        RelatedContourIndex = relatedContourIndex;
        Curve = curve;
    }

    public override string ToString()
    {
        var location = ContourIndex == 0 ? "Outer" : $"Inner[{ContourIndex - 1}]";
        var curveInfo = CurveIndex.HasValue ? $" Curve[{CurveIndex.Value}]" : "";
        return $"[{Severity}] {location}{curveInfo}: {Description}";
    }
}
