namespace BasAutomation.Geometry.Tests;

public class RigidMotionFitTests
{
    private const double Tol = 1e-6;

    // Non-planar, asymmetric — mirrors of this set are NOT rigidly reachable.
    private static Point3D[] Cloud() =>
    [
        new(0, 0, 0),
        new(10, 0, 0),
        new(10, 6, 0),
        new(3, 14, 2),
        new(5, 2, 9),
    ];

    private static Point3D[] Apply(Matrix3D m, Point3D[] points)
        => [.. points.Select(m.Transform)];

    [Fact]
    public void Pure_translation_is_recovered()
    {
        var motion = Matrix3D.Translation(123.4, -56.7, 89.1);
        var moved = Apply(motion, Cloud());

        Assert.True(RigidMotionFit.TryFit(Cloud(), moved, Tol, out var fit));
        for (var i = 0; i < moved.Length; i++)
            Assert.True(fit.Transform(Cloud()[i]).DistanceTo(moved[i]) < Tol);
    }

    [Fact]
    public void Rotation_plus_translation_is_recovered()
    {
        var motion = Matrix3D.Translation(40, -7, 3)
                     * Matrix3D.CreateRotationAroundAxis(new Vector3D(1, 2, 3), 0.7);
        var moved = Apply(motion, Cloud());

        Assert.True(RigidMotionFit.TryFit(Cloud(), moved, Tol, out var fit));
        for (var i = 0; i < moved.Length; i++)
            Assert.True(fit.Transform(Cloud()[i]).DistanceTo(moved[i]) < Tol);
        Assert.True(fit.IsRotationMatrix(1e-9), "the fitted motion must be a proper rotation (det = +1)");
    }

    [Fact]
    public void Mirror_of_a_non_planar_set_is_rejected()
    {
        // Reflection: no proper rotation reproduces it for a non-planar cloud. This is
        // the det=+1 guarantee that the hash-level MovedOnly verdict cannot give.
        var mirrored = Cloud().Select(p => new Point3D(-p.X, p.Y, p.Z)).ToArray();

        Assert.False(RigidMotionFit.TryFit(Cloud(), mirrored, Tol, out _));
    }

    [Fact]
    public void Mirror_of_a_planar_set_within_its_plane_is_a_rigid_motion()
    {
        // A planar set mirrored in its own plane equals a 180° out-of-plane rotation —
        // genuinely rigid, so it must be accepted, not treated as a reflection.
        Point3D[] planar = [new(0, 0, 0), new(10, 0, 0), new(10, 6, 0), new(3, 14, 0)];
        var mirrored = planar.Select(p => new Point3D(-p.X, p.Y, 0)).ToArray();

        Assert.True(RigidMotionFit.TryFit(planar, mirrored, Tol, out var fit));
        for (var i = 0; i < planar.Length; i++)
            Assert.True(fit.Transform(planar[i]).DistanceTo(mirrored[i]) < Tol);
    }

    [Fact]
    public void Reshape_is_rejected()
    {
        var reshaped = Apply(Matrix3D.Translation(5, 5, 5), Cloud());
        reshaped[2] = new Point3D(reshaped[2].X + 2, reshaped[2].Y, reshaped[2].Z);

        Assert.False(RigidMotionFit.TryFit(Cloud(), reshaped, Tol, out _));
    }

    [Fact]
    public void Wrong_correspondence_is_rejected()
    {
        var moved = Apply(Matrix3D.Translation(5, 5, 5), Cloud());
        (moved[0], moved[3]) = (moved[3], moved[0]); // swapped pairing

        Assert.False(RigidMotionFit.TryFit(Cloud(), moved, Tol, out _));
    }

    [Fact]
    public void Collinear_set_moves_rigidly()
    {
        Point3D[] line = [new(0, 0, 0), new(5, 0, 0), new(12, 0, 0)];
        var motion = Matrix3D.Translation(1, 2, 3)
                     * Matrix3D.CreateRotationAroundAxis(new Vector3D(0, 0, 1), 0.5);
        var moved = Apply(motion, line);

        Assert.True(RigidMotionFit.TryFit(line, moved, Tol, out var fit));
        for (var i = 0; i < line.Length; i++)
            Assert.True(fit.Transform(line[i]).DistanceTo(moved[i]) < Tol);
    }

    [Fact]
    public void Mismatched_or_empty_inputs_are_rejected()
    {
        Assert.False(RigidMotionFit.TryFit([], [], Tol, out _));
        Assert.False(RigidMotionFit.TryFit(Cloud(), Cloud().Take(3).ToArray(), Tol, out _));
    }
}
