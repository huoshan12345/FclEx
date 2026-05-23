// ReSharper disable ConvertToAutoPropertyWithPrivateSetter
namespace System.Collections.Generic;

/// <summary>
/// Represents an ordered index of values associated with scores.
/// Elements are kept sorted by score and support efficient rank and range queries.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><description>Each value is unique within the index.</description></item>
/// <item><description>Elements are ordered by score.</description></item>
/// <item><description>Ordering is stable: elements with the same score keep their insertion order.</description></item>
/// <item><description>Enumeration returns elements in score order.</description></item>
/// </list>
/// </remarks>
public class OrderedIndex<TScore, TValue> : ICollection<(TScore Score, TValue Value)>
    where TValue : notnull
{
    private const int MaxLevel = 32;
    private readonly Node _head = new(MaxLevel, default!, default!, 0);
    private readonly Node[] _update = new Node[MaxLevel];
    private readonly int[] _rank = new int[MaxLevel];
    private readonly Dictionary<TValue, Node> _map;
    private readonly IComparer<TScore> _scoreComparer;
    private int _level = 1;
    private int _count;
    private long _sequence;
    private int _version;

    public OrderedIndex(IComparer<TScore>? comparer = null, int capacity = 0)
    {
        _scoreComparer = comparer ?? Comparer<TScore>.Default;
        _map = new Dictionary<TValue, Node>(capacity);
    }

    public int Count => _count;

    public bool IsReadOnly => false;

    /// <summary>
    /// Determines whether the specified value exists in the index.
    /// </summary>
    public bool Contains(TValue value) => _map.TryGetValue(value, out _);

    /// <summary>
    /// Attempts to get the score associated with the specified value.
    /// </summary>
    public bool TryGetScore(TValue value, out TScore score)
    {
        if (_map.TryGetValue(value, out var node))
        {
            score = node.Score;
            return true;
        }

        score = default!;
        return false;
    }

    /// <summary>
    /// Removes all elements from the index.
    /// </summary>
    public void Clear()
    {
        _head.Levels.Clear();
        _map.Clear();

        _level = 1;
        _count = 0;
        ++_version;
    }

    private static int RandomLevel()
    {
        // Randomly generate a node level using geometric distribution.
        // 
        // Each additional level is added with probability p = 1/2.
        // This produces an exponential distribution:
        //
        // level 1: 50%
        // level 2: 25%
        // level 3: 12.5%
        // ...
        //
        // The result is a pyramid structure where higher levels
        // contain exponentially fewer nodes, enabling O(log n)
        // search complexity.

        var r = (uint)Random.Shared.Next();
        var level = 1;

        while ((r & 3) == 3 && level < MaxLevel)
        {
            level++;
            r >>= 1;
        }

        return level;
    }

    [MethodImpl(AggressiveInlining)]
    private int Compare(Node node, TScore score, long seq)
    {
        var c = _scoreComparer.Compare(node.Score, score);

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (c != 0)
            return c;

        return node.Sequence.CompareTo(seq);
    }

    /// <summary>
    /// Adds a value with the specified score.
    /// </summary>
    /// <returns>
    /// True if the value was added; false if the value already exists.
    /// </returns>
    public bool Add(TScore score, TValue value)
    {
        if (_map.ContainsKey(value))
            return false;

        var seq = ++_sequence;
        var x = _head;

        for (var i = _level - 1; i >= 0; i--)
        {
            _rank[i] = i == _level - 1 ? 0 : _rank[i + 1];

            while (x.Levels[i].Forward is { } forward &&
                   Compare(forward, score, seq) < 0)
            {
                _rank[i] += x.Levels[i].Span;
                x = forward;
            }

            _update[i] = x;
        }

        var lvl = RandomLevel();

        if (lvl > _level)
        {
            for (var i = _level; i < lvl; i++)
            {
                _rank[i] = 0;
                _update[i] = _head;
                _head.Levels[i].Span = _count;
            }

            _level = lvl;
        }

        var node = new Node(lvl, score, value, seq);

        for (var i = 0; i < lvl; i++)
        {
            node.Levels[i].Forward = _update[i].Levels[i].Forward;
            _update[i].Levels[i].Forward = node;

            node.Levels[i].Span = _update[i].Levels[i].Span - (_rank[0] - _rank[i]);
            _update[i].Levels[i].Span = (_rank[0] - _rank[i]) + 1;
        }

        for (var i = lvl; i < _level; i++)
        {
            _update[i].Levels[i].Span++;
        }

        node.Backward = _update[0] == _head ? null : _update[0];
        node.Levels[0].Forward?.Backward = node;

        _map[value] = node;
        ++_count;
        ++_version;

        return true;
    }

    private void RemoveNode(Node node)
    {
        var x = _head;
        var score = node.Score;
        var seq = node.Sequence;

        for (var i = _level - 1; i >= 0; i--)
        {
            while (x.Levels[i].Forward is { } forward
                   && forward != node
                   && Compare(forward, score, seq) < 0)
            {
                x = forward;
            }

            _update[i] = x;
        }

        for (var i = 0; i < _level; i++)
        {
            if (_update[i].Levels[i].Forward == node)
            {
                _update[i].Levels[i].Span += node.Levels[i].Span - 1;
                _update[i].Levels[i].Forward = node.Levels[i].Forward;
            }
            else
            {
                _update[i].Levels[i].Span--;
            }
        }

        node.Levels[0].Forward?.Backward = node.Backward;

        while (_level > 1 && _head.Levels[_level - 1].Forward == null)
            _level--;
    }

    /// <summary>
    /// Removes the specified value from the index.
    /// </summary>
    /// <returns>
    /// True if the value was removed; otherwise false.
    /// </returns>
    public bool Remove(TValue value)
    {
        if (!_map.TryGetValue(value, out var node))
            return false;

        RemoveNode(node);

        _map.Remove(value);
        --_count;

        ++_version;

        return true;
    }

    /// <summary>
    /// Updates the score of the specified value.
    /// </summary>
    /// <returns>
    /// True if the score was updated; false if the value does not exist.
    /// </returns>
    public bool UpdateScore(TValue value, TScore newScore)
    {
        if (!Remove(value))
            return false;

        Add(newScore, value);

        return true;
    }

    /// <summary>
    /// Gets the zero-based rank of the specified value.
    /// </summary>
    /// <returns>
    /// The zero-based rank of the value; or -1 if the value does not exist.
    /// </returns>
    public int Rank(TValue value)
    {
        if (!_map.TryGetValue(value, out var node))
            return -1;

        if (TryFast(node, out var result))
            return result;

        var rank = 0;
        var x = _head;
        var score = node.Score;
        var seq = node.Sequence;

        for (var i = _level - 1; i >= 0; i--)
        {
            while (x.Levels[i].Forward is { } forward)
            {
                if (forward == node)
                {
                    rank += x.Levels[i].Span;
                    return rank - 1;
                }

                if (Compare(forward, score, seq) > 0)
                    break;

                rank += x.Levels[i].Span;
                x = forward;
            }

            if (x == node)
                return rank - 1;
        }

        return -1;

        static bool TryFast(Node node, out int rank)
        {
            rank = 0;
            var x = node;

            while (rank < 8)
            {
                if (x.Backward == null)
                    return true;

                x = x.Backward;
                rank++;
            }

            rank = 0;
            return false;
        }
    }

    /// <summary>
    /// Attempts to get the element at the specified rank.
    /// </summary>
    public bool TryGetByRank(int rank, out (TScore Score, TValue Value) item)
    {
        item = default;

        if (rank < 0 || rank >= _count)
            return false;

        var x = _head;
        var traversed = 0;

        for (var i = _level - 1; i >= 0; i--)
        {
            while (x.Levels[i].Forward is { } forward &&
                   traversed + x.Levels[i].Span <= rank)
            {
                traversed += x.Levels[i].Span;
                x = forward;
            }
        }

        x = x.Levels[0].Forward!;
        item = (x.Score, x.Value);

        return true;
    }

    /// <summary>
    /// Returns a sequence of elements starting at the specified rank.
    /// </summary>
    public RankRangeEnumerable RangeByRank(int start, int count)
    {
        if (start < 0)
            start = 0;

        if (start >= _count || count <= 0)
            return [];

        var end = Math.Min(start + count, _count);

        var x = _head;
        var traversed = 0;

        for (var i = _level - 1; i >= 0; i--)
        {
            while (x.Levels[i].Forward is { } forward &&
                   traversed + x.Levels[i].Span <= start)
            {
                traversed += x.Levels[i].Span;
                x = forward;
            }
        }

        var e = new RankRangeEnumerator(x, end - start);
        return new(e);
    }

    /// <summary>
    /// Returns elements whose scores are within the inclusive range [min, max].
    /// </summary>
    public ScoreRangeEnumerable RangeByScore(TScore min, TScore max)
    {
        var x = _head;

        for (var i = _level - 1; i >= 0; i--)
        {
            while (x.Levels[i].Forward is { } forward &&
                   _scoreComparer.Compare(forward.Score, min) < 0)
            {
                x = forward;
            }
        }

        var e = new ScoreRangeEnumerator(x, max, _scoreComparer);
        return new(e);
    }

    /// <summary>
    /// Removes a range of elements by rank.
    /// </summary>
    /// <returns>The number of removed elements.</returns>
    public int RemoveByRank(int start, int count)
    {
        if (count <= 0 || start < 0 || start >= _count)
            return 0;

        var x = _head;
        var traversed = 0;

        for (var i = _level - 1; i >= 0; i--)
        {
            while (x.Levels[i].Forward != null &&
                   traversed + x.Levels[i].Span <= start)
            {
                traversed += x.Levels[i].Span;
                x = x.Levels[i].Forward!;
            }

            _update[i] = x;
        }

        var removed = 0;
        x = _update[0].Levels[0].Forward;

        while (x != null && removed < count)
        {
            var next = x.Levels[0].Forward;

            for (var i = 0; i < _level; i++)
            {
                if (_update[i].Levels[i].Forward == x)
                {
                    _update[i].Levels[i].Span += x.Levels[i].Span - 1;
                    _update[i].Levels[i].Forward = x.Levels[i].Forward;
                }
                else
                {
                    _update[i].Levels[i].Span--;
                }
            }

            x.Levels[0].Forward?.Backward = x.Backward;

            _map.Remove(x.Value);

            _count--;
            removed++;

            x = next;
        }

        while (_level > 1 && _head.Levels[_level - 1].Forward == null)
            _level--;

        if (removed > 0)
        {
            ++_version;
        }

        return removed;
    }

    /// <summary>
    /// Removes elements whose scores are within the inclusive range [min, max].
    /// </summary>
    /// <returns>
    /// The number of removed elements.
    /// </returns>
    public int RemoveByScore(TScore min, TScore max)
    {
        var x = _head;

        for (var i = _level - 1; i >= 0; i--)
        {
            while (x.Levels[i].Forward != null &&
                   _scoreComparer.Compare(x.Levels[i].Forward!.Score, min) < 0)
            {
                x = x.Levels[i].Forward!;
            }

            _update[i] = x;
        }

        var removed = 0;
        x = _update[0].Levels[0].Forward;

        while (x != null &&
               _scoreComparer.Compare(x.Score, max) <= 0)
        {
            var next = x.Levels[0].Forward;

            for (var i = 0; i < _level; i++)
            {
                if (_update[i].Levels[i].Forward == x)
                {
                    _update[i].Levels[i].Span += x.Levels[i].Span - 1;
                    _update[i].Levels[i].Forward = x.Levels[i].Forward;
                }
                else
                {
                    _update[i].Levels[i].Span--;
                }
            }

            x.Levels[0].Forward?.Backward = x.Backward;

            _map.Remove(x.Value);

            _count--;
            removed++;

            x = next;
        }

        while (_level > 1 && _head.Levels[_level - 1].Forward == null)
            _level--;

        if (removed > 0)
        {
            ++_version;
        }

        return removed;
    }

    IEnumerator<(TScore Score, TValue Value)> IEnumerable<(TScore Score, TValue Value)>.GetEnumerator()
    {
        // use singleton empty enumerator to avoid unnecessary allocation when the dictionary is empty.
        return Count == 0
            ? GenericEmptyEnumerator<(TScore Score, TValue Value)>.Instance
            : GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<(TScore Score, TValue Value)>)this).GetEnumerator();

    /// <summary>
    /// Returns an enumerator that iterates through the elements in score order.
    /// </summary>
    public Enumerator GetEnumerator() => new(this);

    internal struct Level
    {
        public Node? Forward;
        public int Span;
    }

    internal sealed class Node(int level, TScore score, TValue value, long sequence)
    {
        public readonly TScore Score = score;
        public readonly TValue Value = value;
        public readonly long Sequence = sequence;
        public Node? Backward;
        public readonly Level[] Levels = new Level[level];
    }

    [MethodImpl(AggressiveInlining)]
    private static (TScore Score, TValue Value) GetScoreValue(Node? node)
    {
        return node is null
            ? throw new InvalidOperationException()
            : (node.Score, node.Value);
    }

    public void Add((TScore Score, TValue Value) item)
    {
        Add(item.Score, item.Value);
    }

    bool ICollection<(TScore Score, TValue Value)>.Contains((TScore Score, TValue Value) item)
    {
        return _map.TryGetValue(item.Value, out var node)
               && _scoreComparer.Compare(node.Score, item.Score) == 0;
    }

    public void CopyTo((TScore Score, TValue Value)[] array, int arrayIndex)
    {
        Check.CanCopyTo(array, arrayIndex, _count);

        foreach (var item in this)
        {
            array[arrayIndex++] = item;
        }
    }

    bool ICollection<(TScore Score, TValue Value)>.Remove((TScore Score, TValue Value) item)
    {
        if (!_map.TryGetValue(item.Value, out var node)
            || _scoreComparer.Compare(node.Score, item.Score) != 0)
            return false;

        RemoveNode(node);

        _map.Remove(item.Value);
        --_count;
        ++_version;

        return true;
    }

    /// <summary>
    /// Enumerates the elements of the <see cref="OrderedIndex{TScore, TValue}"/> in score order.
    /// </summary>
    public struct Enumerator : IEnumerator<(TScore Score, TValue Value)>
    {
        private readonly OrderedIndex<TScore, TValue> _orderedIndex;
        private readonly int _version;
        private Node? _node;
        private (TScore Score, TValue Value) _current;

        internal Enumerator(OrderedIndex<TScore, TValue> orderedIndex)
        {
            _orderedIndex = orderedIndex;
            _version = orderedIndex._version;
            _node = orderedIndex._head;
        }

        public bool MoveNext()
        {
            Check.VersionEqual(_orderedIndex._version, _version);

            _node = _node?.Levels[0].Forward;
            if (_node == null)
            {
                _current = default;
                return false;
            }

            _current = (_node.Score, _node.Value);
            return true;
        }

        // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
        public readonly (TScore Score, TValue Value) Current => _current;
        readonly object IEnumerator.Current => Current;

        public void Reset()
        {
            Check.VersionEqual(_orderedIndex._version, _version);
            _node = _orderedIndex._head;
        }

        public readonly void Dispose() { }
    }

    /// <summary>
    /// Enumerates elements within a specified rank range.
    /// </summary>
    public struct RankRangeEnumerator : IEnumerator<(TScore Score, TValue Value)>
    {
        private readonly Node? _start;
        private readonly int _count;
        private Node? _node;
        private int _remaining;

        internal RankRangeEnumerator(Node? start, int count)
        {
            _start = start;
            _count = count;
            _node = _start;
            _remaining = _count;
        }

        public bool MoveNext()
        {
            if (_remaining == 0 || _node == null)
                return false;

            _node = _node.Levels[0].Forward;
            _remaining--;

            return true;
        }

        public readonly (TScore Score, TValue Value) Current
            => GetScoreValue(_node);

        readonly object IEnumerator.Current => Current;

        public void Reset()
        {
            _node = _start;
            _remaining = _count;
        }

        public readonly void Dispose() { }
    }

    /// <summary>
    /// Represents a sequence of elements returned by a rank range query.
    /// </summary>
    public readonly struct RankRangeEnumerable(RankRangeEnumerator enumerator) : IEnumerable<(TScore Score, TValue Value)>
    {
        IEnumerator<(TScore Score, TValue Value)> IEnumerable<(TScore Score, TValue Value)>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public RankRangeEnumerator GetEnumerator() => enumerator;
    }

    /// <summary>
    /// Enumerates elements whose scores fall within a specified range.
    /// </summary>
    public struct ScoreRangeEnumerator : IEnumerator<(TScore Score, TValue Value)>
    {
        private readonly Node? _start;
        private Node? _node;
        private readonly TScore _max;
        private readonly IComparer<TScore> _comparer;

        internal ScoreRangeEnumerator(Node? start, TScore max, IComparer<TScore> comparer)
        {
            _start = start;
            _max = max;
            _comparer = comparer;
            _node = _start;
        }

        public bool MoveNext()
        {
            _node = _node?.Levels[0].Forward;

            if (_node == null)
                return false;

            return _comparer.Compare(_node.Score, _max) <= 0;
        }

        public readonly (TScore Score, TValue Value) Current
            => GetScoreValue(_node);

        readonly object IEnumerator.Current => Current;

        public void Reset()
        {
            _node = _start;
        }

        public readonly void Dispose() { }
    }

    /// <summary>
    /// Represents a sequence of elements returned by a score range query.
    /// </summary>
    public readonly struct ScoreRangeEnumerable(ScoreRangeEnumerator enumerator) : IEnumerable<(TScore Score, TValue Value)>
    {
        IEnumerator<(TScore Score, TValue Value)> IEnumerable<(TScore Score, TValue Value)>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public ScoreRangeEnumerator GetEnumerator() => enumerator;
    }
}