using BasAutomation.Geometry.Curves;
using BasAutomation.Geometry.Validation;

namespace BasAutomation.Geometry.Tests.Validation;

public class CrossSectionValidatorTests
{
    private readonly CrossSectionValidator validator = new();

    #region Valid Cross-Sections

    [Fact]
    public void Validate_SimpleRectangle_IsValid()
    {
        // Arrange - CCW rectangle
        var curves = new List<ICurve2D>
        {
            new Line2D(new Point2D(0, 0), new Point2D(10, 0)),
            new Line2D(new Point2D(10, 0), new Point2D(10, 5)),
            new Line2D(new Point2D(10, 5), new Point2D(0, 5)),
            new Line2D(new Point2D(0, 5), new Point2D(0, 0))
        };

        // Act
        var result = validator.Validate(curves);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(0, result.ErrorCount);
        Assert.Equal(0, result.WarningCount);
    }

    [Fact]
    public void Validate_RectangleWithArc_IsValid()
    {
        // Arrange - Rectangle with one rounded corner (CCW)
        // Arc from (10, 4) to (9, 5) with center at (10, 5) - quarter circle radius 1
        var curves = new List<ICurve2D>
        {
            new Line2D(new Point2D(0, 0), new Point2D(10, 0)),
            new Line2D(new Point2D(10, 0), new Point2D(10, 4)),
            new Arc2D(new Point2D(10, 4), new Point2D(9, 5), new Point2D(10, 5), isCounterClockwise: true),
            new Line2D(new Point2D(9, 5), new Point2D(0, 5)),
            new Line2D(new Point2D(0, 5), new Point2D(0, 0))
        };

        // Act
        var result = validator.Validate(curves);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(0, result.ErrorCount);
    }

    [Fact]
    public void Validate_CircleAsOuterCurve_IsValid()
    {
        // Arrange
        var curves = new List<ICurve2D>
        {
            new Circle2D(new Point2D(5, 5), 5)
        };

        // Act
        var result = validator.Validate(curves);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(0, result.ErrorCount);
    }

    #endregion

    #region Zero-Length and Near-Zero Tests

    [Fact]
    public void Validate_ZeroLengthLine_ReturnsError()
    {
        // Arrange
        var curves = new List<ICurve2D>
        {
            new Line2D(new Point2D(0, 0), new Point2D(0, 0)), // Zero length
            new Line2D(new Point2D(0, 0), new Point2D(10, 0)),
            new Line2D(new Point2D(10, 0), new Point2D(0, 0))
        };

        // Act
        var result = validator.Validate(curves);

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.HasErrors);
        Assert.Contains(result.Errors, e => e.Type == ValidationIssueType.ZeroLengthCurve);
    }

    [Fact]
    public void Validate_NearZeroLengthLine_ReturnsWarning()
    {
        // Arrange - Very short line (0.005 units)
        var curves = new List<ICurve2D>
        {
            new Line2D(new Point2D(0, 0), new Point2D(0.005, 0)),
            new Line2D(new Point2D(0.005, 0), new Point2D(10, 0)),
            new Line2D(new Point2D(10, 0), new Point2D(10, 5)),
            new Line2D(new Point2D(10, 5), new Point2D(0, 5)),
            new Line2D(new Point2D(0, 5), new Point2D(0, 0))
        };

        // Act
        var result = validator.Validate(curves);

        // Assert
        Assert.True(result.IsValid); // No errors, but has warnings
        Assert.True(result.HasWarnings);
        Assert.Contains(result.Warnings, w => w.Type == ValidationIssueType.NearZeroLengthCurve);
    }

    #endregion

    #region Continuity and Closure Tests

    [Fact]
    public void Validate_DiscontinuousContour_ReturnsError()
    {
        // Arrange - Gap between second and third line
        var curves = new List<ICurve2D>
        {
            new Line2D(new Point2D(0, 0), new Point2D(10, 0)),
            new Line2D(new Point2D(10, 0), new Point2D(10, 5)),
            new Line2D(new Point2D(12, 5), new Point2D(0, 5)), // Gap!
            new Line2D(new Point2D(0, 5), new Point2D(0, 0))
        };

        // Act
        var result = validator.Validate(curves);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Type == ValidationIssueType.Discontinuity);
    }

    [Fact]
    public void Validate_SmallGap_ReturnsWarning()
    {
        // Arrange - Small gap (0.05 units) between consecutive lines
        // Creating rectangle where one connection has a small gap
        var curves = new List<ICurve2D>
        {
            new Line2D(new Point2D(0, 0), new Point2D(10, 0)),
            new Line2D(new Point2D(10, 0), new Point2D(10, 5)),
            new Line2D(new Point2D(10, 5), new Point2D(0.05, 5)), // Small gap at end
            new Line2D(new Point2D(0, 5), new Point2D(0, 0))  // Small gap at start
        };

        // Act
        var result = validator.Validate(curves);

        // Assert
        Assert.True(result.IsValid); // Valid but has warnings
        Assert.True(result.HasWarnings);
        Assert.Contains(result.Warnings, w => w.Type == ValidationIssueType.NearDiscontinuity);
    }

    [Fact]
    public void Validate_NotClosedContour_ReturnsError()
    {
        // Arrange - Last line doesn't connect to first
        var curves = new List<ICurve2D>
        {
            new Line2D(new Point2D(0, 0), new Point2D(10, 0)),
            new Line2D(new Point2D(10, 0), new Point2D(10, 5)),
            new Line2D(new Point2D(10, 5), new Point2D(0, 5)),
            new Line2D(new Point2D(0, 5), new Point2D(0, 2)) // Doesn't close!
        };

        // Act
        var result = validator.Validate(curves);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Type == ValidationIssueType.NotClosed);
    }

    [Fact]
    public void Validate_EmptyContour_ReturnsError()
    {
        // Arrange
        var curves = new List<ICurve2D>();

        // Act
        var result = validator.Validate(curves);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Type == ValidationIssueType.NotClosed);
    }

    #endregion

    #region Self-Intersection Tests

    [Fact]
    public void Validate_SelfIntersectingLines_ReturnsError()
    {
        // Arrange - Figure-eight shape (self-intersecting)
        var curves = new List<ICurve2D>
        {
            new Line2D(new Point2D(0, 0), new Point2D(10, 10)),
            new Line2D(new Point2D(10, 10), new Point2D(10, 0)),
            new Line2D(new Point2D(10, 0), new Point2D(0, 10)),
            new Line2D(new Point2D(0, 10), new Point2D(0, 0))
        };

        // Act
        var result = validator.Validate(curves);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Type == ValidationIssueType.SelfIntersection);
    }

    [Fact]
    public void Validate_AdjacentCurvesShareEndpoint_NoFalsePositive()
    {
        // Arrange - Normal rectangle (adjacent curves share endpoints, should not be flagged)
        var curves = new List<ICurve2D>
        {
            new Line2D(new Point2D(0, 0), new Point2D(10, 0)),
            new Line2D(new Point2D(10, 0), new Point2D(10, 5)),
            new Line2D(new Point2D(10, 5), new Point2D(0, 5)),
            new Line2D(new Point2D(0, 5), new Point2D(0, 0))
        };

        // Act
        var result = validator.Validate(curves);

        // Assert
        Assert.True(result.IsValid);
        Assert.DoesNotContain(result.Errors, e => e.Type == ValidationIssueType.SelfIntersection);
    }

    #endregion

    #region Winding Order Tests

    [Fact]
    public void Validate_ClockwiseOuter_ReturnsWarning()
    {
        // Arrange - CW rectangle (should be CCW for outer)
        var curves = new List<ICurve2D>
        {
            new Line2D(new Point2D(0, 0), new Point2D(0, 5)),
            new Line2D(new Point2D(0, 5), new Point2D(10, 5)),
            new Line2D(new Point2D(10, 5), new Point2D(10, 0)),
            new Line2D(new Point2D(10, 0), new Point2D(0, 0))
        };

        // Act
        var result = validator.Validate(curves);

        // Assert
        Assert.True(result.IsValid); // Valid but has warning
        Assert.True(result.HasWarnings);
        Assert.Contains(result.Warnings, w => w.Type == ValidationIssueType.IncorrectWindingOrder);
    }

    [Fact]
    public void Validate_CounterClockwiseOuter_NoWindingWarning()
    {
        // Arrange - CCW rectangle (correct for outer)
        var curves = new List<ICurve2D>
        {
            new Line2D(new Point2D(0, 0), new Point2D(10, 0)),
            new Line2D(new Point2D(10, 0), new Point2D(10, 5)),
            new Line2D(new Point2D(10, 5), new Point2D(0, 5)),
            new Line2D(new Point2D(0, 5), new Point2D(0, 0))
        };

        // Act
        var result = validator.Validate(curves);

        // Assert
        Assert.True(result.IsValid);
        Assert.DoesNotContain(result.Warnings, w => w.Type == ValidationIssueType.IncorrectWindingOrder);
    }

    #endregion

    #region Inner Contour Tests

    [Fact]
    public void Validate_ValidRectangleWithHole_IsValid()
    {
        // Arrange - Outer CCW, Inner CW
        var outerCurves = new List<ICurve2D>
        {
            new Line2D(new Point2D(0, 0), new Point2D(10, 0)),
            new Line2D(new Point2D(10, 0), new Point2D(10, 10)),
            new Line2D(new Point2D(10, 10), new Point2D(0, 10)),
            new Line2D(new Point2D(0, 10), new Point2D(0, 0))
        };

        var innerCurves = new List<ICurve2D>
        {
            new Line2D(new Point2D(2, 2), new Point2D(2, 8)),
            new Line2D(new Point2D(2, 8), new Point2D(8, 8)),
            new Line2D(new Point2D(8, 8), new Point2D(8, 2)),
            new Line2D(new Point2D(8, 2), new Point2D(2, 2))
        };

        // Act
        var result = validator.Validate(outerCurves, new[] { innerCurves });

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(0, result.ErrorCount);
    }

    [Fact]
    public void Validate_HoleOutsideBoundary_ReturnsError()
    {
        // Arrange
        var outerCurves = new List<ICurve2D>
        {
            new Line2D(new Point2D(0, 0), new Point2D(10, 0)),
            new Line2D(new Point2D(10, 0), new Point2D(10, 10)),
            new Line2D(new Point2D(10, 10), new Point2D(0, 10)),
            new Line2D(new Point2D(0, 10), new Point2D(0, 0))
        };

        var innerCurves = new List<ICurve2D>
        {
            new Line2D(new Point2D(12, 2), new Point2D(12, 8)), // Outside!
            new Line2D(new Point2D(12, 8), new Point2D(18, 8)),
            new Line2D(new Point2D(18, 8), new Point2D(18, 2)),
            new Line2D(new Point2D(18, 2), new Point2D(12, 2))
        };

        // Act
        var result = validator.Validate(outerCurves, new[] { innerCurves });

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Type == ValidationIssueType.HoleOutsideBoundary);
    }

    [Fact]
    public void Validate_InnerIntersectsOuter_ReturnsError()
    {
        // Arrange
        var outerCurves = new List<ICurve2D>
        {
            new Line2D(new Point2D(0, 0), new Point2D(10, 0)),
            new Line2D(new Point2D(10, 0), new Point2D(10, 10)),
            new Line2D(new Point2D(10, 10), new Point2D(0, 10)),
            new Line2D(new Point2D(0, 10), new Point2D(0, 0))
        };

        var innerCurves = new List<ICurve2D>
        {
            new Line2D(new Point2D(5, 5), new Point2D(5, 15)), // Crosses outer!
            new Line2D(new Point2D(5, 15), new Point2D(8, 15)),
            new Line2D(new Point2D(8, 15), new Point2D(8, 5)),
            new Line2D(new Point2D(8, 5), new Point2D(5, 5))
        };

        // Act
        var result = validator.Validate(outerCurves, new[] { innerCurves });

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Type == ValidationIssueType.InnerOuterIntersection);
    }

    [Fact]
    public void Validate_TwoIntersectingHoles_ReturnsError()
    {
        // Arrange
        var outerCurves = new List<ICurve2D>
        {
            new Line2D(new Point2D(0, 0), new Point2D(20, 0)),
            new Line2D(new Point2D(20, 0), new Point2D(20, 20)),
            new Line2D(new Point2D(20, 20), new Point2D(0, 20)),
            new Line2D(new Point2D(0, 20), new Point2D(0, 0))
        };

        var inner1 = new List<ICurve2D>
        {
            new Line2D(new Point2D(2, 2), new Point2D(10, 2)),
            new Line2D(new Point2D(10, 2), new Point2D(10, 10)),
            new Line2D(new Point2D(10, 10), new Point2D(2, 10)),
            new Line2D(new Point2D(2, 10), new Point2D(2, 2))
        };

        var inner2 = new List<ICurve2D>
        {
            new Line2D(new Point2D(8, 8), new Point2D(18, 8)), // Overlaps inner1
            new Line2D(new Point2D(18, 8), new Point2D(18, 18)),
            new Line2D(new Point2D(18, 18), new Point2D(8, 18)),
            new Line2D(new Point2D(8, 18), new Point2D(8, 8))
        };

        // Act
        var result = validator.Validate(outerCurves, new[] { inner1, inner2 });

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Type == ValidationIssueType.InnerInnerIntersection);
    }

    [Fact]
    public void Validate_CounterClockwiseInner_ReturnsWarning()
    {
        // Arrange - Inner should be CW, not CCW
        var outerCurves = new List<ICurve2D>
        {
            new Line2D(new Point2D(0, 0), new Point2D(10, 0)),
            new Line2D(new Point2D(10, 0), new Point2D(10, 10)),
            new Line2D(new Point2D(10, 10), new Point2D(0, 10)),
            new Line2D(new Point2D(0, 10), new Point2D(0, 0))
        };

        var innerCurves = new List<ICurve2D>
        {
            new Line2D(new Point2D(2, 2), new Point2D(8, 2)), // CCW (wrong!)
            new Line2D(new Point2D(8, 2), new Point2D(8, 8)),
            new Line2D(new Point2D(8, 8), new Point2D(2, 8)),
            new Line2D(new Point2D(2, 8), new Point2D(2, 2))
        };

        // Act
        var result = validator.Validate(outerCurves, new[] { innerCurves });

        // Assert
        Assert.True(result.IsValid); // Valid but warning
        Assert.True(result.HasWarnings);
        Assert.Contains(result.Warnings, w => w.Type == ValidationIssueType.IncorrectWindingOrder);
    }

    #endregion

    #region ValidationResult Tests

    [Fact]
    public void ValidationResult_GetSummary_ReturnsCorrectString()
    {
        // Arrange
        var curves = new List<ICurve2D>
        {
            new Line2D(new Point2D(0, 0), new Point2D(0, 0)), // Error
            new Line2D(new Point2D(0, 0), new Point2D(0.005, 0)), // Warning
            new Line2D(new Point2D(0.005, 0), new Point2D(10, 0)),
            new Line2D(new Point2D(10, 0), new Point2D(0, 0))
        };

        // Act
        var result = validator.Validate(curves);
        var summary = result.GetSummary();

        // Assert
        Assert.Contains("error", summary, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("warning", summary, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidationResult_GetDetailedReport_ContainsAllIssues()
    {
        // Arrange
        var curves = new List<ICurve2D>
        {
            new Line2D(new Point2D(0, 0), new Point2D(0, 0)), // Zero length error
            new Line2D(new Point2D(0, 0), new Point2D(10, 0)),
            new Line2D(new Point2D(10, 0), new Point2D(0, 0))
        };

        // Act
        var result = validator.Validate(curves);
        var report = result.GetDetailedReport();

        // Assert
        Assert.Contains("ERRORS:", report);
        Assert.Contains("zero length", report, StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}
