namespace BasAutomation.Geometry;

/// <summary>
/// Defines a method for creating a copy of an object.
/// </summary>
/// <typeparam name="T">The type of object that can be copied.</typeparam>
/// <remarks>
/// This interface is used throughout the geometry library to provide a consistent
/// way of creating deep copies of immutable or value-type geometric objects.
/// Implementations should return a new instance with the same values as the original.
/// </remarks>
public interface ICopyable<out T>
{
    /// <summary>
    /// Creates a copy of the current object.
    /// </summary>
    /// <returns>A new instance with the same values as the current object.</returns>
    T Copy();
}
