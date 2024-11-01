namespace FclEx.Collections;

public interface IReadOnlyContainer<T> : IReadOnlyCollection<T>
{
    bool Contains(T item);
}