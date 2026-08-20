namespace BasAutomation.Geometry.Tests;

public class Vector2DTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var v = new Vector2D(3.0, 4.0);
        Assert.Equal(3.0, v.X);
        Assert.Equal(4.0, v.Y);
    }

    [Fact]
    public void Length_CalculatesCorrectly()
    {
        var v = new Vector2D(3.0, 4.0);
        Assert.Equal(5.0, v.Length);
    }

    [Fact]
    public void LengthSquared_CalculatesCorrectly()
    {
        var v = new Vector2D(3.0, 4.0);
        Assert.Equal(25.0, v.LengthSquared);
    }

    [Fact]
    public void Normalized_ReturnsUnitVector()
    {
        var v = new Vector2D(3.0, 4.0);
        var normalized = v.Normalized();
        Assert.Equal(1.0, normalized.Length, 10);
        Assert.Equal(0.6, normalized.X, 10);
        Assert.Equal(0.8, normalized.Y, 10);
    }

    [Fact]
    public void Normalized_ZeroVector_ReturnsZero()
    {
        var v = new Vector2D(0.0, 0.0);
        var normalized = v.Normalized();
        Assert.Equal(0.0, normalized.X);
        Assert.Equal(0.0, normalized.Y);
    }

    [Fact]
    public void Add_ReturnsCorrectVector()
    {
        var v1 = new Vector2D(1.0, 2.0);
        var v2 = new Vector2D(3.0, 4.0);
        var result = v1 + v2;
        Assert.Equal(4.0, result.X);
        Assert.Equal(6.0, result.Y);
    }

    [Fact]
    public void Subtract_ReturnsCorrectVector()
    {
        var v1 = new Vector2D(5.0, 7.0);
        var v2 = new Vector2D(2.0, 3.0);
        var result = v1 - v2;
        Assert.Equal(3.0, result.X);
        Assert.Equal(4.0, result.Y);
    }

    [Fact]
    public void MultiplyScalar_ReturnsCorrectVector()
    {
        var v = new Vector2D(2.0, 3.0);
        var result1 = v * 2.0;
        var result2 = 2.0 * v;
        Assert.Equal(4.0, result1.X);
        Assert.Equal(6.0, result1.Y);
        Assert.Equal(result1, result2);
    }

    [Fact]
    public void DivideScalar_ReturnsCorrectVector()
    {
        var v = new Vector2D(4.0, 6.0);
        var result = v / 2.0;
        Assert.Equal(2.0, result.X);
        Assert.Equal(3.0, result.Y);
    }

    [Fact]
    public void Negate_ReturnsCorrectVector()
    {
        var v = new Vector2D(2.0, -3.0);
        var result = -v;
        Assert.Equal(-2.0, result.X);
        Assert.Equal(3.0, result.Y);
    }

    [Fact]
    public void Dot_CalculatesCorrectly()
    {
        var v1 = new Vector2D(2.0, 3.0);
        var v2 = new Vector2D(4.0, 5.0);
        Assert.Equal(23.0, v1.Dot(v2));
    }

    [Fact]
    public void Cross_CalculatesCorrectly()
    {
        var v1 = new Vector2D(2.0, 3.0);
        var v2 = new Vector2D(4.0, 5.0);
        Assert.Equal(-2.0, v1.Cross(v2));
    }

    [Fact]
    public void AngleTo_CalculatesCorrectly()
    {
        var v1 = new Vector2D(1.0, 0.0);
        var v2 = new Vector2D(0.0, 1.0);
        Assert.Equal(Math.PI / 2, v1.AngleTo(v2), 10);
    }

    [Fact]
    public void IsParallel_ParallelVectors_ReturnsTrue()
    {
        var v1 = new Vector2D(2, 4);
        var v2 = new Vector2D(1, 2);
        Assert.True(v1.IsParallel(v2));
    }

    [Fact]
    public void IsParallel_NonParallelVectors_ReturnsFalse()
    {
        var v1 = new Vector2D(1, 0);
        var v2 = new Vector2D(0, 1);
        Assert.False(v1.IsParallel(v2));
    }

    [Fact]
    public void IsPerpendicular_PerpendicularVectors_ReturnsTrue()
    {
        var v1 = new Vector2D(1, 0);
        var v2 = new Vector2D(0, 1);
        Assert.True(v1.IsPerpendicular(v2));
    }

    [Fact]
    public void IsPerpendicular_NonPerpendicularVectors_ReturnsFalse()
    {
        var v1 = new Vector2D(1, 0);
        var v2 = new Vector2D(1, 1);
        Assert.False(v1.IsPerpendicular(v2));
    }

    [Fact]
    public void IsSameDirection_SameDirectionVectors_ReturnsTrue()
    {
        var v1 = new Vector2D(2, 4);
        var v2 = new Vector2D(1, 2);
        Assert.True(v1.IsSameDirection(v2));
    }

    [Fact]
    public void IsSameDirection_OppositeDirectionVectors_ReturnsFalse()
    {
        var v1 = new Vector2D(1, 2);
        var v2 = new Vector2D(-1, -2);
        Assert.False(v1.IsSameDirection(v2));
    }

    [Fact]
    public void IsOppositeDirection_OppositeVectors_ReturnsTrue()
    {
        var v1 = new Vector2D(1, 2);
        var v2 = new Vector2D(-2, -4);
        Assert.True(v1.IsOppositeDirection(v2));
    }

    [Fact]
    public void GetPerpendicular_ReturnsCorrectPerpendicularVector()
    {
        var v = new Vector2D(3, 4);
        var perp = v.GetPerpendicular();
        Assert.Equal(-4, perp.X);
        Assert.Equal(3, perp.Y);
        Assert.True(v.IsPerpendicular(perp));
    }

    [Fact]
    public void Rotate_90Degrees_ReturnsRotatedVector()
    {
        var v = new Vector2D(1, 0);
        var rotated = v.Rotate(Math.PI / 2);
        Assert.Equal(0, rotated.X, 10);
        Assert.Equal(1, rotated.Y, 10);
    }

    [Fact]
    public void Rotate_180Degrees_ReturnsOppositeVector()
    {
        var v = new Vector2D(1, 2);
        var rotated = v.Rotate(Math.PI);
        Assert.Equal(-1, rotated.X, 10);
        Assert.Equal(-2, rotated.Y, 10);
    }
}