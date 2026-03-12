namespace System.Collections.Generic;

partial class BPlusTreeDictionaryTests
{
    private static void AssertEqual<TKey, TValue>(
        BPlusTreeDictionary<TKey, TValue> tree,
        SortedDictionary<TKey, TValue> dict)
        where TKey : notnull
    {
        Assert.Equal(dict.Count, tree.Count);

        // Enumerator 顺序一致
        var treeList = tree.ToList();
        var dictList = dict.ToList();

        Assert.Equal(dictList.Count, treeList.Count);

        for (var i = 0; i < dictList.Count; i++)
        {
            Assert.Equal(dictList[i].Key, treeList[i].Key);
            Assert.Equal(dictList[i].Value, treeList[i].Value);
        }

        // Keys
        Assert.True(tree.Keys.SequenceEqual(dict.Keys));

        // Values
        Assert.True(tree.Values.SequenceEqual(dict.Values));

        // ContainsKey / TryGetValue
        foreach (var (k, v) in dict)
        {
            Assert.True(tree.ContainsKey(k));
            Assert.True(tree.TryGetValue(k, out var tv));
            Assert.Equal(v, tv);
        }
    }

    private static void AssertSorted<TKey, TValue>(BPlusTreeDictionary<TKey, TValue> tree, IComparer<TKey>? comparer = null)
        where TKey : IMinMaxValue<TKey>
    {
        comparer ??= Comparer<TKey>.Default;
        var prev = TKey.MinValue;

        foreach (var kv in tree)
        {
            Assert.True(comparer.Compare(kv.Key, prev) > 0);
            prev = kv.Key;
        }
    }

    [Fact]
    public void Random_Insert_Remove_Update_Compare_With_SortedDictionary()
    {
        var tree = CreateTree();
        var dict = new SortedDictionary<int, string>();

        var rand = new Random(1234);

        const int operations = 20000;

        for (var i = 0; i < operations; i++)
        {
            var op = rand.Next(4);
            var key = rand.Next(2000);
            var value = rand.NextString(10);

            switch (op)
            {
                // insert/update
                case 0:
                case 1:
                    tree[key] = value;
                    dict[key] = value;
                    break;

                // remove
                case 2:
                    Assert.Equal(dict.Remove(key), tree.Remove(key));
                    break;

                // lookup
                case 3:
                    Assert.Equal(dict.ContainsKey(key), tree.ContainsKey(key));

                    if (dict.TryGetValue(key, out var dv))
                    {
                        Assert.True(tree.TryGetValue(key, out var tv));
                        Assert.Equal(dv, tv);
                    }
                    else
                    {
                        Assert.False(tree.TryGetValue(key, out _));
                    }
                    break;
            }

            if (i % 200 == 0)
                AssertEqual(tree, dict);
        }

        AssertEqual(tree, dict);
    }

    [Fact]
    public void Random_Insert_Order_ShouldRemainSorted()
    {
        var tree = CreateTree();
        var rand = new Random(42);

        for (var i = 0; i < 10000; i++)
        {
            tree[rand.Next(100000)] = i.ToString();
        }

        var prev = int.MinValue;

        foreach (var kv in tree)
        {
            Assert.True(kv.Key >= prev);
            prev = kv.Key;
        }
    }

    [Fact]
    public void Random_Remove_All()
    {
        var tree = CreateTree();
        var rand = new Random(7);

        var list = Enumerable.Range(0, 5000).ToList();

        foreach (var i in list)
            tree.Add(i, i.ToString());

        list = list.OrderBy(_ => rand.Next()).ToList();

        foreach (var i in list)
        {
            Assert.True(tree.Remove(i));
        }

        Assert.Equal(0, tree.Count);
    }

    [Fact]
    public void Fuzz_Test()
    {
        var tree = new BPlusTreeDictionary<int, int>(3);
        var dict = new SortedDictionary<int, int>();

        var rand = new Random(0);

        for (var i = 0; i < 100000; i++)
        {
            var op = rand.Next(3);
            var k = rand.Next(5000);

            switch (op)
            {
                case 0:
                    var v = rand.Next();
                    tree[k] = v;
                    dict[k] = v;
                    break;

                case 1:
                    Assert.Equal(dict.Remove(k), tree.Remove(k));
                    break;

                case 2:
                    Assert.Equal(dict.ContainsKey(k), tree.ContainsKey(k));
                    break;
            }

            if (i % 1000 == 0)
            {
                Assert.Equal(dict, tree);
            }
        }
    }

    [Fact]
    public void Enumerator_After_Many_Operations()
    {
        var tree = CreateTree();
        var rand = new Random(0);

        for (var i = 0; i < 20000; i++)
        {
            var k = rand.Next(5000);

            if (rand.Next(2) == 0)
                tree[k] = i.ToString();
            else
                tree.Remove(k);
        }

        AssertSorted(tree);
    }

    [Fact]
    public void Stress_Different_Degrees()
    {
        foreach (var t in new[] { 2, 3, 4, 8, 16 })
        {
            var tree = CreateTree(t);

            var rand = new Random(t);

            for (var i = 0; i < 10000; i++)
            {
                var k = rand.Next(5000);

                if (rand.Next(3) == 0)
                    tree.Remove(k);
                else
                    tree[k] = i.ToString();
            }

            AssertSorted(tree);
        }
    }
}