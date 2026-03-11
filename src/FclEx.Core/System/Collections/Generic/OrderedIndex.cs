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
        _head.Forward.Clear();
        _head.Span.Clear();
        _map.Clear();

        _level = 1;
        _count = 0;
    }

    private static int RandomLevel()
    {
        var r = (uint)Random.Shared.Next();

        var level = 1;

        while ((r & 1) == 1 && level < MaxLevel)
        {
            level++;
            r >>= 1;
        }

        return level;
    }

    private int CompareNode(Node a, Node b)
    {
        var c = _scoreComparer.Compare(a.Score, b.Score);
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (c != 0)
            return c;

        return a.Sequence.CompareTo(b.Sequence);
    }

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

            while (x.Forward[i] != null &&
                   Compare(x.Forward[i]!, score, seq) < 0)
            {
                _rank[i] += x.Span[i];
                x = x.Forward[i]!;
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
                _head.Span[i] = _count;
            }

            _level = lvl;
        }

        var node = new Node(lvl, score, value, seq);

        for (var i = 0; i < lvl; i++)
        {
            node.Forward[i] = _update[i].Forward[i];
            _update[i].Forward[i] = node;

            node.Span[i] = _update[i].Span[i] - (_rank[0] - _rank[i]);
            _update[i].Span[i] = (_rank[0] - _rank[i]) + 1;
        }

        for (var i = lvl; i < _level; i++)
        {
            _update[i].Span[i]++;
        }

        node.Backward = _update[0] == _head ? null : _update[0];

        if (node.Forward[0] != null)
            node.Forward[0]!.Backward = node;

        _map[value] = node;
        _count++;

        return true;
    }

    private void RemoveNode(Node node)
    {
        var x = _head;

        for (var i = _level - 1; i >= 0; i--)
        {
            while (x.Forward[i] is { } forward
                   && CompareNode(forward, node) < 0)
            {
                x = forward;
            }

            _update[i] = x;
        }

        for (var i = 0; i < _level; i++)
        {
            if (_update[i].Forward[i] == node)
            {
                _update[i].Span[i] += node.Span[i] - 1;
                _update[i].Forward[i] = node.Forward[i];
            }
            else
            {
                _update[i].Span[i]--;
            }
        }

        node.Forward[0]?.Backward = node.Backward;

        while (_level > 1 && _head.Forward[_level - 1] == null)
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

        var rank = 0;
        var x = _head;

        for (var i = _level - 1; i >= 0; i--)
        {
            while (x.Forward[i] is { } forward)
            {
                var cmp = CompareNode(forward, node);

                if (cmp > 0)
                    break;

                rank += x.Span[i];

                if (forward == node)
                    return rank - 1;

                x = forward;
            }

            if (x == node)
                return rank - 1;
        }

        return -1;
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
            while (x.Forward[i] is { } forward &&
                   traversed + x.Span[i] <= rank)
            {
                traversed += x.Span[i];
                x = forward;
            }
        }

        x = x.Forward[0]!;

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
            while (x.Forward[i] is { } forward &&
                   traversed + x.Span[i] <= start)
            {
                traversed += x.Span[i];
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
            while (x.Forward[i] is { } forward &&
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
            while (x.Forward[i] != null &&
                   traversed + x.Span[i] <= start)
            {
                traversed += x.Span[i];
                x = x.Forward[i]!;
            }

            _update[i] = x;
        }

        var removed = 0;
        x = _update[0].Forward[0];

        while (x != null && removed < count)
        {
            var next = x.Forward[0];

            for (var i = 0; i < _level; i++)
            {
                if (_update[i].Forward[i] == x)
                {
                    _update[i].Span[i] += x.Span[i] - 1;
                    _update[i].Forward[i] = x.Forward[i];
                }
                else
                {
                    _update[i].Span[i]--;
                }
            }

            x.Forward[0]?.Backward = x.Backward;

            _map.Remove(x.Value);

            _count--;
            removed++;

            x = next;
        }

        while (_level > 1 && _head.Forward[_level - 1] == null)
            _level--;

        return removed;
    }

    public int RemoveByScore(TScore min, TScore max)
    {
        var x = _head;

        for (var i = _level - 1; i >= 0; i--)
        {
            while (x.Forward[i] != null &&
                   _scoreComparer.Compare(x.Forward[i]!.Score, min) < 0)
            {
                x = x.Forward[i]!;
            }

            _update[i] = x;
        }

        var removed = 0;
        x = _update[0].Forward[0];

        while (x != null &&
               _scoreComparer.Compare(x.Score, max) <= 0)
        {
            var next = x.Forward[0];

            for (var i = 0; i < _level; i++)
            {
                if (_update[i].Forward[i] == x)
                {
                    _update[i].Span[i] += x.Span[i] - 1;
                    _update[i].Forward[i] = x.Forward[i];
                }
                else
                {
                    _update[i].Span[i]--;
                }
            }

            x.Forward[0]?.Backward = x.Backward;

            _map.Remove(x.Value);

            _count--;
            removed++;

            x = next;
        }

        while (_level > 1 && _head.Forward[_level - 1] == null)
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
        return new Enumerator(_head, null);
    }

    internal sealed class Node(int level, TScore score, TValue value, long sequence)
    {
        public readonly Node?[] Forward = new Node[level];
        public readonly int[] Span = new int[level];
        public readonly TScore Score = score;
        public readonly TValue Value = value;
        public readonly long Sequence = sequence;
        public Node? Backward;
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
        private readonly Node? _end;
        private Node? _node;

        internal Enumerator(Node? start, Node? end)
        {
            _start = start;
            _end = end;
            _node = start;
        }

        public bool MoveNext()
        {
            _node = _node?.Forward[0];
            return _node != null && _node != _end;
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

            _node = _node.Forward[0];
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
            _node = _node?.Forward[0];

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