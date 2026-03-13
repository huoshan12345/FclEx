#pragma warning disable CA1825 // Avoid zero-length array allocations

namespace System.Collections.Generic;

public partial class OrderedListTestsBasic
{
    // ----------------------------
    // Test Data
    // ----------------------------

    public static TheoryData<int[], int, int, int> BoundCases { get; } = new (int[], int, int, int)[]
    {
        ([], 5, 0, 0),
        ([1, 3, 5, 7], 0, 0, 0),
        ([1, 3, 5, 7], 4, 2, 2),
        ([1, 3, 5, 7], 7, 3, 4),
        ([1, 3, 3, 3, 5], 3, 1, 4),
        ([1, 3, 3, 3, 5], 2, 1, 1)
    }.ToTheoryData();

    public static TheoryData<int[], int, int, int[]> BetweenCases { get; } = new (int[], int, int, int[])[]
    {
        ([], 0, 10, []),
        ([1, 3, 5, 7, 9], 3, 7, [3, 5, 7]),
        ([1, 3, 3, 3, 5], 3, 3, [3, 3, 3]),
        ([1, 3, 5, 7, 9], 4, 8, [5, 7]),
        ([1, 3, 5, 7, 9], 10, 20, []),
    }.ToTheoryData();

    public static TheoryData<int[], int[], int[]> AddRangeCases { get; } = new (int[], int[], int[])[]
    {
        ([5, 1, 3], [2, 4], [1, 2, 3, 4, 5]),
        ([1, 2, 3], [4, 5, 6], [1, 2, 3, 4, 5, 6]),
        ([], [3, 1, 2], [1, 2, 3]),
    }.ToTheoryData();

    // ----------------------------
    // Basic Add / Ordering
    // ----------------------------

    [Fact]
    public void Add_KeepsSortedOrder()
    {
        var list = new OrderedList<int>
        {
            5,
            1,
            3
        };

        Assert.Equal(new[] { 1, 3, 5 }, list.ToArray());
    }

    // ----------------------------
    // AddRange
    // ----------------------------

    [Theory]
    [MemberData(nameof(AddRangeCases))]
    public void AddRange_MergesCorrectly(int[] initial, int[] add, int[] expected)
    {
        var list = new OrderedList<int>();

        foreach (var i in initial)
            list.Add(i);

        list.AddRange(add);

        Assert.Equal(expected, list.ToArray());
    }

    // ----------------------------
    // LowerBound / UpperBound
    // ----------------------------

    [Theory]
    [MemberData(nameof(BoundCases))]
    public void Bounds_Work(int[] source, int value, int lower, int upper)
    {
        var list = new OrderedList<int>();

        foreach (var i in source)
            list.Add(i);

        Assert.Equal(lower, list.LowerBound(value));
        Assert.Equal(upper, list.UpperBound(value));
    }

    // ----------------------------
    // Between
    // ----------------------------

    [Theory]
    [MemberData(nameof(BetweenCases))]
    public void Between_ReturnsExpectedRange(int[] source, int min, int max, int[] expected)
    {
        var list = new OrderedList<int>();

        foreach (var i in source)
            list.Add(i);

        var result = list.Between(min, max).ToArray();

        Assert.Equal(expected, result);
    }

    // ----------------------------
    // IndexOf
    // ----------------------------

    [Fact]
    public void IndexOf_ReturnsCorrectIndex()
    {
        var list = new OrderedList<int>();
        list.AddRange([1, 3, 5]);

        Assert.Equal(1, list.IndexOf(3));
        Assert.Equal(-1, list.IndexOf(2));
    }

    // ----------------------------
    // RemoveAt
    // ----------------------------

    [Fact]
    public void RemoveAt_RemovesCorrectElement()
    {
        var list = new OrderedList<int>();
        list.AddRange([1, 2, 3]);

        list.RemoveAt(1);

        Assert.Equal(new[] { 1, 3 }, list.ToArray());
    }

    // ----------------------------
    // Clear
    // ----------------------------

    [Fact]
    public void Clear_EmptiesCollection()
    {
        var list = new OrderedList<int>();
        list.AddRange([1, 2, 3]);

        list.Clear();

        Assert.Empty(list);
    }

    // ----------------------------
    // CopyTo
    // ----------------------------

    [Fact]
    public void CopyTo_Works()
    {
        var list = new OrderedList<int>();
        list.AddRange([1, 2, 3]);

        var arr = new int[3];
        list.CopyTo(arr, 0);

        Assert.Equal(new[] { 1, 2, 3 }, arr);
    }

    // ----------------------------
    // Unsupported Operations
    // ----------------------------

    [Fact]
    public void Insert_ShouldThrow()
    {
        // ReSharper disable once CollectionNeverQueried.Local
        IList<int> list = new OrderedList<int>();

        Assert.Throws<NotSupportedException>(() =>
            list.Insert(0, 1));
    }

    [Fact]
    public void IndexSetter_ShouldThrow()
    {
        var list = new OrderedList<int> { 1 };

        Assert.Throws<NotSupportedException>(() =>
            list[0] = 5);
    }

    [Fact]
    public void CountOf_And_RemoveAll_Should_Work_Correctly()
    {
        var list = new OrderedList<Item>(new ItemComparer());

        list.AddRange([
            new Item(10, 1),
            new Item(10, 2),
            new Item(10, 3),
            new Item(20, 4),
            new Item(20, 5),
            new Item(30, 6)
        ]);

        // CountOf existing keys
        Assert.Equal(3, list.CountOf(new Item(10, 0)));
        Assert.Equal(2, list.CountOf(new Item(20, 0)));
        Assert.Equal(1, list.CountOf(new Item(30, 0)));

        // CountOf non-existing key
        Assert.Equal(0, list.CountOf(new Item(40, 0)));

        // RemoveAll middle range
        var removed = list.RemoveAll(new Item(20, 0));
        Assert.Equal(2, removed);

        Assert.Equal(
            [1, 2, 3, 6],
            list.Select(x => x.Id)
        );

        // CountOf after removal
        Assert.Equal(0, list.CountOf(new Item(20, 0)));

        // RemoveAll first range
        removed = list.RemoveAll(new Item(10, 0));
        Assert.Equal(3, removed);

        Assert.Equal([6], list.Select(x => x.Id));

        // RemoveAll last element
        removed = list.RemoveAll(new Item(30, 0));
        Assert.Equal(1, removed);

        Assert.Empty(list);

        // RemoveAll on empty list
        removed = list.RemoveAll(new Item(50, 0));
        Assert.Equal(0, removed);
    }

    [Fact]
    public void Fuzz_AllOperations_ShouldMatchReferenceModel()
    {
        var rand = new Random(123);

        var list = new OrderedList<Item>(new ItemComparer());
        var mirror = new List<Item>();

        var id = 0;

        for (var step = 0; step < 20000; step++)
        {
            var op = rand.Next(5);

            if (step == 36)
            {

            }

            switch (op)
            {
                case 0: // Add
                {
                    var item = new Item(rand.Next(0, 10), id++);
                    list.Add(item);
                    mirror.Add(item);
                    break;
                }
                case 1: // AddRange
                {
                    var items = Enumerable.Range(0, rand.Next(1, 5))
                        .Select(_ => new Item(rand.Next(0, 10), id++))
                        .ToArray();

                    list.AddRange(items);
                    mirror.AddRange(items);
                    break;
                }
                case 2 when mirror.Count > 0: // Remove
                {
                    var index = rand.Next(mirror.Count);
                    var item = mirror[index];

                    list.RemoveOne(item);

                    var removeItem = mirror.First(m => m.Key == item.Key);
                    mirror.Remove(removeItem);
                    break;
                }
                case 3: // RemoveAll
                {
                    var key = rand.Next(0, 10);

                    list.RemoveAll(new Item(key, 0));
                    mirror.RemoveAll(x => x.Key == key);
                    break;
                }
                case 4: // CountOf
                {
                    var key = rand.Next(0, 10);

                    var expected = mirror.Count(x => x.Key == key);
                    var actual = list.CountOf(new Item(key, 0));

                    Assert.Equal(expected, actual);
                    break;
                }
            }

            // reference ordering
            var expectedList = mirror
                .OrderBy(x => x.Key)
                .ThenBy(x => x.Id)
                .ToArray();

            var actualList = list.ToArray();

            Assert.Equal(
                expectedList.Select(x => (x.Key, x.Id)),
                actualList.Select(x => (x.Key, x.Id))
            );

            // LowerBound / UpperBound validation
            for (var key = 0; key < 10; key++)
            {
                var lb = list.LowerBound(new Item(key, 0));
                var ub = list.UpperBound(new Item(key, 0));

                for (var i = 0; i < lb; i++)
                    Assert.True(actualList[i].Key < key);

                for (var i = lb; i < ub; i++)
                    Assert.Equal(key, actualList[i].Key);

                for (var i = ub; i < actualList.Length; i++)
                    Assert.True(actualList[i].Key > key);
            }
        }
    }
}