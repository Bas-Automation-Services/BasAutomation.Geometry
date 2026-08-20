namespace BasAutomation.Geometry.Curves;

/// <summary>
/// Specifies the winding order for 2D curves.
/// </summary>
public enum WindingOrder
{
    /// <summary>
    /// Clockwise winding order.
    /// </summary>
    Clockwise,

    /// <summary>
    /// Counterclockwise winding order.
    /// </summary>
    CounterClockwise
}

/// <summary>
/// Helper class for ordering 2D curves in a specific winding direction.
/// </summary>
public static class CurveOrderHelper2D
{
    /// <summary>
    /// Orders a collection of curves so they connect end-to-end and have the specified winding order.
    /// </summary>
    /// <param name="curves">The collection of curves to order.</param>
    /// <param name="desiredOrder">The desired winding order (clockwise or counterclockwise).</param>
    /// <param name="tolerance">The tolerance for matching endpoints. Default is 1e-6.</param>
    /// <returns>A list of ordered curves with the specified winding direction.</returns>
    /// <exception cref="ArgumentException">Thrown when curves cannot form a continuous path.</exception>
    public static List<ICurve2D> OrderCurves(IEnumerable<ICurve2D> curves, WindingOrder desiredOrder, double tolerance = 1e-6)
    {
        return OrderCurves(curves, desiredOrder, null, tolerance);
    }

    /// <summary>
    /// Orders a collection of curves so they connect end-to-end and have the specified winding order, starting with a specific curve.
    /// </summary>
    /// <param name="curves">The collection of curves to order.</param>
    /// <param name="desiredOrder">The desired winding order (clockwise or counterclockwise).</param>
    /// <param name="startCurve">The curve to start with. If null, the first curve in the collection will be used.</param>
    /// <param name="tolerance">The tolerance for matching endpoints. Default is 1e-6.</param>
    /// <returns>A list of ordered curves with the specified winding direction.</returns>
    /// <exception cref="ArgumentException">Thrown when curves cannot form a continuous path or start curve is not in the collection.</exception>
    public static List<ICurve2D> OrderCurves(IEnumerable<ICurve2D> curves, WindingOrder desiredOrder, ICurve2D? startCurve, double tolerance = 1e-6)
    {
        var curveList = curves.ToList();
        if (curveList.Count == 0)
        {
            return new List<ICurve2D>();
        }

        // First, order the curves to form a continuous path
        var orderedCurves = OrderCurvesToFormPath(curveList, startCurve, tolerance);

        // Determine the current winding order
        var currentOrder = DetermineWindingOrder(orderedCurves);

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
    /// <param name="startCurve">The curve to start with. If null, the first curve in the collection will be used.</param>
    /// <param name="tolerance">The tolerance for matching endpoints.</param>
    /// <returns>A list of ordered curves forming a continuous path.</returns>
    /// <exception cref="ArgumentException">Thrown when curves cannot form a continuous path or start curve is not in the collection.</exception>
    private static List<ICurve2D> OrderCurvesToFormPath(List<ICurve2D> curves, ICurve2D? startCurve, double tolerance)
    {
        if (curves.Count == 0)
        {
            return new List<ICurve2D>();
        }

        var ordered = new List<ICurve2D>();
        var remaining = new List<ICurve2D>(curves);

        // Start with the specified curve or the first curve
        ICurve2D current;
        if (startCurve != null)
        {
            current = startCurve;
            remaining.Remove(startCurve);
        }
        else
        {
            current = remaining[0];
            remaining.RemoveAt(0);
        }

        ordered.Add(current);
        Point2D currentEnd = current.EndPoint;

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
                throw new ArgumentException($"Cannot form a continuous path. No curve found that connects to the current endpoint ({currentEnd.X:F6}, {currentEnd.Y:F6}). Remaining curves: {remaining.Count}");
            }
        }

        // Verify that the path is closed (end point matches start point)
        if (!ordered[0].StartPoint.IsEqualTo(ordered[^1].EndPoint, tolerance))
        {
            throw new ArgumentException($"Curves do not form a closed path. Start: ({ordered[0].StartPoint.X:F6}, {ordered[0].StartPoint.Y:F6}), End: ({ordered[^1].EndPoint.X:F6}, {ordered[^1].EndPoint.Y:F6})");
        }

        return ordered;
    }

    /// <summary>
    /// Determines the winding order of an ordered list of curves using the signed area method.
    /// </summary>
    /// <param name="curves">The ordered curves forming a closed path.</param>
    /// <returns>The winding order of the curves.</returns>
    private static WindingOrder DetermineWindingOrder(List<ICurve2D> curves)
    {
        double signedArea = 0.0;

        foreach (var curve in curves)
        {
            // For line segments, use the simple formula
            if (curve is Line2D line)
            {
                signedArea += (line.EndPoint.X - line.StartPoint.X) * (line.EndPoint.Y + line.StartPoint.Y);
            }
            // For arcs and other curves, sample points along the curve
            else
            {
                const int sampleCount = 20;
                var prevPoint = curve.StartPoint;

                for (int i = 1; i <= sampleCount; i++)
                {
                    double t = (double)i / sampleCount;
                    var currentPoint = curve.PointAt(t);
                    signedArea += (currentPoint.X - prevPoint.X) * (currentPoint.Y + prevPoint.Y);
                    prevPoint = currentPoint;
                }
            }
        }

        // Positive area indicates counterclockwise, negative indicates clockwise
        return signedArea > 0 ? WindingOrder.CounterClockwise : WindingOrder.Clockwise;
    }

    /// <summary>
    /// Orders curves to form a continuous path and returns them with the specified winding order.
    /// Overload that accepts arrays.
    /// </summary>
    /// <param name="curves">The array of curves to order.</param>
    /// <param name="desiredOrder">The desired winding order.</param>
    /// <param name="tolerance">The tolerance for matching endpoints.</param>
    /// <returns>A list of ordered curves with the specified winding direction.</returns>
    public static List<ICurve2D> OrderCurves(ICurve2D[] curves, WindingOrder desiredOrder, double tolerance = 1e-6)
    {
        return OrderCurves(curves.AsEnumerable(), desiredOrder, null, tolerance);
    }

    /// <summary>
    /// Orders curves to form a continuous path and returns them with the specified winding order, starting with a specific curve.
    /// Overload that accepts arrays.
    /// </summary>
    /// <param name="curves">The array of curves to order.</param>
    /// <param name="desiredOrder">The desired winding order.</param>
    /// <param name="startCurve">The curve to start with. If null, the first curve in the collection will be used.</param>
    /// <param name="tolerance">The tolerance for matching endpoints.</param>
    /// <returns>A list of ordered curves with the specified winding direction.</returns>
    public static List<ICurve2D> OrderCurves(ICurve2D[] curves, WindingOrder desiredOrder, ICurve2D? startCurve, double tolerance = 1e-6)
    {
        return OrderCurves(curves.AsEnumerable(), desiredOrder, startCurve, tolerance);
    }
}
