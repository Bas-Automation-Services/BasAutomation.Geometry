namespace BasAutomation.Geometry.Curves;

/// <summary>
/// Helper class for ordering 3D curves to form a continuous path.
/// </summary>
public static class CurveOrderHelper3D
{
    /// <summary>
    /// Orders a collection of curves so they connect end-to-end.
    /// </summary>
    /// <param name="curves">The collection of curves to order.</param>
    /// <param name="tolerance">The tolerance for matching endpoints. Default is 1e-6.</param>
    /// <returns>A list of ordered curves forming a continuous path.</returns>
    /// <exception cref="ArgumentException">Thrown when curves cannot form a continuous path.</exception>
    public static List<ICurve3D> OrderCurves(IEnumerable<ICurve3D> curves, double tolerance = 1e-6)
    {
        return OrderCurvesToFormPath(curves.ToList(), tolerance);
    }

    /// <summary>
    /// Orders a collection of curves and reverses the entire path if needed based on a reference direction.
    /// </summary>
    /// <param name="curves">The collection of curves to order.</param>
    /// <param name="referenceDirection">A reference direction vector. The path will be oriented so the average tangent aligns with this direction.</param>
    /// <param name="tolerance">The tolerance for matching endpoints. Default is 1e-6.</param>
    /// <returns>A list of ordered curves.</returns>
    public static List<ICurve3D> OrderCurves(IEnumerable<ICurve3D> curves, Vector3D referenceDirection, double tolerance = 1e-6)
    {
        var orderedCurves = OrderCurvesToFormPath(curves.ToList(), tolerance);

        // Calculate the average tangent direction
        var avgTangent = Vector3D.Zero;
        foreach (var curve in orderedCurves)
        {
            avgTangent += curve.TangentAt(0.5);
        }
        avgTangent = avgTangent.Normalized();

        // If the average tangent is opposite to the reference direction, reverse the path
        if (avgTangent.Dot(referenceDirection) < 0)
        {
            orderedCurves.Reverse();
            for (int i = 0; i < orderedCurves.Count; i++)
            {
                orderedCurves[i] = orderedCurves[i].Reverse();
            }
        }

        return orderedCurves;
    }

    /// <summary>
    /// Orders curves based on winding order relative to a plane normal.
    /// For planar curves, this determines if they wind clockwise or counterclockwise when viewed along the normal.
    /// </summary>
    /// <param name="curves">The collection of curves to order.</param>
    /// <param name="planeNormal">The normal vector of the plane to use for determining winding order.</param>
    /// <param name="desiredOrder">The desired winding order when viewed along the plane normal.</param>
    /// <param name="tolerance">The tolerance for matching endpoints. Default is 1e-6.</param>
    /// <returns>A list of ordered curves with the specified winding direction.</returns>
    public static List<ICurve3D> OrderCurves(IEnumerable<ICurve3D> curves, Vector3D planeNormal, WindingOrder desiredOrder, double tolerance = 1e-6)
    {
        var orderedCurves = OrderCurvesToFormPath(curves.ToList(), tolerance, requireClosed: true);

        // Calculate the signed area projected onto the plane
        var currentOrder = DetermineWindingOrder(orderedCurves, planeNormal);

        // If the current order doesn't match the desired order, reverse all curves
        if (currentOrder != desiredOrder)
        {
            orderedCurves.Reverse();
            for (int i = 0; i < orderedCurves.Count; i++)
            {
                orderedCurves[i] = orderedCurves[i].Reverse();
            }
        }

        return orderedCurves;
    }

    /// <summary>
    /// Orders curves to form a continuous path, reversing individual curves as needed.
    /// </summary>
    /// <param name="curves">The curves to order.</param>
    /// <param name="tolerance">The tolerance for matching endpoints.</param>
    /// <param name="requireClosed">When <see langword="true"/>, throws an <see cref="ArgumentException"/> if the resulting path is not closed (end point does not match start point).</param>
    /// <returns>A list of ordered curves forming a continuous path.</returns>
    /// <exception cref="ArgumentException">Thrown when curves cannot form a continuous path, or when <paramref name="requireClosed"/> is <see langword="true"/> and the path is not closed.</exception>
    private static List<ICurve3D> OrderCurvesToFormPath(List<ICurve3D> curves, double tolerance, bool requireClosed = false)
    {
        if (curves.Count == 0)
        {
            if (requireClosed)
            {
                throw new ArgumentException("Curves do not form a closed path. Winding order determination requires a closed loop. The input collection is empty.");
            }

            return new List<ICurve3D>();
        }

        if (PathOrderDiagnostics.Enabled)
        {
            PathOrderDiagnostics.SetLastReport(
                PathOrderDiagnostics.BuildInputCurvesReport(curves, tolerance, "OrderCurves input"));
        }

        var ordered = new List<ICurve3D>();
        var remaining = new List<ICurve3D>(curves);

        // Start with the first curve
        var current = remaining[0];
        remaining.RemoveAt(0);
        ordered.Add(current);

        var currentEnd = current.EndPoint;

        // Keep adding curves until all are ordered
        while (remaining.Count > 0)
        {
            bool found = false;

            for (int i = 0; i < remaining.Count; i++)
            {
                var candidate = remaining[i];

                // Check if the candidate's start matches the current end
                if (currentEnd.IsEqualTo(candidate.StartPoint, tolerance))
                {
                    ordered.Add(candidate);
                    currentEnd = candidate.EndPoint;
                    remaining.RemoveAt(i);
                    found = true;
                    break;
                }
                // Check if the candidate's end matches the current end (need to reverse)
                else if (currentEnd.IsEqualTo(candidate.EndPoint, tolerance))
                {
                    var reversed = candidate.Reverse();
                    ordered.Add(reversed);
                    currentEnd = reversed.EndPoint;
                    remaining.RemoveAt(i);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                var failureReport = PathOrderDiagnostics.Enabled
                    ? PathOrderDiagnostics.BuildOrderFailureReport(ordered, currentEnd, remaining, tolerance)
                    : null;
                PathOrderDiagnostics.SetLastReport(failureReport);

                var message = $"Cannot form a continuous path. No curve found that connects to the current endpoint ({currentEnd.X:F6}, {currentEnd.Y:F6}, {currentEnd.Z:F6}). Remaining curves: {remaining.Count}";
                if (!string.IsNullOrEmpty(failureReport))
                {
                    message += Environment.NewLine + failureReport;
                }

                throw new ArgumentException(message);
            }
        }

        if (requireClosed && !ordered[0].StartPoint.IsEqualTo(ordered[^1].EndPoint, tolerance))
        {
            throw new ArgumentException(
                $"Curves do not form a closed path. Winding order determination requires a closed loop. " +
                $"Start: ({ordered[0].StartPoint.X:F6}, {ordered[0].StartPoint.Y:F6}, {ordered[0].StartPoint.Z:F6}), " +
                $"End: ({ordered[^1].EndPoint.X:F6}, {ordered[^1].EndPoint.Y:F6}, {ordered[^1].EndPoint.Z:F6})");
        }

        if (PathOrderDiagnostics.Enabled)
        {
            PathOrderDiagnostics.SetLastReport(
                PathOrderDiagnostics.BuildOrderSuccessReport(ordered, tolerance));
        }

        return ordered;
    }

    /// <summary>
    /// Determines the winding order of curves relative to a plane normal using the signed area method.
    /// </summary>
    /// <param name="curves">The ordered curves forming a closed path.</param>
    /// <param name="planeNormal">The normal vector of the reference plane.</param>
    /// <returns>The winding order when viewed along the plane normal.</returns>
    private static WindingOrder DetermineWindingOrder(List<ICurve3D> curves, Vector3D planeNormal)
    {
        planeNormal = planeNormal.Normalized();
        double signedVolume = 0.0;

        foreach (var curve in curves)
        {
            // Sample points along the curve and calculate signed area contribution
            const int sampleCount = 20;
            var prevPoint = curve.StartPoint;

            for (int i = 1; i <= sampleCount; i++)
            {
                double t = (double)i / sampleCount;
                var currentPoint = curve.PointAt(t);

                // Calculate the cross product contribution
                var edge = currentPoint - prevPoint;
                // Position vector to the midpoint (from origin)
                var midPointVector = (prevPoint.ToVector() + currentPoint.ToVector()) * 0.5;
                var contribution = midPointVector.Cross(edge);
                signedVolume += contribution.Dot(planeNormal);

                prevPoint = currentPoint;
            }
        }

        // Positive volume indicates counterclockwise when viewed along the normal
        return signedVolume > 0 ? WindingOrder.CounterClockwise : WindingOrder.Clockwise;
    }

    /// <summary>
    /// Orders curves to form a continuous path.
    /// Overload that accepts arrays.
    /// </summary>
    /// <param name="curves">The array of curves to order.</param>
    /// <param name="tolerance">The tolerance for matching endpoints.</param>
    /// <returns>A list of ordered curves forming a continuous path.</returns>
    public static List<ICurve3D> OrderCurves(ICurve3D[] curves, double tolerance = 1e-6)
    {
        return OrderCurves(curves.AsEnumerable(), tolerance);
    }

    /// <summary>
    /// Orders curves based on winding order relative to a plane normal.
    /// Overload that accepts arrays.
    /// </summary>
    public static List<ICurve3D> OrderCurves(ICurve3D[] curves, Vector3D planeNormal, WindingOrder desiredOrder, double tolerance = 1e-6)
    {
        return OrderCurves(curves.AsEnumerable(), planeNormal, desiredOrder, tolerance);
    }
}
