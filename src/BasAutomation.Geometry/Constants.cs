namespace BasAutomation.Geometry;

/// <summary>
/// Shared geometric constants.
/// </summary>
public static class Constants
{
    /// <summary>
    /// The default tolerance for floating-point comparisons.
    /// </summary>
    public const double Tolerance = 1e-6;

    /// <summary>
    /// A tighter tolerance for geometry equality, path ordering and trim validation where 1e-6 is too coarse.
    /// </summary>
    public const double GeometryTolerance = 1e-8;

    /// <summary>
    /// CAD (Inventor) uses cm, BasAutomation.Geometry uses mm. Multiply by this to go CAD→mm, divide to go mm→CAD.
    /// </summary>
    public const double InventorScale = 10;
}
