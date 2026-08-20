using BasAutomation.Geometry.Curves;

namespace BasAutomation.Geometry.Surfaces.SweptSurfaces;

public static class CylinderFromArcSweep
{
    public static (Point3D Center, Vector3D Axis, double Radius) Compute(Line3D profileLine, Arc3D centerline)
    {
        var toLineMid = profileLine.MidPoint - centerline.Center;
        var heightComponent = toLineMid.Dot(centerline.Normal);
        var radialComponent = toLineMid - centerline.Normal * heightComponent;
        var center = centerline.Center + centerline.Normal * heightComponent;
        return (center, centerline.Normal, radialComponent.Length);
    }
}
