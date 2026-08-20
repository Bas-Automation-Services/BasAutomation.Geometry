using BasAutomation.Geometry.Curves;

namespace BasAutomation.Geometry.Tests;

public class Circle3DTests
{
    private const double Tolerance = Constants.Tolerance;

    [Fact]
    public void Constructor_WithCenterRadiusNormal_CreatesCircle()
    {
        // Arrange
        var center = new Point3D(5, 10, 15);
        var radius = 3.5;
        var normal = new Vector3D(0, 0, 1);

        // Act
        var circle = new Circle3D(center, radius, normal);

        // Assert
        Assert.Equal(center, circle.Center);
        Assert.Equal(radius, circle.Radius);
        Assert.True(circle.Normal.IsEqualTo(normal.Normalized()));
    }

    [Fact]
    public void Constructor_WithNegativeRadius_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new Circle3D(new Point3D(0, 0, 0), -1, new Vector3D(0, 0, 1)));
    }

    [Fact]
    public void Constructor_NormalizesNormalVector()
    {
        // Arrange
        var normal = new Vector3D(3, 4, 0); // Length = 5

        // Act
        var circle = new Circle3D(new Point3D(0, 0, 0), 1, normal);

        // Assert
        Assert.Equal(0.6, circle.Normal.X, Tolerance);
        Assert.Equal(0.8, circle.Normal.Y, Tolerance);
        Assert.Equal(0, circle.Normal.Z, Tolerance);
    }

    [Fact]
    public void FromThreePoints_ValidPoints_CreatesCircle()
    {
        // Arrange
        var p1 = new Point3D(1, 0, 0);
        var p2 = new Point3D(0, 1, 0);
        var p3 = new Point3D(-1, 0, 0);

        // Act
        var circle = Circle3D.FromThreePoints(p1, p2, p3);

        // Assert
        Assert.Equal(0, circle.Center.X, Tolerance);
        Assert.Equal(0, circle.Center.Y, Tolerance);
        Assert.Equal(0, circle.Center.Z, Tolerance);
        Assert.Equal(1, circle.Radius, Tolerance);
        Assert.True(circle.Normal.IsEqualTo(new Vector3D(0, 0, 1)) ||
                    circle.Normal.IsEqualTo(new Vector3D(0, 0, -1)));
    }

    [Fact]
    public void FromThreePoints_CollinearPoints_ThrowsException()
    {
        // Arrange
        var p1 = new Point3D(0, 0, 0);
        var p2 = new Point3D(1, 0, 0);
        var p3 = new Point3D(2, 0, 0);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Circle3D.FromThreePoints(p1, p2, p3));
    }

    [Fact]
    public void Properties_CalculateCorrectly()
    {
        // Arrange
        var circle = new Circle3D(new Point3D(0, 0, 0), 5, new Vector3D(0, 0, 1));

        // Act & Assert
        Assert.Equal(10, circle.Diameter, Tolerance);
        Assert.Equal(2 * Math.PI * 5, circle.Circumference, Tolerance);
        Assert.Equal(Math.PI * 25, circle.Area, Tolerance);
    }

    [Fact]
    public void Plane_ReturnsCorrectPlane()
    {
        // Arrange
        var center = new Point3D(1, 2, 3);
        var normal = new Vector3D(1, 0, 0);
        var circle = new Circle3D(center, 5, normal);

        // Act
        var plane = circle.Plane;

        // Assert
        Assert.Equal(center, plane.Origin);
        Assert.Equal(normal, plane.Normal);
    }

    [Fact]
    public void PointAt_ReturnsCorrectPoints()
    {
        // Arrange
        var circle = new Circle3D(new Point3D(0, 0, 0), 10, new Vector3D(0, 0, 1));

        // Act & Assert
        var p0 = circle.PointAt(0);
        Assert.Equal(10, p0.X, Tolerance);
        Assert.Equal(0, p0.Y, Tolerance);
        Assert.Equal(0, p0.Z, Tolerance);

        var p025 = circle.PointAt(0.25);
        Assert.Equal(0, p025.X, Tolerance);
        Assert.Equal(10, p025.Y, Tolerance);
        Assert.Equal(0, p025.Z, Tolerance);

        var p05 = circle.PointAt(0.5);
        Assert.Equal(-10, p05.X, Tolerance);
        Assert.Equal(0, p05.Y, Tolerance);
        Assert.Equal(0, p05.Z, Tolerance);
    }

    [Fact]
    public void PointAt_ArbitraryNormal_ReturnsCorrectPoints()
    {
        // Arrange - Circle in YZ plane
        var circle = new Circle3D(new Point3D(5, 0, 0), 10, new Vector3D(1, 0, 0));

        // Act
        var p0 = circle.PointAt(0);
        var p025 = circle.PointAt(0.25);

        // Assert
        Assert.Equal(5, p0.X, Tolerance);
        Assert.True(Math.Abs(p0.Y) < Tolerance || Math.Abs(p0.Y - 10) < Tolerance ||
                    Math.Abs(p0.Y + 10) < Tolerance);

        Assert.Equal(5, p025.X, Tolerance);
        var distanceFromCenter = Math.Sqrt(p025.Y * p025.Y + p025.Z * p025.Z);
        Assert.Equal(10, distanceFromCenter, Tolerance);
    }

    [Fact]
    public void TangentAt_ReturnsUnitVectors()
    {
        // Arrange
        var circle = new Circle3D(new Point3D(0, 0, 0), 5, new Vector3D(0, 0, 1));

        // Act
        var t0 = circle.TangentAt(0);
        var t025 = circle.TangentAt(0.25);
        var t05 = circle.TangentAt(0.5);

        // Assert
        Assert.Equal(1, t0.Length, Tolerance);
        Assert.Equal(1, t025.Length, Tolerance);
        Assert.Equal(1, t05.Length, Tolerance);
    }

    [Fact]
    public void NormalAt_PointsTowardCenter()
    {
        // Arrange
        var circle = new Circle3D(new Point3D(0, 0, 0), 5, new Vector3D(0, 0, 1));

        // Act
        var normal = circle.NormalAt(0);

        // Assert
        Assert.Equal(-1, normal.X, Tolerance);
        Assert.Equal(0, normal.Y, Tolerance);
        Assert.Equal(0, normal.Z, Tolerance);
    }

    [Fact]
    public void ParameterAtDistance_CalculatesCorrectly()
    {
        // Arrange
        var circle = new Circle3D(new Point3D(0, 0, 0), 10, new Vector3D(0, 0, 1));
        var quarterCircumference = circle.Circumference / 4;

        // Act
        var param = circle.ParameterAtDistance(quarterCircumference);

        // Assert
        Assert.Equal(0.25, param, Tolerance);
    }

    [Fact]
    public void ContainsPoint_PointOnCircle_ReturnsTrue()
    {
        // Arrange
        var circle = new Circle3D(new Point3D(0, 0, 0), 5, new Vector3D(0, 0, 1));
        var point = new Point3D(5, 0, 0);

        // Act & Assert
        Assert.True(circle.ContainsPoint(point));
    }

    [Fact]
    public void ContainsPoint_PointNotOnPlane_ReturnsFalse()
    {
        // Arrange
        var circle = new Circle3D(new Point3D(0, 0, 0), 5, new Vector3D(0, 0, 1));
        var point = new Point3D(5, 0, 1); // Off the plane

        // Act & Assert
        Assert.False(circle.ContainsPoint(point));
    }

    [Fact]
    public void ContainsPoint_PointOnPlaneWrongRadius_ReturnsFalse()
    {
        // Arrange
        var circle = new Circle3D(new Point3D(0, 0, 0), 5, new Vector3D(0, 0, 1));
        var point = new Point3D(3, 0, 0); // Wrong radius

        // Act & Assert
        Assert.False(circle.ContainsPoint(point));
    }

    [Fact]
    public void ProjectPoint_PointOffPlane_ProjectsCorrectly()
    {
        // Arrange
        var circle = new Circle3D(new Point3D(0, 0, 0), 5, new Vector3D(0, 0, 1));
        var point = new Point3D(3, 4, 10); // Off plane, inside circle projection

        // Act
        var projected = circle.ProjectPoint(point);

        // Assert
        Assert.Equal(3, projected.X, Tolerance);
        Assert.Equal(4, projected.Y, Tolerance);
        Assert.Equal(0, projected.Z, Tolerance);
    }

    [Fact]
    public void ProjectPoint_CenterPoint_ReturnsPointOnCircle()
    {
        // Arrange
        var circle = new Circle3D(new Point3D(0, 0, 0), 5, new Vector3D(0, 0, 1));
        var point = new Point3D(0, 0, 0); // At center

        // Act
        var projected = circle.ProjectPoint(point);

        // Assert
        var distance = projected.DistanceTo(circle.Center);
        Assert.Equal(5, distance, Tolerance);
    }

    [Fact]
    public void Transform_Translation_MovesCircle()
    {
        // Arrange
        var circle = new Circle3D(new Point3D(0, 0, 0), 5, new Vector3D(0, 0, 1));
        var matrix = Matrix3D.Translation(10, 20, 30);

        // Act
        var transformed = circle.Transform(matrix);

        // Assert
        Assert.Equal(10, transformed.Center.X, Tolerance);
        Assert.Equal(20, transformed.Center.Y, Tolerance);
        Assert.Equal(30, transformed.Center.Z, Tolerance);
        Assert.Equal(5, transformed.Radius, Tolerance);
        Assert.True(transformed.Normal.IsEqualTo(new Vector3D(0, 0, 1)));
    }

    [Fact]
    public void Transform_Rotation_RotatesCircle()
    {
        // Arrange
        var circle = new Circle3D(new Point3D(0, 0, 0), 5, new Vector3D(0, 0, 1));
        var matrix = Matrix3D.RotationX(Math.PI / 2); // 90 degrees around X

        // Act
        var transformed = circle.Transform(matrix);

        // Assert
        Assert.Equal(0, transformed.Center.X, Tolerance);
        Assert.Equal(0, transformed.Center.Y, Tolerance);
        Assert.Equal(0, transformed.Center.Z, Tolerance);
        Assert.Equal(5, transformed.Radius, Tolerance);
        Assert.True(transformed.Normal.IsEqualTo(new Vector3D(0, -1, 0)));
    }

    [Fact]
    public void GetBoundingBox_HorizontalCircle_CalculatesCorrectly()
    {
        // Arrange
        var circle = new Circle3D(new Point3D(5, 10, 15), 3, new Vector3D(0, 0, 1));

        // Act
        var bbox = circle.GetBoundingBox();

        // Assert
        Assert.Equal(2, bbox.Min.X, Tolerance);
        Assert.Equal(7, bbox.Min.Y, Tolerance);
        Assert.Equal(15, bbox.Min.Z, Tolerance);
        Assert.Equal(8, bbox.Max.X, Tolerance);
        Assert.Equal(13, bbox.Max.Y, Tolerance);
        Assert.Equal(15, bbox.Max.Z, Tolerance);
    }

    [Fact]
    public void GetBoundingBox_VerticalCircle_CalculatesCorrectly()
    {
        // Arrange
        var circle = new Circle3D(new Point3D(0, 0, 0), 5, new Vector3D(1, 0, 0));

        // Act
        var bbox = circle.GetBoundingBox();

        // Assert
        Assert.Equal(0, bbox.Min.X, Tolerance);
        Assert.Equal(-5, bbox.Min.Y, Tolerance);
        Assert.Equal(-5, bbox.Min.Z, Tolerance);
        Assert.Equal(0, bbox.Max.X, Tolerance);
        Assert.Equal(5, bbox.Max.Y, Tolerance);
        Assert.Equal(5, bbox.Max.Z, Tolerance);
    }

    [Fact]
    public void IsEqualTo_EqualCircles_ReturnsTrue()
    {
        // Arrange
        var circle1 = new Circle3D(new Point3D(5, 10, 15), 3, new Vector3D(0, 0, 1));
        var circle2 = new Circle3D(new Point3D(5, 10, 15), 3, new Vector3D(0, 0, 1));

        // Act & Assert
        Assert.True(circle1.IsEqualTo(circle2));
        Assert.True(circle1 == circle2);
        Assert.False(circle1 != circle2);
    }

    [Fact]
    public void IsEqualTo_OppositeNormals_ReturnsTrue()
    {
        // Arrange
        var circle1 = new Circle3D(new Point3D(0, 0, 0), 5, new Vector3D(0, 0, 1));
        var circle2 = new Circle3D(new Point3D(0, 0, 0), 5, new Vector3D(0, 0, -1));

        // Act & Assert
        Assert.True(circle1.IsEqualTo(circle2));
    }

    [Fact]
    public void IsEqualTo_DifferentCircles_ReturnsFalse()
    {
        // Arrange
        var circle1 = new Circle3D(new Point3D(0, 0, 0), 5, new Vector3D(0, 0, 1));
        var circle2 = new Circle3D(new Point3D(0, 0, 0), 6, new Vector3D(0, 0, 1));

        // Act & Assert
        Assert.False(circle1.IsEqualTo(circle2));
        Assert.False(circle1 == circle2);
        Assert.True(circle1 != circle2);
    }

    [Fact]
    public void Clone_CreatesIdenticalCopy()
    {
        // Arrange
        var circle = new Circle3D(new Point3D(1, 2, 3), 4, new Vector3D(1, 0, 0));

        // Act
        var clone = (Circle3D)circle.Clone();

        // Assert
        Assert.Equal(circle.Center, clone.Center);
        Assert.Equal(circle.Radius, clone.Radius);
        Assert.Equal(circle.Normal, clone.Normal);
    }
    
    [Fact]
    public void StartPointEndPoint_AreSame()
    {
        // Arrange
        var circle = new Circle3D(new Point3D(0, 0, 0), 5, new Vector3D(0, 0, 1));

        // Act & Assert
        Assert.Equal(circle.StartPoint, circle.EndPoint);
    }

    [Fact]
    public void Length_EqualsCircumference()
    {
        // Arrange
        var circle = new Circle3D(new Point3D(0, 0, 0), 7, new Vector3D(0, 1, 0));

        // Act & Assert
        Assert.Equal(circle.Circumference, circle.Length);
    }
}