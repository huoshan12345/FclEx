namespace System.Collections.Generic;

public struct ListEnumerator<TList, T> : IEnumerator<T> where TList : IReadOnlyList<T>
{
    private readonly TList _list;
    private readonly int _start;
    private readonly int _length;
    private int _i;

    public ListEnumerator(TList list) : this(list, 0, list.Count) { }

    public ListEnumerator(TList list, int start, int length)
    {
        if (list == null)
            throw new ArgumentNullException(nameof(list));

        if ((uint)start > (uint)list.Count)
            throw new ArgumentOutOfRangeException(nameof(start));

        if ((uint)length > (uint)(list.Count - start))
            throw new ArgumentOutOfRangeException(nameof(length));

        _list = list;
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

            return _list[_start + _i];
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