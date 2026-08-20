namespace BasAutomation.Geometry;

public static class Helpers
{
    /// <summary>
    /// Clamps a value to the specified range.
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <param name="min">The minimum allowed value.</param>
    /// <param name="max">The maximum allowed value.</param>
    /// <returns>The clamped value.</returns>
    public static double Clamp(double value, double min, double max)
    {
        return Math.Max(min, Math.Min(max, value));
    }

    /// <summary>
    /// Performs linear interpolation between two values.
    /// </summary>
    /// <param name="a">The start value.</param>
    /// <param name="b">The end value.</param>
    /// <param name="t">The interpolation factor (0 to 1).</param>
    /// <returns>The interpolated value.</returns>
    public static double Lerp(double a, double b, double t)
    {
        return a + (b - a) * Clamp(t, 0, 1);
    }
}