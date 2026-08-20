namespace BasAutomation.Geometry.Curves;

/// <summary>
/// Interface for parametric 3D curves.
/// </summary>
public interface ICurve3D
{
    /// <summary>
    /// Gets the start point of the curve.
    /// </summary>
    Point3D StartPoint { get; }

    /// <summary>
    /// Gets the end point of the curve.
    /// </summary>
    Point3D EndPoint { get; }

    /// <summary>
    /// Gets the length of the curve.
    /// </summary>
    double Length { get; }

    /// <summary>
    /// Gets a point on the curve at the specified parameter.
    /// </summary>
    /// <param name="t">The parameter (0 = start, 1 = end).</param>
    /// <returns>The point at the specified parameter.</returns>
    Point3D PointAt(double t);

    /// <summary>
    /// Gets the tangent vector at the specified parameter.
    /// </summary>
    /// <param name="t">The parameter (0 = start, 1 = end).</param>
    /// <returns>The normalized tangent vector at the specified parameter.</returns>
    Vector3D TangentAt(double t);

    /// <summary>
    /// Gets the normal vector at the specified parameter.
    /// </summary>
    /// <param name="t">The parameter (0 = start, 1 = end).</param>
    /// <returns>The normal vector at the specified parameter.</returns>
    Vector3D NormalAt(double t);

    /// <summary>
    /// Gets the parameter at the specified distance along the curve.
    /// </summary>
    /// <param name="distance">The distance along the curve.</param>
    /// <returns>The parameter value.</returns>
    double ParameterAtDistance(double distance);

    /// <summary>
    /// Clones the curve
    /// </summary>
    /// <returns></returns>
    ICurve3D Clone();

    /// <summary>
    /// Creates a plane perpendicular to the curve at the specified parameter.
    /// </summary>
    /// <param name="t">The parameter along the curve (0 = start, 1 = end).</param>
    /// <returns>A plane positioned at the curve point with normal aligned to the curve tangent.</returns>
    ///  TODO: Create base class? Implementation is the same everywhere. Just annoying with navigation.
    Plane3D CreatePlaneAt(double t);

    /// <summary>
    /// Reverses the direction of the curve (swaps start and end points).
    /// </summary>
    /// <returns>A new curve with reversed direction.</returns>
    /// <exception cref="NotSupportedException">Thrown when the curve type does not support reversal (e.g., Circle3D).</exception>
    ICurve3D Reverse();
}