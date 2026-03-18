#pragma warning disable xUnit1044 // Avoid using TheoryData type arguments that are not serializable
namespace System.Collections.Generic.Heap;

public partial class HeapTests : TestBase
{
    protected Heap<int> CreateSmallHeap(out HashSet<int> items)
    {
        items = new HashSet<int>
        {
            1,
            2,
            3,
        };
        var heap = new Heap<int>(items);
        return heap;
    }

    protected Heap<int> CreateHeap(int initialCapacity, int count)
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
        var heap = new Heap<string>();

        Assert.Equal("hello", heap.PushPop("hello"));
        Assert.Equal(0, heap.Count);
    }

    [Fact]
    public void Heap_PushPop_SmallerThanMin()
    {
        var heap = CreateSmallHeap(out var items);

        var actualElement = heap.PushPop(0);

        Assert.Equal(0, actualElement);
        Assert.True(items.SetEquals(heap));
    }

    [Fact]
    public void Heap_PushPop_LargerThanMin()
    {
        var heap = CreateSmallHeap(out _);

        var actualElement = heap.PushPop(4);

        Assert.Equal(1, actualElement);
        Assert.Equal(2, heap.Pop());
        Assert.Equal(3, heap.Pop());
        Assert.Equal(4, heap.Pop());
    }

    [Fact]
    public void Heap_PushPop_EqualToMin()
    {
        var heap = CreateSmallHeap(out var items);

        var actualElement = heap.PushPop(1);

        Assert.Equal(1, actualElement);
        Assert.True(items.SetEquals(heap));
    }

    [Fact]
    public void Heap_EmptyCollection_PopPush_ShouldThrowInvalidOperationException()
    {
        var heap = new Heap<int>();

        Assert.Equal(0, heap.Count);
        Assert.Throws<InvalidOperationException>(() => heap.PopPush(1));
        Assert.Equal(0, heap.Count);
    }

    [Fact]
    public void Heap_PopPush_SmallerThanMin()
    {
        var heap = CreateSmallHeap(out _);

        var actualElement = heap.PopPush(0);

        Assert.Equal(1, actualElement);
        Assert.Equal(0, heap.Pop());
        Assert.Equal(2, heap.Pop());
        Assert.Equal(3, heap.Pop());
    }

    [Fact]
    public void Heap_PopPush_LargerThanMin()
    {
        var heap = CreateSmallHeap(out _);

        var actualElement = heap.PopPush(4);

        Assert.Equal(1, actualElement);
        Assert.Equal(2, heap.Pop());
        Assert.Equal(3, heap.Pop());
        Assert.Equal(4, heap.Pop());
    }

    [Fact]
    public void Heap_PopPush_EqualToMin()
    {
        var heap = CreateSmallHeap(out _);

        var actualElement = heap.PopPush(1);

        Assert.Equal(1, actualElement);
        Assert.Equal(1, heap.Pop());
        Assert.Equal(2, heap.Pop());
        Assert.Equal(3, heap.Pop());
    }

    [Fact]
    public void Heap_Constructor_IEnumerable_Null()
    {
        var items = new[] { null, "1" };
        var heap = new Heap<string?>(items);
        Assert.Null(heap.Pop());
        Assert.Equal("1", heap.Pop());
    }

    [Fact]
    public void Heap_Push_Null()
    {
        var heap = new Heap<string?>();

        heap.Push(null);
        heap.Push("0");
        heap.Push("2");

        Assert.Null(heap.Pop());
        Assert.Equal("0", heap.Pop());
        Assert.Equal("2", heap.Pop());
    }

    [Fact]
    public void Heap_PushRange_Null()
    {
        var heap = new Heap<string?>();

        heap.PushRange([null, null, null]);
        heap.PushRange(["not null"]);
        heap.PushRange([null, null, null]);

        for (var i = 0; i < 6; ++i)
        {
            Assert.Null(heap.Pop());
        }

        Assert.Equal("not null", heap.Pop());
    }

    [Fact]
    public void Heap_Constructor_Int_Negative_ThrowsArgumentOutOfRangeException()
    {
        AssertExtensions.Throws<ArgumentOutOfRangeException>("capacity", () => new Heap<int>(-1));
        AssertExtensions.Throws<ArgumentOutOfRangeException>("capacity", () => new Heap<int>(int.MinValue));
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
        var heap = new Heap<int>();
        AssertExtensions.Throws<ArgumentNullException>("items", () => heap.PushRange(null!));
    }

    [Fact]
    public void Heap_EmptyCollection_Pop_ShouldThrowException()
    {
        var heap = new Heap<int>();

        Assert.Equal(0, heap.Count);
        Assert.False(heap.TryPop(out _));
        Assert.Throws<InvalidOperationException>(() => heap.Pop());
    }

    [Fact]
    public void Heap_EmptyCollection_Peek_ShouldReturnFalse()
    {
        var heap = new Heap<int>();

        Assert.False(heap.TryPeek(out _));
        Assert.Throws<InvalidOperationException>(() => heap.Peek());
    }

    #region EnsureCapacity, TrimExcess

    [Fact]
    public void Heap_EnsureCapacity_Negative_ShouldThrowException()
    {
        var heap = new Heap<int>();
        AssertExtensions.Throws<ArgumentOutOfRangeException>("capacity", () => heap.EnsureCapacity(-1));
        AssertExtensions.Throws<ArgumentOutOfRangeException>("capacity", () => heap.EnsureCapacity(int.MinValue));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(0, 5)]
    [InlineData(1, 1)]
    [InlineData(3, 100)]
    public void Heap_TrimExcess_ShouldNotChangeCount(int initialCapacity, int count)
    {
        var heap = CreateHeap(initialCapacity, count);

        Assert.Equal(count, heap.Count);
        heap.TrimExcess();
        Assert.Equal(count, heap.Count);
    }

    [Theory]
    [MemberData(nameof(ValidPositiveCollectionSizes))]
    public void Heap_TrimExcess_Repeatedly_ShouldNotChangeCount(int count)
    {
        var heap = CreateHeap(initialCapacity: count, count);

        Assert.Equal(count, heap.Count);
        heap.TrimExcess();
        heap.TrimExcess();
        heap.TrimExcess();
        Assert.Equal(count, heap.Count);
    }

    [Theory]
    [MemberData(nameof(ValidPositiveCollectionSizes))]
    public void Heap_EnsureCapacityAndTrimExcess(int count)
    {
        var items = Enumerable.Range(1, count).ToArray();
        var heap = new Heap<int>();
        var expectedCount = 0;
        var random = new Random(Seed: 34);

        foreach (var element in items)
        {
            TrimAndEnsureCapacity();
            heap.Push(element);
            expectedCount++;
            Assert.Equal(expectedCount, heap.Count);
        }

        while (expectedCount > 0)
        {
            heap.Pop();
            TrimAndEnsureCapacity();
            expectedCount--;
            Assert.Equal(expectedCount, heap.Count);
        }

        TrimAndEnsureCapacity();
        Assert.Equal(0, heap.Count);

        int GetNextEnsureCapacity()
        {
            return random.Next(0, count * 2);
        }

        void TrimAndEnsureCapacity()
        {
            heap.TrimExcess();

            var capacityAfterEnsureCapacity = heap.EnsureCapacity(GetNextEnsureCapacity());
            Assert.Equal(capacityAfterEnsureCapacity, GetUnderlyingBufferCapacity(heap));

            var capacityAfterTrimExcess = heap.Count < (int)(capacityAfterEnsureCapacity * 0.9) ? heap.Count : capacityAfterEnsureCapacity;
            heap.TrimExcess();
            Assert.Equal(capacityAfterTrimExcess, GetUnderlyingBufferCapacity(heap));
        }
    }

    private static int GetUnderlyingBufferCapacity<T>(Heap<T> heap)
    {
        var field = typeof(Heap<T>).GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(field);
        var array = (T[]?)field.GetValue(heap);
        Assert.NotNull(array);
        return array.Length;
    }

    #endregion

    #region Enumeration

    [Theory]
    [MemberData(nameof(NonModifyingOperations))]
    public void Heap_Enumeration_ValidOnNonModifyingOperation(Action<Heap<int>> nonModifyingOperation, int count)
    {
        var heap = CreateHeap(initialCapacity: count, count: count);
        using var enumerator = heap.GetEnumerator();
        nonModifyingOperation(heap);
        enumerator.MoveNext();
    }

    [Theory]
    [MemberData(nameof(ModifyingOperations))]
    public void Heap_Enumeration_InvalidationOnModifyingOperation(Action<Heap<int>> modifyingOperation, int count)
    {
        {
            var heap = CreateHeap(initialCapacity: count, count: count);
            using var enumerator = heap.GetEnumerator();
            modifyingOperation(heap);

            Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
            Assert.Default(enumerator.Current);
        }
        {
            var heap = CreateHeap(initialCapacity: count, count: count);
            using var enumerator = ((IEnumerable<int>)heap).GetEnumerator();
            modifyingOperation(heap);

            if (count == 0)
            {
                // GenericEmptyEnumerator does not throw on MoveNext() even if the collection was modified
                Assert.False(enumerator.MoveNext());

                // GenericEmptyEnumerator throws on Current
                Assert.Throws<InvalidOperationException>(() => enumerator.Current);

            }
            else
            {
                Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
                Assert.Default(enumerator.Current);
            }
        }
    }

    public static readonly TheoryData<Action<Heap<int>>, int> ModifyingOperations = new()
    {
        (m => m.Push(42), 0),
        (m => m.Pop(), 5),
        (m => m.TryPop(out _), 5),
        (m => m.PushPop(5), 6),
        (m => m.PushRange([1]), 0),
        (m => m.PushRange([1]), 10),
        (m => m.PushRange([1, 2]), 0),
        (m => m.PushRange([1, 2]), 10),
        (m => m.Clear(), 5),
        (m => m.Clear(), 0),
    };

    public static readonly TheoryData<Action<Heap<int>>, int> NonModifyingOperations = new()
    {
        (m => m.Peek(), 1),
        (m => m.TryPeek(out _), 1),
        (m => m.TryPop(out _), 0),
        (m => m.PushPop(-1), 5), // the min element before the operation is 0, so PushPop(-1) will return -1 and leave the heap unchanged
        (m => m.PushPop(0), 5),
        (m => m.PushRange([]), 5),
        (m => m.PushRange([]), 5),
        (m => m.EnsureCapacity(5), 5),
    };

    #endregion
}