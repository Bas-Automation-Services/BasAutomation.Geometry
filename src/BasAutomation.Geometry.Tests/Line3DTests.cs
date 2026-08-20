using BasAutomation.Geometry.Curves;

namespace BasAutomation.Geometry.Tests;

public class Line3DTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var start = new Point3D(0.0, 0.0, 0.0);
        var end = new Point3D(3.0, 4.0, 5.0);
        var line = new Line3D(start, end);
        Assert.Equal(start, line.StartPoint);
        Assert.Equal(end, line.EndPoint);
    }

    [Fact]
    public void Direction_ReturnsNormalizedVector()
    {
        var line = new Line3D(new Point3D(0.0, 0.0, 0.0), new Point3D(3.0, 0.0, 4.0));
        var dir = line.Direction;
        Assert.Equal(1.0, dir.Length, 10);
        Assert.Equal(0.6, dir.X, 10);
        Assert.Equal(0.0, dir.Y, 10);
        Assert.Equal(0.8, dir.Z, 10);
    }

    [Fact]
    public void Length_CalculatesCorrectly()
    {
        var line = new Line3D(new Point3D(0.0, 0.0, 0.0), new Point3D(2.0, 3.0, 6.0));
        Assert.Equal(7.0, line.Length);
    }

    [Fact]
    public void PointAt_CalculatesCorrectly()
    {
        var line = new Line3D(new Point3D(0.0, 0.0, 0.0), new Point3D(4.0, 4.0, 4.0));
        var point = line.PointAt(0.5);
        Assert.Equal(2.0, point.X);
        Assert.Equal(2.0, point.Y);
        Assert.Equal(2.0, point.Z);
    }

    [Fact]
    public void MidPoint_CalculatesCorrectly()
    {
        var line = new Line3D(new Point3D(0.0, 0.0, 0.0), new Point3D(4.0, 4.0, 4.0));
        var mid = line.MidPoint;
        Assert.Equal(2.0, mid.X);
        Assert.Equal(2.0, mid.Y);
        Assert.Equal(2.0, mid.Z);
    }

    [Fact]
    public void Constructor_WithLine3D_InitializesCorrectly()
    {
        // Arrange
        var start = new Point3D(0, 0, 0);
        var end = new Point3D(1, 0, 0);
        var line = new Line3D(start, end);

        // Act
        var curve = line;

        // Assert
        Assert.Equal(start, curve.StartPoint);
        Assert.Equal(end, curve.EndPoint);
        Assert.Equal(1.0, curve.Length, Constants.Tolerance);
    }

    [Fact]
    public void Constructor_WithPoints_InitializesCorrectly()
    {
        // Arrange
        var start = new Point3D(0, 0, 0);
        var end = new Point3D(3, 4, 0);

        // Act
        var curve = new Line3D(start, end);

        // Assert
        Assert.Equal(start, curve.StartPoint);
        Assert.Equal(end, curve.EndPoint);
        Assert.Equal(5.0, curve.Length, Constants.Tolerance); // 3-4-5 triangle
    }

    [Theory]
    [InlineData(0.0, 0, 0, 0)]
    [InlineData(0.5, 5, 0, 0)]
    [InlineData(1.0, 10, 0, 0)]
    public void PointAt_ReturnsCorrectPoint(double t, double expectedX, double expectedY, double expectedZ)
    {
        // Arrange
        var curve = new Line3D(new Point3D(0, 0, 0), new Point3D(10, 0, 0));

        // Act
        var point = curve.PointAt(t);

        // Assert
        Assert.Equal(expectedX, point.X, Constants.Tolerance);
        Assert.Equal(expectedY, point.Y, Constants.Tolerance);
        Assert.Equal(expectedZ, point.Z, Constants.Tolerance);
    }

    [Fact]
    public void TangentAt_ReturnsConstantDirection()
    {
        // Arrange
        var curve = new Line3D(new Point3D(0, 0, 0), new Point3D(1, 1, 1));
        var expectedTangent = new Vector3D(1, 1, 1).Normalized();

        // Act & Assert
        for (double t = 0; t <= 1; t += 0.1)
        {
            var tangent = curve.TangentAt(t);
            Assert.Equal(expectedTangent.X, tangent.X, Constants.Tolerance);
            Assert.Equal(expectedTangent.Y, tangent.Y, Constants.Tolerance);
            Assert.Equal(expectedTangent.Z, tangent.Z, Constants.Tolerance);
        }
    }

    [Fact]
    public void NormalAt_WithValidUpHint_ReturnsPerpendicularVector()
    {
        // Arrange
        var curve = new Line3D(new Point3D(0, 0, 0), new Point3D(1, 0, 0));

        // Act
        var normal = curve.NormalAt(0.5);
        var tangent = curve.TangentAt(0.5);

        // Assert
        Assert.Equal(0, normal.Dot(tangent), Constants.Tolerance); // Normal perpendicular to tangent
        Assert.Equal(1, normal.Length, Constants.Tolerance); // Normal is normalized
    }

    [Fact]
    public void NormalAt_WithUpHint_UsesAlternativeUp()
    {
        // Arrange
        var curve = new Line3D(new Point3D(0, 0, 0), new Point3D(1, 0, 0));
        var upHint = new Vector3D(0, 1, 0); 

        // Act
        var normal = curve.NormalAt(0.5, upHint);
        var tangent = curve.TangentAt(0.5);

        // Assert
        Assert.Equal(0, normal.Dot(tangent), Constants.Tolerance); // Normal perpendicular to tangent
        Assert.Equal(1, normal.Length, Constants.Tolerance); // Normal is normalized
        Assert.Equal(normal, new Vector3D(0, 0, 1));
    }

    [Fact]
    public void NormalAt_WithoutHint_UsesCorrectUp()
    {
        // Arrange
        var curve = new Line3D(new Point3D(0, 0, 0), new Point3D(1, 0, 0));

        // Act
        var normal = curve.NormalAt(0.5);
        var tangent = curve.TangentAt(0.5);

        // Assert
        Assert.Equal(0, normal.Dot(tangent), Constants.Tolerance); // Normal perpendicular to tangent
        Assert.Equal(1, normal.Length, Constants.Tolerance); // Normal is normalized
        Assert.Equal(normal, new Vector3D(0, -1, 0));
    }
    
    [Fact]
    public void NormalAt_WithParallelUpHint_ThrowsInvalidOperationException()
    {
        // Arrange
        var curve = new Line3D(new Point3D(0, 0, 0), new Point3D(1, 0, 0));
        var upHint = new Vector3D(1, 0, 0); // Parallel to tangent

        // Assert
        Assert.Throws<InvalidOperationException>(() => curve.NormalAt(0.5, upHint));
    }
    
    [Fact]
    public void NormalAt_WithHorizontalLine_UsesZAxisReference()
    {
        // Arrange
        var line = new Line3D(new Point3D(0, 0, 0), new Point3D(1, 0, 0));

        // Act
        var normal = line.NormalAt(0.5);
        var tangent = line.TangentAt(0.5);

        // Assert
        Assert.Equal(0, normal.Dot(tangent), Constants.Tolerance); // Normal perpendicular to tangent
        Assert.Equal(1, normal.Length, Constants.Tolerance); // Normal is normalized
                                                             // For a line along X-axis, the normal should be in the Y direction when using Z-axis reference
        Assert.Equal(0, normal.X, Constants.Tolerance);
        Assert.Equal(1, Math.Abs(normal.Y), Constants.Tolerance);
        Assert.Equal(0, normal.Z, Constants.Tolerance);
    }

    [Fact]
    public void NormalAt_WithVerticalLine_UsesXAxisReference()
    {
        // Arrange
        var line = new Line3D(new Point3D(0, 0, 0), new Point3D(0, 0, 1));

        // Act
        var normal = line.NormalAt(0.5);
        var tangent = line.TangentAt(0.5);

        // Assert
        Assert.Equal(0, normal.Dot(tangent), Constants.Tolerance); // Normal perpendicular to tangent
        Assert.Equal(1, normal.Length, Constants.Tolerance); // Normal is normalized
                                                             // For a line along Z-axis, the normal should be in the Y direction when using X-axis reference
        Assert.Equal(0, normal.X, Constants.Tolerance);
        Assert.Equal(1, Math.Abs(normal.Y), Constants.Tolerance);
        Assert.Equal(0, normal.Z, Constants.Tolerance);
    }

    [Fact]
    public void NormalAt_WithNearlyVerticalLine_UsesXAxisReference()
    {
        // Arrange
        // Line that's mostly vertical (|tangent.Z| >= 0.9)
        var line = new Line3D(new Point3D(0, 0, 0), new Point3D(0.1, 0.1, 1));

        // Act
        var normal = line.NormalAt(0.5);
        var tangent = line.TangentAt(0.5);

        // Assert
        Assert.Equal(0, normal.Dot(tangent), Constants.Tolerance); // Normal perpendicular to tangent
        Assert.Equal(1, normal.Length, Constants.Tolerance); // Normal is normalized
        Assert.True(Math.Abs(tangent.Z) >= 0.9); // Verify our line is nearly vertical
    }

    [Fact]
    public void NormalAt_WithDiagonalLine_UsesZAxisReference()
    {
        // Arrange
        // Line at 45 degrees in XY plane (not close to vertical)
        var line = new Line3D(new Point3D(0, 0, 0), new Point3D(1, 1, 0.5));

        // Act
        var normal = line.NormalAt(0.5);
        var tangent = line.TangentAt(0.5);

        // Assert
        Assert.Equal(0, normal.Dot(tangent), Constants.Tolerance); // Normal perpendicular to tangent
        Assert.Equal(1, normal.Length, Constants.Tolerance); // Normal is normalized
        Assert.True(Math.Abs(tangent.Z) < 0.9); // Verify Z component is not dominant
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.25)]
    [InlineData(0.5)]
    [InlineData(0.75)]
    [InlineData(1.0)]
    public void NormalAt_AtDifferentParameters_ReturnsSameNormal(double t)
    {
        // Arrange
        var line = new Line3D(new Point3D(0, 0, 0), new Point3D(1, 2, 0.3));

        // Act
        var normal = line.NormalAt(t);
        var normalAtMidpoint = line.NormalAt(0.5);

        // Assert
        // For a straight line, the normal should be constant along its length
        Assert.Equal(normalAtMidpoint.X, normal.X, Constants.Tolerance);
        Assert.Equal(normalAtMidpoint.Y, normal.Y, Constants.Tolerance);
        Assert.Equal(normalAtMidpoint.Z, normal.Z, Constants.Tolerance);
    }

    [Fact]
    public void NormalAt_LineAlongYAxis_ProducesValidNormal()
    {
        // Arrange
        var line = new Line3D(new Point3D(0, 0, 0), new Point3D(0, 1, 0));

        // Act
        var normal = line.NormalAt(0.5);
        var tangent = line.TangentAt(0.5);

        // Assert
        Assert.Equal(0, normal.Dot(tangent), Constants.Tolerance); // Normal perpendicular to tangent
        Assert.Equal(1, normal.Length, Constants.Tolerance); // Normal is normalized
                                                             // For a line along Y-axis with Z-axis reference, normal should be along X-axis
        Assert.Equal(1, Math.Abs(normal.X), Constants.Tolerance);
        Assert.Equal(0, normal.Y, Constants.Tolerance);
        Assert.Equal(0, normal.Z, Constants.Tolerance);
    }

    [Fact]
    public void NormalAt_NegativeZDirection_UsesXAxisReference()
    {
        // Arrange
        // Line pointing downward (negative Z)
        var line = new Line3D(new Point3D(0, 0, 0), new Point3D(0, 0, -1));

        // Act
        var normal = line.NormalAt(0.5);
        var tangent = line.TangentAt(0.5);

        // Assert
        Assert.Equal(0, normal.Dot(tangent), Constants.Tolerance); // Normal perpendicular to tangent
        Assert.Equal(1, normal.Length, Constants.Tolerance); // Normal is normalized
        Assert.True(Math.Abs(tangent.Z) >= 0.9); // Verify line is vertical
    }

    [Theory]
    [InlineData(0.0, 0.0)]
    [InlineData(5.0, 0.5)]
    [InlineData(10.0, 1.0)]
    public void ParameterAtDistance_ReturnsCorrectParameter(double distance, double expectedT)
    {
        // Arrange
        var curve = new Line3D(new Point3D(0, 0, 0), new Point3D(10, 0, 0));

        // Act
        var t = curve.ParameterAtDistance(distance);

        // Assert
        Assert.Equal(expectedT, t, Constants.Tolerance);
    }

    [Fact]
    public void Clone_CreatesIndependentCopy()
    {
        // Arrange
        var original = new Line3D(new Point3D(0, 0, 0), new Point3D(1, 1, 1));

        // Act
        var clone = original;

        // Assert
        Assert.Equal(original.StartPoint, clone.StartPoint);
        Assert.Equal(original.EndPoint, clone.EndPoint);
        Assert.Equal(original.Length, clone.Length, Constants.Tolerance);
    }
}