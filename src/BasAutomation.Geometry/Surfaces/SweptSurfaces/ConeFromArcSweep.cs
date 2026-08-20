using BasAutomation.Geometry.Curves;

namespace BasAutomation.Geometry.Surfaces.SweptSurfaces;

public static class ConeFromArcSweep
{
    /// <summary>
    /// Computes Inventor-style cone parameters for a tilted line profile swept along an arc.
    /// The root point is on the centerline axis at the smaller-radius end; the axis points
    /// toward the larger-radius end; the cone always expands along its axis.
    /// </summary>
    public static (Point3D Root, Vector3D Axis, double RootRadius, double HalfAngle) Compute(Line3D profileLine, Arc3D centerline)
    {
        var (startRadius, startHeight) = LineSweptSurfaceClassifier.GetRadialDistanceAndHeight(profileLine.StartPoint, centerline);
        var (endRadius, endHeight) = LineSweptSurfaceClassifier.GetRadialDistanceAndHeight(profileLine.EndPoint, centerline);

        var heightDiff = endHeight - startHeight;
        var radiusDiff = endRadius - startRadius;
        var halfAngle = Math.Atan2(Math.Abs(radiusDiff), Math.Abs(heightDiff));

        Point3D rootPoint;
        double rootRadius;
        Vector3D axis;
        if (startRadius <= endRadius)
        {
            rootPoint = centerline.Center + centerline.Normal * startHeight;
            rootRadius = startRadius;
            axis = endHeight > startHeight ? centerline.Normal : -centerline.Normal;
        }
        else
        {
            rootPoint = centerline.Center + centerline.Normal * endHeight;
            rootRadius = endRadius;
            axis = startHeight > endHeight ? centerline.Normal : -centerline.Normal;
        }

        return (rootPoint, axis, rootRadius, halfAngle);
    }
}
