namespace BasAutomation.Geometry.Tests;

public class Matrix3DTests
{
    [Fact]
    public void Identity_CreatesIdentityMatrix()
    {
        var m = Matrix3D.Identity;
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                Assert.Equal(i == j ? 1.0 : 0.0, m[i, j]);
            }
        }
    }

    [Fact]
    public void Translation_CreatesCorrectMatrix()
    {
        var m = Matrix3D.Translation(5.0, 10.0, 15.0);
        Assert.Equal(5.0, m[0, 3]);
        Assert.Equal(10.0, m[1, 3]);
        Assert.Equal(15.0, m[2, 3]);
    }

    [Fact]
    public void Scale_CreatesCorrectMatrix()
    {
        var m = Matrix3D.Scale(2.0, 3.0, 4.0);
        Assert.Equal(2.0, m[0, 0]);
        Assert.Equal(3.0, m[1, 1]);
        Assert.Equal(4.0, m[2, 2]);
    }

    [Fact]
    public void RotationX_CreatesCorrectMatrix()
    {
        var m = Matrix3D.RotationX(Math.PI / 2);
        Assert.Equal(1.0, m[0, 0]);
        Assert.Equal(0.0, m[1, 1], 10);
        Assert.Equal(-1.0, m[1, 2], 10);
        Assert.Equal(1.0, m[2, 1], 10);
        Assert.Equal(0.0, m[2, 2], 10);
    }

    [Fact]
    public void RotationY_CreatesCorrectMatrix()
    {
        var m = Matrix3D.RotationY(Math.PI / 2);
        Assert.Equal(0.0, m[0, 0], 10);
        Assert.Equal(1.0, m[0, 2], 10);
        Assert.Equal(1.0, m[1, 1]);
        Assert.Equal(-1.0, m[2, 0], 10);
        Assert.Equal(0.0, m[2, 2], 10);
    }

    [Fact]
    public void RotationZ_CreatesCorrectMatrix()
    {
        var m = Matrix3D.RotationZ(Math.PI / 2);
        Assert.Equal(0.0, m[0, 0], 10);
        Assert.Equal(-1.0, m[0, 1], 10);
        Assert.Equal(1.0, m[1, 0], 10);
        Assert.Equal(0.0, m[1, 1], 10);
        Assert.Equal(1.0, m[2, 2]);
    }

    [Fact]
    public void Multiply_Matrices_CalculatesCorrectly()
    {
        var m1 = Matrix3D.Translation(5.0, 0.0, 0.0);
        var m2 = Matrix3D.Scale(2.0, 2.0, 2.0);
        var result = m1 * m2;

        var p = new Point3D(1.0, 1.0, 1.0);
        var transformed = result.Transform(p);
        Assert.Equal(7.0, transformed.X, 10);
        Assert.Equal(2.0, transformed.Y, 10);
        Assert.Equal(2.0, transformed.Z, 10);
    }

    [Fact]
    public void TransformPoint_AppliesCorrectly()
    {
        var m = Matrix3D.Translation(3.0, 4.0, 5.0);
        var p = new Point3D(1.0, 2.0, 3.0);
        var result = m.Transform(p);
        Assert.Equal(4.0, result.X);
        Assert.Equal(6.0, result.Y);
        Assert.Equal(8.0, result.Z);
    }

    [Fact]
    public void TransformVector_AppliesCorrectly()
    {
        var m = Matrix3D.Scale(2.0, 3.0, 4.0);
        var v = new Vector3D(1.0, 2.0, 3.0);
        var result = m.Transform(v);
        Assert.Equal(2.0, result.X);
        Assert.Equal(6.0, result.Y);
        Assert.Equal(12.0, result.Z);
    }

    [Fact]
    public void GetTranslation_ReturnsCorrectTranslation()
    {
        var matrix = Matrix3D.Translation(10, 20, 30);
        var translation = matrix.GetTranslation();

        Assert.Equal(10, translation.X);
        Assert.Equal(20, translation.Y);
        Assert.Equal(30, translation.Z);
    }

    [Fact]
    public void SetTranslation_WithPoint_SetsCorrectTranslation()
    {
        var matrix = Matrix3D.Identity;
        var result = matrix.SetTranslation(new Point3D(5, 10, 15));

        var translation = result.GetTranslation();
        Assert.Equal(5, translation.X);
        Assert.Equal(10, translation.Y);
        Assert.Equal(15, translation.Z);
    }

    [Fact]
    public void SetTranslation_WithCoordinates_PreservesRotation()
    {
        var matrix = Matrix3D.RotationZ(Math.PI / 4);
        var result = matrix.SetTranslation(3, 4, 5);

        var translation = result.GetTranslation();
        Assert.Equal(3, translation.X);
        Assert.Equal(4, translation.Y);
        Assert.Equal(5, translation.Z);

        // Check rotation is preserved
        Assert.True(result.XAxis.Length > 0.99 && result.XAxis.Length < 1.01);
    }

    [Fact]
    public void XAxis_YAxis_ZAxis_ExtractCorrectAxes()
    {
        var matrix = Matrix3D.Identity;

        Assert.Equal(new Vector3D(1, 0, 0), matrix.XAxis);
        Assert.Equal(new Vector3D(0, 1, 0), matrix.YAxis);
        Assert.Equal(new Vector3D(0, 0, 1), matrix.ZAxis);
    }

    [Fact]
    public void SetAxes_SetsCorrectAxes()
    {
        var matrix = Matrix3D.Identity;
        var xAxis = new Vector3D(0, 1, 0);
        var yAxis = new Vector3D(0, 0, 1);
        var zAxis = new Vector3D(1, 0, 0);

        var result = matrix.SetAxes(xAxis, yAxis, zAxis);

        Assert.Equal(xAxis, result.XAxis);
        Assert.Equal(yAxis, result.YAxis);
        Assert.Equal(zAxis, result.ZAxis);
    }

    [Fact]
    public void Determinant_IdentityMatrix_ReturnsOne()
    {
        var matrix = Matrix3D.Identity;
        Assert.Equal(1, matrix.Determinant, Constants.Tolerance);
    }

    [Fact]
    public void Determinant_ScaleMatrix_ReturnsProduct()
    {
        var matrix = Matrix3D.Scale(2, 3, 4);
        Assert.Equal(24, matrix.Determinant, Constants.Tolerance);
    }

    [Fact]
    public void Inverse_IdentityMatrix_ReturnsIdentity()
    {
        var matrix = Matrix3D.Identity;
        var inverse = matrix.Inverse();

        Assert.True(inverse.IsEqualTo(Matrix3D.Identity, Constants.Tolerance));
    }

    [Fact]
    public void Inverse_TranslationMatrix_ReturnsNegativeTranslation()
    {
        var matrix = Matrix3D.Translation(10, 20, 30);
        var inverse = matrix.Inverse();

        var point = new Point3D(5, 5, 5);
        var transformed = matrix.Transform(point);
        var backToOriginal = inverse.Transform(transformed);

        Assert.True(backToOriginal.IsEqualTo(point, Constants.Tolerance));
    }

    [Fact]
    public void Inverse_RotationMatrix_ReturnsTranspose()
    {
        var matrix = Matrix3D.RotationX(Math.PI / 4);
        var inverse = matrix.Inverse();

        var product = matrix * inverse;
        Assert.True(product.IsIdentity);
    }

    [Fact]
    public void Inverse_SingularMatrix_ThrowsException()
    {
        var matrix = new Matrix3D(
            1, 2, 3, 0,
            4, 5, 6, 0,
            7, 8, 9, 0,
            0, 0, 0, 1
        );

        Assert.Throws<InvalidOperationException>(() => matrix.Inverse());
    }

    [Fact]
    public void CreateRotationAroundAxis_90DegreesZ_RotatesCorrectly()
    {
        var axis = new Vector3D(0, 0, 1);
        var angle = Math.PI / 2;

        var matrix = Matrix3D.CreateRotationAroundAxis(axis, angle);
        var point = new Point3D(1, 0, 0);
        var rotated = matrix.Transform(point);

        Assert.Equal(0, rotated.X, Constants.Tolerance);
        Assert.Equal(1, rotated.Y, Constants.Tolerance);
        Assert.Equal(0, rotated.Z, Constants.Tolerance);
    }

    [Fact]
    public void CreateRotationAroundPoint_RotatesCorrectly()
    {
        var axis = new Vector3D(0, 0, 1);
        var angle = Math.PI / 2;
        var center = new Point3D(1, 1, 0);

        var matrix = Matrix3D.CreateRotationAroundPoint(axis, angle, center);
        var point = new Point3D(2, 1, 0);
        var rotated = matrix.Transform(point);

        Assert.Equal(1, rotated.X, Constants.Tolerance);
        Assert.Equal(2, rotated.Y, Constants.Tolerance);
        Assert.Equal(0, rotated.Z, Constants.Tolerance);
    }

    [Fact]
    public void CreateRotationFromTo_AlignedVectors_ReturnsIdentity()
    {
        var from = new Vector3D(1, 0, 0);
        var to = new Vector3D(2, 0, 0);

        var matrix = Matrix3D.CreateRotationFromTo(from, to);

        Assert.True(matrix.IsIdentity);
    }

    [Fact]
    public void CreateRotationFromTo_PerpendicularVectors_Rotates90()
    {
        var from = new Vector3D(1, 0, 0);
        var to = new Vector3D(0, 1, 0);

        var matrix = Matrix3D.CreateRotationFromTo(from, to);
        var rotated = matrix.Transform(from);

        Assert.True(rotated.IsEqualTo(to, Constants.Tolerance));
    }

    [Fact]
    public void CreateRotationFromTo_OppositeVectors_Rotates180()
    {
        var from = new Vector3D(1, 0, 0);
        var to = new Vector3D(-1, 0, 0);

        var matrix = Matrix3D.CreateRotationFromTo(from, to);
        var rotated = matrix.Transform(from);

        Assert.True(rotated.IsEqualTo(to, Constants.Tolerance));
    }

    [Fact]
    public void CreateRotationFromTo_ZAxisToYAxis_MapsZOntoY()
    {
        // Local +Z must land on world +Y.
        var matrix = Matrix3D.CreateRotationFromTo(Vector3D.ZAxis, Vector3D.YAxis);

        Assert.True(matrix.Transform(Vector3D.ZAxis).IsEqualTo(Vector3D.YAxis, Constants.Tolerance));
        Assert.True(matrix.IsRotationMatrix());
    }

    [Fact]
    public void CreateRotationFromTo_YAxisToZAxis_MapsYOntoZ()
    {
        // Inverse direction also used by Path3D / planar-frame builders.
        var matrix = Matrix3D.CreateRotationFromTo(Vector3D.YAxis, Vector3D.ZAxis);

        Assert.True(matrix.Transform(Vector3D.YAxis).IsEqualTo(Vector3D.ZAxis, Constants.Tolerance));
        Assert.True(matrix.IsRotationMatrix());
    }

    [Fact]
    public void CreateRotationFromTo_AntiParallelAlongZ_Rotates180()
    {
        // Anti-parallel branch for the Z axis (today only X is exercised). The branch
        // picks a perpendicular axis and rotates 180° — must produce a finite valid rotation.
        var matrix = Matrix3D.CreateRotationFromTo(Vector3D.ZAxis, -Vector3D.ZAxis);

        var rotated = matrix.Transform(Vector3D.ZAxis);
        Assert.True(rotated.IsEqualTo(-Vector3D.ZAxis, Constants.Tolerance));
        Assert.False(double.IsNaN(rotated.X));
        Assert.False(double.IsNaN(rotated.Y));
        Assert.False(double.IsNaN(rotated.Z));
        Assert.True(matrix.IsRotationMatrix());
    }

    [Fact]
    public void CreateRotationFromTo_ZeroVectorInput_DocumentsCurrentBehaviour()
    {
        // Pins the current zero-input behaviour so any future hardening (e.g. returning
        // Identity on a zero-length input) is an intentional change. With from = (0,0,0)
        // and to = Z, the implementation reaches the general branch with a zero axis,
        // producing a non-rotation matrix (rotation block ≈ 0). Kept as a regression anchor.
        var matrix = Matrix3D.CreateRotationFromTo(Vector3D.Zero, Vector3D.ZAxis);

        Assert.False(matrix.IsRotationMatrix());
        Assert.False(double.IsNaN(matrix[0, 0]));
        Assert.False(double.IsNaN(matrix[1, 1]));
        Assert.False(double.IsNaN(matrix[2, 2]));
    }

    [Fact]
    public void CreateReflection_AcrossXYPlane_ReflectsCorrectly()
    {
        var plane = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var matrix = Matrix3D.CreateReflection(plane);

        var point = new Point3D(1, 2, 3);
        var reflected = matrix.Transform(point);

        Assert.Equal(1, reflected.X, Constants.Tolerance);
        Assert.Equal(2, reflected.Y, Constants.Tolerance);
        Assert.Equal(-3, reflected.Z, Constants.Tolerance);
    }

    [Fact]
    public void OrthonormalizeRotation_NonOrthogonalMatrix_Orthonormalizes()
    {
        // Create slightly non-orthogonal matrix
        var matrix = new Matrix3D(
            1, 0.1, 0, 0,
            0, 1, 0.1, 0,
            0, 0, 1, 0,
            0, 0, 0, 1
        );

        var normalized = matrix.OrthonormalizeRotation();

        // Check axes are unit length
        Assert.Equal(1, normalized.XAxis.Length, Constants.Tolerance);
        Assert.Equal(1, normalized.YAxis.Length, Constants.Tolerance);
        Assert.Equal(1, normalized.ZAxis.Length, Constants.Tolerance);

        // Check axes are orthogonal
        Assert.Equal(0, normalized.XAxis.Dot(normalized.YAxis), Constants.Tolerance);
        Assert.Equal(0, normalized.YAxis.Dot(normalized.ZAxis), Constants.Tolerance);
        Assert.Equal(0, normalized.ZAxis.Dot(normalized.XAxis), Constants.Tolerance);
    }

    [Fact]
    public void IsRotationMatrix_ValidRotation_ReturnsTrue()
    {
        var matrix = Matrix3D.RotationY(Math.PI / 3);
        Assert.True(matrix.IsRotationMatrix());
    }

    [Fact]
    public void IsRotationMatrix_ScaleMatrix_ReturnsFalse()
    {
        var matrix = Matrix3D.Scale(2, 2, 2);
        Assert.False(matrix.IsRotationMatrix());
    }

    [Fact]
    public void Decompose_SimpleTranslation_ExtractsCorrectly()
    {
        var matrix = Matrix3D.Translation(5, 10, 15);

        matrix.Decompose(out var translation, out var scale, out var rotation);

        Assert.Equal(5, translation.X, Constants.Tolerance);
        Assert.Equal(10, translation.Y, Constants.Tolerance);
        Assert.Equal(15, translation.Z, Constants.Tolerance);
        Assert.Equal(1, scale.X, Constants.Tolerance);
        Assert.Equal(1, scale.Y, Constants.Tolerance);
        Assert.Equal(1, scale.Z, Constants.Tolerance);
        Assert.True(rotation.IsIdentity);
    }

    [Fact]
    public void Decompose_TranslationRotationScale_ExtractsCorrectly()
    {
        var t = Matrix3D.Translation(5, 10, 15);
        var r = Matrix3D.RotationZ(Math.PI / 6);
        var s = Matrix3D.Scale(2, 3, 4);
        var matrix = t * r * s;

        matrix.Decompose(out var translation, out var scale, out var rotation);

        Assert.Equal(5, translation.X, Constants.Tolerance);
        Assert.Equal(10, translation.Y, Constants.Tolerance);
        Assert.Equal(15, translation.Z, Constants.Tolerance);
        Assert.Equal(2, scale.X, Constants.Tolerance);
        Assert.Equal(3, scale.Y, Constants.Tolerance);
        Assert.Equal(4, scale.Z, Constants.Tolerance);
        Assert.True(rotation.IsRotationMatrix());
    }

    [Fact]
    public void IsIdentity_IdentityMatrix_ReturnsTrue()
    {
        var matrix = Matrix3D.Identity;
        Assert.True(matrix.IsIdentity);
    }

    [Fact]
    public void IsIdentity_NonIdentityMatrix_ReturnsFalse()
    {
        var matrix = Matrix3D.Translation(1, 0, 0);
        Assert.False(matrix.IsIdentity);
    }
}