namespace System.Collections.Generic;

public struct ArrayEnumerator<T> : IEnumerator<T>
{
    private readonly T[] _array;
    private readonly int _start;
    private readonly int _length;
    private int _i;

    public ArrayEnumerator(T[] array, int start, int length)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array));

        if ((uint)start > (uint)array.Length)
            throw new ArgumentOutOfRangeException(nameof(start));

        if ((uint)length > (uint)(array.Length - start))
            throw new ArgumentOutOfRangeException(nameof(length));

        _array = array;
        _start = start;
        _length = length;
        _i = -1;
    }

    public readonly T Current
    {
        get
        {
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if ((uint)_i >= (uint)_length)
                throw new InvalidOperationException();

            return _array[_start + _i];
        }
    }

    readonly object IEnumerator.Current => Current!;

    public bool MoveNext()
    {
        var i = _i + 1;
        if (i >= _length)
            return false;

        _i = i;
        return true;
    }

    public void Reset() => _i = -1;

    public readonly void Dispose() { }
}