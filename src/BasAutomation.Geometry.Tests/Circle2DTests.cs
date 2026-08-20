using BasAutomation.Geometry.Curves;

namespace BasAutomation.Geometry.Tests;

public class Circle2DTests
{
    private const double Tolerance = Constants.Tolerance;

    [Fact]
    public void Constructor_WithCenterAndRadius_CreatesCircle()
    {
        // Arrange
        var center = new Point2D(5, 10);
        var radius = 3.5;

        // Act
        var circle = new Circle2D(center, radius);

        // Assert
        Assert.Equal(center, circle.Center);
        Assert.Equal(radius, circle.Radius);
    }

    [Fact]
    public void Constructor_WithCoordinatesAndRadius_CreatesCircle()
    {
        // Act
        var circle = new Circle2D(5, 10, 3.5);

        // Assert
        Assert.Equal(5, circle.Center.X);
        Assert.Equal(10, circle.Center.Y);
        Assert.Equal(3.5, circle.Radius);
    }

    [Fact]
    public void Constructor_WithNegativeRadius_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Circle2D(new Point2D(0, 0), -1));
    }

    [Fact]
    public void FromThreePoints_ValidPoints_CreatesCircle()
    {
        // Arrange
        var p1 = new Point2D(0, 0);
        var p2 = new Point2D(4, 0);
        var p3 = new Point2D(2, 2);

        // Act
        var circle = Circle2D.FromThreePoints(p1, p2, p3);

        // Assert
        Assert.Equal(2, circle.Center.X, Tolerance);
        Assert.Equal(0, circle.Center.Y, Tolerance);
        Assert.Equal(2, circle.Radius, Tolerance);
    }

    [Fact]
    public void FromThreePoints_CollinearPoints_ThrowsException()
    {
        // Arrange
        var p1 = new Point2D(0, 0);
        var p2 = new Point2D(1, 0);
        var p3 = new Point2D(2, 0);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Circle2D.FromThreePoints(p1, p2, p3));
    }

    [Fact]
    public void FromDiameter_ValidPoints_CreatesCircle()
    {
        // Arrange
        var p1 = new Point2D(0, 0);
        var p2 = new Point2D(10, 0);

        // Act
        var circle = Circle2D.FromDiameter(p1, p2);

        // Assert
        Assert.Equal(5, circle.Center.X, Tolerance);
        Assert.Equal(0, circle.Center.Y, Tolerance);
        Assert.Equal(5, circle.Radius, Tolerance);
    }

    [Fact]
    public void Properties_CalculateCorrectly()
    {
        // Arrange
        var circle = new Circle2D(new Point2D(0, 0), 5);

        // Act & Assert
        Assert.Equal(10, circle.Diameter, Tolerance);
        Assert.Equal(2 * Math.PI * 5, circle.Circumference, Tolerance);
        Assert.Equal(Math.PI * 25, circle.Area, Tolerance);
    }

    [Fact]
    public void BoundingBox_CalculatesCorrectly()
    {
        // Arrange
        var circle = new Circle2D(new Point2D(5, 10), 3);

        // Act
        var bbox = circle.BoundingBox;

        // Assert
        Assert.Equal(2, bbox.Min.X, Tolerance);
        Assert.Equal(7, bbox.Min.Y, Tolerance);
        Assert.Equal(8, bbox.Max.X, Tolerance);
        Assert.Equal(13, bbox.Max.Y, Tolerance);
    }

    [Fact]
    public void PointAt_ReturnsCorrectPoints()
    {
        // Arrange
        var circle = new Circle2D(new Point2D(0, 0), 10);

        // Act & Assert
        var p0 = circle.PointAt(0);
        Assert.Equal(10, p0.X, Tolerance);
        Assert.Equal(0, p0.Y, Tolerance);

        var p025 = circle.PointAt(0.25);
        Assert.Equal(0, p025.X, Tolerance);
        Assert.Equal(10, p025.Y, Tolerance);

        var p05 = circle.PointAt(0.5);
        Assert.Equal(-10, p05.X, Tolerance);
        Assert.Equal(0, p05.Y, Tolerance);

        var p075 = circle.PointAt(0.75);
        Assert.Equal(0, p075.X, Tolerance);
        Assert.Equal(-10, p075.Y, Tolerance);
    }

    [Fact]
    public void TangentAt_ReturnsCorrectVectors()
    {
        // Arrange
        var circle = new Circle2D(new Point2D(0, 0), 5);

        // Act & Assert
        var t0 = circle.TangentAt(0);
        Assert.Equal(0, t0.X, Tolerance);
        Assert.Equal(1, t0.Y, Tolerance);

        var t025 = circle.TangentAt(0.25);
        Assert.Equal(-1, t025.X, Tolerance);
        Assert.Equal(0, t025.Y, Tolerance);
    }

    [Fact]
    public void ContainsPoint_PointOnCircle_ReturnsTrue()
    {
        // Arrange
        var circle = new Circle2D(new Point2D(0, 0), 5);
        var point = new Point2D(5, 0);

        // Act & Assert
        Assert.True(circle.ContainsPoint(point));
    }

    [Fact]
    public void ContainsPoint_PointNotOnCircle_ReturnsFalse()
    {
        // Arrange
        var circle = new Circle2D(new Point2D(0, 0), 5);
        var point = new Point2D(3, 3);

        // Act & Assert
        Assert.False(circle.ContainsPoint(point));
    }

    [Fact]
    public void IsPointInside_PointInside_ReturnsTrue()
    {
        // Arrange
        var circle = new Circle2D(new Point2D(0, 0), 5);
        var point = new Point2D(2, 2);

        // Act & Assert
        Assert.True(circle.IsPointInside(point));
    }

    [Fact]
    public void IsPointInside_PointOutside_ReturnsFalse()
    {
        // Arrange
        var circle = new Circle2D(new Point2D(0, 0), 5);
        var point = new Point2D(4, 4);

        // Act & Assert
        Assert.False(circle.IsPointInside(point));
    }

    [Fact]
    public void Transform_Translation_MovesCircle()
    {
        // Arrange
        var circle = new Circle2D(new Point2D(0, 0), 5);
        var matrix = Matrix2D.Translation(10, 20);

        // Act
        var transformed = (Circle2D)circle.Transform(matrix);

        // Assert
        Assert.Equal(10, transformed.Center.X, Tolerance);
        Assert.Equal(20, transformed.Center.Y, Tolerance);
        Assert.Equal(5, transformed.Radius, Tolerance);
    }

    [Fact]
    public void Transform_UniformScale_ScalesCircle()
    {
        // Arrange
        var circle = new Circle2D(new Point2D(5, 5), 3);
        var matrix = Matrix2D.Scale(2, 2);

        // Act
        var transformed = (Circle2D)circle.Transform(matrix);

        // Assert
        Assert.Equal(10, transformed.Center.X, Tolerance);
        Assert.Equal(10, transformed.Center.Y, Tolerance);
        Assert.Equal(6, transformed.Radius, Tolerance);
    }

    [Fact]
    public void IntersectCircle_NonIntersectingCircles_ReturnsZero()
    {
        // Arrange
        var circle1 = new Circle2D(new Point2D(0, 0), 2);
        var circle2 = new Circle2D(new Point2D(10, 0), 2);

        // Act
        var count = circle1.IntersectCircle(circle2, out _, out _);

        // Assert
        Assert.Equal(0, count);
    }

    [Fact]
    public void IntersectCircle_TangentCircles_ReturnsOne()
    {
        // Arrange
        var circle1 = new Circle2D(new Point2D(0, 0), 3);
        var circle2 = new Circle2D(new Point2D(5, 0), 2);

        // Act
        var count = circle1.IntersectCircle(circle2, out var p1, out _);

        // Assert
        Assert.Equal(1, count);
        Assert.Equal(3, p1.X, Tolerance);
        Assert.Equal(0, p1.Y, Tolerance);
    }

    [Fact]
    public void IntersectCircle_IntersectingCircles_ReturnsTwo()
    {
        // Arrange
        var circle1 = new Circle2D(new Point2D(0, 0), 5);
        var circle2 = new Circle2D(new Point2D(8, 0), 5);

        // Act
        var count = circle1.IntersectCircle(circle2, out var p1, out var p2);

        // Assert
        Assert.Equal(2, count);
        Assert.Equal(4, p1.X, Tolerance);
        Assert.Equal(3, Math.Abs(p1.Y), Tolerance);
        Assert.Equal(4, p2.X, Tolerance);
        Assert.Equal(3, Math.Abs(p2.Y), Tolerance);
    }

    [Fact]
    public void IntersectLine_NoIntersection_ReturnsZero()
    {
        // Arrange
        var circle = new Circle2D(new Point2D(0, 0), 2);
        var line = new Line2D(new Point2D(5, -1), new Point2D(5, 1));

        // Act
        var count = circle.IntersectLine(line, out _, out _);

        // Assert
        Assert.Equal(0, count);
    }

    [Fact]
    public void IntersectLine_TangentLine_ReturnsOne()
    {
        // Arrange
        var circle = new Circle2D(new Point2D(0, 0), 5);
        var line = new Line2D(new Point2D(5, -10), new Point2D(5, 10));

        // Act
        var count = circle.IntersectLine(line, out var p1, out _);

        // Assert
        Assert.Equal(1, count);
        Assert.Equal(5, p1.X, Tolerance);
        Assert.Equal(0, p1.Y, Tolerance);
    }

    [Fact]
    public void IntersectLine_IntersectingLine_ReturnsTwo()
    {
        // Arrange
        var circle = new Circle2D(new Point2D(0, 0), 5);
        var line = new Line2D(new Point2D(-10, 0), new Point2D(10, 0));

        // Act
        var count = circle.IntersectLine(line, out var p1, out var p2);

        // Assert
        Assert.Equal(2, count);
        Assert.True((Math.Abs(p1.X - 5) < Tolerance && Math.Abs(p2.X + 5) < Tolerance) ||
                    (Math.Abs(p1.X + 5) < Tolerance && Math.Abs(p2.X - 5) < Tolerance));
    }

    [Fact]
    public void IsEqualTo_EqualCircles_ReturnsTrue()
    {
        // Arrange
        var circle1 = new Circle2D(new Point2D(5, 10), 3);
        var circle2 = new Circle2D(new Point2D(5, 10), 3);

        // Act & Assert
        Assert.True(circle1.IsEqualTo(circle2));
        Assert.True(circle1 == circle2);
        Assert.False(circle1 != circle2);
    }

    [Fact]
    public void IsEqualTo_DifferentCircles_ReturnsFalse()
    {
        // Arrange
        var circle1 = new Circle2D(new Point2D(5, 10), 3);
        var circle2 = new Circle2D(new Point2D(5, 10), 4);

        // Act & Assert
        Assert.False(circle1.IsEqualTo(circle2));
        Assert.False(circle1 == circle2);
        Assert.True(circle1 != circle2);
    }

    [Fact]
    public void ToArc_CreatesFullCircleArc()
    {
        // Arrange
        var circle = new Circle2D(new Point2D(5, 10), 3);

        // Act
        var arc = circle.ToArc();

        // Assert
        Assert.Equal(circle.Center, arc.Center);
        Assert.Equal(circle.Radius, arc.Radius);
        Assert.Equal(0, arc.StartAngle, Tolerance);
        // Since Arc2D normalizes angles, 2π becomes 0, but the sweep angle should still be 2π
        Assert.Equal(2 * Math.PI, Math.Abs(arc.SweepAngle), Tolerance);
        Assert.True(arc.IsCounterClockwise);
        // The arc should represent a full circle
        Assert.Equal(circle.Circumference, arc.Length, Tolerance);
    }

    [Fact]
    public void ToString_ReturnsExpectedFormat()
    {
        // Arrange
        var circle = new Circle2D(new Point2D(1, 2), 3);

        // Act
        var str = circle.ToString();

        // Assert
        Assert.Equal("Circle2D(Center: (1, 2), Radius: 3)", str);
    }
}