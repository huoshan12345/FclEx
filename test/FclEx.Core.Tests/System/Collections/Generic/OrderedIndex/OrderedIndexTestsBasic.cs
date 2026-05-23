namespace System.Collections.Generic.OrderedIndex;

public class OrderedIndexTestsBasic
{
    [Fact]
    public void Add_ShouldIncreaseCount()
    {
        var idx = new OrderedIndex<int, string>
        {
            { 10, "a" },
            { 20, "b" }
        };

        Assert.Equal(2, idx.Count);
    }

    [Fact]
    public void Contains_ShouldWork()
    {
        var idx = new OrderedIndex<int, string> { { 1, "a" } };

        Assert.True(idx.Contains("a"));
        Assert.False(idx.Contains("b"));
    }

    [Fact]
    public void TryGetScore_ShouldWork()
    {
        var idx = new OrderedIndex<int, string> { { 5, "a" } };

        Assert.True(idx.TryGetScore("a", out var s));
        Assert.Equal(5, s);
    }

    [Fact]
    public void Remove_ShouldWork()
    {
        var idx = new OrderedIndex<int, string>
        {
            { 1, "a" },
            { 2, "b" }
        };

        Assert.True(idx.Remove("a"));

        Assert.False(idx.Contains("a"));
        Assert.Equal(1, idx.Count);
    }

    [Fact]
    public void Rank_ShouldWork()
    {
        var idx = new OrderedIndex<int, string>
        {
            { 10, "a" },
            { 20, "b" },
            { 30, "c" }
        };

        Assert.Equal(0, idx.Rank("a"));
        Assert.Equal(1, idx.Rank("b"));
        Assert.Equal(2, idx.Rank("c"));
    }

    [Fact]
    public void TryGetByRank_ShouldWork()
    {
        var idx = new OrderedIndex<int, string>
        {
            { 10, "a" },
            { 20, "b" }
        };

        Assert.True(idx.TryGetByRank(1, out var item));

        Assert.Equal("b", item.Value);
        Assert.Equal(20, item.Score);
    }

    [Fact]
    public void RangeByRank_ShouldWork()
    {
        var idx = new OrderedIndex<int, int>();

        for (var i = 0; i < 10; i++)
            idx.Add(i, i);

        var r = idx.RangeByRank(3, 4).Select(x => x.Value).ToArray();

        Assert.Equal(new[] { 3, 4, 5, 6 }, r);
    }

    [Fact]
    public void RangeByScore_ShouldWork()
    {
        var idx = new OrderedIndex<int, int>();

        for (var i = 0; i < 10; i++)
            idx.Add(i, i);

        var r = idx.RangeByScore(3, 6).Select(x => x.Value).ToArray();

        Assert.Equal(new[] { 3, 4, 5, 6 }, r);
    }

    [Fact]
    public void UpdateScore_ShouldReorder()
    {
        var idx = new OrderedIndex<int, string>
        {
            { 10, "a" },
            { 20, "b" }
        };

        idx.UpdateScore("a", 30);

        Assert.Equal(1, idx.Rank("a"));
        Assert.Equal(0, idx.Rank("b"));
    }

    [Fact]
    public void StableSort_ShouldPreserveInsertionOrder()
    {
        var idx = new OrderedIndex<int, string>
        {
            { 10, "a" },
            { 10, "b" },
            { 10, "c" }
        };

        var r = idx.Select(x => x.Value).ToArray();

        Assert.Equal(new[] { "a", "b", "c" }, r);
    }

    [Fact]
    public void RemoveByRank_ShouldWork()
    {
        var idx = new OrderedIndex<int, int>();

        for (var i = 0; i < 10; i++)
            idx.Add(i, i);

        var removed = idx.RemoveByRank(3, 3);

        Assert.Equal(3, removed);

        var r = idx.Select(x => x.Value).ToArray();

        Assert.Equal(new[] { 0, 1, 2, 6, 7, 8, 9 }, r);
    }

    [Fact]
    public void RemoveByScore_ShouldWork()
    {
        var idx = new OrderedIndex<int, int>();

        for (var i = 0; i < 10; i++)
            idx.Add(i, i);

        var removed = idx.RemoveByScore(3, 6);

        Assert.Equal(4, removed);

        var r = idx.Select(x => x.Value).ToArray();

        Assert.Equal(new[] { 0, 1, 2, 7, 8, 9 }, r);
    }

    [Fact]
    public void Enumeration_ShouldBeOrdered()
    {
        var idx = new OrderedIndex<int, int>
        {
            { 5, 5 },
            { 1, 1 },
            { 3, 3 }
        };

        var r = idx.Select(x => x.Value).ToArray();

        Assert.Equal(new[] { 1, 3, 5 }, r);
    }

    [Fact]
    public void DuplicateValue_ShouldFail()
    {
        var idx = new OrderedIndex<int, string> { { 1, "a" } };

        Assert.False(idx.Add(2, "a"));
    }

    [Fact]
    public void RandomizedStressTest()
    {
        var idx = new OrderedIndex<int, int>();

        var rnd = new Random(1);

        for (var i = 0; i < 5000; i++)
        {
            var score = rnd.Next(10000);

            idx.Add(score, i);
        }

        var arr = idx.ToArray();

        for (var i = 1; i < arr.Length; i++)
        {
            Assert.True(arr[i - 1].Score <= arr[i].Score);
        }
    }

    [Fact]
    public void Rank_NotFound_ShouldReturnMinus1()
    {
        var idx = new OrderedIndex<int, string>();

        idx.Add(1, "a");

        Assert.Equal(-1, idx.Rank("b"));
    }

    [Fact]
    public void TryGetByRank_OutOfRange()
    {
        var idx = new OrderedIndex<int, int>();

        idx.Add(1, 1);

        Assert.False(idx.TryGetByRank(-1, out _));
        Assert.False(idx.TryGetByRank(1, out _));
    }

    [Fact]
    public void RangeByRank_EmptyRange()
    {
        var idx = new OrderedIndex<int, int>();

        for (var i = 0; i < 5; i++)
            idx.Add(i, i);

        var r = idx.RangeByRank(10, 5).ToArray();

        Assert.Empty(r);
    }

    [Fact]
    public void RangeByScore_NoMatch()
    {
        var idx = new OrderedIndex<int, int>();

        for (var i = 0; i < 5; i++)
            idx.Add(i, i);

        var r = idx.RangeByScore(100, 200).ToArray();

        Assert.Empty(r);
    }

    [Fact]
    public void RemoveByRank_RemoveAll()
    {
        var idx = new OrderedIndex<int, int>();

        for (var i = 0; i < 10; i++)
            idx.Add(i, i);

        idx.RemoveByRank(0, 100);

        Assert.Empty(idx);
    }

    [Fact]
    public void RemoveByScore_RemoveAll()
    {
        var idx = new OrderedIndex<int, int>();

        for (var i = 0; i < 10; i++)
            idx.Add(i, i);

        idx.RemoveByScore(int.MinValue, int.MaxValue);

        Assert.Empty(idx);
    }

    [Fact]
    public void UpdateScore_ToSameScore_ShouldMoveToEndOfStableGroup()
    {
        var idx = new OrderedIndex<int, string>();

        idx.Add(10, "a");
        idx.Add(10, "b");
        idx.Add(10, "c");

        idx.UpdateScore("a", 10);

        var r = idx.Select(x => x.Value).ToArray();

        Assert.Equal(new[] { "b", "c", "a" }, r);
    }

    [Fact]
    public void UpdateScore_MoveToFront()
    {
        var idx = new OrderedIndex<int, string>();

        idx.Add(10, "a");
        idx.Add(20, "b");
        idx.Add(30, "c");

        idx.UpdateScore("c", 5);

        var r = idx.Select(x => x.Value).ToArray();

        Assert.Equal(new[] { "c", "a", "b" }, r);
    }

    [Fact]
    public void Enumeration_ShouldBeStableForSameScore()
    {
        var idx = new OrderedIndex<int, int>();

        idx.Add(10, 1);
        idx.Add(10, 2);
        idx.Add(10, 3);

        var r = idx.Select(x => x.Value).ToArray();

        Assert.Equal(new[] { 1, 2, 3 }, r);
    }

    [Fact]
    public void Clear_ShouldResetStructure()
    {
        var idx = new OrderedIndex<int, int>();

        for (var i = 0; i < 10; i++)
            idx.Add(i, i);

        idx.Clear();

        Assert.Empty(idx);

        idx.Add(1, 1);

        Assert.Single(idx);
    }

    [Fact]
    public void RandomInsertRemoveStress()
    {
        var idx = new OrderedIndex<int, int>();

        var rnd = new Random(123);

        var map = new Dictionary<int, int>();

        for (var i = 0; i < 2000; i++)
        {
            var v = rnd.Next(5000);
            var s = rnd.Next(1000);

            if (!map.ContainsKey(v))
            {
                idx.Add(s, v);
                map[v] = s;
            }
        }

        for (var i = 0; i < 1000; i++)
        {
            var v = rnd.Next(5000);

            idx.Remove(v);
            map.Remove(v);
        }

        var arr = idx.ToArray();

        for (var i = 1; i < arr.Length; i++)
            Assert.True(arr[i - 1].Score <= arr[i].Score);
    }

    [Fact]
    public void Rank_ShouldMatchEnumeration()
    {
        var idx = new OrderedIndex<int, int>();

        for (var i = 0; i < 100; i++)
            idx.Add(i, i);

        var r = 0;

        foreach (var item in idx)
        {
            Assert.Equal(r, idx.Rank(item.Value));
            r++;
        }
    }

    [Fact]
    public void Rank_FastPath()
    {
        var idx = new OrderedIndex<int, int>
        {
            { 10, 1 },
            { 20, 2 },
            { 30, 3 }
        };

        Assert.Equal(0, idx.Rank(1));
        Assert.Equal(1, idx.Rank(2));
        Assert.Equal(2, idx.Rank(3));
    }

    [Fact]
    public void TryGetByRank_ShouldMatchEnumeration()
    {
        var idx = new OrderedIndex<int, int>();

        for (var i = 0; i < 100; i++)
            idx.Add(i, i);

        for (var i = 0; i < 100; i++)
        {
            idx.TryGetByRank(i, out var item);

            Assert.Equal(i, item.Value);
        }
    }

    [Fact]
    public void MassiveRandomTest()
    {
        var idx = new OrderedIndex<int, int>();

        var rnd = new Random(1);

        for (var i = 0; i < 20000; i++)
        {
            idx.Add(rnd.Next(), i);
        }

        var arr = idx.ToArray();

        for (var i = 1; i < arr.Length; i++)
            Assert.True(arr[i - 1].Score <= arr[i].Score);
    }

    [Fact]
    public void ModelConsistency_RangeByScore()
    {
        var idx = new OrderedIndex<int, int>();

        var model = new List<(int Score, int Value)>();

        var rnd = new Random(2);

        for (var i = 0; i < 2000; i++)
        {
            var s = rnd.Next(1000);
            idx.Add(s, i);
            model.Add((s, i));
        }

        model.Sort((a, b) =>
        {
            var c = a.Score.CompareTo(b.Score);
            return c != 0
                ? c
                : a.Value.CompareTo(b.Value);
        });

        for (var i = 0; i < 200; i++)
        {
            var min = rnd.Next(1000);
            var max = rnd.Next(1000);

            if (min > max)
                (min, max) = (max, min);

            var r1 = idx.RangeByScore(min, max).ToArray();

            var r2 = model
                .Where(x => x.Score >= min && x.Score <= max)
                .ToArray();

            Assert.Equal(r2.Length, r1.Length);

            for (var j = 0; j < r1.Length; j++)
            {
                Assert.Equal(r2[j].Value, r1[j].Value);
            }
        }
    }

    [Fact]
    public void RankScoreConsistency()
    {
        var idx = new OrderedIndex<int, int>();

        var rnd = new Random(3);

        for (var i = 0; i < 2000; i++)
            idx.Add(rnd.Next(1000), i);

        var arr = idx.ToArray();

        for (var i = 0; i < arr.Length; i++)
        {
            var v = arr[i].Value;
            var r = idx.Rank(v);
            idx.TryGetByRank(r, out var item);

            Assert.Equal(v, item.Value);
        }
    }

    [Fact]
    public void Enumerator_ShouldIterateInOrder()
    {
        var idx = new OrderedIndex<int, int>();

        for (var i = 0; i < 10; i++)
            idx.Add(i, i);

        var result = new List<int>();

        foreach (var (_, v) in idx)
            result.Add(v);

        Assert.Equal(Enumerable.Range(0, 10), result);
    }

    [Fact]
    public void Enumerator_Empty()
    {
        var idx = new OrderedIndex<int, int>();
        var count = 0;

        // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
        foreach (var _ in idx)
            count++;

        Assert.Equal(0, count);
    }

    [Fact]
    public void Enumerator_AfterRemove()
    {
        var idx = new OrderedIndex<int, int>();

        for (var i = 0; i < 10; i++)
            idx.Add(i, i);

        idx.Remove(5);
        idx.Remove(7);

        var result = new List<int>();

        foreach (var (_, v) in idx)
            result.Add(v);

        Assert.Equal(new[] { 0, 1, 2, 3, 4, 6, 8, 9 }, result);
    }

    [Fact]
    public void Enumerator_ShouldMatchModel()
    {
        var idx = new OrderedIndex<int, int>();
        var model = new List<(int score, int value)>();

        var rand = new Random(123);

        for (var i = 0; i < 1000; i++)
        {
            var score = rand.Next(10000);
            var value = i;

            idx.Add(score, value);
            model.Add((score, value));
        }

        model.Sort((a, b) => a.score.CompareTo(b.score));

        var enumerated = idx.Select(x => x.Score).ToList();
        var expected = model.Select(x => x.score).ToList();

        Assert.Equal(expected, enumerated);
    }

    [Fact]
    public void Enumerator_RandomStress()
    {
        var idx = new OrderedIndex<int, int>();

        var rand = new Random(42);

        for (var i = 0; i < 10000; i++)
            idx.Add(rand.Next(), i);

        var prev = int.MinValue;

        foreach (var (score, _) in idx)
        {
            Assert.True(score >= prev);
            prev = score;
        }
    }

    [Fact]
    public void Enumerator_OrderIsStable()
    {
        var idx = new OrderedIndex<int, int>
        {
            { 10, 1 },
            { 5, 2 },
            { 20, 3 }
        };

        var scores = idx.Select(x => x.Score).ToArray();

        Assert.Equal(new[] { 5, 10, 20 }, scores);
    }

    private sealed record ModelItem(int Score, int Value, long Seq);

    [Fact]
    public void ModelConsistency_RandomOperations()
    {
        var idx = new OrderedIndex<int, int>();

        var model = new List<ModelItem>();

        var rnd = new Random(1);

        var nextValue = 0;
        long seq = 0;

        for (var step = 0; step < 10000; step++)
        {
            var op = rnd.Next(4);

            switch (op)
            {
                case 0: // Add
                {
                    var score = rnd.Next(1000);
                    var value = nextValue++;
                    idx.Add(score, value);
                    model.Add(new ModelItem(score, value, seq++));

                    break;
                }
                case 1: // Remove
                {
                    if (model.Count == 0)
                        break;

                    var i = rnd.Next(model.Count);
                    var item = model[i];
                    idx.Remove(item.Value);
                    model.RemoveAt(i);

                    break;
                }
                case 2: // UpdateScore
                {
                    if (model.Count == 0)
                        break;

                    var i = rnd.Next(model.Count);
                    var item = model[i];
                    var newScore = rnd.Next(1000);
                    idx.UpdateScore(item.Value, newScore);
                    model.RemoveAt(i);
                    model.Add(new ModelItem(newScore, item.Value, seq++));

                    break;
                }
                case 3: // RemoveByRank
                {
                    if (model.Count == 0)
                        break;

                    var start = rnd.Next(model.Count);
                    var count = rnd.Next(5);
                    idx.RemoveByRank(start, count);
                    var removeCount = Math.Min(count, model.Count - start);
                    model.RemoveRange(start, removeCount);

                    break;
                }
            }

            SortModel(model);

            Validate(idx, model);
        }
    }

    private static void SortModel(List<ModelItem> model)
    {
        model.Sort((a, b) =>
        {
            var c = a.Score.CompareTo(b.Score);
            if (c != 0)
                return c;

            return a.Seq.CompareTo(b.Seq);
        });
    }

    private static void Validate(OrderedIndex<int, int> idx, List<ModelItem> model)
    {
        var arr = idx.ToArray();

        Assert.Equal(model.Count, arr.Length);

        for (var i = 0; i < arr.Length; i++)
        {
            Assert.Equal(model[i].Score, arr[i].Score);
            Assert.Equal(model[i].Value, arr[i].Value);

            var rank = idx.Rank(arr[i].Value);

            if (rank != i)
            {
                throw new Exception(
                    $"Rank mismatch value={arr[i].Value} expected={i} actual={rank}"
                );
            }
        }

        for (var i = 0; i < arr.Length; i++)
        {
            idx.TryGetByRank(i, out var item);

            Assert.Equal(arr[i].Value, item.Value);
            Assert.Equal(arr[i].Score, item.Score);
        }
    }
}
