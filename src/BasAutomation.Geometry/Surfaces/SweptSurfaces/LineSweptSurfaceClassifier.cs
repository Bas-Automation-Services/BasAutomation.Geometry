using BasAutomation.Geometry.Curves;

namespace BasAutomation.Geometry.Surfaces.SweptSurfaces;

public enum LineSweptSurfaceKind
{
    RadialPlane,
    Cone,
    Cylinder
}

public static class LineSweptSurfaceClassifier
{
    private const double Tolerance = 1e-6;

    public static LineSweptSurfaceKind Classify(Line3D profileLine, Arc3D centerline)
    {
        if (IsRadial(profileLine, centerline))
        {
            return LineSweptSurfaceKind.RadialPlane;
        }

        var (startRadius, _) = GetRadialDistanceAndHeight(profileLine.StartPoint, centerline);
        var (endRadius, _) = GetRadialDistanceAndHeight(profileLine.EndPoint, centerline);
        return Math.Abs(startRadius - endRadius) > Tolerance
            ? LineSweptSurfaceKind.Cone
            : LineSweptSurfaceKind.Cylinder;
    }

    public static bool IsRadial(Line3D profileLine, Arc3D centerline)
    {
        var lineDir = profileLine.Direction.Normalized();

        var toLineMid = profileLine.MidPoint - centerline.Center;
        var heightComponent = toLineMid.Dot(centerline.Normal);
        var radialDir = (toLineMid - centerline.Normal * heightComponent).Normalized();

        if (Math.Abs(lineDir.Dot(radialDir)) <= 0.9) return false;

        var (_, startHeight) = GetRadialDistanceAndHeight(profileLine.StartPoint, centerline);
        var (_, endHeight) = GetRadialDistanceAndHeight(profileLine.EndPoint, centerline);
        return Math.Abs(startHeight - endHeight) < Tolerance;
    }

    public static (double RadialDistance, double HeightOffset) GetRadialDistanceAndHeight(Point3D point, Arc3D centerline)
    {
        var toPoint = point - centerline.Center;
        var heightOffset = toPoint.Dot(centerline.Normal);
        var radialComponent = toPoint - centerline.Normal * heightOffset;
        return (radialComponent.Length, heightOffset);
    }
}
