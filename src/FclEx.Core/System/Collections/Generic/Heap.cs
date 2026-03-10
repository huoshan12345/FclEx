namespace System.Collections.Generic;

public class Heap<T> : IReadOnlyCollection<T>
{
    private const int Arity = 4;

    private T[] _data;
    private int _count;
    private readonly IComparer<T> _comparer;

    public Heap(int capacity = 4, IComparer<T>? comparer = null)
    {
        if (capacity < 4) 
            capacity = 4;

        _data = new T[capacity];
        _comparer = comparer ?? Comparer<T>.Default;
    }

    public Heap(IComparer<T> comparer) : this(4, comparer)
    {
    }

    public Heap(IEnumerable<T> items, IComparer<T>? comparer = null)
    {
        _comparer = comparer ?? Comparer<T>.Default;
        _data = items.ToArray();
        _count = _data.Length;

        if (_data.Length < 4)
            Array.Resize(ref _data, 4);

        Heapify();
    }

    // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
    public int Count => _count;

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public ArrayEnumerator<T> GetEnumerator()
    {
        return new ArrayEnumerator<T>(_data, 0, _count);
    }

    public int Capacity => _data.Length;

    public void Clear()
    {
#if NET5_0_OR_GREATER
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            Array.Clear(_data, 0, _count);
#else
        Array.Clear(_data, 0, _count);
#endif
        _count = 0;
    }

    public void EnsureCapacity(int capacity)
    {
        if (capacity < 0)
            throw new ArgumentOutOfRangeException(nameof(capacity));

        if (_data.Length < capacity)
            Array.Resize(ref _data, capacity);
    }

    public void Push(T item)
    {
        var count = _count;

        if (count == _data.Length)
        {
            Grow();
        }

        _count = count + 1;
        SiftUp(count, item);
    }

    public T Pop()
    {
        if (_count == 0)
            throw new InvalidOperationException();

        var last = --_count;

        var root = _data[0];
        var x = _data[last];

#if NET5_0_OR_GREATER
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            _data[last] = default!;
#else
        _data[last] = default!;
#endif

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
        var root = data[0];

        SiftDown(0, item);

        return root;
    }

    public bool TryReplaceTop(T item, out T? old)
    {
        if (_count == 0)
        {
            old = default;
            Push(item);
            return false;
        }

        old = _data[0];
        SiftDown(0, item);
        return true;
    }

    public T PushPop(T item)
    {
        if (_count == 0 || _comparer.Compare(item, _data[0]) <= 0)
            return item;

        var root = _data[0];
        SiftDown(0, item);
        return root;
    }

    public void TrimExcess(int? capacity = null)
    {
        var count = _count;

        var newCapacity = capacity ?? count;

        if (newCapacity < count)
            newCapacity = count;

        if (newCapacity < 4)
            newCapacity = 4;

        if (newCapacity >= _data.Length)
            return;

        Array.Resize(ref _data, newCapacity);
    }

    private void Heapify()
    {
        var start = Parent(_count - 1);

        for (var i = start; i >= 0; i--)
        {
            SiftDown(i, _data[i]);
        }
    }

    private void Grow()
    {
        var newSize = _data.Length * 2;
        if (newSize < 4)
            newSize = 4;

        if ((uint)newSize > Array.MaxLength)
            newSize = Array.MaxLength;

        Array.Resize(ref _data, newSize);
    }

    private static int Parent(int i) => (i - 1) / Arity;

    private static int FirstChild(int i) => i * Arity + 1;

    private void SiftUp(int i, T item)
    {
        while (i > 0)
        {
            var parent = Parent(i);
            var p = _data[parent];

            if (_comparer.Compare(item, p) >= 0)
                break;

            _data[i] = p;
            i = parent;
        }

        _data[i] = item;
    }

    private void SiftDown(int i, T item)
    {
        while (true)
        {
            var first = FirstChild(i);
            if (first >= _count)
                break;

            var best = first;
            var bestValue = _data[first];
            var last = first + Arity;

            if (last > _count)
                last = _count;

            for (var j = first + 1; j < last; j++)
            {
                var v = _data[j];
                if (_comparer.Compare(_data[j], bestValue) < 0)
                {
                    best = j;
                    bestValue = v;
                }
            }

            var child = _data[best];

            if (_comparer.Compare(child, item) >= 0)
                break;

            _data[i] = child;
            i = best;
        }

        _data[i] = item;
    }
}
