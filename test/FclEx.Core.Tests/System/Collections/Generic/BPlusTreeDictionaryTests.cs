namespace System.Collections.Generic;

public partial class BPlusTreeDictionaryTests
{
    private static BPlusTreeDictionary<TKey, TValue> CreateTree<TKey, TValue>(int degree = 3) where TKey : notnull
    {
        return new BPlusTreeDictionary<TKey, TValue>(degree);
    }

    private static BPlusTreeDictionary<int, string> CreateTree(int degree = 3)
    {
        return CreateTree<int, string>(degree);
    }

    [Fact]
    public void Basic_Add_Get_Remove()
    {
        var tree = CreateTree();

        tree.Add(1, 10.ToString());
        tree.Add(2, 20.ToString());

        Assert.Equal(2, tree.Count);

        Assert.Equal(10.ToString(), tree[1]);

        tree[1] = 15.ToString();
        Assert.Equal(15.ToString(), tree[1]);

        Assert.True(tree.ContainsKey(2));

        Assert.True(tree.Remove(2));
        Assert.False(tree.ContainsKey(2));
    }

    [Fact]
    public void Clear_Test()
    {
        var tree = CreateTree();

        for (var i = 0; i < 1000; i++)
            tree.Add(i, i.ToString());

        tree.Clear();

        Assert.Equal(0, tree.Count);

        tree.Add(1, 1.ToString());
        Assert.Equal(1, tree.Count);
    }

    [Fact]
    public void Enumerator_Order()
    {
        var tree = CreateTree();

        tree.Add(5, 1.ToString());
        tree.Add(1, 1.ToString());
        tree.Add(3, 1.ToString());

        var keys = tree.Select(x => x.Key).ToList();

        Assert.Equal(new[] { 1, 3, 5 }, keys);
    }

    [Fact]
    public void Remove_Every_Other()
    {
        var tree = CreateTree();

        for (var i = 0; i < 10000; i++)
            tree.Add(i, i.ToString());

        for (var i = 0; i < 10000; i += 2)
            tree.Remove(i);

        Assert.Equal(5000, tree.Count);

        AssertSorted(tree);
    }

    [Fact]
    public void Remove_All()
    {
        var tree = CreateTree();

        for (var i = 0; i < 5000; i++)
            tree.Add(i, i.ToString());

        for (var i = 0; i < 5000; i++)
            tree.Remove(i);

        Assert.Equal(0, tree.Count);
    }

    [Fact]
    public void Random_Compare_Test()
    {
        var tree = CreateTree();
        var dict = new SortedDictionary<int, string>();

        var rand = new Random(0);

        for (var i = 0; i < 20000; i++)
        {
            var op = rand.Next(3);
            var k = rand.Next(2000);

            switch (op)
            {
                case 0:
                case 1:
                    var v = rand.Next().ToString();
                    tree[k] = v;
                    dict[k] = v;
                    break;

                case 2:
                    Assert.Equal(dict.Remove(k), tree.Remove(k));
                    break;
            }

            if (i % 200 == 0)
                AssertEqual(tree, dict);
        }

        AssertEqual(tree, dict);
    }

    [Fact]
    public void Heavy_Fuzz_Test()
    {
        var tree = CreateTree();
        var dict = new SortedDictionary<int, string>();

        var rand = new Random(1);

        for (var i = 0; i < 50000; i++)
        {
            var op = rand.Next(4);
            var k = rand.Next(5000);

            switch (op)
            {
                case 0:
                case 1:
                    var v = rand.Next().ToString();
                    tree[k] = v;
                    dict[k] = v;
                    break;

                case 2:
                    Assert.Equal(dict.Remove(k), tree.Remove(k));
                    break;

                case 3:
                    Assert.Equal(dict.ContainsKey(k), tree.ContainsKey(k));
                    break;
            }

            if (i % 1000 == 0)
                Assert.Equal(tree, dict);
        }
    }

    [Fact]
    public void Enumerator_Count_Match()
    {
        var tree = CreateTree();

        for (var i = 0; i < 3000; i++)
            tree.Add(i, i.ToString());

        var count = 0;

        foreach (var _ in tree)
            count++;

        Assert.Equal(tree.Count, count);
    }

    [Fact]
    public void Degree_Variations()
    {
        foreach (var t in new[] { 2, 3, 4, 8, 16 })
        {
            var tree = CreateTree(t);

            var rand = new Random(t);

            for (var i = 0; i < 10000; i++)
            {
                var k = rand.Next(3000);

                if (rand.Next(3) == 0)
                    tree.Remove(k);
                else
                    tree[k] = i.ToString();
            }

            AssertSorted(tree);
        }
    }

    [Fact]
    public void Add_And_Count()
    {
        var tree = CreateTree();

        tree.Add(1, "a");
        tree.Add(2, "b");
        tree.Add(3, "c");

        Assert.Equal(3, tree.Count);
    }

    [Fact]
    public void Add_DuplicateKey_ShouldThrow()
    {
        var tree = CreateTree();

        tree.Add(1, "a");

        Assert.Throws<ArgumentException>(() => tree.Add(1, "b"));
    }

    [Fact]
    public void Indexer_Get_Set()
    {
        var tree = CreateTree();

        tree[1] = "a";
        Assert.Equal("a", tree[1]);

        tree[1] = "b";
        Assert.Equal("b", tree[1]);

        Assert.Equal(1, tree.Count);
    }

    [Fact]
    public void ContainsKey_Works()
    {
        var tree = CreateTree();

        tree.Add(1, "a");

        Assert.True(tree.ContainsKey(1));
        Assert.False(tree.ContainsKey(2));
    }

    [Fact]
    public void TryGetValue_Works()
    {
        var tree = CreateTree();

        tree.Add(1, "a");

        Assert.True(tree.TryGetValue(1, out var v));
        Assert.Equal("a", v);

        Assert.False(tree.TryGetValue(2, out _));
    }

    [Fact]
    public void Remove_Key()
    {
        var tree = CreateTree();

        tree.Add(1, "a");

        Assert.True(tree.Remove(1));
        Assert.False(tree.ContainsKey(1));
        Assert.Equal(0, tree.Count);
    }

    [Fact]
    public void Remove_Key_NotExist()
    {
        var tree = CreateTree();

        tree.Add(1, "a");

        Assert.False(tree.Remove(2));
    }

    [Fact]
    public void Clear_RemovesAll()
    {
        var tree = CreateTree();

        for (var i = 0; i < 100; i++)
            tree.Add(i, i.ToString());

        tree.Clear();

        Assert.Equal(0, tree.Count);
        Assert.Empty(tree);
    }

    [Fact]
    public void Keys_Collection()
    {
        var tree = CreateTree();

        tree.Add(1, "a");
        tree.Add(2, "b");

        var keys = tree.Keys.ToList();

        Assert.Contains(1, keys);
        Assert.Contains(2, keys);
    }

    [Fact]
    public void Values_Collection()
    {
        var tree = CreateTree();

        tree.Add(1, "a");
        tree.Add(2, "b");

        var values = tree.Values.ToList();

        Assert.Contains("a", values);
        Assert.Contains("b", values);
    }

    [Fact]
    public void Enumerator_ReturnsSortedOrder()
    {
        var tree = CreateTree();

        tree.Add(5, "e");
        tree.Add(1, "a");
        tree.Add(3, "c");

        var keys = tree.Select(x => x.Key).ToList();

        Assert.Equal(new[] { 1, 3, 5 }, keys);
    }

    [Fact]
    public void CopyTo_Works()
    {
        var tree = CreateTree();

        tree.Add(1, "a");
        tree.Add(2, "b");

        var array = new KeyValuePair<int, string>[5];

        tree.CopyTo(array, 1);

        Assert.Equal(1, array[1].Key);
        Assert.Equal(2, array[2].Key);
    }

    [Fact]
    public void ICollection_Add_KeyValuePair()
    {
        var tree = CreateTree();

        tree.Add(new KeyValuePair<int, string>(1, "a"));

        Assert.True(tree.ContainsKey(1));
    }

    [Fact]
    public void ICollection_Remove_KeyValuePair()
    {
        var tree = CreateTree();

        var kv = new KeyValuePair<int, string>(1, "a");

        tree.Add(kv);

        Assert.True(tree.Remove(kv));
        Assert.False(tree.ContainsKey(1));
    }

    [Fact]
    public void ICollection_Contains_KeyValuePair()
    {
        var tree = CreateTree();

        var kv = new KeyValuePair<int, string>(1, "a");

        tree.Add(kv);

        Assert.Contains(kv, tree);
        Assert.DoesNotContain(new KeyValuePair<int, string>(1, "b"), tree);
    }

    [Fact]
    public void Large_Insert_Search_Remove()
    {
        var tree = CreateTree();

        var n = 5000;

        for (var i = 0; i < n; i++)
            tree.Add(i, i.ToString());

        Assert.Equal(n, tree.Count);

        for (var i = 0; i < n; i++)
            Assert.True(tree.ContainsKey(i));

        for (var i = 0; i < n; i++)
            Assert.True(tree.Remove(i));

        Assert.Equal(0, tree.Count);
    }

    [Fact]
    public void ContainsValue_Works()
    {
        var tree = CreateTree();

        tree.Add(1, "a");
        tree.Add(2, "b");

        Assert.True(tree.ContainsValue("a"));
        Assert.False(tree.ContainsValue("c"));
    }

    [Fact]
    public void Enumerator_Matches_Count()
    {
        var tree = CreateTree();

        for (var i = 0; i < 100; i++)
            tree.Add(i, i.ToString());

        var count = 0;

        foreach (var _ in tree)
            count++;

        Assert.Equal(tree.Count, count);
    }

    [Fact]
    public void Massive_Insert_Remove()
    {
        var tree = CreateTree();

        const int n = 20000;

        for (var i = 0; i < n; i++)
            tree.Add(i, i.ToString());

        Assert.Equal(n, tree.Count);

        for (var i = 0; i < n; i++)
            Assert.True(tree.ContainsKey(i));

        for (var i = 0; i < n; i++)
            Assert.True(tree.Remove(i));

        Assert.Equal(0, tree.Count);
        Assert.Empty(tree);
    }

    [Fact]
    public void Enumerator_ShouldMatch_Count()
    {
        var tree = CreateTree();

        for (var i = 0; i < 3000; i++)
            tree.Add(i, i.ToString());

        var count = 0;

        // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
        foreach (var _ in tree)
            count++;

        Assert.Equal(tree.Count, count);
    }

    [Fact]
    public void Clear_ShouldResetTree()
    {
        var tree = CreateTree();

        for (var i = 0; i < 5000; i++)
            tree.Add(i, i.ToString());

        tree.Clear();

        Assert.Equal(0, tree.Count);
        Assert.Empty(tree);

        tree.Add(1, 1.ToString());
        Assert.Equal(1, tree.Count);
    }

    [Fact]
    public void ContainsValue_ShouldMatch()
    {
        var tree = CreateTree();

        tree.Add(1, "10");
        tree.Add(2, "20");

        Assert.True(tree.ContainsValue("10"));
        Assert.True(tree.ContainsValue("20"));
        Assert.False(tree.ContainsValue("30"));
    }

    [Fact]
    public void CopyTo_ShouldMatchEnumeration()
    {
        var tree = CreateTree();

        for (var i = 0; i < 100; i++)
            tree.Add(i, i.ToString());

        var arr = new KeyValuePair<int, string>[100];

        tree.CopyTo(arr, 0);

        Assert.Equal(arr, tree);
    }

    [Fact]
    public void Sequential_Insert()
    {
        var tree = CreateTree();

        for (var i = 0; i < 10000; i++)
            tree.Add(i, i.ToString());

        Assert.Equal(10000, tree.Count);
        AssertSorted(tree);
    }

    [Fact]
    public void Reverse_Insert()
    {
        var tree = CreateTree();

        for (var i = 10000; i >= 0; i--)
            tree.Add(i, i.ToString());

        Assert.Equal(10001, tree.Count);
        AssertSorted(tree);
    }

    [Fact]
    public void Insert_Delete_Insert_Again()
    {
        var tree = CreateTree();

        for (var i = 0; i < 5000; i++)
            tree.Add(i, i.ToString());

        for (var i = 0; i < 5000; i++)
            tree.Remove(i);

        Assert.Equal(0, tree.Count);

        for (var i = 0; i < 5000; i++)
            tree.Add(i, i.ToString());

        Assert.Equal(5000, tree.Count);

        AssertSorted(tree);
    }

    [Fact]
    public void Remove_From_Middle()
    {
        var tree = CreateTree();

        for (var i = 0; i < 10000; i++)
            tree.Add(i, i.ToString());

        for (var i = 3000; i < 7000; i++)
            Assert.True(tree.Remove(i));

        Assert.Equal(6000, tree.Count);

        AssertSorted(tree);
    }

    [Fact]
    public void Remove_Every_Other_Key()
    {
        var tree = CreateTree();

        for (var i = 0; i < 10000; i++)
            tree.Add(i, i.ToString());

        for (var i = 0; i < 10000; i += 2)
            tree.Remove(i);

        Assert.Equal(5000, tree.Count);

        AssertSorted(tree);
    }

    [Fact]
    public void Remove_All_Root_Shrink()
    {
        var tree = CreateTree();

        for (var i = 0; i < 2000; i++)
            tree.Add(i, i.ToString());

        for (var i = 0; i < 2000; i++)
            tree.Remove(i);

        Assert.Equal(0, tree.Count);

        tree.Add(1, 1.ToString());
        Assert.Equal(1, tree.Count);
    }

    [Fact]
    public void Keys_And_Values_Stay_In_Sync()
    {
        var tree = CreateTree();

        for (var i = 0; i < 1000; i++)
            tree.Add(i, (i * 10).ToString());

        var keys = tree.Keys.ToList();
        var values = tree.Values.ToList();

        Assert.Equal(keys.Count, values.Count);

        for (var i = 0; i < keys.Count; i++)
        {
            Assert.Equal((keys[i] * 10).ToString(), values[i]);
        }
    }

    [Fact]
    public void Enumerator_Matches_Keys()
    {
        var tree = CreateTree();

        for (var i = 0; i < 1000; i++)
            tree.Add(i, i.ToString());

        var enumKeys = tree.Select(x => x.Key);
        var keys = tree.Keys;

        Assert.True(enumKeys.SequenceEqual(keys));
    }

    [Fact]
    public void Range_Scan_Test()
    {
        var tree = new BPlusTreeDictionary<int, int>(3);

        for (var i = 0; i < 10000; i++)
            tree.Add(i, i);

        var range = tree
            .Where(x => x.Key is >= 2000 and <= 4000)
            .ToList();

        Assert.Equal(2001, range.Count);

        for (var i = 0; i < range.Count; i++)
        {
            Assert.Equal(2000 + i, range[i].Key);
        }
    }

    [Fact]
    public void Degree2_Test()
    {
        var tree = new BPlusTreeDictionary<int, int>(2);

        for (var i = 0; i < 5000; i++)
            tree.Add(i, i);

        for (var i = 0; i < 5000; i++)
            tree.Remove(i);

        Assert.Equal(0, tree.Count);
    }
}
