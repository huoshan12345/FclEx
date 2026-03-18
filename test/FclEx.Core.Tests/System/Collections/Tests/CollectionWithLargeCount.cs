namespace System.Collections.Tests;

public class CollectionWithLargeCount<T> : ICollection<T>
{
    public int Count => int.MaxValue;

    public bool IsReadOnly => throw new NotImplementedException();
    public void Add(T item) => throw new NotImplementedException();
    public void Clear() => throw new NotImplementedException();
    public bool Contains(T item) => throw new NotImplementedException();
    public void CopyTo(T[] array, int arrayIndex) => throw new NotImplementedException();
    public IEnumerator<T> GetEnumerator() => throw new NotImplementedException();
    public bool Remove(T item) => throw new NotImplementedException();
    IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
}