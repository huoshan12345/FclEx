#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace FclEx.Benchmarks;

[MemoryDiagnoser]
public class OrderedIndexBenchmark
{
    [Params(1000, 10000, 100000)]
    public int N;

    private OrderedIndex<int, int> _idx = null!;
    private List<(int score, int value)> _list = null!;
    private Random _rand = null!;

    [GlobalSetup]
    public void Setup()
    {
        _rand = new Random(42);

        _idx = new OrderedIndex<int, int>();
        _list = new List<(int, int)>(N);

        for (var i = 0; i < N; i++)
        {
            _idx.Add(i, i);
            _list.Add((i, i));
        }
    }

    // -------------------------
    // Rank
    // -------------------------

    [Benchmark]
    public int Rank_OrderedIndex()
    {
        var v = _rand.Next(N);
        return _idx.Rank(v);
    }

    [Benchmark]
    public int Rank_ListBinarySearch()
    {
        var v = _rand.Next(N);

        return _list.BinarySearch(
            (v, v),
            Comparer<(int score, int value)>.Create((a, b) => a.score.CompareTo(b.score)));
    }

    // -------------------------
    // Insert random
    // -------------------------

    [Benchmark]
    public void Insert_OrderedIndex()
    {
        var v = _rand.Next();

        _idx.Add(v, v);
        _idx.Remove(v);
    }

    [Benchmark]
    public void Insert_List()
    {
        var v = _rand.Next();

        var pos = _list.BinarySearch(
            (v, v),
            Comparer<(int, int)>.Create((a, b) => a.Item1.CompareTo(b.Item1)));

        if (pos < 0) pos = ~pos;

        _list.Insert(pos, (v, v));
        _list.RemoveAt(pos);
    }

    // -------------------------
    // Remove random
    // -------------------------

    [Benchmark]
    public void Remove_OrderedIndex()
    {
        var v = _rand.Next(N);

        _idx.Remove(v);
        _idx.Add(v, v);
    }

    [Benchmark]
    public void Remove_List()
    {
        var v = _rand.Next(N);

        var pos = _list.BinarySearch(
            (v, v),
            Comparer<(int, int)>.Create((a, b) => a.Item1.CompareTo(b.Item1)));

        if (pos >= 0)
        {
            _list.RemoveAt(pos);
            _list.Insert(pos, (v, v));
        }
    }

    // -------------------------
    // Leaderboard workload
    // -------------------------

    [Benchmark]
    public void Leaderboard_OrderedIndex()
    {
        var user = _rand.Next(N);

        _idx.Remove(user);

        var score = _rand.Next();

        _idx.Add(score, user);

        _idx.Rank(user);
    }

    [Benchmark]
    public void Leaderboard_List()
    {
        var user = _rand.Next(N);

        var pos = _list.BinarySearch(
            (user, user),
            Comparer<(int, int)>.Create((a, b) => a.Item1.CompareTo(b.Item1)));

        if (pos >= 0)
            _list.RemoveAt(pos);

        var score = _rand.Next();

        var insert = _list.BinarySearch(
            (score, user),
            Comparer<(int, int)>.Create((a, b) => a.Item1.CompareTo(b.Item1)));

        if (insert < 0) insert = ~insert;

        _list.Insert(insert, (score, user));

        _ = _list.BinarySearch(
            (score, user),
            Comparer<(int, int)>.Create((a, b) => a.Item1.CompareTo(b.Item1)));
    }
}
