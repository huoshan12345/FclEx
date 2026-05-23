#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace FclEx.Benchmarks;

[WarmupCount(3)]
[IterationCount(10)]
[MemoryDiagnoser]
public class OrderedIndexBenchmark
{
    [Params(100, 10000)]
    public int N;

    private OrderedIndex<int, int> _ordered;
    private SortedSet<(int score, int id)> _set;
    private Dictionary<int, int> _map;
    private Random _rng;
    private int[] _ids;

    [GlobalSetup]
    public void Setup()
    {
        _rng = new Random(42);

        _ordered = [];
        _set = [];
        _map = [];

        _ids = new int[N];

        for (var i = 0; i < N; i++)
        {
            _ids[i] = i;

            var score = _rng.Next(1_000_000);

            _ordered.Add(i, score);

            _set.Add((score, i));
            _map[i] = score;
        }
    }

    [Benchmark]
    public void UpdateScore_OrderedIndex()
    {
        var id = _ids[_rng.Next(N)];
        var newScore = _rng.Next(1_000_000);

        _ordered.UpdateScore(id, newScore);
    }

    [Benchmark]
    public void UpdateScore_SortedSet()
    {
        var id = _ids[_rng.Next(N)];
        var newScore = _rng.Next(1_000_000);

        var oldScore = _map[id];

        _set.Remove((oldScore, id));
        _set.Add((newScore, id));

        _map[id] = newScore;
    }

    [Benchmark]
    public int GetRank_OrderedIndex()
    {
        var id = _ids[_rng.Next(N)];
        return _ordered.Rank(id);
    }

    [Benchmark]
    public int GetRank_SortedSet()
    {
        var id = _ids[_rng.Next(N)];
        var score = _map[id];

        var rank = 0;

        foreach (var v in _set)
        {
            if (v == (score, id))
                return rank;

            rank++;
        }

        return -1;
    }

    [Benchmark]
    public int Top10_OrderedIndex()
    {
        var sum = 0;
        var count = 0;

        foreach (var e in _ordered)
        {
            sum += e.Score;

            if (++count == 10)
                break;
        }

        return sum;
    }

    [Benchmark]
    public int Top10_SortedSet()
    {
        var sum = 0;
        var count = 0;

        foreach (var e in _set)
        {
            sum += e.score;

            if (++count == 10)
                break;
        }

        return sum;
    }

    [Benchmark]
    public int LeaderboardWorkload_OrderedIndex()
    {
        var result = 0;

        for (var i = 0; i < 100; i++)
        {
            var r = _rng.Next(100);

            switch (r)
            {
                case < 50:
                {
                    var id = _ids[_rng.Next(N)];
                    var newScore = _rng.Next(1_000_000);

                    _ordered.UpdateScore(id, newScore);
                    break;
                }
                case < 80:
                {
                    var id = _ids[_rng.Next(N)];
                    result += _ordered.Rank(id);
                    break;
                }
                default:
                {
                    var count = 0;

                    foreach (var e in _ordered)
                    {
                        result += e.Score;

                        if (++count == 10)
                            break;
                    }

                    break;
                }
            }
        }

        return result;
    }

    [Benchmark]
    public int LeaderboardWorkload_SortedSet()
    {
        var result = 0;

        for (var i = 0; i < 100; i++)
        {
            var r = _rng.Next(100);

            switch (r)
            {
                case < 50:
                {
                    var id = _ids[_rng.Next(N)];
                    var newScore = _rng.Next(1_000_000);

                    var oldScore = _map[id];

                    _set.Remove((oldScore, id));
                    _set.Add((newScore, id));

                    _map[id] = newScore;
                    break;
                }
                case < 80:
                {
                    var id = _ids[_rng.Next(N)];
                    var score = _map[id];

                    var rank = 0;

                    foreach (var v in _set)
                    {
                        if (v == (score, id))
                        {
                            result += rank;
                            break;
                        }

                        rank++;
                    }

                    break;
                }
                default:
                {
                    var count = 0;

                    foreach (var v in _set)
                    {
                        result += v.score;

                        if (++count == 10)
                            break;
                    }

                    break;
                }
            }
        }

        return result;
    }
}
