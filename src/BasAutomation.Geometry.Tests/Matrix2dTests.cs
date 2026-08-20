namespace BasAutomation.Geometry.Tests;

public class Matrix2DTests
{
    [Fact]
    public void Identity_CreatesIdentityMatrix()
    {
        var m = Matrix2D.Identity;
        Assert.Equal(1.0, m[0, 0]);
        Assert.Equal(0.0, m[0, 1]);
        Assert.Equal(0.0, m[0, 2]);
        Assert.Equal(0.0, m[1, 0]);
        Assert.Equal(1.0, m[1, 1]);
        Assert.Equal(0.0, m[1, 2]);
        Assert.Equal(0.0, m[2, 0]);
        Assert.Equal(0.0, m[2, 1]);
        Assert.Equal(1.0, m[2, 2]);
    }

    [Fact]
    public void Translation_CreatesCorrectMatrix()
    {
        var m = Matrix2D.Translation(5.0, 10.0);
        Assert.Equal(5.0, m[0, 2]);
        Assert.Equal(10.0, m[1, 2]);
    }

    [Fact]
    public void Scale_CreatesCorrectMatrix()
    {
        var m = Matrix2D.Scale(2.0, 3.0);
        Assert.Equal(2.0, m[0, 0]);
        Assert.Equal(3.0, m[1, 1]);
    }

    [Fact]
    public void Rotation_CreatesCorrectMatrix()
    {
        var m = Matrix2D.Rotation(Math.PI / 2);
        Assert.Equal(0.0, m[0, 0], 10);
        Assert.Equal(-1.0, m[0, 1], 10);
        Assert.Equal(1.0, m[1, 0], 10);
        Assert.Equal(0.0, m[1, 1], 10);
    }

    [Fact]
    public void Multiply_Matrices_CalculatesCorrectly()
    {
        var m1 = Matrix2D.Translation(5.0, 0.0);
        var m2 = Matrix2D.Scale(2.0, 2.0);
        var result = m1 * m2;

        var p = new Point2D(1.0, 1.0);
        var transformed = result.Transform(p);
        Assert.Equal(7.0, transformed.X, 10);
        Assert.Equal(2.0, transformed.Y, 10);
    }

    [Fact]
    public void TransformPoint_AppliesCorrectly()
    {
        var m = Matrix2D.Translation(3.0, 4.0);
        var p = new Point2D(1.0, 2.0);
        var result = m.Transform(p);
        Assert.Equal(4.0, result.X);
        Assert.Equal(6.0, result.Y);
    }

    [Fact]
    public void TransformVector_AppliesCorrectly()
    {
        var m = Matrix2D.Scale(2.0, 3.0);
        var v = new Vector2D(1.0, 2.0);
        var result = m.Transform(v);
        Assert.Equal(2.0, result.X);
        Assert.Equal(6.0, result.Y);
    }

    [Fact]
    public void IsEqualTo_WithinTolerance_ReturnsTrue()
    {
        var m1 = Matrix2D.Identity;
        var m2 = Matrix2D.Identity.WithElement(0, 0, 1.00001);
        Assert.True(m1.IsEqualTo(m2, 0.001));
    }

    [Fact]
    public void IsEqualTo_OutsideTolerance_ReturnsFalse()
    {
        var m1 = Matrix2D.Identity;
        var m2 = Matrix2D.Identity.WithElement(0, 0, 1.1);
        Assert.False(m1.IsEqualTo(m2, 0.001));
    }

    // Alternative approach - create matrices directly with different values
    [Fact]
    public void IsEqualTo_WithinTolerance_ReturnsTrue_Alternative()
    {
        var m1 = Matrix2D.Identity;
        var m2 = new Matrix2D(
            1.00001, 0, 0,  // Slightly different from identity
            0, 1, 0,
            0, 0, 1
        );
        Assert.True(m1.IsEqualTo(m2, 0.001));
    }

    [Fact]
    public void IsEqualTo_OutsideTolerance_ReturnsFalse_Alternative()
    {
        var m1 = Matrix2D.Identity;
        var m2 = new Matrix2D(
            1.1, 0, 0,  // Significantly different from identity
            0, 1, 0,
            0, 0, 1
        );
        Assert.False(m1.IsEqualTo(m2, 0.001));
    }

    [Fact]
    public void Translation_ReturnsCorrectTranslation()
    {
        var matrix = Matrix2D.Translation(10, 20);
        var translation = matrix.GetTranslation();

        Assert.Equal(10, translation.X);
        Assert.Equal(20, translation.Y);
    }

    [Fact]
    public void SetTranslation_WithPoint_SetsCorrectTranslation()
    {
        var matrix = Matrix2D.Identity;
        var result = matrix.SetTranslation(new Point2D(5, 10));

        Assert.Equal(5, result.GetTranslation().X);
        Assert.Equal(10, result.GetTranslation().Y);
    }

    [Fact]
    public void SetTranslation_WithCoordinates_SetsCorrectTranslation()
    {
        var matrix = Matrix2D.Rotation(Math.PI / 4);
        var result = matrix.SetTranslation(3, 4);

        Assert.Equal(3, result.GetTranslation().X);
        Assert.Equal(4, result.GetTranslation().Y);

        // Rotation should be preserved
        Assert.True(result.XAxis.Length > 0.99 && result.XAxis.Length < 1.01);
    }

    [Fact]
    public void XAxis_YAxis_ExtractCorrectAxes()
    {
        var matrix = Matrix2D.Identity;

        Assert.Equal(new Vector2D(1, 0), matrix.XAxis);
        Assert.Equal(new Vector2D(0, 1), matrix.YAxis);
    }

    [Fact]
    public void SetAxes_SetsCorrectAxes()
    {
        var matrix = Matrix2D.Identity;
        var xAxis = new Vector2D(0, 1);  // Rotated 90 degrees
        var yAxis = new Vector2D(-1, 0);

        var result = matrix.SetAxes(xAxis, yAxis);

        Assert.Equal(xAxis, result.XAxis);
        Assert.Equal(yAxis, result.YAxis);
    }

    [Fact]
    public void Determinant_IdentityMatrix_ReturnsOne()
    {
        var matrix = Matrix2D.Identity;
        Assert.Equal(1, matrix.Determinant, Constants.Tolerance);
    }

    [Fact]
    public void Determinant_ScaleMatrix_ReturnsProduct()
    {
        var matrix = Matrix2D.Scale(2, 3);
        Assert.Equal(6, matrix.Determinant, Constants.Tolerance);
    }

    [Fact]
    public void Determinant_SingularMatrix_ReturnsZero()
    {
        var matrix = new Matrix2D(
            1, 2, 0,
            2, 4, 0,
            0, 0, 1
        );
        Assert.Equal(0, matrix.Determinant, Constants.Tolerance);
    }

    [Fact]
    public void Inverse_IdentityMatrix_ReturnsIdentity()
    {
        var matrix = Matrix2D.Identity;
        var inverse = matrix.Inverse();

        Assert.True(inverse.IsEqualTo(Matrix2D.Identity, Constants.Tolerance));
    }

    [Fact]
    public void Inverse_TranslationMatrix_ReturnsNegativeTranslation()
    {
        var matrix = Matrix2D.Translation(10, 20);
        var inverse = matrix.Inverse();

        var point = new Point2D(5, 5);
        var transformed = matrix.Transform(point);
        var backToOriginal = inverse.Transform(transformed);

        Assert.True(backToOriginal.IsEqualTo(point, Constants.Tolerance));
    }

    [Fact]
    public void Inverse_RotationMatrix_ReturnsTranspose()
    {
        var matrix = Matrix2D.Rotation(Math.PI / 4);
        var inverse = matrix.Inverse();

        var product = matrix * inverse;
        Assert.True(product.IsIdentity);
    }

    [Fact]
    public void Inverse_SingularMatrix_ThrowsException()
    {
        var matrix = new Matrix2D(
            1, 2, 0,
            2, 4, 0,
            0, 0, 1
        );

        Assert.Throws<InvalidOperationException>(() => matrix.Inverse());
    }

    [Fact]
    public void CreateRotationFromTo_AlignedVectors_ReturnsIdentity()
    {
        var from = new Vector2D(1, 0);
        var to = new Vector2D(2, 0);

        var matrix = Matrix2D.CreateRotationFromTo(from, to);

        Assert.True(matrix.IsIdentity);
    }

    [Fact]
    public void CreateRotationFromTo_Perpendicular_Rotates90()
    {
        var from = new Vector2D(1, 0);
        var to = new Vector2D(0, 1);

        var matrix = Matrix2D.CreateRotationFromTo(from, to);
        var rotated = matrix.Transform(from);

        Assert.True(rotated.IsEqualTo(to, Constants.Tolerance));
    }

    [Fact]
    public void CreateRotationFromTo_OppositeVectors_Rotates180()
    {
        var from = new Vector2D(1, 0);
        var to = new Vector2D(-1, 0);

        var matrix = Matrix2D.CreateRotationFromTo(from, to);
        var rotated = matrix.Transform(from);

        Assert.True(rotated.IsEqualTo(to, Constants.Tolerance));
    }

    [Fact]
    public void CreateReflection_AcrossXAxis_ReflectsCorrectly()
    {
        var normal = new Vector2D(0, 1); // Normal to X axis
        var matrix = Matrix2D.CreateReflectionFromNormal(normal);

        var point = new Vector2D(1, 2);
        var reflected = matrix.Transform(point);

        Assert.Equal(1, reflected.X, Constants.Tolerance);
        Assert.Equal(-2, reflected.Y, Constants.Tolerance);
    }

    [Fact]
    public void CreateReflection_AcrossYAxis_ReflectsCorrectly()
    {
        var normal = new Vector2D(1, 0); // Normal to Y axis
        var matrix = Matrix2D.CreateReflectionFromNormal(normal);

        var point = new Vector2D(2, 1);
        var reflected = matrix.Transform(point);

        Assert.Equal(-2, reflected.X, Constants.Tolerance);
        Assert.Equal(1, reflected.Y, Constants.Tolerance);
    }

    [Fact]
    public void Decompose_SimpleTranslation_ExtractsCorrectly()
    {
        var matrix = Matrix2D.Translation(5, 10);

        matrix.Decompose(out var translation, out var rotation, out var scale);

        Assert.Equal(5, translation.X, Constants.Tolerance);
        Assert.Equal(10, translation.Y, Constants.Tolerance);
        Assert.Equal(0, rotation, Constants.Tolerance);
        Assert.Equal(1, scale.X, Constants.Tolerance);
        Assert.Equal(1, scale.Y, Constants.Tolerance);
    }

    [Fact]
    public void Decompose_RotationThenTranslation_ExtractsCorrectly()
    {
        var angle = Math.PI / 4;
        var matrix = Matrix2D.Translation(3, 4) * Matrix2D.Rotation(angle);
        matrix.Decompose(out var translation, out var rotation, out var scale);

        Assert.Equal(3, translation.X, Constants.Tolerance);
        Assert.Equal(4, translation.Y, Constants.Tolerance);
        Assert.Equal(angle, rotation, Constants.Tolerance);
        Assert.Equal(1, scale.X, Constants.Tolerance);
        Assert.Equal(1, scale.Y, Constants.Tolerance);
    }

    [Fact]
    public void CreateRotationAroundPoint_RotatesCorrectly()
    {
        var center = new Point2D(1, 1);
        var angle = Math.PI / 2;
        var matrix = Matrix2D.CreateRotationAroundPoint(angle, center);
        var point = new Point2D(2, 1);
        var rotated = matrix.Transform(point);
        Assert.Equal(1, rotated.X, Constants.Tolerance);
        Assert.Equal(2, rotated.Y, Constants.Tolerance);
    }

    [Fact]
    public void Decompose_TranslationThenRotation_ExtractsCorrectly()
    {
        // Apply translation first, then rotation
        var angle = Math.PI / 4;
        var translationFirst = Matrix2D.Translation(3, 4);
        var rotationSecond = Matrix2D.Rotation(angle);
        var matrix = translationFirst * rotationSecond;

        matrix.Decompose(out var translation, out var rotation, out var scale);

        Assert.Equal(3, translation.X, Constants.Tolerance);
        Assert.Equal(4, translation.Y, Constants.Tolerance);
        Assert.Equal(angle, rotation, Constants.Tolerance);
        Assert.Equal(1, scale.X, Constants.Tolerance);
        Assert.Equal(1, scale.Y, Constants.Tolerance);
    }

    [Fact]
    public void Decompose_ScaleThenRotationThenTranslation_ExtractsCorrectly()
    {
        var angle = Math.PI / 6;
        var scaleFirst = Matrix2D.Scale(2, 3);
        var rotationSecond = Matrix2D.Rotation(angle);
        var translationThird = Matrix2D.Translation(5, 6);
        var matrix = translationThird * rotationSecond * scaleFirst;

        matrix.Decompose(out var translation, out var rotation, out var scale);

        Assert.Equal(5, translation.X, Constants.Tolerance);
        Assert.Equal(6, translation.Y, Constants.Tolerance);
        Assert.Equal(angle, rotation, Constants.Tolerance);
        Assert.Equal(2, scale.X, Constants.Tolerance);
        Assert.Equal(3, scale.Y, Constants.Tolerance);
    }

    [Fact]
    public void IsIdentity_IdentityMatrix_ReturnsTrue()
    {
        var matrix = Matrix2D.Identity;
        Assert.True(matrix.IsIdentity);
    }

    [Fact]
    public void IsIdentity_NonIdentityMatrix_ReturnsFalse()
    {
        var matrix = Matrix2D.Translation(1, 0);
        Assert.False(matrix.IsIdentity);
    }
}