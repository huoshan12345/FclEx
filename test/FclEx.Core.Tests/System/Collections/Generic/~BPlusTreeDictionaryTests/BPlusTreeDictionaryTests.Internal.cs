namespace System.Collections.Generic;

partial class BPlusTreeDictionaryTests
{
    [Fact]
    public void Structural_Fuzz_Test()
    {
        var tree = new BPlusTreeDictionary<int, int>(3);
        var dict = new SortedDictionary<int, int>();

        var rand = new Random(0);

        for (var i = 0; i < 50000; i++)
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

            if (i % 500 != 0) 
                continue;

            Assert.Equal(tree, dict);
            BPlusTreeValidator.Validate(tree);
            BPlusTreeValidator.ValidateLeafChain(tree);
        }
    }
}

public static class BPlusTreeValidator
{
    public static void Validate<TKey, TValue>(BPlusTreeDictionary<TKey, TValue> tree)
        where TKey : IComparable<TKey>
    {
        if (tree.Count == 0)
            return;

        var root = tree.Root() ?? throw new InvalidOperationException("root should not be null");

        var leafDepth = -1;

        ValidateNode(
            root,
            isRoot: true,
            depth: 0,
            ref leafDepth,
            tree.MinDegree);
    }

    private static void ValidateNode<TKey, TValue>(
        BPlusTreeDictionary<TKey, TValue>.BPlusTreeNode node,
        bool isRoot,
        int depth,
        ref int leafDepth,
        int t)
        where TKey : IComparable<TKey>
    {
        var keyCount = node.KeyCount;

        if (!isRoot)
        {
            if (keyCount < t - 1 || keyCount > 2 * t - 1)
                throw new Exception("Key count invalid");
        }

        // key order
        for (var i = 1; i < keyCount; i++)
        {
            if (node.Keys[i - 1].CompareTo(node.Keys[i]) >= 0)
                throw new Exception("Keys not sorted");
        }

        if (node.IsLeafNode)
        {
            if (leafDepth == -1)
                leafDepth = depth;
            else if (leafDepth != depth)
                throw new Exception("Leaf depth mismatch");

            return;
        }
        
        foreach (var child in node.Children.Take(node.KeyCount))
        {
            ValidateNode(
                child,
                false,
                depth + 1,
                ref leafDepth,
                t);
        }
    }

    public static void ValidateLeafChain<TKey, TValue>(BPlusTreeDictionary<TKey, TValue> tree) where TKey : notnull
    {
        if (tree.Count == 0)
            return;

        var node = tree.Root() ?? throw new InvalidOperationException("root should not be null");

        while (!node.IsLeafNode)
            node = node.Children[0];

        var list = new List<TKey>();

        while (node != null)
        {
            for (var i = 0; i < node.KeyCount; i++)
                list.Add(node.Keys[i]);

            node = node.Next;
        }

        var enumKeys = tree.Select(x => x.Key);

        if (!list.SequenceEqual(enumKeys))
            throw new Exception("Leaf chain broken");
    }
}

file static class Extensions
{
    public static BPlusTreeDictionary<TKey, TValue>.BPlusTreeNode? Root<TKey, TValue>(this BPlusTreeDictionary<TKey, TValue> tree)
        where TKey : notnull
    {
        var root = (BPlusTreeDictionary<TKey, TValue>.BPlusTreeNode?)tree.GetType().InvokeMember(
            name: "_root",
            invokeAttr: BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
            binder: null,
            target: tree,
            args: null);
        return root;
    }
}