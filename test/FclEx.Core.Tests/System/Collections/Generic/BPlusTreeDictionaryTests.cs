namespace System.Collections.Generic;

public class BPlusTreeDictionaryTests
{
    private static BPlusTreeDictionary<int, string> CreateTree()
    {
        return new BPlusTreeDictionary<int, string>(3);
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

        for (int i = 0; i < 100; i++)
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

        Assert.True(tree.Contains(kv));
        Assert.False(tree.Contains(new KeyValuePair<int, string>(1, "b")));
    }

    [Fact]
    public void Large_Insert_Search_Remove()
    {
        var tree = CreateTree();

        int n = 5000;

        for (int i = 0; i < n; i++)
            tree.Add(i, i.ToString());

        Assert.Equal(n, tree.Count);

        for (int i = 0; i < n; i++)
            Assert.True(tree.ContainsKey(i));

        for (int i = 0; i < n; i++)
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

        for (int i = 0; i < 100; i++)
            tree.Add(i, i.ToString());

        int count = 0;

        foreach (var _ in tree)
            count++;

        Assert.Equal(tree.Count, count);
    }
}
