namespace BasAutomation.Geometry.Tests;

public class ExtensionsTests
{
    [Fact]
    public void GetCentroid_CalculatesCorrectly()
    {
        var otherPoints = new List<Point3D>
        {
            new(0, 0, 0),
            new(4, 0, 0),
            new(0, 3, 0),
            new(0, 0, 6)
        };

        var centroid = otherPoints.GetCentroid();

        Assert.Equal(1, centroid.X); // (0+4+0+0)/4
        Assert.Equal(0.75, centroid.Y); // (0+0+3+0)/4
        Assert.Equal(1.5, centroid.Z); // (0+0+0+6)/4
    }

    // -------------------------------------------------------------------------
    // Point3D.Round
    // -------------------------------------------------------------------------

    [Fact]
    public void Point3D_Round_RoundsEachCoordinateIndependently()
    {
        var p = new Point3D(1.23456789, -2.98765432, 0.00049999);

        var result = p.Round(3);

        Assert.Equal(1.235, result.X);
        Assert.Equal(-2.988, result.Y);
        Assert.Equal(0.000, result.Z);
    }

    [Fact]
    public void Point3D_Round_DefaultDecimalsIs6()
    {
        var p = new Point3D(1.12345674, -2.99999995, 0.00000014);

        var result = p.Round(); // default = 6

        Assert.Equal(1.123457,  result.X);
        Assert.Equal(-3.0,      result.Y);
        Assert.Equal(0.0,       result.Z);
    }

    // -------------------------------------------------------------------------
    // Vector3D.Round
    // -------------------------------------------------------------------------

    [Fact]
    public void Vector3D_Round_RoundsEachComponentIndependently()
    {
        var v = new Vector3D(0.12345678, -0.98765432, 0.00049999);

        var result = v.Round(4);

        Assert.Equal(0.1235,  result.X);
        Assert.Equal(-0.9877, result.Y);
        Assert.Equal(0.0005,  result.Z);
    }

    [Fact]
    public void Vector3D_Round_DefaultDecimalsIs6()
    {
        var v = new Vector3D(0.71234565, 0.00000005, -0.71234564);

        var result = v.Round(); // default = 6

        Assert.Equal(0.712346,  result.X);
        Assert.Equal(0.0,       result.Y);
        Assert.Equal(-0.712346, result.Z);
    }

    // -------------------------------------------------------------------------
    // Plane3D.Round
    // -------------------------------------------------------------------------

    [Fact]
    public void Plane3D_Round_RoundsOriginCoordinates()
    {
        var plane = new Plane3D(new Point3D(10.123456789, -5.987654321, 0.000499999), Vector3D.ZAxis);

        var result = plane.Round(3);

        Assert.Equal(10.123, result.Origin.X);
        Assert.Equal(-5.988, result.Origin.Y);
        Assert.Equal(0.000,  result.Origin.Z);
    }

    [Fact]
    public void Plane3D_Round_NearZeroDriftInNormalSnapsToZero()
    {
        // A normal that is almost Z-axis but has floating-point noise in X/Y.
        // After rounding to 6 decimals the noise disappears, and re-normalisation
        // produces exactly the Z-axis.
        var driftedNormal = new Vector3D(1e-10, -1e-10, 1.0);
        var plane = new Plane3D(new Point3D(0, 0, 0), driftedNormal);

        var result = plane.Round(6);

        Assert.Equal(0.0, result.Normal.X);
        Assert.Equal(0.0, result.Normal.Y);
        Assert.Equal(1.0, result.Normal.Z);
    }

    [Fact]
    public void Plane3D_Round_NormalRemainsUnitLengthAfterRounding()
    {
        // 45-degree miter normal — components are irrational; after rounding and
        // re-normalisation the result must still be a unit vector.
        var normal = new Vector3D(0.70710678118654752, 0.0, 0.70710678118654752);
        var plane = new Plane3D(new Point3D(100.0, 200.0, 300.0), normal);

        var result = plane.Round(6);

        var length = Math.Sqrt(result.Normal.X * result.Normal.X
                             + result.Normal.Y * result.Normal.Y
                             + result.Normal.Z * result.Normal.Z);
        Assert.Equal(1.0, length, precision: 10);
    }

    [Fact]
    public void Plane3D_Round_OriginAndNormalBothRoundedTogether()
    {
        var plane = new Plane3D(
            new Point3D(1.999994, 2.000006, -0.000003),
            new Vector3D(1.0, 0.0, 0.0));

        var result = plane.Round(5);

        // origin coordinates rounded to 5 decimal places
        Assert.Equal(1.99999, result.Origin.X);
        Assert.Equal(2.00001, result.Origin.Y);
        Assert.Equal(0.0,     result.Origin.Z);
        // axis-aligned normal is unchanged after rounding + re-normalisation
        Assert.Equal(1.0, result.Normal.X);
        Assert.Equal(0.0, result.Normal.Y);
        Assert.Equal(0.0, result.Normal.Z);
    }
}