namespace System.Collections.Generic;

public class OrderedIndex<TScore, TValue> :
    IReadOnlyCollection<(TScore Score, TValue Value)>
    where TValue : notnull
{
    private const int MaxLevel = 32;
    private readonly Node _head;
    private readonly Dictionary<TValue, Node> _map;
    private readonly IComparer<TScore> _scoreComparer;
    private readonly Node[] _update = new Node[MaxLevel];
    private readonly int[] _rank = new int[MaxLevel];
    private int _level = 1;
    private int _count;
    private long _sequence;

    public OrderedIndex(IComparer<TScore>? comparer = null, int capacity = 0)
    {
        _scoreComparer = comparer ?? Comparer<TScore>.Default;

        _map = new Dictionary<TValue, Node>(capacity);

        _head = new Node(MaxLevel, default!, default!, 0);
    }

    // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
    public int Count => _count;

    public bool Contains(TValue value) => _map.TryGetValue(value, out _);

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

    public void Clear()
    {
        _head.Levels.Clear();
        _map.Clear();

        _level = 1;
        _count = 0;
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int Compare(Node node, TScore score, long seq)
    {
        var c = _scoreComparer.Compare(node.Score, score);
        if (c != 0) return c;

        return node.Sequence.CompareTo(seq);
    }

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
        _count++;

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

    public bool Remove(TValue value)
    {
        if (!_map.TryGetValue(value, out var node))
            return false;

        RemoveNode(node);

        _map.Remove(value);
        _count--;

        return true;
    }

    public bool UpdateScore(TValue value, TScore newScore)
    {
        if (!Remove(value))
            return false;

        Add(newScore, value);

        return true;
    }

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

    public RankRangeEnumerable RangeByRank(int start, int count)
    {
        if (start < 0) start = 0;
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

        return removed;
    }

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

        return removed;
    }

    IEnumerator<(TScore Score, TValue Value)> IEnumerable<(TScore Score, TValue Value)>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public Enumerator GetEnumerator()
    {
        return new Enumerator(_head);
    }

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static (TScore Score, TValue Value) GetScoreValue(Node? node)
    {
        return node is null
            ? throw new InvalidOperationException()
            : (node.Score, node.Value);
    }

    public struct Enumerator : IEnumerator<(TScore Score, TValue Value)>
    {
        private readonly Node? _start;
        private Node? _node;

        internal Enumerator(Node? start)
        {
            _start = start;
            _node = start;
        }

        public bool MoveNext()
        {
            _node = _node?.Levels[0].Forward;
            return _node != null;
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

    public readonly struct RankRangeEnumerable(RankRangeEnumerator enumerator)
        : IEnumerable<(TScore Score, TValue Value)>
    {
        IEnumerator<(TScore Score, TValue Value)> IEnumerable<(TScore Score, TValue Value)>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public RankRangeEnumerator GetEnumerator() => enumerator;
    }

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

    public readonly struct ScoreRangeEnumerable(ScoreRangeEnumerator enumerator)
        : IEnumerable<(TScore Score, TValue Value)>
    {
        IEnumerator<(TScore Score, TValue Value)> IEnumerable<(TScore Score, TValue Value)>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public ScoreRangeEnumerator GetEnumerator() => enumerator;
    }
}