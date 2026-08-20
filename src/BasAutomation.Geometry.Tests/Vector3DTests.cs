namespace BasAutomation.Geometry.Tests;

public class Vector3DTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var v = new Vector3D(3.0, 4.0, 5.0);
        Assert.Equal(3.0, v.X);
        Assert.Equal(4.0, v.Y);
        Assert.Equal(5.0, v.Z);
    }

    [Fact]
    public void Length_CalculatesCorrectly()
    {
        var v = new Vector3D(2.0, 3.0, 6.0);
        Assert.Equal(7.0, v.Length);
    }

    [Fact]
    public void LengthSquared_CalculatesCorrectly()
    {
        var v = new Vector3D(2.0, 3.0, 6.0);
        Assert.Equal(49.0, v.LengthSquared);
    }

    [Fact]
    public void Normalized_ReturnsUnitVector()
    {
        var v = new Vector3D(3.0, 0.0, 4.0);
        var normalized = v.Normalized();
        Assert.Equal(1.0, normalized.Length, 10);
        Assert.Equal(0.6, normalized.X, 10);
        Assert.Equal(0.0, normalized.Y, 10);
        Assert.Equal(0.8, normalized.Z, 10);
    }

    [Fact]
    public void Cross_CalculatesCorrectly()
    {
        var v1 = new Vector3D(1.0, 0.0, 0.0);
        var v2 = new Vector3D(0.0, 1.0, 0.0);
        var cross = v1.Cross(v2);
        Assert.Equal(0.0, cross.X);
        Assert.Equal(0.0, cross.Y);
        Assert.Equal(1.0, cross.Z);
    }

    [Fact]
    public void Dot_CalculatesCorrectly()
    {
        var v1 = new Vector3D(2.0, 3.0, 4.0);
        var v2 = new Vector3D(5.0, 6.0, 7.0);
        Assert.Equal(56.0, v1.Dot(v2));
    }

    [Fact]
    public void AngleTo_CalculatesCorrectly()
    {
        var v1 = new Vector3D(1.0, 0.0, 0.0);
        var v2 = new Vector3D(0.0, 1.0, 0.0);
        Assert.Equal(Math.PI / 2, v1.AngleTo(v2), 10);
    }
    [Fact]
    public void IsParallel_ParallelVectors_ReturnsTrue()
    {
        var v1 = new Vector3D(2, 4, 6);
        var v2 = new Vector3D(1, 2, 3);
        Assert.True(v1.IsParallel(v2));
    }

    [Fact]
    public void IsParallel_NonParallelVectors_ReturnsFalse()
    {
        var v1 = new Vector3D(1, 0, 0);
        var v2 = new Vector3D(0, 1, 0);
        Assert.False(v1.IsParallel(v2));
    }

    [Fact]
    public void IsPerpendicular_PerpendicularVectors_ReturnsTrue()
    {
        var v1 = new Vector3D(1, 0, 0);
        var v2 = new Vector3D(0, 1, 0);
        Assert.True(v1.IsPerpendicular(v2));
    }

    [Fact]
    public void IsPerpendicular_NonPerpendicularVectors_ReturnsFalse()
    {
        var v1 = new Vector3D(1, 0, 0);
        var v2 = new Vector3D(1, 1, 0);
        Assert.False(v1.IsPerpendicular(v2));
    }

    [Fact]
    public void IsSameDirection_AlreadyDefinedInStruct_WorksCorrectly()
    {
        var v1 = new Vector3D(2, 4, 6);
        var v2 = new Vector3D(1, 2, 3);
        Assert.True(v1.IsSameDirection(v2));
    }

    [Fact]
    public void IsSameDirection_OppositeVectors_ReturnsFalse()
    {
        var v1 = new Vector3D(2, 4, 6);
        var v2 = new Vector3D(-1, -2, -3);
        Assert.False(v1.IsSameDirection(v2));
    }

    [Fact]
    public void IsOppositeDirection_OppositeVectors_ReturnsTrue()
    {
        var v1 = new Vector3D(1, 2, 3);
        var v2 = new Vector3D(-2, -4, -6);
        Assert.True(v1.IsOppositeDirection(v2));
    }

    [Fact]
    public void RotateAroundAxis_90DegreesAroundZ_RotatesCorrectly()
    {
        var v = new Vector3D(1, 0, 0);
        var axis = new Vector3D(0, 0, 1);
        var rotated = v.RotateAroundAxis(axis, Math.PI / 2);
        Assert.Equal(0, rotated.X, 10);
        Assert.Equal(1, rotated.Y, 10);
        Assert.Equal(0, rotated.Z, 10);
    }

    [Fact]
    public void RotateAroundAxis_180DegreesAroundY_RotatesCorrectly()
    {
        var v = new Vector3D(1, 0, 1);
        var axis = new Vector3D(0, 1, 0);
        var rotated = v.RotateAroundAxis(axis, Math.PI);
        Assert.Equal(-1, rotated.X, 10);
        Assert.Equal(0, rotated.Y, 10);
        Assert.Equal(-1, rotated.Z, 10);
    }

    [Fact]
    public void ProjectPointOntoLine_ProjectsCorrectly()
    {
        var lineDirection = new Vector3D(1, 0, 0);
        var linePoint = new Point3D(0, 0, 0);
        var pointToProject = new Point3D(5, 3, 2);

        var projected = lineDirection.ProjectPointOntoLine(linePoint, pointToProject);

        Assert.Equal(5, projected.X);
        Assert.Equal(0, projected.Y);
        Assert.Equal(0, projected.Z);
    }

    [Fact]
    public void ProjectPointOntoLine_DiagonalLine_ProjectsCorrectly()
    {
        var lineDirection = new Vector3D(1, 1, 1);
        var linePoint = new Point3D(0, 0, 0);
        var pointToProject = new Point3D(3, 0, 0);

        var projected = lineDirection.ProjectPointOntoLine(linePoint, pointToProject);

        // Point should project to (1, 1, 1)
        Assert.Equal(1, projected.X, 10);
        Assert.Equal(1, projected.Y, 10);
        Assert.Equal(1, projected.Z, 10);
    }
}