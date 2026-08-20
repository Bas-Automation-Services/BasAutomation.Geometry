using BasAutomation.Geometry;
using BasAutomation.Geometry.Curves;

namespace BasAutomation.Geometry.Surfaces;

/// <summary>
/// Computes the NURBS (BSplineSurface) parameters for an ellipse swept along a circular arc.
///
/// Mathematical basis: both the ellipse and the circular arc can be exactly represented as
/// rational quadratic B-splines. Their tensor product gives the exact swept surface.
///
/// U direction: ellipse profile (9 control points, 4 quarter-ellipse Bezier spans)
/// V direction: arc sweep path (2n+1 control points for n segments of at most 90 degrees)
/// </summary>
public static class BSplineEllipseSweep
{
    /// <summary>
    /// The weight for intermediate control points of a rational quadratic Bezier
    /// representing a 90-degree conic arc (1/sqrt(2)).
    /// </summary>
    private static readonly double W90 = 1.0 / Math.Sqrt(2.0);

    /// <summary>
    /// Computes BSplineSurface parameters for an ellipse swept along a circular arc.
    /// Output coordinates are in the same units as the input geometry.
    /// </summary>
    /// <param name="ellipse">The 3D ellipse at the arc start position (t=0)</param>
    /// <param name="arc">The centerline arc defining the sweep path</param>
    /// <returns>NURBS surface data: Order, Poles (XYZ interleaved), KnotsU, KnotsV, Weights, IsPeriodic</returns>
    public static (int[] Order, double[] Poles, double[] KnotsU, double[] KnotsV,
                    double[] Weights, bool[] IsPeriodic)
        ComputeSurface(Ellipse3D ellipse, Arc3D arc)
    {
        // --- U direction: ellipse as rational quadratic NURBS ---
        // 9 control points forming 4 quarter-ellipse Bezier spans (clamped, non-periodic)
        var ellipseOffsets = ComputeEllipseControlPoints2D(ellipse);
        var ellipseWeights = new[] { 1.0, W90, 1.0, W90, 1.0, W90, 1.0, W90, 1.0 };
        var numU = 9;

        // --- V direction: arc as rational quadratic NURBS ---
        var absSweep = Math.Abs(arc.SweepAngle);
        var numSegments = Math.Max(1, (int)Math.Ceiling(absSweep / (Math.PI / 2)));
        var numV = 2 * numSegments + 1;

        // Compute the V-direction (arc) control point parameters and weights
        var arcWeights = ComputeArcWeights(arc, numSegments);

        // --- Compute 3D surface control points (tensor product) ---
        var totalPoles = numU * numV;
        var poles = new double[totalPoles * 3];
        var weights = new double[totalPoles];

        // For each ellipse control point, compute its concentric sweep arc,
        // then extract the Bezier control points along that arc.
        for (var i = 0; i < numU; i++)
        {
            var (cx, cy) = ellipseOffsets[i];

            // 3D position of this ellipse control point at arc start (t=0)
            var startPos = ellipse.Center
                + cx * ellipse.MajorAxisDirection
                + cy * ellipse.MinorAxisDirection;

            // Compute the V-direction control points for this ellipse control point
            var vPoints = ComputeArcBezierControlPoints(arc, startPos, numSegments);

            for (var j = 0; j < numV; j++)
            {
                var idx = j * numU + i; // U-major layout: step through all U for each V row
                var pt = vPoints[j];

                poles[idx * 3 + 0] = pt.X;
                poles[idx * 3 + 1] = pt.Y;
                poles[idx * 3 + 2] = pt.Z;

                weights[idx] = ellipseWeights[i] * arcWeights[j];
            }
        }

        // --- Knot vectors ---
        var knotsU = ComputeClampedBezierKnots(4); // 4 spans for ellipse
        var knotsV = ComputeClampedBezierKnots(numSegments);

        var order = new[] { 3, 3 }; // degree 2 in both directions
        var isPeriodic = new[] { false, false };

        return (order, poles, knotsU, knotsV, weights, isPeriodic);
    }

    /// <summary>
    /// Computes the 9 ellipse control points as 2D offsets (cx, cy) from the ellipse center,
    /// in the ellipse's local coordinate system (MajorAxisDirection, MinorAxisDirection).
    /// </summary>
    private static (double cx, double cy)[] ComputeEllipseControlPoints2D(Ellipse3D ellipse)
    {
        var a = ellipse.SemiMajorAxis;
        var b = ellipse.SemiMinorAxis;

        return new[]
        {
            (+a, 0.0),   // 0: right
            (+a, +b),    // 1: top-right (off-curve, weighted)
            (0.0, +b),   // 2: top
            (-a, +b),    // 3: top-left (off-curve, weighted)
            (-a, 0.0),   // 4: left
            (-a, -b),    // 5: bottom-left (off-curve, weighted)
            (0.0, -b),   // 6: bottom
            (+a, -b),    // 7: bottom-right (off-curve, weighted)
            (+a, 0.0),   // 8: right (= #0, closure)
        };
    }

    /// <summary>
    /// Computes the V-direction weights for the arc Bezier decomposition.
    /// On-arc points get weight 1, intermediate (tangent-intersection) points get cos(alpha/2).
    /// </summary>
    private static double[] ComputeArcWeights(Arc3D arc, int numSegments)
    {
        var numV = 2 * numSegments + 1;
        var arcWeights = new double[numV];
        var segmentSweep = arc.SweepAngle / numSegments;

        for (var k = 0; k < numSegments; k++)
        {
            var baseIdx = 2 * k;
            arcWeights[baseIdx] = 1.0;                                  // on-arc start of segment
            arcWeights[baseIdx + 1] = Math.Cos(Math.Abs(segmentSweep) / 2.0);  // tangent-intersection
        }
        arcWeights[numV - 1] = 1.0; // on-arc end of last segment

        return arcWeights;
    }

    /// <summary>
    /// Computes the V-direction Bezier control points for a single point swept along the arc.
    /// Uses CreateConcentricArc to get the sweep path, then extracts rational quadratic
    /// Bezier control points for each arc segment.
    /// </summary>
    /// <param name="arc">The centerline arc</param>
    /// <param name="startPos">The 3D position at arc start (t=0) to sweep</param>
    /// <param name="numSegments">Number of Bezier segments to split the arc into</param>
    /// <returns>Array of 2*numSegments+1 control points along the V direction</returns>
    private static Point3D[] ComputeArcBezierControlPoints(Arc3D arc, Point3D startPos, int numSegments)
    {
        // Create the concentric arc for this point
        var concentricArc = arc.CreateConcentricArc(startPos);
        var numV = 2 * numSegments + 1;
        var points = new Point3D[numV];

        var segmentSweep = arc.SweepAngle / numSegments;

        for (var k = 0; k < numSegments; k++)
        {
            var tStart = (double)k / numSegments;
            var tEnd = (double)(k + 1) / numSegments;

            var pStart = concentricArc.PointAt(tStart);
            // Compute the tangent-intersection control point
            // For a rational quadratic Bezier arc segment with sweep alpha:
            // P_mid = P_start + tangent_start * (R * tan(|alpha|/2))
            // where R is the concentric arc's radius
            var tangentStart = concentricArc.TangentAt(tStart);
            var halfAngle = Math.Abs(segmentSweep) / 2.0;
            var offset = concentricArc.Radius * Math.Tan(halfAngle);
            var pMid = pStart + tangentStart * offset;

            var baseIdx = 2 * k;
            points[baseIdx] = pStart;
            points[baseIdx + 1] = pMid;
        }
        points[numV - 1] = concentricArc.PointAt(1.0); // end of last segment

        return points;
    }

    /// <summary>
    /// Computes a clamped knot vector for a series of quadratic Bezier spans.
    /// For n spans with order 3: [0,0,0, 1,1, 2,2, ..., n-1,n-1, n,n,n]
    /// </summary>
    private static double[] ComputeClampedBezierKnots(int numSpans)
    {
        // Number of knots = numControlPoints + order = (2*numSpans+1) + 3 = 2*numSpans + 4
        var numKnots = 2 * numSpans + 4;
        var knots = new double[numKnots];

        // First 3 knots clamped at 0
        knots[0] = 0;
        knots[1] = 0;
        knots[2] = 0;

        // Internal double knots at each segment boundary
        for (var k = 1; k < numSpans; k++)
        {
            knots[2 * k + 1] = k;
            knots[2 * k + 2] = k;
        }

        // Last 3 knots clamped at numSpans
        knots[numKnots - 3] = numSpans;
        knots[numKnots - 2] = numSpans;
        knots[numKnots - 1] = numSpans;

        return knots;
    }
}
