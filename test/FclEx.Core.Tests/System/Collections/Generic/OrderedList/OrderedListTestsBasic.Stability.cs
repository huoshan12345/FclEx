namespace System.Collections.Generic;

partial class OrderedListTestsBasic
{
    [DebuggerDisplay("Key = {Key}, Id = {Id}")]
    private class Item
    {
        public int Key { get; }
        public int Id { get; }

        public Item(int key, int id)
        {
            Key = key;
            Id = id;
        }
    }

    private class ItemComparer : IComparer<Item>
    {
        public int Compare(Item? x, Item? y)
            => x!.Key.CompareTo(y!.Key);
    }

    [Fact]
    public void Add_ShouldPreserveOrder_ForEqualKeys()
    {
        var list = new OrderedList<Item>(new ItemComparer())
        {
            new(3, 1),
            new(3, 2),
            new(3, 3)
        };

        var ids = list.Select(x => x.Id).ToArray();

        Assert.Equal(new[] { 1, 2, 3 }, ids);
    }

    [Fact]
    public void Add_ShouldKeepStableOrder_WhenMixedKeys()
    {
        var list = new OrderedList<Item>(new ItemComparer())
        {
            new(2, 1),
            new(1, 2),
            new(2, 3),
            new(1, 4)
        };

        var result = list.Select(x => (x.Key, x.Id)).ToArray();

        Assert.Equal(new[]
        {
            (1,2),
            (1,4),
            (2,1),
            (2,3)
        }, result);
    }

    [Fact]
    public void AddRange_ShouldPreserveOrder_ForEqualKeys()
    {
        var list = new OrderedList<Item>(new ItemComparer())
        {
            new(2, 1),
            new(2, 2)
        };

        list.AddRange([
            new Item(2,3),
            new Item(2,4)
        ]);

        var ids = list.Select(x => x.Id).ToArray();

        Assert.Equal(new[] { 1, 2, 3, 4 }, ids);
    }

    [Fact]
    public void AddRange_ShouldBeStable_WhenInterleaving()
    {
        var list = new OrderedList<Item>(new ItemComparer())
        {
            new(1, 1),
            new(3, 2)
        };

        list.AddRange([
            new Item(2,3),
            new Item(3,4),
            new Item(3,5)
        ]);

        var result = list.Select(x => (x.Key, x.Id)).ToArray();

        Assert.Equal(new[]
        {
            (1,1),
            (2,3),
            (3,2),
            (3,4),
            (3,5)
        }, result);
    }

    [Fact]
    public void Add_ShouldBeStable_RandomTest()
    {
        var rand = new Random(123);
        var list = new OrderedList<Item>(new ItemComparer());

        var id = 0;

        for (var i = 0; i < 1000; i++)
        {
            var key = rand.Next(0, 10);
            list.Add(new Item(key, id++));
        }

        var groups = list.GroupBy(x => x.Key);

        foreach (var g in groups)
        {
            var ids = g.Select(x => x.Id).ToArray();
            var sorted = ids.OrderBy(x => x).ToArray();

            Assert.Equal(sorted, ids);
        }
    }

    [Fact]
    public void AddRange_ShouldPreserveStability()
    {
        var list = new OrderedList<Item>(new ItemComparer())
        {
            new(10, 1),
            new(10, 2)
        };
        list.AddRange([new Item(10, 3), new Item(10, 4)]);

        var result = list.ToArray();

        Assert.Equal(
            [1, 2, 3, 4],
            result.Select(x => x.Id));
    }

    [Fact]
    public void RandomizedInvariantTest()
    {
        var rand = new Random(123);
        var list = new OrderedList<Item>(new ItemComparer());

        var id = 0;

        for (var i = 0; i < 2000; i++)
        {
            var key = rand.Next(0, 20);
            list.Add(new Item(key, id++));
        }

        var arr = list.ToArray();

        // 1. 排序正确
        for (var i = 1; i < arr.Length; i++)
        {
            Assert.True(arr[i - 1].Key <= arr[i].Key);
        }

        // 2. 稳定性
        var groups = arr.GroupBy(x => x.Key);

        foreach (var g in groups)
        {
            var ids = g.Select(x => x.Id).ToArray();
            var sorted = ids.OrderBy(x => x).ToArray();

            Assert.Equal(sorted, ids);
        }

        // 3. LowerBound / UpperBound
        for (var key = 0; key < 20; key++)
        {
            var lb = list.LowerBound(new Item(key, 0));
            var ub = list.UpperBound(new Item(key, 0));

            for (var i = 0; i < lb; i++)
                Assert.True(arr[i].Key < key);

            for (var i = lb; i < ub; i++)
                Assert.Equal(key, arr[i].Key);

            for (var i = ub; i < arr.Length; i++)
                Assert.True(arr[i].Key > key);
        }
    }
}
