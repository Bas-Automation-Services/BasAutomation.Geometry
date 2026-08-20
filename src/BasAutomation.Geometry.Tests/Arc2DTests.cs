using BasAutomation.Geometry.Curves;

namespace BasAutomation.Geometry.Tests;

public class Arc2DTests
{
    [Fact]
    public void Constructor_WithCenterRadiusAngles_CreatesCorrectArc()
    {
        // Arrange
        var center = new Point2D(0, 0);
        var radius = 5.0;
        var startAngle = 0.0;
        var endAngle = Math.PI / 2;

        // Act
        var arc = new Arc2D(center, radius, startAngle, endAngle);

        // Assert
        Assert.Equal(center, arc.Center);
        Assert.Equal(radius, arc.Radius);
        Assert.Equal(startAngle, arc.StartAngle, Constants.Tolerance);
        Assert.Equal(endAngle, arc.EndAngle, Constants.Tolerance);
        Assert.True(arc.IsCounterClockwise);
    }

    [Fact]
    public void Constructor_WithStartEndCenterPoints_CreatesCorrectArc()
    {
        // Arrange
        var center = new Point2D(0, 0);
        var startPoint = new Point2D(5, 0);  // 0 degrees
        var endPoint = new Point2D(0, 5);    // 90 degrees

        // Act
        var arc = new Arc2D(startPoint, endPoint, center, true); // Explicitly set counterclockwise

        // Assert
        Assert.Equal(center, arc.Center);
        Assert.Equal(5.0, arc.Radius, Constants.Tolerance);
        Assert.True(arc.StartPoint.IsEqualTo(startPoint, Constants.Tolerance));
        Assert.True(arc.EndPoint.IsEqualTo(endPoint, Constants.Tolerance));
        Assert.True(arc.IsCounterClockwise);
        Assert.Equal(Math.PI / 2, arc.SweepAngle, Constants.Tolerance); // 90 degrees CCW
    }

    [Fact]
    public void Constructor_WithThreePointsOnArc_CreatesCorrectArc()
    {
        // Arrange - Create three points on a circle centered at (0,0) with radius 5
        var point1 = new Point2D(5, 0);    // 0 degrees
        var point2 = new Point2D(0, 5);    // 90 degrees  
        var point3 = new Point2D(-5, 0);   // 180 degrees

        // Act
        var arc = new Arc2D(point1, point2, point3);

        // Assert
        Assert.Equal(0, arc.Center.X, Constants.Tolerance);
        Assert.Equal(0, arc.Center.Y, Constants.Tolerance);
        Assert.Equal(5, arc.Radius, Constants.Tolerance);
        Assert.True(arc.IsCounterClockwise); // Should go CCW from point1 through point2 to point3

        // Verify start and end points
        Assert.True(arc.StartPoint.IsEqualTo(point1, Constants.Tolerance));
        Assert.True(arc.EndPoint.IsEqualTo(point3, Constants.Tolerance));

        // Verify that point2 is on the arc
        Assert.True(arc.ContainsPoint(point2, Constants.Tolerance));
    }

    [Fact]
    public void Constructor_WithThreePointsOnArc_CreatesClockwiseArc()
    {
        // Arrange - Create three points that form a clockwise arc
        var point1 = new Point2D(5, 0);    // 0 degrees
        var point2 = new Point2D(0, -5);   // -90 degrees (270 degrees)
        var point3 = new Point2D(-5, 0);   // 180 degrees

        // Act
        var arc = new Arc2D(point1, point2, point3);

        // Assert
        Assert.Equal(0, arc.Center.X, Constants.Tolerance);
        Assert.Equal(0, arc.Center.Y, Constants.Tolerance);
        Assert.Equal(5, arc.Radius, Constants.Tolerance);
        Assert.False(arc.IsCounterClockwise); // Should go CW from point1 through point2 to point3

        // Verify start and end points
        Assert.True(arc.StartPoint.IsEqualTo(point1, Constants.Tolerance));
        Assert.True(arc.EndPoint.IsEqualTo(point3, Constants.Tolerance));

        // Verify that point2 is on the arc
        Assert.True(arc.ContainsPoint(point2, Constants.Tolerance));
    }

    [Fact]
    public void Constructor_WithThreeCollinearPoints_ThrowsException()
    {
        // Arrange - Three collinear points
        var point1 = new Point2D(0, 0);
        var point2 = new Point2D(1, 1);
        var point3 = new Point2D(2, 2);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Arc2D(point1, point2, point3));
        Assert.Contains("collinear", exception.Message);
    }

    [Fact]
    public void Constructor_WithThreePoints_SmallArc()
    {
        // Arrange - Three points forming a small arc (less than 180 degrees)
        var center = new Point2D(10, 10);
        var radius = 5.0;
        var angle1 = 0.0;
        var angle2 = Math.PI / 4;  // 45 degrees
        var angle3 = Math.PI / 2;  // 90 degrees

        var point1 = new Point2D(center.X + radius * Math.Cos(angle1), center.Y + radius * Math.Sin(angle1));
        var point2 = new Point2D(center.X + radius * Math.Cos(angle2), center.Y + radius * Math.Sin(angle2));
        var point3 = new Point2D(center.X + radius * Math.Cos(angle3), center.Y + radius * Math.Sin(angle3));

        // Act
        var arc = new Arc2D(point1, point2, point3);

        // Assert
        Assert.Equal(center.X, arc.Center.X, Constants.Tolerance);
        Assert.Equal(center.Y, arc.Center.Y, Constants.Tolerance);
        Assert.Equal(radius, arc.Radius, Constants.Tolerance);
        Assert.True(arc.IsCounterClockwise);

        // Verify the sweep angle is approximately 90 degrees
        Assert.Equal(Math.PI / 2, Math.Abs(arc.SweepAngle), 0.01);
    }

    [Fact]
    public void Constructor_WithStartEndCenterPoints_ThrowsIfPointsNotEquidistant()
    {
        // Arrange
        var center = new Point2D(0, 0);
        var startPoint = new Point2D(5, 0);
        var endPoint = new Point2D(0, 3);  // Different radius

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Arc2D(startPoint, endPoint, center, true));
        Assert.Contains("equidistant", exception.Message);
    }

    [Fact]
    public void FromCenterRadiusAngles_WithPositiveSweep_CreatesCounterClockwiseArc()
    {
        // Arrange
        var center = new Point2D(1, 1);
        var radius = 3.0;
        var startAngle = 0.0;
        var sweepAngle = Math.PI / 2;

        // Act
        var arc = new Arc2D(center, radius, startAngle, sweepAngle);

        // Assert
        Assert.Equal(center, arc.Center);
        Assert.Equal(radius, arc.Radius);
        Assert.Equal(startAngle, arc.StartAngle, Constants.Tolerance);
        Assert.Equal(Math.PI / 2, arc.EndAngle, Constants.Tolerance);
        Assert.True(arc.IsCounterClockwise);
        Assert.Equal(sweepAngle, arc.SweepAngle, Constants.Tolerance);
    }

    [Fact]
    public void FromCenterRadiusAngles_WithNegativeSweep_CreatesClockwiseArc()
    {
        // Arrange
        var center = new Point2D(0, 0);
        var radius = 2.0;
        var startAngle = Math.PI / 2;      // 90°
        var sweepAngle = -Math.PI / 2;     // -90° (desired sweep)
        var endAngle = startAngle + sweepAngle;  // 0° (this is what you need to pass!)
        var isCounterClockwise = false;     // Clockwise

        // Act
        var arc = new Arc2D(center, radius, startAngle, endAngle, isCounterClockwise);

        // Assert
        Assert.False(arc.IsCounterClockwise);
        Assert.Equal(sweepAngle, arc.SweepAngle, Constants.Tolerance);
    }

    [Fact]
    public void FromThreePointsOnArc_CreatesCorrectArc()
    {
        // Arrange - Points on a unit circle
        var p1 = new Point2D(1, 0);    // 0 degrees (start)
        var p2 = new Point2D(0, 1);    // 90 degrees (middle)
        var p3 = new Point2D(-1, 0);   // 180 degrees (end)

        // Act
        var arc = new Arc2D(p1, p2, p3);

        // Assert
        Assert.True(arc.Center.IsEqualTo(new Point2D(0, 0), Constants.Tolerance));
        Assert.Equal(1.0, arc.Radius, Constants.Tolerance);
        Assert.True(arc.ContainsPoint(p1, Constants.Tolerance));
        Assert.True(arc.ContainsPoint(p2, Constants.Tolerance));
        Assert.True(arc.ContainsPoint(p3, Constants.Tolerance));
        Assert.True(arc.IsCounterClockwise);
        Assert.Equal(Math.PI, arc.SweepAngle, Constants.Tolerance); // 180 degrees
    }

    [Fact]
    public void FromStartCenterEnd_CreatesCorrectArc()
    {
        // Arrange
        var startPoint = new Point2D(1, 0);   // 0 degrees
        var centerPoint = new Point2D(0, 0);  // center at origin
        var endPoint = new Point2D(0, 1);     // 90 degrees

        // Act
        var arc = new Arc2D(startPoint, endPoint, centerPoint, true);

        // Assert
        Assert.Equal(centerPoint, arc.Center);
        Assert.Equal(1.0, arc.Radius, Constants.Tolerance);
        Assert.True(arc.StartPoint.IsEqualTo(startPoint, Constants.Tolerance));
        Assert.True(arc.EndPoint.IsEqualTo(endPoint, Constants.Tolerance));
        Assert.True(arc.IsCounterClockwise);
        Assert.Equal(Math.PI / 2, arc.SweepAngle, Constants.Tolerance); // 90 degrees
    }

    [Fact]
    public void FromStartCenterEnd_ClockwiseArc_CreatesCorrectArc()
    {
        // Arrange
        var startPoint = new Point2D(1, 0);   // 0 degrees
        var centerPoint = new Point2D(0, 0);  // center at origin
        var endPoint = new Point2D(0, 1);     // 90 degrees

        // Act
        var arc = new Arc2D(startPoint, endPoint, centerPoint, false);

        // Assert
        Assert.Equal(centerPoint, arc.Center);
        Assert.Equal(1.0, arc.Radius, Constants.Tolerance);
        Assert.True(arc.StartPoint.IsEqualTo(startPoint, Constants.Tolerance));
        Assert.True(arc.EndPoint.IsEqualTo(endPoint, Constants.Tolerance));
        Assert.False(arc.IsCounterClockwise);
        Assert.Equal(-3 * Math.PI / 2, arc.SweepAngle, Constants.Tolerance); // -270 degrees
    }

    [Fact]
    public void FromStartCenterEnd_InvalidRadius_ThrowsException()
    {
        // Arrange
        var startPoint = new Point2D(1, 0);
        var centerPoint = new Point2D(0, 0);
        var endPoint = new Point2D(0, 2); // Different radius

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new Arc2D(startPoint, endPoint, centerPoint, true));
    }

    [Fact]
    public void FromThreePointsOnArc_WithCollinearPoints_ThrowsException()
    {
        // Arrange - Three collinear points
        var p1 = new Point2D(0, 0);
        var p2 = new Point2D(1, 0);
        var p3 = new Point2D(2, 0);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Arc2D(p1, p2, p3));
    }

    [Fact]
    public void StartPoint_ReturnsCorrectPoint()
    {
        // Arrange
        var center = new Point2D(0, 0);
        var radius = 5.0;
        var arc = new Arc2D(center, radius, 0, Math.PI / 2);

        // Act
        var startPoint = arc.StartPoint;

        // Assert
        Assert.Equal(5.0, startPoint.X, Constants.Tolerance);
        Assert.Equal(0.0, startPoint.Y, Constants.Tolerance);
    }

    [Fact]      
    public void EndPoint_ReturnsCorrectPoint()
    {
        // Arrange
        var center = new Point2D(0, 0);
        var radius = 5.0;
        var arc = new Arc2D(center, radius, 0, Math.PI / 2);

        // Act
        var endPoint = arc.EndPoint;

        // Assert
        Assert.Equal(0.0, endPoint.X, Constants.Tolerance);
        Assert.Equal(5.0, endPoint.Y, Constants.Tolerance);
    }

    [Fact]
    public void SweepAngle_CounterClockwise_ReturnsPositiveAngle()
    {
        // Arrange
        var arc = new Arc2D(new Point2D(0, 0), 1, 0, Math.PI / 2, true);

        // Act & Assert
        Assert.Equal(Math.PI / 2, arc.SweepAngle, Constants.Tolerance);
    }

    [Fact]
    public void SweepAngle_Clockwise_ReturnsNegativeAngle()
    {
        // Arrange
        var arc = new Arc2D(new Point2D(0, 0), 1, 0, Math.PI / 2, false);

        // Act & Assert
        Assert.Equal(-3 * Math.PI / 2, arc.SweepAngle, Constants.Tolerance);
    }

    [Fact]
    public void SweepAngle_CrossingZero_CounterClockwise_ReturnsCorrectAngle()
    {
        // Arrange - Arc from 315° to 45° counterclockwise
        var arc = new Arc2D(new Point2D(0, 0), 1, 7 * Math.PI / 4, Math.PI / 4, true);

        // Act & Assert
        Assert.Equal(Math.PI / 2, arc.SweepAngle, Constants.Tolerance);
    }

    [Fact]
    public void Length_ReturnsCorrectArcLength()
    {
        // Arrange - Quarter circle with radius 2
        var arc = new Arc2D(new Point2D(0, 0), 2, 0, Math.PI / 2);

        // Act & Assert
        Assert.Equal(Math.PI, arc.Length, Constants.Tolerance);  // 2 * π/2 = π
    }

    [Fact]
    public void MidPoint_ReturnsCorrectPoint()
    {
        // Arrange - Quarter circle from 0 to 90 degrees
        var arc = new Arc2D(new Point2D(0, 0), 1, 0, Math.PI / 2);

        // Act
        var midPoint = arc.MidPoint;

        // Assert - Should be at 45 degrees
        var expected = new Point2D(Math.Cos(Math.PI / 4), Math.Sin(Math.PI / 4));
        Assert.True(midPoint.IsEqualTo(expected, Constants.Tolerance));
    }

    [Fact]
    public void PointAt_VariousParameters_ReturnsCorrectPoints()
    {
        // Arrange
        var arc = new Arc2D(new Point2D(0, 0), 2, 0, Math.PI);

        // Act & Assert
        Assert.True(arc.PointAt(0).IsEqualTo(new Point2D(2, 0), Constants.Tolerance));
        Assert.True(arc.PointAt(0.5).IsEqualTo(new Point2D(0, 2), Constants.Tolerance));
        Assert.True(arc.PointAt(1).IsEqualTo(new Point2D(-2, 0), Constants.Tolerance));
    }

    [Fact]
    public void TangentAt_ReturnsCorrectTangentVectors()
    {
        // Arrange - Quarter circle counterclockwise
        var arc = new Arc2D(new Point2D(0, 0), 1, 0, Math.PI / 2);

        // Act & Assert
        // At start (0°), tangent should point upward (0, 1)
        var tangentStart = arc.TangentAt(0);
        Assert.True(tangentStart.IsEqualTo(new Vector2D(0, 1), Constants.Tolerance));

        // At end (90°), tangent should point left (-1, 0)
        var tangentEnd = arc.TangentAt(1);
        Assert.True(tangentEnd.IsEqualTo(new Vector2D(-1, 0), Constants.Tolerance));
    }

    [Fact]
    public void ContainsPoint_PointOnArc_ReturnsTrue()
    {
        // Arrange
        var arc = new Arc2D(new Point2D(0, 0), 5, 0, Math.PI / 2);
        var pointOnArc = new Point2D(5 * Math.Cos(Math.PI / 4), 5 * Math.Sin(Math.PI / 4));

        // Act & Assert
        Assert.True(arc.ContainsPoint(pointOnArc));
    }

    [Fact]
    public void ContainsPoint_PointNotOnArc_ReturnsFalse()
    {
        // Arrange
        var arc = new Arc2D(new Point2D(0, 0), 5, 0, Math.PI / 2);

        // Point at wrong radius
        var pointWrongRadius = new Point2D(3, 0);

        // Point outside angular range
        var pointOutsideRange = new Point2D(-5, 0);

        // Act & Assert
        Assert.False(arc.ContainsPoint(pointWrongRadius));
        Assert.False(arc.ContainsPoint(pointOutsideRange));
    }

    [Fact]
    public void GetBoundingBox_QuarterCircle_ReturnsCorrectBox()
    {
        // Arrange - Quarter circle from 0° to 90°
        var arc = new Arc2D(new Point2D(0, 0), 5, 0, Math.PI / 2);

        // Act
        var box = arc.GetBoundingBox();

        // Assert
        Assert.Equal(0, box.Min.X, Constants.Tolerance);
        Assert.Equal(0, box.Min.Y, Constants.Tolerance);
        Assert.Equal(5, box.Max.X, Constants.Tolerance);
        Assert.Equal(5, box.Max.Y, Constants.Tolerance);
    }

    [Fact]
    public void GetBoundingBox_ArcCrossingCardinalDirections_IncludesExtremities()
    {
        // Arrange - Arc from 45° to 225° (crosses 90° and 180°)
        var arc = new Arc2D(new Point2D(0, 0), 2, Math.PI / 4, 5 * Math.PI / 4);

        // Act
        var box = arc.GetBoundingBox();

        // Assert - Should include points at 90° and 180°
        Assert.Equal(-2, box.Min.X, Constants.Tolerance);  // From 180°
        Assert.True(box.Min.Y <= -Math.Sqrt(2) + Constants.Tolerance);  // From 225°
        Assert.True(box.Max.X >= Math.Sqrt(2) - Constants.Tolerance);   // From 45°
        Assert.Equal(2, box.Max.Y, Constants.Tolerance);   // From 90°
    }

    [Fact]
    public void Transform_Translation_MovesArcCorrectly()
    {
        // Arrange
        var arc = new Arc2D(new Point2D(0, 0), 1, 0, Math.PI / 2);
        var translation = Matrix2D.Translation(2, 3);

        // Act
        var transformed = (Arc2D)arc.Transform(translation);

        // Assert
        Assert.True(transformed.Center.IsEqualTo(new Point2D(2, 3), Constants.Tolerance));
        Assert.Equal(arc.Radius, transformed.Radius, Constants.Tolerance);
    }

    [Fact]
    public void Transform_Rotation_RotatesArcCorrectly()
    {
        // Arrange
        var arc = new Arc2D(new Point2D(1, 0), 1, 0, Math.PI / 2);
        var rotation = Matrix2D.Rotation(Math.PI / 2);  // 90° rotation

        // Act
        var transformed = (Arc2D)arc.Transform(rotation);

        // Assert
        Assert.True(transformed.Center.IsEqualTo(new Point2D(0, 1), Constants.Tolerance));
        Assert.Equal(arc.Radius, transformed.Radius, Constants.Tolerance);
    }

    [Fact]
    public void Reverse_ReversesArcDirection()
    {
        // Arrange
        var arc = new Arc2D(new Point2D(0, 0), 5, 0, Math.PI / 2, true);

        // Act
        var reversed = arc.Reverse();

        // Assert
        Assert.Equal(arc.Center, reversed.Center);
        Assert.Equal(arc.Radius, reversed.Radius);
        Assert.Equal(arc.StartAngle, reversed.EndAngle, Constants.Tolerance);
        Assert.Equal(arc.EndAngle, reversed.StartAngle, Constants.Tolerance);
        Assert.False(reversed.IsCounterClockwise);
    }

    [Fact]
    public void IsEqualTo_SameArcs_ReturnsTrue()
    {
        // Arrange
        var arc1 = new Arc2D(new Point2D(1, 2), 3, 0, Math.PI / 2);
        var arc2 = new Arc2D(new Point2D(1, 2), 3, 0, Math.PI / 2);

        // Act & Assert
        Assert.True(arc1.IsEqualTo(arc2));
        Assert.True(arc1.Equals(arc2));
        Assert.True(arc1 == arc2);
    }

    [Fact]
    public void IsEqualTo_DifferentArcs_ReturnsFalse()
    {
        // Arrange
        var arc1 = new Arc2D(new Point2D(0, 0), 1, 0, Math.PI / 2);
        var arc2 = new Arc2D(new Point2D(0, 0), 2, 0, Math.PI / 2);  // Different radius

        // Act & Assert
        Assert.False(arc1.IsEqualTo(arc2));
        Assert.False(arc1.Equals(arc2));
        Assert.True(arc1 != arc2);
    }

    [Fact]
    public void GetHashCode_EqualArcs_ReturnsSameHashCode()
    {
        // Arrange
        var arc1 = new Arc2D(new Point2D(1, 2), 3, 0, Math.PI);
        var arc2 = new Arc2D(new Point2D(1, 2), 3, 0, Math.PI);

        // Act & Assert
        Assert.Equal(arc1.GetHashCode(), arc2.GetHashCode());
    }

    [Fact]
    public void ToString_ReturnsReadableString()
    {
        // Arrange
        var arc = new Arc2D(new Point2D(1, 2), 3, 0, Math.PI / 2);

        // Act
        var str = arc.ToString();

        // Assert
        Assert.Contains("Arc2D", str);
        Assert.Contains("Center", str);
        Assert.Contains("Radius", str);
        Assert.Contains("StartAngle", str);
        Assert.Contains("EndAngle", str);
        Assert.Contains("CCW", str);
    }

    [Fact]
    public void AngleNormalization_HandlesNegativeAngles()
    {
        // Arrange
        var arc = new Arc2D(new Point2D(0, 0), 1, -Math.PI / 2, Math.PI / 2);

        // Act & Assert
        Assert.Equal(3 * Math.PI / 2, arc.StartAngle, Constants.Tolerance);  // -π/2 normalized to 3π/2
        Assert.Equal(Math.PI / 2, arc.EndAngle, Constants.Tolerance);
    }

    [Fact]
    public void AngleNormalization_HandlesLargeAngles()
    {
        // Arrange
        var arc = new Arc2D(new Point2D(0, 0), 1, 5 * Math.PI, 6 * Math.PI);

        // Act & Assert
        Assert.Equal(Math.PI, arc.StartAngle, Constants.Tolerance);  // 5π normalized to π
        Assert.Equal(0, arc.EndAngle, Constants.Tolerance);          // 6π normalized to 0
    }

    [Fact]
    public void FullCircle_CounterClockwise_HasCorrectSweepAngle()
    {
        // Arrange - Full circle from 0 to 0 (or 2π)
        var arc = new Arc2D(new Point2D(0, 0), 1, 0, 2 * Math.PI, true);

        // Act & Assert
        Assert.Equal(2 * Math.PI, arc.SweepAngle, Constants.Tolerance);
    }

    [Fact]
    public void FullCircle_Clockwise_HasCorrectSweepAngle()
    {
        // Arrange - Full circle from 0 to 0 clockwise
        var arc = new Arc2D(new Point2D(0, 0), 1, 0, 2 * Math.PI, false);

        // Act & Assert
        Assert.Equal(-2 * Math.PI, arc.SweepAngle, Constants.Tolerance);
    }
}