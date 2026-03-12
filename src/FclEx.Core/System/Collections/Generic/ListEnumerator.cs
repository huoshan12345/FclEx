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
        Check.NotNull(list);
        Check.Between(start, 0, list.Count - 1);
        Check.Between(length, 0, list.Count - start);

        _list = list;
        _start = start;
        _length = length;
        _i = -1;
    }

    public readonly T Current
    {
        get
        {
            Check.Between(_i, 0, _length - _start - 1);
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