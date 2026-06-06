namespace FclEx.Utils;

public interface IValueProvider<out T>
{
    T Value { get; }
}