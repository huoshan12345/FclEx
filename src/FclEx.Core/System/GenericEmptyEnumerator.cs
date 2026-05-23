namespace System;

internal abstract class GenericEmptyEnumeratorBase : IDisposable, IEnumerator
{
    public bool MoveNext() => false;
    public object Current => throw new InvalidOperationException();
    public void Reset() { }
    public void Dispose() { }
}

internal sealed class GenericEmptyEnumerator<T> : GenericEmptyEnumeratorBase, IEnumerator<T>
{
    public static readonly GenericEmptyEnumerator<T> Instance = new();
    private GenericEmptyEnumerator() { }
    public new T Current => throw new InvalidOperationException();
}