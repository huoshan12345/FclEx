namespace System.Collections.Generic;

public abstract class ReadOnlyItemCollection<T, TEnumerator> : ICollection<T>, IReadOnlyCollection<T>
    where TEnumerator : IEnumerator<T>
{
    public abstract TEnumerator GetEnumerator();
    public abstract bool Contains(T item);
    public abstract void CopyTo(T[] array, int arrayIndex);

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>)this).GetEnumerator();
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return Count == 0
            // use singleton empty enumerator to avoid unnecessary allocation when the dictionary is empty.
            ? GenericEmptyEnumerator<T>.Instance
            : GetEnumerator();
    }

    void ICollection<T>.Add(T item) => throw new NotSupportedException();
    void ICollection<T>.Clear() => throw new NotSupportedException();
    bool ICollection<T>.Remove(T item) => throw new NotSupportedException();
    public bool IsReadOnly => true;
    public abstract int Count { get; }
}
