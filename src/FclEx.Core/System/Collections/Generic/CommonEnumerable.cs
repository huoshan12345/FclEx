namespace System.Collections.Generic;

public readonly struct CommonEnumerable<TEnumerator, T>(TEnumerator enumerator)
    : IEnumerable<T> where TEnumerator : IEnumerator<T>
{
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public TEnumerator GetEnumerator()
    {
        return enumerator;
    }
}