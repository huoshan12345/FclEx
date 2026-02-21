namespace System.Collections.Generic.Heap;

public class HeapTestsBasic
{
    [Fact]
    public void Push_Pop_ShouldReturnSorted()
    {
        var heap = new Heap<int>();

        heap.Push(5);
        heap.Push(1);
        heap.Push(3);
        heap.Push(2);
        heap.Push(4);

        var result = new List<int>();

        while (heap.TryPop(out var x))
            result.Add(x);

        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, result);
    }

    [Fact]
    public void Peek_ShouldReturnMinimum()
    {
        var heap = new Heap<int>();

        heap.Push(5);
        heap.Push(2);
        heap.Push(3);

        Assert.Equal(2, heap.Peek());
        Assert.Equal(3, heap.Count);
    }

    [Fact]
    public void ReplaceTop_ShouldReplaceAndReturnOld()
    {
        var heap = new Heap<int>();

        heap.Push(1);
        heap.Push(2);
        heap.Push(3);

        var old = heap.ReplaceTop(10);

        Assert.Equal(1, old);
        Assert.Equal(2, heap.Pop());
    }

    [Fact]
    public void TryPop_Empty_ShouldReturnFalse()
    {
        var heap = new Heap<int>();

        var ok = heap.TryPop(out var v);

        Assert.False(ok);
        Assert.Equal(default, v);
    }

    [Fact]
    public void TryPeek_Empty_ShouldReturnFalse()
    {
        var heap = new Heap<int>();

        var ok = heap.TryPeek(out var v);

        Assert.False(ok);
        Assert.Equal(default, v);
    }

    [Fact]
    public void Pop_Empty_ShouldThrow()
    {
        var heap = new Heap<int>();

        Assert.Throws<InvalidOperationException>(() => heap.Pop());
    }

    [Fact]
    public void Peek_Empty_ShouldThrow()
    {
        var heap = new Heap<int>();

        Assert.Throws<InvalidOperationException>(() => heap.Peek());
    }

    [Fact]
    public void Heapify_FromEnumerable_ShouldWork()
    {
        var heap = new Heap<int>([5, 1, 4, 2, 3]);

        var result = new List<int>();

        while (heap.TryPop(out var x))
            result.Add(x);

        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, result);
    }

    [Fact]
    public void Clear_ShouldResetCount()
    {
        var heap = new Heap<int>();

        heap.Push(1);
        heap.Push(2);

        heap.Clear();

        Assert.Equal(0, heap.Count);
        Assert.False(heap.TryPop(out _));
    }

    [Fact]
    public void EnsureCapacity_ShouldIncreaseCapacity()
    {
        var heap = new Heap<int>(4);

        heap.EnsureCapacity(100);

        Assert.True(heap.Capacity >= 100);
    }

    [Fact]
    public void TrimExcess_ShouldShrink()
    {
        var heap = new Heap<int>();

        for (var i = 0; i < 100; i++)
            heap.Push(i);

        while (heap.Count > 10)
            heap.Pop();

        heap.TrimExcess();

        Assert.True(heap.Capacity >= heap.Count);
        Assert.True(heap.Capacity <= 20);
    }

    [Fact]
    public void Enumeration_ShouldReturnAllElements()
    {
        var heap = new Heap<int>();

        heap.Push(3);
        heap.Push(1);
        heap.Push(2);

        var items = heap.ToList();

        Assert.Equal(3, items.Count);
        Assert.Contains(1, items);
        Assert.Contains(2, items);
        Assert.Contains(3, items);
    }

    [Fact]
    public void LargeRandomTest()
    {
        var rand = new Random(1);

        var heap = new Heap<int>();
        var list = new List<int>();

        for (var i = 0; i < 10000; i++)
        {
            var v = rand.Next();
            heap.Push(v);
            list.Add(v);
        }

        list.Sort();

        for (var i = 0; i < list.Count; i++)
        {
            Assert.Equal(list[i], heap.Pop());
        }
    }

    [Fact]
    public void MixedOperations()
    {
        var rand = new Random(0);

        var heap = new Heap<int>();
        var list = new List<int>();

        for (var i = 0; i < 5000; i++)
        {
            if (rand.Next(2) == 0 || list.Count == 0)
            {
                var v = rand.Next();
                heap.Push(v);
                list.Add(v);
            }
            else
            {
                list.Sort();
                var expected = list[0];
                list.RemoveAt(0);

                Assert.Equal(expected, heap.Pop());
            }
        }
    }

    [Fact]
    public void RandomizedOperations_ShouldMatchSortedList()
    {
        var rand = new Random(1234);

        var heap = new Heap<int>();
        var list = new List<int>();

        for (var i = 0; i < 2000; i++)
        {
            if (rand.Next(3) != 0)
            {
                var v = rand.Next();
                heap.Push(v);
                list.Add(v);
            }
            else if (list.Count > 0)
            {
                list.Sort();

                var expected = list[0];
                list.RemoveAt(0);

                Assert.Equal(expected, heap.Pop());
            }
        }

        list.Sort();

        foreach (var v in list)
            Assert.Equal(v, heap.Pop());
    }

    [Fact]
    public void Heapify_ShouldProduceValidHeap()
    {
        var rand = new Random(1);

        var data = new int[10000];

        for (var i = 0; i < data.Length; i++)
            data[i] = rand.Next();

        var heap = new Heap<int>(data);

        var prev = heap.Pop();

        while (heap.TryPop(out var v))
        {
            Assert.True(prev <= v);
            prev = v;
        }
    }

    [Fact]
    public void ReplaceTop_ShouldMaintainHeap()
    {
        var heap = new Heap<int>();

        for (var i = 0; i < 1000; i++)
            heap.Push(i);

        heap.ReplaceTop(5000);

        var prev = heap.Pop();

        while (heap.TryPop(out var v))
        {
            Assert.True(prev <= v);
            prev = v;
        }
    }

    [Fact]
    public void ArityBoundaryTest()
    {
        var heap = new Heap<int>();

        for (var i = 20; i >= 0; i--)
            heap.Push(i);

        var prev = heap.Pop();

        while (heap.TryPop(out var v))
        {
            Assert.True(prev <= v);
            prev = v;
        }
    }

    [Fact]
    public void Grow_ShouldNotBreakHeap()
    {
        var heap = new Heap<int>(4);

        for (var i = 10000; i >= 0; i--)
            heap.Push(i);

        var prev = heap.Pop();

        while (heap.TryPop(out var v))
        {
            Assert.True(prev <= v);
            prev = v;
        }
    }

    [Fact]
    public void Enumerator_ShouldIterateAllElements()
    {
        var heap = new Heap<int>();

        for (var i = 0; i < 100; i++)
            heap.Push(i);

        var set = new HashSet<int>();

        foreach (var v in heap)
            set.Add(v);

        Assert.Equal(100, set.Count);
    }

    [Fact]
    public void Clear_ShouldAllowReuse()
    {
        var heap = new Heap<int>();

        for (var i = 0; i < 100; i++)
            heap.Push(i);

        heap.Clear();

        for (var i = 200; i >= 100; i--)
            heap.Push(i);

        var prev = heap.Pop();

        while (heap.TryPop(out var v))
        {
            Assert.True(prev <= v);
            prev = v;
        }
    }

    [Fact]
    public void FuzzTest()
    {
        var rand = new Random(12345);

        var heap = new Heap<int>();
        var list = new List<int>();

        for (var step = 0; step < 10000; step++)
        {
            var op = rand.Next(5);

            switch (op)
            {
                case 0: // push
                {
                    var v = rand.Next(100000);

                    heap.Push(v);
                    list.Add(v);
                    break;
                }
                case 1: // pop
                {
                    if (list.Count == 0)
                        break;

                    list.Sort();

                    var expected = list[0];
                    list.RemoveAt(0);

                    var actual = heap.Pop();

                    Assert.Equal(expected, actual);
                    break;
                }
                case 2: // peek
                {
                    if (list.Count == 0)
                        break;

                    list.Sort();

                    var expected = list[0];
                    var actual = heap.Peek();

                    Assert.Equal(expected, actual);
                    break;
                }
                case 3: // ReplaceTop
                {
                    var v = rand.Next(100000);

                    if (list.Count == 0)
                    {
                        heap.ReplaceTop(v);
                        list.Add(v);
                        break;
                    }

                    list.Sort();

                    var expected = list[0];
                    list[0] = v;

                    var actual = heap.ReplaceTop(v);

                    Assert.Equal(expected, actual);
                    break;
                }
                case 4: // clear
                {
                    if (rand.Next(100) == 0)
                    {
                        heap.Clear();
                        list.Clear();
                    }
                    break;
                }
            }

            Assert.Equal(list.Count, heap.Count);
            Assert.AssertHeapInvariant(heap);
        }

        list.Sort();

        foreach (var v in list)
            Assert.Equal(v, heap.Pop());

        Assert.Equal(0, heap.Count);
    }

    [Fact]
    public void ChildRangeBoundary()
    {
        var heap = new Heap<int>();

        for (var i = 50; i >= 0; i--)
            heap.Push(i);

        var prev = heap.Pop();

        while (heap.TryPop(out var v))
        {
            Assert.True(prev <= v);
            prev = v;
        }
    }

    [Fact]
    public void ExactArityBoundary()
    {
        var heap = new Heap<int>();

        for (var i = 0; i < 5; i++)
            heap.Push(i);

        Assert.Equal(0, heap.Pop());
        Assert.Equal(1, heap.Pop());
        Assert.Equal(2, heap.Pop());
        Assert.Equal(3, heap.Pop());
        Assert.Equal(4, heap.Pop());
    }

    [Fact]
    public void ArityPlusOneBoundary()
    {
        var heap = new Heap<int>();

        for (var i = 5; i >= 0; i--)
            heap.Push(i);

        for (var i = 0; i <= 5; i++)
            Assert.Equal(i, heap.Pop());
    }

    [Fact]
    public void HeapifyChildBoundary()
    {
        var data = new[] { 9, 8, 7, 6, 5, 4, 3 };

        var heap = new Heap<int>(data);

        var prev = heap.Pop();

        while (heap.TryPop(out var v))
        {
            Assert.True(prev <= v);
            prev = v;
        }
    }

    [Fact]
    public void ReplaceTopStress()
    {
        var heap = new Heap<int>();

        for (var i = 0; i < 1000; i++)
            heap.Push(i);

        for (var i = 0; i < 1000; i++)
        {
            heap.ReplaceTop(10000 - i);
        }

        var prev = heap.Pop();

        while (heap.TryPop(out var v))
        {
            Assert.True(prev <= v);
            prev = v;
        }
    }

    [Fact]
    public void DeepTree()
    {
        var heap = new Heap<int>();

        for (var i = 20000; i >= 0; i--)
            heap.Push(i);

        var prev = heap.Pop();

        while (heap.TryPop(out var v))
        {
            Assert.True(prev <= v);
            prev = v;
        }
    }
}

file static class HeapTestExtensions
{
    private const int Arity = 4;

    private static readonly FieldInfo _fieldItems = typeof(Heap<>).GetRequiredField("_items");

    extension(Assert)
    {
        public static void AssertHeapInvariant(Heap<int> heap)
        {
            var items = ObjectHelper.GetRequiredFieldValue<int[]>(heap, "_items");
            var count = heap.Count;

            // 1. Verify the heap property: parent <= child
            for (var i = 0; i < count; i++)
            {
                for (var k = 1; k <= Arity; k++)
                {
                    var child = i * Arity + k;

                    if (child >= count)
                        break;

                    Assert.True(
                        items[i] <= items[child],
                        $"Heap property violated: parent {items[i]} > child {items[child]}"
                    );
                }
            }

            // 2. Verify that the root contains the minimum element
            var min = items[0];

            for (var i = 1; i < count; i++)
            {
                if (items[i] < min)
                    Assert.Fail($"Root is not the minimum element: {items[i]} < {min}");
            }

            // 3. Verify that count does not exceed the array length
            Assert.True(count <= items.Length);
        }
    }
}
