namespace System.Collections.Generic;

public sealed class Heap<T> : IReadOnlyCollection<T>
{
    private const int Arity = 4;

    private T[] _data;
    private int _count;
    private readonly IComparer<T> _cmp;

    public Heap(int capacity = 4, IComparer<T>? comparer = null)
    {
        if (capacity < 4) capacity = 4;
        _data = new T[capacity];
        _cmp = comparer ?? Comparer<T>.Default;
    }

    public Heap(IEnumerable<T> items, IComparer<T>? comparer = null)
    {
        _cmp = comparer ?? Comparer<T>.Default;
        _data = items.ToArray();
        _count = _data.Length;

        if (_data.Length < 4)
            Array.Resize(ref _data, 4);

        Heapify();
    }

    // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
    public int Count => _count;

    public IEnumerator<T> GetEnumerator()
    {
        for (var i = 0; i < _count; i++)
            yield return _data[i];
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Capacity => _data.Length;

    public void Clear() => _count = 0;

    public void EnsureCapacity(int capacity)
    {
        if (_data.Length < capacity)
            Array.Resize(ref _data, capacity);
    }

    public void Push(T item)
    {
        var data = _data;
        int count = _count;

        if (count == data.Length)
        {
            Grow();
            data = _data;
        }

        _count = count + 1;
        SiftUp(count, item);
    }

    public T Pop()
    {
        if (_count == 0)
            throw new InvalidOperationException();

        var data = _data;
        int last = --_count;

        T root = data[0];
        T x = data[last];

        if (last > 0)
            SiftDown(0, x);

        return root;
    }

    public bool TryPop(out T value)
    {
        if (_count == 0)
        {
            value = default!;
            return false;
        }

        value = Pop();
        return true;
    }

    public T Peek()
    {
        if (_count == 0)
            throw new InvalidOperationException();

        return _data[0];
    }

    public bool TryPeek(out T value)
    {
        if (_count == 0)
        {
            value = default!;
            return false;
        }

        value = _data[0];
        return true;
    }

    public T ReplaceTop(T item)
    {
        if (_count == 0)
        {
            Push(item);
            return item;
        }

        var data = _data;
        T root = data[0];

        SiftDown(0, item);

        return root;
    }

    private void Heapify()
    {
        int start = Parent(_count - 1);

        for (int i = start; i >= 0; i--)
        {
            SiftDown(i, _data[i]);
        }
    }

    private void Grow()
    {
        int newSize = _data.Length * 2;
        if (newSize < 4) newSize = 4;

        Array.Resize(ref _data, newSize);
    }

    private static int Parent(int i) => (i - 1) / Arity;

    private static int FirstChild(int i) => i * Arity + 1;

    private void SiftUp(int i, T item)
    {
        var data = _data;
        var cmp = _cmp;

        while (i > 0)
        {
            int parent = (i - 1) / Arity;
            T p = data[parent];

            if (cmp.Compare(item, p) >= 0)
                break;

            data[i] = p;
            i = parent;
        }

        data[i] = item;
    }

    private void SiftDown(int i, T item)
    {
        var data = _data;
        var cmp = _cmp;
        int count = _count;

        while (true)
        {
            int first = i * Arity + 1;
            if (first >= count)
                break;

            int best = first;
            int last = first + Arity;

            if (last > count)
                last = count;

            for (int j = first + 1; j < last; j++)
            {
                if (cmp.Compare(data[j], data[best]) < 0)
                    best = j;
            }

            T child = data[best];

            if (cmp.Compare(child, item) >= 0)
                break;

            data[i] = child;
            i = best;
        }

        data[i] = item;
    }
}
