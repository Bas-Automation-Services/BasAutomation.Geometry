namespace BasAutomation.Geometry;


/// <summary>
/// Defines a method for comparing geometric objects with a tolerance.
/// </summary>
/// <typeparam name="T">The type of object to compare.</typeparam>
public interface IGeometryComparable<in T>
{
    /// <summary>
    /// Determines whether this object is equal to another object within the specified tolerance.
    /// </summary>
    /// <param name="other">The other object to compare.</param>
    /// <param name="tolerance">The tolerance for floating-point comparison. Default is Constants.Tolerance.</param>
    /// <returns>true if the objects are equal within tolerance; otherwise, false.</returns>
    bool IsEqualTo(T other, double tolerance = Constants.Tolerance);
}