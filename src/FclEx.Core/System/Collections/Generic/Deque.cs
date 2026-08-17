// ReSharper disable All

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Collections.Generic;

/// <summary>Provides a double-ended queue data structure.</summary>
/// <typeparam name="T">The type of elements stored in the deque.</typeparam>
[DebuggerDisplay("Count = {_size}")]
public sealed class Deque<T> : IReadOnlyCollection<T>
{
    private T[] _array = [];
    private int _head; // First valid element in the queue
    private int _tail; // First open slot in the dequeue, unless the dequeue is full
    private int _size; // Number of elements.

    public int Count => _size;

    public bool IsEmpty => _size == 0;

    public void EnqueueTail(T item)
    {
        if (_size == _array.Length)
        {
            Grow();
        }

        _array[_tail] = item;
        if (++_tail == _array.Length)
        {
            _tail = 0;
        }
        _size++;
    }
        
    public void EnqueueHead(T item)
    {
        if (_size == _array.Length)
        {
            Grow();
        }

        _head = (_head == 0 ? _array.Length : _head) - 1;
        _array[_head] = item;
        _size++;
    }

    /// <summary>Removes and returns the element at the head of the deque.</summary>
    /// <exception cref="InvalidOperationException">The deque is empty.</exception>
    public T DequeueHead()
    {
        EnsureNotEmpty();

        T item = _array[_head];
        _array[_head] = default!;

        if (++_head == _array.Length)
        {
            _head = 0;
        }
        _size--;

        return item;
    }

    /// <summary>Returns the element at the head of the deque without removing it.</summary>
    /// <exception cref="InvalidOperationException">The deque is empty.</exception>
    public T PeekHead()
    {
        EnsureNotEmpty();
        return _array[_head];
    }

    /// <summary>Returns the element at the tail of the deque without removing it.</summary>
    /// <exception cref="InvalidOperationException">The deque is empty.</exception>
    public T PeekTail()
    {
        EnsureNotEmpty();
        var index = _tail - 1;
        if (index == -1)
        {
            index = _array.Length - 1;
        }
        return _array[index];
    }

    /// <summary>Removes and returns the element at the tail of the deque.</summary>
    /// <exception cref="InvalidOperationException">The deque is empty.</exception>
    public T DequeueTail()
    {
        EnsureNotEmpty();

        if (--_tail == -1)
        {
            _tail = _array.Length - 1;
        }

        T item = _array[_tail];
        _array[_tail] = default!;

        _size--;
        return item;
    }

    /// <summary>Attempts to remove and return the element at the head of the deque.</summary>
    public bool TryDequeueHead([MaybeNullWhen(false)] out T item)
    {
        if (IsEmpty)
        {
            item = default;
            return false;
        }

        item = DequeueHead();
        return true;
    }

    /// <summary>Attempts to remove and return the element at the tail of the deque.</summary>
    public bool TryDequeueTail([MaybeNullWhen(false)] out T item)
    {
        if (IsEmpty)
        {
            item = default;
            return false;
        }

        item = DequeueTail();
        return true;
    }

    /// <summary>Attempts to return the element at the head of the deque without removing it.</summary>
    public bool TryPeekHead([MaybeNullWhen(false)] out T item)
    {
        if (IsEmpty)
        {
            item = default;
            return false;
        }

        item = _array[_head];
        return true;
    }

    /// <summary>Attempts to return the element at the tail of the deque without removing it.</summary>
    public bool TryPeekTail([MaybeNullWhen(false)] out T item)
    {
        if (IsEmpty)
        {
            item = default;
            return false;
        }

        var index = _tail == 0 ? _array.Length - 1 : _tail - 1;
        item = _array[index];
        return true;
    }

    public IEnumerator<T> GetEnumerator() // meant for debug purposes only
    {
        int pos = _head;
        int count = _size;
        while (count-- > 0)
        {
            yield return _array[pos];
            pos = (pos + 1) % _array.Length;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private void EnsureNotEmpty()
    {
        if (IsEmpty)
            throw new InvalidOperationException("The deque is empty.");
    }

    private void Grow()
    {
        Debug.Assert(_size == _array.Length);
        Debug.Assert(_head == _tail);

        const int MinimumGrow = 4;

        int capacity = (int)(_array.Length * 2L);
        if (capacity < _array.Length + MinimumGrow)
        {
            capacity = _array.Length + MinimumGrow;
        }

        T[] newArray = new T[capacity];

        if (_head == 0)
        {
            Array.Copy(_array, newArray, _size);
        }
        else
        {
            Array.Copy(_array, _head, newArray, 0, _array.Length - _head);
            Array.Copy(_array, 0, newArray, _array.Length - _head, _tail);
        }

        _array = newArray;
        _head = 0;
        _tail = _size;
    }
}
