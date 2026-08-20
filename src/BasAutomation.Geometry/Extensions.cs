namespace BasAutomation.Geometry
{
    public static class Extensions
    {
        /// <summary>
        /// Returns a new <see cref="Point3D"/> with each coordinate rounded to
        /// <paramref name="decimals"/> decimal places.
        /// </summary>
        public static Point3D Round(this Point3D p, int decimals = 6) =>
            new Point3D(Math.Round(p.X, decimals), Math.Round(p.Y, decimals), Math.Round(p.Z, decimals));

        /// <summary>
        /// Returns a new <see cref="Vector3D"/> with each component rounded to
        /// <paramref name="decimals"/> decimal places.
        /// </summary>
        public static Vector3D Round(this Vector3D v, int decimals = 6) =>
            new Vector3D(Math.Round(v.X, decimals), Math.Round(v.Y, decimals), Math.Round(v.Z, decimals));

        /// <summary>
        /// Returns a new <see cref="Plane3D"/> with the origin and normal rounded to
        /// <paramref name="decimals"/> decimal places.  The <see cref="Plane3D"/>
        /// constructor re-normalises the normal, so near-zero drift snaps cleanly to zero.
        /// </summary>
        public static Plane3D Round(this Plane3D plane, int decimals = 6) =>
            new Plane3D(plane.Origin.Round(decimals), plane.Normal.Round(decimals));

        /// <summary>
        /// Returns a copy of the point with the X coordinate replaced.
        /// </summary>
        public static Point3D WithX(this Point3D p, double x) => new Point3D(x, p.Y, p.Z);

        /// <summary>
        /// Returns a copy of the point with the Y coordinate replaced.
        /// </summary>
        public static Point3D WithY(this Point3D p, double y) => new Point3D(p.X, y, p.Z);

        /// <summary>
        /// Returns a copy of the point with the Z coordinate replaced.
        /// </summary>
        public static Point3D WithZ(this Point3D p, double z) => new Point3D(p.X, p.Y, z);

        /// <summary>
        /// Returns the vector projected onto the XY plane (Z component zeroed).
        /// </summary>
        public static Vector3D MakeHorizontal(this Vector3D v) => new Vector3D(v.X, v.Y, 0.0);

        /// <summary>
        /// Returns a matrix whose three basis axes have been rescaled to unit length.
        /// Useful when a transform has drifted off |det| == 1 due to accumulated scale.
        /// </summary>
        public static Matrix3D WithNormalizedAxes(this Matrix3D m) =>
            m.SetAxes(m.XAxis.Normalized(), m.YAxis.Normalized(), m.ZAxis.Normalized());

        /// <summary>
        /// Returns a Y axis on this plane such that (xAxis, result, Normal) forms a right-handed
        /// basis with Z = Normal. The xAxis is assumed to lie on the plane.
        /// Note: this is Normal × xAxis — opposite sign to <see cref="Plane3D.GetVAxis"/>,
        /// which uses the u × n convention.
        /// </summary>
        public static Vector3D YAxisFromXAxis(this Plane3D plane, Vector3D xAxis) =>
            plane.Normal.Cross(xAxis).Normalized();

        /// <summary>
        /// Calculates the centroid of a set of points including this point.
        /// </summary>
        public static Point3D GetCentroid(this IEnumerable<Point3D> otherPoints)
        {
            var sumX = 0d;
            var sumY = 0d;
            var sumZ = 0d;
            var count = 0;

            foreach (var p in otherPoints)
            {
                sumX += p.X;
                sumY += p.Y;
                sumZ += p.Z;
                count++;
            }

            return new Point3D(sumX / count, sumY / count, sumZ / count);
        }
    }
}