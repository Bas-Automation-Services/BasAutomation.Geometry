namespace BasAutomation.Geometry;

public interface IGeometryCopyable<out T>
{
    T Copy();
}