using BasAutomation.Geometry.Curves;

namespace BasAutomation.Geometry.Tests;

public class Arc3DTests
{
    private const double Tolerance = Constants.Tolerance;

    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Arrange
        var center = new Point3D(0, 0, 0);
        var normal = new Vector3D(0, 0, 1);
        var xAxis = new Vector3D(1, 0, 0);
        var radius = 5.0;
        var startAngle = 0.0;
        var endAngle = Math.PI / 2; // 90 degrees

        // Act
        var arc = new Arc3D(center, normal, xAxis, radius, startAngle, endAngle);

        // Assert
        Assert.Equal(new Point3D(5, 0, 0), arc.StartPoint);
        Assert.Equal(new Point3D(0, 5, 0), arc.EndPoint);
        Assert.Equal(radius * Math.PI / 2, arc.Length, Tolerance);
    }

    [Theory]
    [InlineData(0.0, 1, 0, 0)]  // Start point
    [InlineData(0.5, 0.707107, 0.707107, 0)] // 45 degrees
    [InlineData(1.0, 0, 1, 0)]  // End point (90 degrees)
    public void PointAt_ReturnsCorrectPoint(double t, double expectedX, double expectedY, double expectedZ)
    {
        // Arrange
        var arc = new Arc3D(
            new Point3D(0, 0, 0),
            new Vector3D(0, 0, 1),
            new Vector3D(1, 0, 0),
            1.0,
            0,
            Math.PI / 2,
            true);

        // Act
        var point = arc.PointAt(t);

        // Assert
        Assert.Equal(expectedX, point.X, Constants.Tolerance);
        Assert.Equal(expectedY, point.Y, Constants.Tolerance);
        Assert.Equal(expectedZ, point.Z, Constants.Tolerance);
    }

    [Fact]
    public void PointAt_ClockwiseArc_ReturnsCorrectPoint()
    {
        // CW arc from start=0 to end=π/2: going clockwise from 0° to 90° is 270° CW
        // (the long way around, consistent with Arc2D.SweepAngle)
        var arc = new Arc3D(
            new Point3D(0, 0, 0),
            new Vector3D(0, 0, 1),
            new Vector3D(1, 0, 0),
            1.0,
            0,
            Math.PI / 2,
            false); // Clockwise

        // The midpoint of the 270° CW arc is at -135° = (−√2/2, −√2/2)
        var midPoint = arc.PointAt(0.5);
        Assert.Equal(-0.707107, midPoint.X, Constants.Tolerance);
        Assert.Equal(-0.707107, midPoint.Y, Constants.Tolerance);

        // Verify the endpoint is at π/2 = (0, 1, 0), NOT at -90° = (0, -1, 0)
        var endPoint = arc.PointAt(1);
        Assert.Equal(0, endPoint.X, Constants.Tolerance);
        Assert.Equal(1, endPoint.Y, Constants.Tolerance);
    }

    [Fact]
    public void TangentAt_Start_PointsInCorrectDirection()
    {
        // Arrange
        var arc = new Arc3D(
            new Point3D(0, 0, 0),
            new Vector3D(0, 0, 1),
            new Vector3D(1, 0, 0),
            1.0,
            0,
            Math.PI / 2,
            true);

        // Act
        var tangent = arc.TangentAt(0);

        // Assert
        Assert.Equal(0, tangent.X, Tolerance);
        Assert.Equal(1, tangent.Y, Tolerance);
        Assert.Equal(0, tangent.Z, Tolerance);
    }

    [Fact]
    public void TangentAt_ClockwiseArc_PointsInOppositeDirection()
    {
        // Arrange
        var arc = new Arc3D(
            new Point3D(0, 0, 0),
            new Vector3D(0, 0, 1),
            new Vector3D(1, 0, 0),
            1.0,
            0,
            Math.PI / 2,
            false); // Clockwise

        // Act
        var tangent = arc.TangentAt(0);

        // Assert
        Assert.Equal(0, tangent.X, Tolerance);
        Assert.Equal(-1, tangent.Y, Tolerance); // Opposite direction
        Assert.Equal(0, tangent.Z, Tolerance);
    }

    [Fact]
    public void NormalAt_PointsTowardCenter()
    {
        // Arrange
        var arc = new Arc3D(
            new Point3D(0, 0, 0),
            new Vector3D(0, 0, 1),
            new Vector3D(1, 0, 0),
            1.0,
            0,
            Math.PI / 2,
            true);

        // Act
        var normal = arc.NormalAt(0);

        // Assert
        // At start point (1,0,0), normal should point toward center (-1,0,0)
        Assert.Equal(-1, normal.X, Tolerance);
        Assert.Equal(0, normal.Y, Tolerance);
        Assert.Equal(0, normal.Z, Tolerance);
    }

    [Fact]
    public void Length_FullCircle_Returns2PiR()
    {
        // Arrange
        var radius = 5.0;
        var arc = new Arc3D(
            new Point3D(0, 0, 0),
            new Vector3D(0, 0, 1),
            new Vector3D(1, 0, 0),
            radius,
            0,
            2 * Math.PI,
            true);

        // Act
        var length = arc.Length;

        // Assert
        Assert.Equal(2 * Math.PI * radius, length, Tolerance);
    }

    [Fact]
    public void Length_QuarterCircle_ReturnsPiR2()
    {
        // Arrange
        var radius = 4.0;
        var arc = new Arc3D(
            new Point3D(0, 0, 0),
            new Vector3D(0, 0, 1),
            new Vector3D(1, 0, 0),
            radius,
            0,
            Math.PI / 2,
            true);

        // Act
        var length = arc.Length;

        // Assert
        Assert.Equal(Math.PI * radius / 2, length, Tolerance);
    }

    [Fact]
    public void Length_NegativeSweep_HandledCorrectly()
    {
        // Arrange
        var radius = 1.0;
        var arc = new Arc3D(
            new Point3D(0, 0, 0),
            new Vector3D(0, 0, 1),
            new Vector3D(1, 0, 0),
            radius,
            Math.PI / 2,
            0, // End before start
            true);

        // Act
        var length = arc.Length;

        // Assert
        Assert.Equal(3 * Math.PI / 2, length, Tolerance); // 270 degrees
    }

    [Theory]
    [InlineData(0.0, 0.0)]
    [InlineData(Math.PI / 4, 0.25)]  // Quarter of semicircle
    [InlineData(Math.PI / 2, 0.5)]  // Half semicircle
    [InlineData(Math.PI, 1)]  // Full semicircle
    public void ParameterAtDistance_ReturnsCorrectParameter(double distance, double expectedT)
    {
        // Arrange
        var arc = new Arc3D(
            new Point3D(0, 0, 0),
            new Vector3D(0, 0, 1),
            new Vector3D(1, 0, 0),
            1.0,
            0,
            Math.PI,
            true);

        // Act
        var t = arc.ParameterAtDistance(distance);

        // Assert
        Assert.Equal(expectedT, t, Tolerance);
    }

    [Fact]
    public void Clone_CreatesIndependentCopy()
    {
        // Arrange
        var original = new Arc3D(
            new Point3D(1, 2, 3),
            new Vector3D(0, 0, 1),
            new Vector3D(1, 0, 0),
            5.0,
            0,
            Math.PI,
            true);

        // Act
        var clone = (Arc3D)original.Clone();

        // Assert
        Assert.Equal(original.StartPoint, clone.StartPoint);
        Assert.Equal(original.EndPoint, clone.EndPoint);
        Assert.Equal(original.Length, clone.Length, Tolerance);
    }

    [Fact]
    public void ArcCurve3D_3DOrientation_WorksCorrectly()
    {
        // Arrange - Arc in YZ plane
        var arc = new Arc3D(
            new Point3D(0, 0, 0),
            new Vector3D(1, 0, 0), // Normal along X
            new Vector3D(0, 1, 0), // xAxis along Y
            2.0,
            0,
            Math.PI / 2,
            true);

        // Act
        var start = arc.StartPoint;
        var end = arc.EndPoint;

        // Assert
        Assert.Equal(0, start.X, Tolerance);
        Assert.Equal(2, start.Y, Tolerance);
        Assert.Equal(0, start.Z, Tolerance);

        Assert.Equal(0, end.X, Tolerance);
        Assert.Equal(0, end.Y, Tolerance);
        Assert.Equal(2, end.Z, Tolerance);
    }

    [Fact]
    public void CreateConcentricArc_SameStartPoint_ReturnsSameArc()
    {
        // Arrange - Arc in XY plane, center at origin, radius 10
        var arc = new Arc3D(
            center: new Point3D(0, 0, 0),
            normal: new Vector3D(0, 0, 1),
            xAxis: new Vector3D(1, 0, 0),
            radius: 10.0,
            startAngle: 0,
            endAngle: Math.PI / 2,
            isCounterClockwise: true);

        // Act - Create concentric arc with same start point
        var concentric = arc.CreateConcentricArc(arc.StartPoint);

        // Assert
        Assert.Equal(arc.StartPoint.X, concentric.StartPoint.X, Tolerance);
        Assert.Equal(arc.StartPoint.Y, concentric.StartPoint.Y, Tolerance);
        Assert.Equal(arc.StartPoint.Z, concentric.StartPoint.Z, Tolerance);
        Assert.Equal(arc.EndPoint.X, concentric.EndPoint.X, Tolerance);
        Assert.Equal(arc.EndPoint.Y, concentric.EndPoint.Y, Tolerance);
        Assert.Equal(arc.EndPoint.Z, concentric.EndPoint.Z, Tolerance);
        Assert.Equal(arc.Radius, concentric.Radius, Tolerance);
    }

    [Fact]
    public void CreateConcentricArc_LargerRadius_AdjustsRadiusKeepsSameCenter()
    {
        // Arrange - Arc in XY plane, center at origin, radius 10
        // XAxis points toward +X, so start is at (10, 0, 0)
        var arc = new Arc3D(
            center: new Point3D(0, 0, 0),
            normal: new Vector3D(0, 0, 1),
            xAxis: new Vector3D(1, 0, 0),
            radius: 10.0,
            startAngle: 0,
            endAngle: Math.PI / 2,
            isCounterClockwise: true);

        // New start point at (15, 0, 0) - larger radius in the XAxis direction
        var newStartPoint = new Point3D(15, 0, 0);

        // Act
        var concentric = arc.CreateConcentricArc(newStartPoint);

        // Assert
        // Center stays at origin
        Assert.Equal(0, concentric.Center.X, Tolerance);
        Assert.Equal(0, concentric.Center.Y, Tolerance);

        // New radius = distance from center to new start = 15
        Assert.Equal(15, concentric.Radius, Tolerance);

        // Start point should be at the new position
        Assert.Equal(15, concentric.StartPoint.X, Tolerance);
        Assert.Equal(0, concentric.StartPoint.Y, Tolerance);

        // End point should also be at radius 15
        Assert.Equal(0, concentric.EndPoint.X, Tolerance);
        Assert.Equal(15, concentric.EndPoint.Y, Tolerance);
    }

    [Fact]
    public void CreateConcentricArc_OffsetStartPoint_ComputesCorrectRadius()
    {
        // Arrange - Arc in XY plane, center at origin, radius 10
        // This simulates a curved beam where the profile point is offset from the centerline
        var arc = new Arc3D(
            center: new Point3D(0, 0, 0),
            normal: new Vector3D(0, 0, 1),
            xAxis: new Vector3D(1, 0, 0),
            radius: 10.0,
            startAngle: 0,
            endAngle: Math.PI / 2,
            isCounterClockwise: true);

        // New start point at (10, 5, 0) - same X but offset in Y
        // This is NOT on the XAxis line, so the radius will be sqrt(10^2 + 5^2) = sqrt(125)
        var newStartPoint = new Point3D(10, 5, 0);

        // Act
        var concentric = arc.CreateConcentricArc(newStartPoint);

        // Assert
        // Center stays at origin
        Assert.Equal(0, concentric.Center.X, Tolerance);
        Assert.Equal(0, concentric.Center.Y, Tolerance);

        // New radius = distance from center to new start = sqrt(125) ≈ 11.18
        var expectedRadius = Math.Sqrt(125);
        Assert.Equal(expectedRadius, concentric.Radius, Tolerance);

        // XAxis stays the same (1, 0, 0), so start point is at (radius, 0, 0)
        Assert.Equal(expectedRadius, concentric.StartPoint.X, Tolerance);
        Assert.Equal(0, concentric.StartPoint.Y, Tolerance);
    }

    [Fact]
    public void CreateConcentricArc_WithHeightOffset_RadiusUnchangedCenterTranslated()
    {
        // Arrange - Arc in XY plane, center at origin, radius 10
        // Translation only along Z (perpendicular to the arc plane) should not affect radius
        var arc = new Arc3D(
            center: new Point3D(0, 0, 0),
            normal: new Vector3D(0, 0, 1),
            xAxis: new Vector3D(1, 0, 0),
            radius: 10.0,
            startAngle: 0,
            endAngle: Math.PI / 2,
            isCounterClockwise: true);

        // New start point at (10, 0, 5) - same XY but offset in Z
        var newStartPoint = new Point3D(10, 0, 5);

        // Act
        var concentric = arc.CreateConcentricArc(newStartPoint);

        // Assert - The arc should have same radius 10 (Z offset doesn't affect radial distance)
        Assert.Equal(10.0, concentric.Radius, Tolerance);

        // Start point should be at the correct position
        Assert.Equal(10, concentric.StartPoint.X, Tolerance);
        Assert.Equal(0, concentric.StartPoint.Y, Tolerance);
        Assert.Equal(5, concentric.StartPoint.Z, Tolerance);

        // Midpoint should also be at Z=5
        Assert.Equal(5.0, concentric.MidPoint.Z, Tolerance);

        // Center should be translated up to Z=5
        Assert.Equal(0, concentric.Center.X, Tolerance);
        Assert.Equal(0, concentric.Center.Y, Tolerance);
        Assert.Equal(5, concentric.Center.Z, Tolerance);
    }

    [Fact]
    public void Offset_ZeroOffset_ReturnsSameArc()
    {
        // Arrange - quarter arc in XY plane, center at origin, radius 10
        var arc = new Arc3D(
            center: new Point3D(0, 0, 0),
            normal: new Vector3D(0, 0, 1),
            xAxis: new Vector3D(1, 0, 0),
            radius: 10.0,
            startAngle: 0,
            endAngle: Math.PI / 2,
            isCounterClockwise: true);

        // Act
        var offsetArc = arc.Offset(Vector2D.Zero);

        // Assert - arc should be unchanged
        Assert.Equal(arc.Radius, offsetArc.Radius, Tolerance);
        Assert.Equal(arc.StartPoint.X, offsetArc.StartPoint.X, Tolerance);
        Assert.Equal(arc.StartPoint.Y, offsetArc.StartPoint.Y, Tolerance);
        Assert.Equal(arc.StartPoint.Z, offsetArc.StartPoint.Z, Tolerance);
        Assert.Equal(arc.EndPoint.X, offsetArc.EndPoint.X, Tolerance);
        Assert.Equal(arc.EndPoint.Y, offsetArc.EndPoint.Y, Tolerance);
        Assert.Equal(arc.EndPoint.Z, offsetArc.EndPoint.Z, Tolerance);
    }

    [Fact]
    public void Offset_PositiveX_RadialInward_DecreasesRadius()
    {
        // Arrange - quarter arc in XY plane, center at origin, radius 10
        // Normal = (0,0,1); at start (10,0,0) the radial direction is (1,0,0)
        var arc = new Arc3D(
            center: new Point3D(0, 0, 0),
            normal: new Vector3D(0, 0, 1),
            xAxis: new Vector3D(1, 0, 0),
            radius: 10.0,
            startAngle: 0,
            endAngle: Math.PI / 2,
            isCounterClockwise: true);

        // Act - offset X=2 moves each point inward by 2 along its radial direction
        var offsetArc = arc.Offset(new Vector2D(2, 0));

        // Assert - radius decreases by 2 (inner offset)
        Assert.Equal(8.0, offsetArc.Radius, Tolerance);
        Assert.Equal(8.0, offsetArc.StartPoint.X, Tolerance);
        Assert.Equal(0.0, offsetArc.StartPoint.Y, Tolerance);
        Assert.Equal(0.0, offsetArc.EndPoint.X, Tolerance);
        Assert.Equal(8.0, offsetArc.EndPoint.Y, Tolerance);
        // Center stays at origin
        Assert.Equal(0.0, offsetArc.Center.X, Tolerance);
        Assert.Equal(0.0, offsetArc.Center.Y, Tolerance);
        Assert.Equal(0.0, offsetArc.Center.Z, Tolerance);
    }

    [Fact]
    public void Offset_NegativeX_RadialOutward_IncreasesRadius()
    {
        // Arrange - quarter arc in XY plane, center at origin, radius 10
        var arc = new Arc3D(
            center: new Point3D(0, 0, 0),
            normal: new Vector3D(0, 0, 1),
            xAxis: new Vector3D(1, 0, 0),
            radius: 10.0,
            startAngle: 0,
            endAngle: Math.PI / 2,
            isCounterClockwise: true);

        // Act - offset X=-2 moves each point outward by 2 along its radial direction
        var offsetArc = arc.Offset(new Vector2D(-2, 0));

        // Assert - radius increases by 2 (outer offset)
        Assert.Equal(12.0, offsetArc.Radius, Tolerance);
        Assert.Equal(12.0, offsetArc.StartPoint.X, Tolerance);
        Assert.Equal(0.0, offsetArc.StartPoint.Y, Tolerance);
        Assert.Equal(0.0, offsetArc.EndPoint.X, Tolerance);
        Assert.Equal(12.0, offsetArc.EndPoint.Y, Tolerance);
        // Center stays at origin
        Assert.Equal(0.0, offsetArc.Center.X, Tolerance);
        Assert.Equal(0.0, offsetArc.Center.Y, Tolerance);
        Assert.Equal(0.0, offsetArc.Center.Z, Tolerance);
    }

    [Fact]
    public void Offset_PositiveY_AlongNormal_TranslatesArcAlongNormal()
    {
        // Arrange - quarter arc in XY plane, center at origin, radius 10
        // Normal = (0,0,1)
        var arc = new Arc3D(
            center: new Point3D(0, 0, 0),
            normal: new Vector3D(0, 0, 1),
            xAxis: new Vector3D(1, 0, 0),
            radius: 10.0,
            startAngle: 0,
            endAngle: Math.PI / 2,
            isCounterClockwise: true);

        // Act - offset Y=3 translates all points by 3 along the arc Normal (Z axis)
        var offsetArc = arc.Offset(new Vector2D(0, 3));

        // Assert - radius unchanged, arc translated by 3 along Z
        Assert.Equal(10.0, offsetArc.Radius, Tolerance);
        Assert.Equal(10.0, offsetArc.StartPoint.X, Tolerance);
        Assert.Equal(0.0, offsetArc.StartPoint.Y, Tolerance);
        Assert.Equal(3.0, offsetArc.StartPoint.Z, Tolerance);
        Assert.Equal(0.0, offsetArc.EndPoint.X, Tolerance);
        Assert.Equal(10.0, offsetArc.EndPoint.Y, Tolerance);
        Assert.Equal(3.0, offsetArc.EndPoint.Z, Tolerance);
        // Center also translates along Z
        Assert.Equal(0.0, offsetArc.Center.X, Tolerance);
        Assert.Equal(0.0, offsetArc.Center.Y, Tolerance);
        Assert.Equal(3.0, offsetArc.Center.Z, Tolerance);
    }

    [Fact]
    public void Offset_CombinedRadialAndNormal_AppliesBothOffsets()
    {
        // Arrange - quarter arc in XY plane, center at origin, radius 10
        // Normal = (0,0,1)
        var arc = new Arc3D(
            center: new Point3D(0, 0, 0),
            normal: new Vector3D(0, 0, 1),
            xAxis: new Vector3D(1, 0, 0),
            radius: 10.0,
            startAngle: 0,
            endAngle: Math.PI / 2,
            isCounterClockwise: true);

        // Act - offset X=2 (inward) and Y=5 (along normal)
        var offsetArc = arc.Offset(new Vector2D(2, 5));

        // Assert - radius decreases by 2 AND arc translates along Z by 5
        Assert.Equal(8.0, offsetArc.Radius, Tolerance);
        Assert.Equal(8.0, offsetArc.StartPoint.X, Tolerance);
        Assert.Equal(0.0, offsetArc.StartPoint.Y, Tolerance);
        Assert.Equal(5.0, offsetArc.StartPoint.Z, Tolerance);
        Assert.Equal(0.0, offsetArc.EndPoint.X, Tolerance);
        Assert.Equal(8.0, offsetArc.EndPoint.Y, Tolerance);
        Assert.Equal(5.0, offsetArc.EndPoint.Z, Tolerance);
        // Center shifts along Z only
        Assert.Equal(0.0, offsetArc.Center.X, Tolerance);
        Assert.Equal(0.0, offsetArc.Center.Y, Tolerance);
        Assert.Equal(5.0, offsetArc.Center.Z, Tolerance);
    }
}