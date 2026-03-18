// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Collections.Generic.Heap;

public partial class HeapTests : TestBase
{
    protected Heap<string> CreateSmallPriorityQueue(out HashSet<string> items)
    {
        items = new HashSet<string>
        {
            "one",
            "two",
            "three",
        };
        var queue = new Heap<string>(items);

        return queue;
    }

    protected Heap<int> CreatePriorityQueue(int initialCapacity, int count)
    {
        var pq = new Heap<int>(initialCapacity);
        for (var i = 0; i < count; i++)
        {
            pq.Push(i);
        }

        return pq;
    }

    [Fact]
    public void Heap_PushPop_Empty()
    {
        var queue = new Heap<string>();

        Assert.Equal("hello", queue.PushPop("hello"));
        Assert.Equal(0, queue.Count);
    }

    [Fact]
    public void Heap_PushPop_SmallerThanMin()
    {
        var queue = CreateSmallPriorityQueue(out var enqueuedItems);

        var actualElement = queue.PushPop("zero");

        Assert.Equal("zero", actualElement);
        Assert.True(enqueuedItems.SetEquals(queue));
    }

    [Fact]
    public void Heap_PushPop_LargerThanMin()
    {
        var queue = CreateSmallPriorityQueue(out _);

        var actualElement = queue.PushPop("four");

        Assert.Equal("one", actualElement);
        Assert.Equal("two", queue.Pop());
        Assert.Equal("three", queue.Pop());
        Assert.Equal("four", queue.Pop());
    }

    [Fact]
    public void Heap_PushPop_EqualToMin()
    {
        var queue = CreateSmallPriorityQueue(out var enqueuedItems);

        var actualElement = queue.PushPop("one-not-to-enqueue");

        Assert.Equal("one-not-to-enqueue", actualElement);
        Assert.True(enqueuedItems.SetEquals(queue));
    }

    [Fact]
    public void Heap_EmptyCollection_PopPush_ShouldThrowInvalidOperationException()
    {
        var queue = new Heap<int>();

        Assert.Equal(0, queue.Count);
        Assert.Throws<InvalidOperationException>(() => queue.PopPush(1));
        Assert.Equal(0, queue.Count);
    }

    [Fact]
    public void Heap_PopPush_SmallerThanMin()
    {
        var queue = CreateSmallPriorityQueue(out _);

        var actualElement = queue.PopPush("zero");

        Assert.Equal("one", actualElement);
        Assert.Equal("zero", queue.Pop());
        Assert.Equal("two", queue.Pop());
        Assert.Equal("three", queue.Pop());
    }

    [Fact]
    public void Heap_PopPush_LargerThanMin()
    {
        var queue = CreateSmallPriorityQueue(out _);

        var actualElement = queue.PopPush("four");

        Assert.Equal("one", actualElement);
        Assert.Equal("two", queue.Pop());
        Assert.Equal("three", queue.Pop());
        Assert.Equal("four", queue.Pop());
    }

    [Fact]
    public void Heap_PopPush_EqualToMin()
    {
        var queue = CreateSmallPriorityQueue(out _);

        var actualElement = queue.PopPush("one-to-replace");

        Assert.Equal("one", actualElement);
        Assert.Equal("one-to-replace", queue.Pop());
        Assert.Equal("two", queue.Pop());
        Assert.Equal("three", queue.Pop());
    }

    [Fact]
    public void Heap_Constructor_IEnumerable_Null()
    {
        var itemsToEnqueue = new[] { null, "one" };
        var queue = new Heap<string?>(itemsToEnqueue);
        Assert.Null(queue.Pop());
        Assert.Equal("one", queue.Pop());
    }

    [Fact]
    public void Heap_Enqueue_Null()
    {
        var queue = new Heap<string?>();

        queue.Push(null);
        queue.Push("zero");
        queue.Push("two");

        Assert.Equal("zero", queue.Pop());
        Assert.Null(queue.Pop());
        Assert.Equal("two", queue.Pop());
    }

    [Fact]
    public void Heap_PushRange_Null()
    {
        var queue = new Heap<string?>();

        queue.PushRange([null, null, null]);
        queue.PushRange(["not null"]);
        queue.PushRange([null, null, null]);

        for (var i = 0; i < 6; ++i)
        {
            Assert.Null(queue.Pop());
        }

        Assert.Equal("not null", queue.Pop());
    }

    [Fact]
    public void Heap_Constructor_Int_Negative_ThrowsArgumentOutOfRangeException()
    {
        AssertExtensions.Throws<ArgumentOutOfRangeException>("initialCapacity", () => new Heap<int>(-1));
        AssertExtensions.Throws<ArgumentOutOfRangeException>("initialCapacity", () => new Heap<int>(int.MinValue));
    }

    [Fact]
    public void Heap_Constructor_Enumerable_Null_ThrowsArgumentNullException()
    {
        AssertExtensions.Throws<ArgumentNullException>("items", () => new Heap<int>(items: null!));
        AssertExtensions.Throws<ArgumentNullException>("items", () => new Heap<int>(items: null!, comparer: Comparer<int>.Default));
    }

    [Fact]
    public void Heap_PushRange_Null_ThrowsArgumentNullException()
    {
        var queue = new Heap<int>();
        AssertExtensions.Throws<ArgumentNullException>("items", () => queue.PushRange(null!));
    }

    [Fact]
    public void Heap_EmptyCollection_Dequeue_ShouldThrowException()
    {
        var queue = new Heap<int>();

        Assert.Equal(0, queue.Count);
        Assert.False(queue.TryPop(out _));
        Assert.Throws<InvalidOperationException>(() => queue.Pop());
    }

    [Fact]
    public void Heap_EmptyCollection_Peek_ShouldReturnFalse()
    {
        var queue = new Heap<int>();

        Assert.False(queue.TryPeek(out _));
        Assert.Throws<InvalidOperationException>(() => queue.Peek());
    }

    #region EnsureCapacity, TrimExcess

    [Fact]
    public void Heap_EnsureCapacity_Negative_ShouldThrowException()
    {
        var queue = new Heap<int>();
        AssertExtensions.Throws<ArgumentOutOfRangeException>("capacity", () => queue.EnsureCapacity(-1));
        AssertExtensions.Throws<ArgumentOutOfRangeException>("capacity", () => queue.EnsureCapacity(int.MinValue));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(0, 5)]
    [InlineData(1, 1)]
    [InlineData(3, 100)]
    public void Heap_TrimExcess_ShouldNotChangeCount(int initialCapacity, int count)
    {
        var queue = CreatePriorityQueue(initialCapacity, count);

        Assert.Equal(count, queue.Count);
        queue.TrimExcess();
        Assert.Equal(count, queue.Count);
    }

    [Theory]
    [MemberData(nameof(ValidPositiveCollectionSizes))]
    public void Heap_TrimExcess_Repeatedly_ShouldNotChangeCount(int count)
    {
        var queue = CreatePriorityQueue(initialCapacity: count, count);

        Assert.Equal(count, queue.Count);
        queue.TrimExcess();
        queue.TrimExcess();
        queue.TrimExcess();
        Assert.Equal(count, queue.Count);
    }

    [Theory]
    [MemberData(nameof(ValidPositiveCollectionSizes))]
    public void Heap_EnsureCapacityAndTrimExcess(int count)
    {
        var itemsToEnqueue = Enumerable.Range(1, count).ToArray();
        var queue = new Heap<int>();
        var expectedCount = 0;
        var random = new Random(Seed: 34);

        foreach (var element in itemsToEnqueue)
        {
            TrimAndEnsureCapacity();
            queue.Push(element);
            expectedCount++;
            Assert.Equal(expectedCount, queue.Count);
        }

        while (expectedCount > 0)
        {
            queue.Pop();
            TrimAndEnsureCapacity();
            expectedCount--;
            Assert.Equal(expectedCount, queue.Count);
        }

        TrimAndEnsureCapacity();
        Assert.Equal(0, queue.Count);

        int GetNextEnsureCapacity()
        {
            return random.Next(0, count * 2);
        }

        void TrimAndEnsureCapacity()
        {
            queue.TrimExcess();

            var capacityAfterEnsureCapacity = queue.EnsureCapacity(GetNextEnsureCapacity());
            Assert.Equal(capacityAfterEnsureCapacity, GetUnderlyingBufferCapacity(queue));

            var capacityAfterTrimExcess = queue.Count < (int)(capacityAfterEnsureCapacity * 0.9) ? queue.Count : capacityAfterEnsureCapacity;
            queue.TrimExcess();
            Assert.Equal(capacityAfterTrimExcess, GetUnderlyingBufferCapacity(queue));
        }
    }

    private static int GetUnderlyingBufferCapacity<T>(Heap<T> queue)
    {
        var field = typeof(Heap<T>).GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(field);
        var array = (T[]?)field.GetValue(queue);
        Assert.NotNull(array);
        return array.Length;
    }

    #endregion

    #region Enumeration

    [Theory]
    [MemberData(nameof(NonModifyingOperations))]
    public void Heap_Enumeration_ValidOnNonModifyingOperation(Action<Heap<int>> nonModifyingOperation, int count)
    {
        var queue = CreatePriorityQueue(initialCapacity: count, count: count);
        using var enumerator = queue.GetEnumerator();
        nonModifyingOperation(queue);
        enumerator.MoveNext();
    }

    [Theory]
    [MemberData(nameof(ModifyingOperations))]
    public void Heap_Enumeration_InvalidationOnModifyingOperation(Action<Heap<int>> modifyingOperation, int count)
    {
        var queue = CreatePriorityQueue(initialCapacity: count, count: count);
        using var enumerator = queue.GetEnumerator();
        modifyingOperation(queue);
        Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
    }

    public static readonly TheoryData<Action<Heap<int>>, int> ModifyingOperations = new()
    {
        (queue => queue.Push(42), 0),
        (queue => queue.Pop(), 5),
        (queue => queue.TryPop(out _), 5),
        (queue => queue.PushPop(5), 5),
        (queue => queue.PushPop(5), 5),
        (queue => queue.PushRange([1]), 0),
        (queue => queue.PushRange([1]), 10),
        (queue => queue.PushRange([1, 2]), 0),
        (queue => queue.PushRange([1, 2]), 10),
        (queue => queue.EnsureCapacity(2 * queue.Count), 4),
        (queue => queue.Clear(), 5),
        (queue => queue.Clear(), 0),
    };

    public static readonly TheoryData<Action<Heap<int>>, int> NonModifyingOperations = new()
    {
        (queue => queue.Peek(), 1),
        (queue => queue.TryPeek(out _), 1),
        (queue => queue.TryPop(out _), 0),
        (queue => queue.PushPop(5), 1),
        (queue => queue.PushPop(5), 0),
        (queue => queue.PushRange([]), 5),
        (queue => queue.PushRange([]), 5),
        (queue => queue.EnsureCapacity(5), 5),
    };

    #endregion
}