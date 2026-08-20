using BasAutomation.Geometry.Curves;

namespace BasAutomation.Geometry.Surfaces.SweptSurfaces;

public static class TorusFromArcSweep
{
    public static (Point3D Center, Vector3D Axis, double MajorRadius, double MinorRadius) Compute(Arc3D profileArc, Arc3D centerline)
        => Compute(profileArc.Center, profileArc.Radius, centerline);

    public static (Point3D Center, Vector3D Axis, double MajorRadius, double MinorRadius) Compute(Circle3D profileCircle, Arc3D centerline)
        => Compute(profileCircle.Center, profileCircle.Radius, centerline);

    private static (Point3D Center, Vector3D Axis, double MajorRadius, double MinorRadius) Compute(Point3D profileCenter, double profileRadius, Arc3D centerline)
    {
        var toProfileCenter = profileCenter - centerline.Center;
        var heightComponent = toProfileCenter.Dot(centerline.Normal);
        var radialComponent = toProfileCenter - centerline.Normal * heightComponent;
        var torusCenter = centerline.Center + centerline.Normal * heightComponent;
        return (torusCenter, centerline.Normal, radialComponent.Length, profileRadius);
    }
}
