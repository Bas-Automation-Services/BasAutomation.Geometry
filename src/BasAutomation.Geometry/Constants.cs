namespace BasAutomation.Geometry;

public static class Constants
{
    /// <summary>
    /// The default tolerance for floating-point comparisons.
    /// </summary>
    public const double Tolerance = 1e-6;

    /// <summary>
    /// Tighter tolerance for geometry operations that need stricter end-ordering and trim validation.
    /// </summary>
    public const double GeometryTolerance = 1e-8;

    /// <summary>
    /// CAD (Inventor) uses cm, BasAutomation.Geometry uses mm. Multiply by this to go CAD→mm, divide to go mm→CAD.
    /// </summary>
    public const double InventorScale = 10;
}
