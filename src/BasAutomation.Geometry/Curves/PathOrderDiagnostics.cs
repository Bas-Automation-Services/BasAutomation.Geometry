using System.Text;

namespace BasAutomation.Geometry.Curves;

/// <summary>
/// Temporary diagnostics for path curve ordering. Toggle <see cref="Enabled"/> off when no longer needed.
/// </summary>
public static class PathOrderDiagnostics
{
    /// <summary>When false, ordering behaviour is unchanged and no diagnostic text is produced.</summary>
    public static bool Enabled { get; set; } = false;

    /// <summary>Looser endpoint tolerance (mm), for comparison with geometry tolerance.</summary>
    public const double EndpointEpsilon = 0.001;

    /// <summary>Most recent diagnostic report from curve ordering (success or failure).</summary>
    public static string? LastReport { get; private set; }

    /// <summary>Formats a point for log output (mm).</summary>
    public static string FormatPoint(Point3D point) =>
        $"({point.X:F6}, {point.Y:F6}, {point.Z:F6})";

    /// <summary>Describes one curve segment for diagnostic output.</summary>
    public static string DescribeCurve(string label, int index, ICurve3D curve) =>
        $"[PathOrder] {label}[{index}]: start={FormatPoint(curve.StartPoint)} end={FormatPoint(curve.EndPoint)} length={curve.Length:F6}";

    /// <summary>Builds a report of all input curves before ordering.</summary>
    public static string BuildInputCurvesReport(IReadOnlyList<ICurve3D> curves, double tolerance, string context)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"[PathOrder] {context}: curveCount={curves.Count} tolerance={tolerance:G} endpointEpsilon={EndpointEpsilon}");
        for (int i = 0; i < curves.Count; i++)
        {
            sb.AppendLine(DescribeCurve("input", i, curves[i]));
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>Builds a report when curve ordering fails to connect the next segment.</summary>
    public static string BuildOrderFailureReport(
        IReadOnlyList<ICurve3D> ordered,
        Point3D currentEnd,
        IReadOnlyList<ICurve3D> remaining,
        double tolerance)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"[PathOrder] OrderCurves FAILED tolerance={tolerance:G} endpointEpsilon={EndpointEpsilon}");
        sb.AppendLine($"[PathOrder] Partial path: {ordered.Count} segment(s), currentEnd={FormatPoint(currentEnd)}");

        for (int i = 0; i < ordered.Count; i++)
        {
            sb.AppendLine(DescribeCurve("ordered", i, ordered[i]));
        }

        double bestGap = double.MaxValue;
        string bestLabel = "none";

        for (int i = 0; i < remaining.Count; i++)
        {
            var candidate = remaining[i];
            double distToStart = currentEnd.DistanceTo(candidate.StartPoint);
            double distToEnd = currentEnd.DistanceTo(candidate.EndPoint);
            double minDist = Math.Min(distToStart, distToEnd);

            sb.AppendLine(DescribeCurve("remaining", i, candidate));
            sb.AppendLine(
                $"[PathOrder]   dist(currentEnd→start)={distToStart:F6} dist(currentEnd→end)={distToEnd:F6} " +
                $"min={minDist:F6} withinTol({tolerance:G})={WithinTolerance(minDist, tolerance)} " +
                $"withinEndpointEps={WithinTolerance(minDist, EndpointEpsilon)}");

            if (minDist < bestGap)
            {
                bestGap = minDist;
                bestLabel = $"remaining[{i}] (min endpoint)";
            }
        }

        sb.AppendLine($"[PathOrder] Best gap from currentEnd: {bestGap:F6} at {bestLabel}");
        sb.Append(BuildEndpointPairReport(ordered, remaining, tolerance));

        return sb.ToString().TrimEnd();
    }

    /// <summary>Builds a report after curves were ordered successfully.</summary>
    public static string BuildOrderSuccessReport(IReadOnlyList<ICurve3D> ordered, double tolerance)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"[PathOrder] OrderCurves OK: {ordered.Count} segment(s) tolerance={tolerance:G}");

        for (int i = 0; i < ordered.Count; i++)
        {
            sb.AppendLine(DescribeCurve("segment", i, ordered[i]));
        }

        for (int i = 0; i < ordered.Count - 1; i++)
        {
            double gap = ordered[i].EndPoint.DistanceTo(ordered[i + 1].StartPoint);
            sb.AppendLine(
                $"[PathOrder] joint[{i}→{i + 1}]: gap={gap:F6} withinTol={WithinTolerance(gap, tolerance)} withinEndpointEps={WithinTolerance(gap, EndpointEpsilon)}");
        }

        if (ordered.Count > 0)
        {
            double totalLength = ordered.Sum(c => c.Length);
            sb.AppendLine(
                $"[PathOrder] path start={FormatPoint(ordered[0].StartPoint)} path end={FormatPoint(ordered[^1].EndPoint)} totalLength={totalLength:F6}");
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>Stores the latest report for callers that emit logs outside this assembly.</summary>
    public static void SetLastReport(string? report) => LastReport = report;

    private static string BuildEndpointPairReport(
        IReadOnlyList<ICurve3D> ordered,
        IReadOnlyList<ICurve3D> remaining,
        double tolerance)
    {
        var all = new List<ICurve3D>(ordered.Count + remaining.Count);
        all.AddRange(ordered);
        all.AddRange(remaining);

        if (all.Count < 2)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        sb.AppendLine("[PathOrder] Min endpoint distances between all segments (connectivity check):");

        for (int i = 0; i < all.Count; i++)
        {
            for (int j = i + 1; j < all.Count; j++)
            {
                double minPair = MinEndpointDistance(all[i], all[j]);
                if (minPair <= tolerance * 10 || minPair <= EndpointEpsilon * 10)
                {
                    sb.AppendLine(
                        $"[PathOrder]   [{i}]↔[{j}]: minEndpointDist={minPair:F6} withinTol={WithinTolerance(minPair, tolerance)} withinEndpointEps={WithinTolerance(minPair, EndpointEpsilon)}");
                }
            }
        }

        return sb.ToString().TrimEnd();
    }

    private static double MinEndpointDistance(ICurve3D a, ICurve3D b)
    {
        double[] distances =
        [
            a.StartPoint.DistanceTo(b.StartPoint),
            a.StartPoint.DistanceTo(b.EndPoint),
            a.EndPoint.DistanceTo(b.StartPoint),
            a.EndPoint.DistanceTo(b.EndPoint)
        ];

        return distances.Min();
    }

    private static string WithinTolerance(double distance, double tolerance) =>
        distance <= tolerance ? "yes" : "no";
}
