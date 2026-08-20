using BasAutomation.Geometry.Curves;

namespace BasAutomation.Geometry.Tests;

public class Plane3DTests
{
    private const double Tolerance = Constants.Tolerance;

    [Fact]
    public void Constructor_NormalizesNormalVector()
    {
        // Arrange
        var origin = new Point3D(0, 0, 0);
        var normal = new Vector3D(2, 0, 0); // Not normalized

        // Act
        var plane = new Plane3D(origin, normal);

        // Assert
        Assert.Equal(1.0, plane.Normal.Length, Tolerance);
        Assert.Equal(new Vector3D(1, 0, 0), plane.Normal);
    }

    [Fact]
    public void FromThreePoints_CreatesCorrectPlane()
    {
        // Arrange
        var p1 = new Point3D(0, 0, 0);
        var p2 = new Point3D(1, 0, 0);
        var p3 = new Point3D(0, 1, 0);

        // Act
        var plane = Plane3D.FromThreePoints(p1, p2, p3);

        // Assert
        Assert.Equal(p1, plane.Origin);
        Assert.Equal(new Vector3D(0, 0, 1), plane.Normal);
    }

    [Fact]
    public void FromThreePoints_CollinearPoints_CreatesInvalidPlane()
    {
        // Arrange
        var p1 = new Point3D(0, 0, 0);
        var p2 = new Point3D(1, 0, 0);
        var p3 = new Point3D(2, 0, 0); // Collinear with p1 and p2

        // Act
        var plane = Plane3D.FromThreePoints(p1, p2, p3);

        // Assert
        // Normal should be zero vector normalized (NaN or specific behavior)
        Assert.True(double.IsNaN(plane.Normal.X) || plane.Normal.Length < Tolerance);
    }

    [Fact]
    public void FromPointAndVectors_CreatesCorrectPlane()
    {
        // Arrange
        var origin = new Point3D(1, 2, 3);
        var u = new Vector3D(1, 0, 0);
        var v = new Vector3D(0, 1, 0);

        // Act
        var plane = Plane3D.FromPointAndVectors(origin, u, v);

        // Assert
        Assert.Equal(origin, plane.Origin);
        Assert.Equal(new Vector3D(0, 0, 1), plane.Normal);
    }

    [Theory]
    [InlineData(0, 0, 1, 1.0)]  // Point above plane
    [InlineData(0, 0, -1, -1.0)] // Point below plane
    [InlineData(0, 0, 0, 0.0)]   // Point on plane
    public void SignedDistanceToPoint_ReturnsCorrectDistance(double x, double y, double z, double expected)
    {
        // Arrange
        var plane = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var point = new Point3D(x, y, z);

        // Act
        var distance = plane.SignedDistanceToPoint(point);

        // Assert
        Assert.Equal(expected, distance, Tolerance);
    }

    [Theory]
    [InlineData(0, 0, 1, 1.0)]  // Point above plane
    [InlineData(0, 0, -1, 1.0)] // Point below plane
    [InlineData(0, 0, 0, 0.0)]  // Point on plane
    public void DistanceToPoint_ReturnsAbsoluteDistance(double x, double y, double z, double expected)
    {
        // Arrange
        var plane = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var point = new Point3D(x, y, z);

        // Act
        var distance = plane.DistanceToPoint(point);

        // Assert
        Assert.Equal(expected, distance, Tolerance);
    }

    [Fact]
    public void ProjectPoint_ProjectsCorrectly()
    {
        // Arrange
        var plane = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var point = new Point3D(5, 3, 10);

        // Act
        var projected = plane.ProjectPoint(point);

        // Assert
        Assert.Equal(new Point3D(5, 3, 0), projected);
        Assert.True(plane.ContainsPoint(projected));
    }

    [Fact]
    public void ProjectVector_ProjectsCorrectly()
    {
        // Arrange
        var plane = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var vector = new Vector3D(1, 1, 1);

        // Act
        var projected = plane.ProjectVector(vector);

        // Assert
        Assert.Equal(new Vector3D(1, 1, 0), projected);
        Assert.Equal(0, projected.Dot(plane.Normal), Tolerance);
    }

    [Theory]
    [InlineData(0, 0, 0, true)]    // Point on plane
    [InlineData(0, 0, 0.9e-6, true)] // Point within tolerance
    [InlineData(0, 0, 0.1, false)]  // Point outside tolerance
    public void ContainsPoint_WithDefaultTolerance(double x, double y, double z, bool expected)
    {
        // Arrange
        var plane = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var point = new Point3D(x, y, z);

        // Act
        var contains = plane.ContainsPoint(point);

        // Assert
        Assert.Equal(expected, contains);
    }

    [Fact]
    public void IntersectLine_IntersectingLine_ReturnsTrue()
    {
        // Arrange
        var plane = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var line = new Line3D(new Point3D(0, 0, -1), new Point3D(0, 0, 1));

        // Act
        var result = plane.IntersectLine(line, out var intersection);

        // Assert
        Assert.True(result);
        Assert.Equal(new Point3D(0, 0, 0), intersection);
    }

    [Fact]
    public void IntersectLine_ParallelLine_ReturnsFalse()
    {
        // Arrange
        var plane = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var line = new Line3D(new Point3D(0, 0, 1), new Point3D(1, 0, 1));

        // Act
        var result = plane.IntersectLine(line, out var intersection);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IntersectLine_IntersectionOutsideSegment_ReturnsFalse()
    {
        // Arrange
        var plane = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var line = new Line3D(new Point3D(0, 0, 1), new Point3D(0, 0, 2));

        // Act
        var result = plane.IntersectLine(line, out var intersection);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IntersectRay_IntersectingRay_ReturnsTrue()
    {
        // Arrange
        var plane = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var rayOrigin = new Point3D(0, 0, -1);
        var rayDirection = new Vector3D(0, 0, 1);

        // Act
        var result = plane.IntersectRay(rayOrigin, rayDirection, out var intersection);

        // Assert
        Assert.True(result);
        Assert.Equal(new Point3D(0, 0, 0), intersection);
    }

    [Fact]
    public void IntersectRay_ParallelRay_ReturnsFalse()
    {
        // Arrange
        var plane = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var rayOrigin = new Point3D(0, 0, 1);
        var rayDirection = new Vector3D(1, 0, 0);

        // Act
        var result = plane.IntersectRay(rayOrigin, rayDirection, out var intersection);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IntersectRay_RayPointingAway_ReturnsFalse()
    {
        // Arrange
        var plane = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var rayOrigin = new Point3D(0, 0, 1);
        var rayDirection = new Vector3D(0, 0, 1);

        // Act
        var result = plane.IntersectRay(rayOrigin, rayDirection, out var intersection);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IntersectInfiniteLine_IntersectingLine_ReturnsTrue()
    {
        // Arrange
        var plane = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var pointOnLine = new Point3D(0, 0, -1);
        var lineDirection = new Vector3D(0, 0, 1);

        // Act
        var result = plane.IntersectInfiniteLine(pointOnLine, lineDirection, out var intersection);

        // Assert
        Assert.True(result);
        Assert.Equal(new Point3D(0, 0, 0), intersection);
    }

    [Fact]
    public void IntersectInfiniteLine_ParallelLine_ReturnsFalse()
    {
        // Arrange
        var plane = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var pointOnLine = new Point3D(0, 0, 1);
        var lineDirection = new Vector3D(1, 0, 0);

        // Act
        var result = plane.IntersectInfiniteLine(pointOnLine, lineDirection, out _);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IntersectInfiniteLine_LinePointingAway_StillReturnsTrue()
    {
        // Unlike IntersectRay (which rejects t < 0), the unbounded variant must return the
        // intersection on the opposite side of the line origin, so callers can derive a
        // signed direction from it.
        // Arrange
        var plane = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var pointOnLine = new Point3D(0, 0, 1);
        var lineDirection = new Vector3D(0, 0, 1);

        // Act
        var result = plane.IntersectInfiniteLine(pointOnLine, lineDirection, out var intersection);

        // Assert
        Assert.True(result);
        Assert.Equal(new Point3D(0, 0, 0), intersection);
    }

    [Fact]
    public void IntersectInfiniteLine_NonUnitDirection_NormalizesBeforeStepping()
    {
        // Arrange
        var plane = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var pointOnLine = new Point3D(0, 0, -1);
        var lineDirection = new Vector3D(0, 0, 5);

        // Act
        var result = plane.IntersectInfiniteLine(pointOnLine, lineDirection, out var intersection);

        // Assert
        Assert.True(result);
        Assert.Equal(0.0, intersection.X, Tolerance);
        Assert.Equal(0.0, intersection.Y, Tolerance);
        Assert.Equal(0.0, intersection.Z, Tolerance);
    }

    [Fact]
    public void Flip_ReversesNormal()
    {
        // Arrange
        var plane = new Plane3D(new Point3D(1, 2, 3), new Vector3D(0, 0, 1));

        // Act
        var flipped = plane.Flip();

        // Assert
        Assert.Equal(plane.Origin, flipped.Origin);
        Assert.Equal(new Vector3D(0, 0, -1), flipped.Normal);
    }

    [Fact]
    public void Offset_MovesPlaneAlongNormal()
    {
        // Arrange
        var plane = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var distance = 5.0;

        // Act
        var offset = plane.Offset(distance);

        // Assert
        Assert.Equal(new Point3D(0, 0, 5), offset.Origin);
        Assert.Equal(plane.Normal, offset.Normal);
    }

    [Fact]
    public void Transform_AppliesTransformationCorrectly()
    {
        // Arrange
        var plane = new Plane3D(new Point3D(0, 0, 0), new Vector3D(1, 0, 0));
        var rotation = Matrix3D.RotationZ(Math.PI / 2); // 90 degree rotation around Z

        // Act
        var transformed = plane.Transform(rotation);

        // Assert
        Assert.Equal(new Point3D(0, 0, 0), transformed.Origin);
        Assert.Equal(0, transformed.Normal.X, Tolerance);
        Assert.Equal(1, transformed.Normal.Y, Tolerance);
        Assert.Equal(0, transformed.Normal.Z, Tolerance);
    }

    [Fact]
    public void IsEqualTo_SamePlane_ReturnsTrue()
    {
        // Arrange
        var plane1 = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var plane2 = new Plane3D(new Point3D(1, 1, 0), new Vector3D(0, 0, 1));

        // Act
        var result = plane1.IsEqualTo(plane2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsEqualTo_ParallelButDifferentPlanes_ReturnsFalse()
    {
        // Arrange
        var plane1 = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var plane2 = new Plane3D(new Point3D(0, 0, 1), new Vector3D(0, 0, 1));

        // Act
        var result = plane1.IsEqualTo(plane2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsEqualTo_DifferentNormals_ReturnsFalse()
    {
        // Arrange
        var plane1 = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var plane2 = new Plane3D(new Point3D(0, 0, 0), new Vector3D(1, 0, 0));

        // Act
        var result = plane1.IsEqualTo(plane2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsEqualTo_OppositeNormalsSamePlane_ReturnsTrue()
    {
        // Arrange
        var plane1 = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var plane2 = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, -1));

        // Act
        var result = plane1.IsEqualTo(plane2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_SamePlane_ReturnsTrue()
    {
        // Arrange
        var plane1 = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var plane2 = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));

        // Act & Assert
        Assert.True(plane1.Equals(plane2));
        Assert.True(plane1.Equals((object)plane2));
        Assert.True(plane1 == plane2);
        Assert.False(plane1 != plane2);
    }

    [Fact]
    public void GetHashCode_SamePlanes_ReturnSameHashCode()
    {
        // Arrange
        var plane1 = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var plane2 = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));

        // Act & Assert
        Assert.Equal(plane1.GetHashCode(), plane2.GetHashCode());
    }

    [Fact]
    public void ToString_ReturnsExpectedFormat()
    {
        // Arrange
        var plane = new Plane3D(new Point3D(1, 2, 3), new Vector3D(0, 0, 1));

        // Act
        var result = plane.ToString();

        // Assert
        Assert.Contains("Plane", result);
        Assert.Contains("Origin", result);
        Assert.Contains("Normal", result);
    }


    [Fact]
    public void IsPointOnPositiveSide_PointAbovePlane_ReturnsTrue()
    {
        var plane = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var point = new Point3D(0, 0, 5);

        Assert.True(plane.IsPointOnPositiveSide(point));
    }

    [Fact]
    public void IsPointOnPositiveSide_PointBelowPlane_ReturnsFalse()
    {
        var plane = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var point = new Point3D(0, 0, -5);

        Assert.False(plane.IsPointOnPositiveSide(point));
    }

    [Fact]
    public void IsPointOnPositiveSide_PointOnPlane_ReturnsFalse()
    {
        var plane = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var point = new Point3D(5, 10, 0);

        Assert.False(plane.IsPointOnPositiveSide(point));
    }

    [Fact]
    public void IsVectorPointingAway_VectorAlignedWithNormal_ReturnsTrue()
    {
        var plane = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var vector = new Vector3D(0, 0, 2);

        Assert.True(plane.IsVectorPointingAway(vector));
    }

    [Fact]
    public void IsVectorPointingAway_VectorOppositeToNormal_ReturnsFalse()
    {
        var plane = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var vector = new Vector3D(0, 0, -2);

        Assert.False(plane.IsVectorPointingAway(vector));
    }

    [Fact]
    public void MirrorPoint_MirrorsCorrectlyAcrossXYPlane()
    {
        var plane = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var point = new Point3D(1, 2, 3);
        var mirrored = plane.MirrorPoint(point);

        Assert.Equal(1, mirrored.X, Tolerance);
        Assert.Equal(2, mirrored.Y, Tolerance);
        Assert.Equal(-3, mirrored.Z, Tolerance);
    }

    [Fact]
    public void MirrorPoint_MirrorTwiceReturnsOriginal()
    {
        var plane = new Plane3D(new Point3D(0, 0, 0), new Vector3D(1, 1, 1).Normalized());
        var point = new Point3D(5, -3, 7);
        var mirrored = plane.MirrorPoint(point);
        var doubleMirrored = plane.MirrorPoint(mirrored);

        Assert.True(doubleMirrored.IsEqualTo(point, Tolerance));
    }

    [Fact]
    public void MirrorVector_MirrorsCorrectlyAcrossYZPlane()
    {
        var plane = new Plane3D(new Point3D(0, 0, 0), new Vector3D(1, 0, 0));
        var vector = new Vector3D(1, 2, 3);
        var mirrored = plane.MirrorVector(vector);

        Assert.Equal(-1, mirrored.X, Tolerance);
        Assert.Equal(2, mirrored.Y, Tolerance);
        Assert.Equal(3, mirrored.Z, Tolerance);
    }

    [Fact]
    public void IntersectPlane_ParallelPlanes_ReturnsFalse()
    {
        var plane1 = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var plane2 = new Plane3D(new Point3D(0, 0, 5), new Vector3D(0, 0, 1));

        var result = plane1.IntersectPlane(plane2, out var lineOrigin, out var lineDirection);

        Assert.False(result);
    }

    [Fact]
    public void IntersectPlane_PerpendicularPlanes_ReturnsCorrectLine()
    {
        var plane1 = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1)); // XY plane
        var plane2 = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 1, 0)); // XZ plane

        var result = plane1.IntersectPlane(plane2, out var lineOrigin, out var lineDirection);

        Assert.True(result);
        // Line should be along X axis
        Assert.True(lineDirection.IsParallel(new Vector3D(1, 0, 0), Tolerance) ||
                   lineDirection.IsParallel(new Vector3D(-1, 0, 0), Tolerance));
        // Line should pass through origin
        Assert.True(plane1.ContainsPoint(lineOrigin, Tolerance));
        Assert.True(plane2.ContainsPoint(lineOrigin, Tolerance));
    }

    [Fact]
    public void IntersectPlane_DiagonalPlanes_FindsValidIntersection()
    {
        var plane1 = new Plane3D(new Point3D(1, 0, 0), new Vector3D(1, 1, 0).Normalized());
        var plane2 = new Plane3D(new Point3D(0, 1, 0), new Vector3D(0, 1, 1).Normalized());

        var result = plane1.IntersectPlane(plane2, out var lineOrigin, out var lineDirection);

        Assert.True(result);
        // Verify the line origin is on both planes
        Assert.True(plane1.ContainsPoint(lineOrigin, 1e-6));
        Assert.True(plane2.ContainsPoint(lineOrigin, 1e-6));
        // Verify line direction is perpendicular to both normals
        Assert.True(Math.Abs(lineDirection.Dot(plane1.Normal)) < 1e-6);
        Assert.True(Math.Abs(lineDirection.Dot(plane2.Normal)) < 1e-6);
    }

    [Fact]
    public void Translate_MovesPlaneCorrectly()
    {
        var plane = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var translation = new Vector3D(5, 10, 15);
        var translated = plane.Translate(translation);

        Assert.Equal(5, translated.Origin.X);
        Assert.Equal(10, translated.Origin.Y);
        Assert.Equal(15, translated.Origin.Z);
        Assert.Equal(plane.Normal, translated.Normal);
    }

    [Fact]
    public void GetMirrored_MirrorsPlaneAcrossAnotherPlane()
    {
        var plane = new Plane3D(new Point3D(0, 0, 5), new Vector3D(0, 0, 1));
        var mirrorPlane = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var mirrored = plane.GetMirrored(mirrorPlane);

        Assert.Equal(0, mirrored.Origin.X);
        Assert.Equal(0, mirrored.Origin.Y);
        Assert.Equal(-5, mirrored.Origin.Z);
        Assert.Equal(0, mirrored.Normal.X);
        Assert.Equal(0, mirrored.Normal.Y);
        Assert.Equal(-1, mirrored.Normal.Z); // Normal remains the same for parallel planes
    }

    [Fact]
    public void AngleTo_ParallelPlanes_ReturnsZero()
    {
        var plane1 = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var plane2 = new Plane3D(new Point3D(0, 0, 5), new Vector3D(0, 0, -1)); // Opposite normal

        var angle = plane1.AngleTo(plane2);

        Assert.Equal(0, angle, Tolerance);
    }

    [Fact]
    public void AngleTo_PerpendicularPlanes_ReturnsHalfPi()
    {
        var plane1 = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var plane2 = new Plane3D(new Point3D(0, 0, 0), new Vector3D(1, 0, 0));

        var angle = plane1.AngleTo(plane2);

        Assert.Equal(Math.PI / 2, angle, Tolerance);
    }

    [Fact]
    public void IsParallelTo_ParallelPlanes_ReturnsTrue()
    {
        var plane1 = new Plane3D(new Point3D(0, 0, 0), new Vector3D(1, 2, 3));
        var plane2 = new Plane3D(new Point3D(10, 10, 10), new Vector3D(2, 4, 6)); // Scaled normal

        Assert.True(plane1.IsParallelTo(plane2));
    }

    [Fact]
    public void IsParallelTo_NonParallelPlanes_ReturnsFalse()
    {
        var plane1 = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var plane2 = new Plane3D(new Point3D(0, 0, 0), new Vector3D(1, 0, 0));

        Assert.False(plane1.IsParallelTo(plane2));
    }

    [Fact]
    public void IsParallelTo_Vector_DirectionInPlane_ReturnsTrue()
    {
        var plane = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var direction = new Vector3D(1, 0, 0); // In XY plane, perpendicular to Z normal

        Assert.True(plane.IsParallelTo(direction));
    }

    [Fact]
    public void IsParallelTo_Vector_DirectionNotInPlane_ReturnsFalse()
    {
        var plane = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var direction = new Vector3D(1, 1, 1).Normalized(); // Has Z component

        Assert.False(plane.IsParallelTo(direction));
    }

    [Fact]
    public void IsParallelTo_Vector_DirectionAlongNormal_ReturnsFalse()
    {
        var plane = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var direction = new Vector3D(0, 0, 1); // Exactly along normal

        Assert.False(plane.IsParallelTo(direction));
    }

    [Fact]
    public void IsParallelTo_Vector_NearToleranceEdgeCase_ReturnsTrue()
    {
        var plane = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        // Direction almost in plane but with tiny Z component (within tolerance)
        var direction = new Vector3D(1, 0, 1e-8).Normalized();

        Assert.True(plane.IsParallelTo(direction));
    }

    [Fact]
    public void IsParallelTo_Vector_DiagonalPlane_DirectionInPlane_ReturnsTrue()
    {
        var plane = new Plane3D(new Point3D(0, 0, 0), new Vector3D(1, 1, 1).Normalized());
        // A direction perpendicular to (1,1,1): (-1, 1, 0) works
        var direction = new Vector3D(-1, 1, 0).Normalized();

        Assert.True(plane.IsParallelTo(direction));
    }

    [Fact]
    public void IsPerpendicularTo_PerpendicularPlanes_ReturnsTrue()
    {
        var plane1 = new Plane3D(new Point3D(0, 0, 0), new Vector3D(1, 0, 0));
        var plane2 = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 1, 0));

        Assert.True(plane1.IsPerpendicularTo(plane2));
    }

    [Fact]
    public void DistanceTo_ParallelPlanes_ReturnsCorrectDistance()
    {
        var plane1 = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var plane2 = new Plane3D(new Point3D(0, 0, 5), new Vector3D(0, 0, 1));

        var distance = plane1.DistanceTo(plane2);

        Assert.Equal(5, distance, Tolerance);
    }

    [Fact]
    public void DistanceTo_NonParallelPlanes_ReturnsNaN()
    {
        var plane1 = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var plane2 = new Plane3D(new Point3D(0, 0, 0), new Vector3D(1, 0, 0));

        var distance = plane1.DistanceTo(plane2);

        Assert.True(double.IsNaN(distance));
    }

    #region 2D Plane Coordinate System Tests

    [Fact]
    public void GetUAxis_XYPlane_ReturnsNormalizedVector()
    {
        var plane = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));

        var uAxis = plane.GetUAxis();

        Assert.Equal(1.0, uAxis.Length, Tolerance);
        Assert.Equal(0, uAxis.Dot(plane.Normal), Tolerance);
    }

    [Fact]
    public void GetUAxis_YZPlane_ReturnsNormalizedVector()
    {
        var plane = new Plane3D(new Point3D(0, 0, 0), new Vector3D(1, 0, 0));

        var uAxis = plane.GetUAxis();

        Assert.Equal(1.0, uAxis.Length, Tolerance);
        Assert.Equal(0, uAxis.Dot(plane.Normal), Tolerance);
    }

    [Fact]
    public void GetUAxis_XZPlane_ReturnsNormalizedVector()
    {
        var plane = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 1, 0));

        var uAxis = plane.GetUAxis();

        Assert.Equal(1.0, uAxis.Length, Tolerance);
        Assert.Equal(0, uAxis.Dot(plane.Normal), Tolerance);
    }

    [Fact]
    public void GetVAxis_XYPlane_ReturnsNormalizedVectorPerpendicularToUAxis()
    {
        var plane = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));

        var uAxis = plane.GetUAxis();
        var vAxis = plane.GetVAxis();

        Assert.Equal(1.0, vAxis.Length, Tolerance);
        Assert.Equal(0, vAxis.Dot(plane.Normal), Tolerance);
        Assert.Equal(0, vAxis.Dot(uAxis), Tolerance);
    }

    [Fact]
    public void GetVAxis_DiagonalPlane_ReturnsNormalizedVectorPerpendicularToUAxis()
    {
        var plane = new Plane3D(new Point3D(0, 0, 0), new Vector3D(1, 1, 1).Normalized());

        var uAxis = plane.GetUAxis();
        var vAxis = plane.GetVAxis();

        Assert.Equal(1.0, vAxis.Length, Tolerance);
        Assert.Equal(0, vAxis.Dot(plane.Normal), Tolerance);
        Assert.Equal(0, vAxis.Dot(uAxis), Tolerance);
    }

    [Fact]
    public void TransformToPlane_Point_PointOnPlane_ReturnsOrigin()
    {
        var plane = new Plane3D(new Point3D(5, 5, 5), new Vector3D(0, 0, 1));

        var point2D = plane.TransformToPlane(new Point3D(5, 5, 5));

        Assert.Equal(0, point2D.X, Tolerance);
        Assert.Equal(0, point2D.Y, Tolerance);
    }

    [Fact]
    public void TransformToPlane_Point_XYPlane_ReturnsCorrectCoordinates()
    {
        var plane = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var point3D = new Point3D(3, 4, 10); // Z should be ignored

        var point2D = plane.TransformToPlane(point3D);

        // Point should be projected onto the XY plane
        var backToPlane = plane.TransformFromPlane(point2D);
        Assert.Equal(3, backToPlane.X, Tolerance);
        Assert.Equal(4, backToPlane.Y, Tolerance);
        Assert.Equal(0, backToPlane.Z, Tolerance);
    }

    [Fact]
    public void TransformToPlane_Vector_XYPlane_ReturnsCorrectCoordinates()
    {
        var plane = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var vector3D = new Vector3D(5, 7, 0); // Vector in the plane

        var vector2D = plane.TransformToPlane(vector3D);

        var backToPlane = plane.TransformFromPlane(vector2D);
        Assert.Equal(5, backToPlane.X, Tolerance);
        Assert.Equal(7, backToPlane.Y, Tolerance);
        Assert.Equal(0, backToPlane.Z, Tolerance);
    }

    [Fact]
    public void TransformFromPlane_Point_ReturnsPointOnPlane()
    {
        var plane = new Plane3D(new Point3D(10, 20, 30), new Vector3D(0, 0, 1));
        var point2D = new Point2D(5, 7);

        var point3D = plane.TransformFromPlane(point2D);

        Assert.True(plane.ContainsPoint(point3D, Tolerance));
    }

    [Fact]
    public void TransformFromPlane_Vector_ReturnsVectorInPlane()
    {
        var plane = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var vector2D = new Vector2D(3, 4);

        var vector3D = plane.TransformFromPlane(vector2D);

        Assert.Equal(0, vector3D.Dot(plane.Normal), Tolerance);
        Assert.Equal(5, vector3D.Length, Tolerance); // sqrt(3^2 + 4^2)
    }

    [Fact]
    public void TransformToPlane_And_TransformFromPlane_Point_RoundTrip()
    {
        var plane = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var originalPoint = new Point3D(7, 11, 0); // Point on the plane

        var point2D = plane.TransformToPlane(originalPoint);
        var backTo3D = plane.TransformFromPlane(point2D);

        Assert.True(originalPoint.IsEqualTo(backTo3D, Tolerance));
    }

    [Fact]
    public void TransformToPlane_And_TransformFromPlane_Vector_RoundTrip()
    {
        var plane = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var originalVector = new Vector3D(13, 17, 0); // Vector in the plane

        var vector2D = plane.TransformToPlane(originalVector);
        var backTo3D = plane.TransformFromPlane(vector2D);

        Assert.True(originalVector.IsEqualTo(backTo3D, Tolerance));
    }

    [Fact]
    public void TransformToPlane_DiagonalPlane_RoundTrip()
    {
        var plane = new Plane3D(new Point3D(5, 5, 5), new Vector3D(1, 1, 1).Normalized());
        var uAxis = plane.GetUAxis();
        var vAxis = plane.GetVAxis();

        // Create a point on the plane
        var originalPoint = plane.Origin + uAxis * 3 + vAxis * 4;

        var point2D = plane.TransformToPlane(originalPoint);
        var backTo3D = plane.TransformFromPlane(point2D);

        Assert.True(originalPoint.IsEqualTo(backTo3D, Tolerance));
    }

    [Fact]
    public void TransformToPlane_Point_ReturnsExpected2DCoordinates()
    {
        var plane = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var uAxis = plane.GetUAxis();
        var vAxis = plane.GetVAxis();

        // Create point at (3u, 5v) on the plane
        var point3D = plane.Origin + uAxis * 3 + vAxis * 5;

        var point2D = plane.TransformToPlane(point3D);

        Assert.Equal(3, point2D.X, Tolerance);
        Assert.Equal(5, point2D.Y, Tolerance);
    }

    [Fact]
    public void TransformToPlane_Vector_PreservesLength()
    {
        var plane = new Plane3D(new Point3D(0, 0, 0), new Vector3D(1, 1, 1).Normalized());
        var vector3D = plane.ProjectVector(new Vector3D(10, 5, 3)); // Project to get a vector in the plane

        var vector2D = plane.TransformToPlane(vector3D);
        var backTo3D = plane.TransformFromPlane(vector2D);

        Assert.Equal(vector3D.Length, backTo3D.Length, Tolerance);
    }

    [Fact]
    public void TransformFromPlane_Point_OffsetOrigin_ReturnsCorrectPosition()
    {
        var origin = new Point3D(100, 200, 300);
        var plane = new Plane3D(origin, new Vector3D(0, 0, 1));

        var point2D = new Point2D(0, 0);
        var point3D = plane.TransformFromPlane(point2D);

        Assert.True(origin.IsEqualTo(point3D, Tolerance));
    }

    #endregion
}