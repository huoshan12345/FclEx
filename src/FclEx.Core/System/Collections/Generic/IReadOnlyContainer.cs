namespace System.Collections.Generic;

public interface IReadOnlyContainer<T> : IReadOnlyCollection<T>
{
    bool Contains(T item);
}