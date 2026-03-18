namespace System.Collections.Generic.Heap;

public partial class HeapTests
{
    const int MaxTest = 100;
    const int Seed = 42;

    private static readonly IComparer<string> _stringComparer = StringComparer.Ordinal;

    [Theory]
    [MemberData(nameof(GetRandomStringArrays))]
    public static void HeapSort_Heapify_String(string[] elements)
    {
        var expected = elements.OrderBy(e => e, _stringComparer);
        var actual = HeapSort_Heapify(elements, _stringComparer);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(GetRandomIntArrays))]
    public static void HeapSort_Heapify_Int(int[] elements)
    {
        var expected = elements.OrderBy(e => e);
        var actual = HeapSort_Heapify(elements);
        Assert.Equal(expected, actual);
    }

    private static IEnumerable<T> HeapSort_Heapify<T>(IEnumerable<T> inputs, IComparer<T>? comparer = null)
    {
        var heap = new Heap<T>(inputs, comparer);
        foreach (var element in DrainHeap(heap))
        {
            yield return element;
        }
    }

    [Theory]
    [MemberData(nameof(GetRandomStringArrays))]
    public static void HeapSort_EnqueueRange_String(string[] elements)
    {
        var expected = elements.OrderBy(e => e, _stringComparer);
        var actual = HeapSort_EnqueueRange(elements, _stringComparer);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(GetRandomIntArrays))]
    public static void HeapSort_EnqueueRange_Int(int[] elements)
    {
        var expected = elements.OrderBy(e => e);
        var actual = HeapSort_EnqueueRange(elements);
        Assert.Equal(expected, actual);
    }

    private static IEnumerable<T> HeapSort_EnqueueRange<T>(IEnumerable<T> inputs, IComparer<T>? comparer = null)
    {
        var heap = new Heap<T>(comparer);
        heap.PushRange(inputs);
        foreach (var element in DrainHeap(heap))
        {
            yield return element;
        }
    }

    [Theory]
    [MemberData(nameof(GetRandomStringArrays))]
    public static void HeapSort_Enqueue_String(string[] elements)
    {
        var expected = elements.OrderBy(e => e, _stringComparer);
        var actual = HeapSort_Enqueue(elements, _stringComparer);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(GetRandomIntArrays))]
    public static void HeapSort_Enqueue_Int(int[] elements)
    {
        var expected = elements.OrderBy(e => e);
        var actual = HeapSort_Enqueue(elements);
        Assert.Equal(expected, actual);
    }

    private static IEnumerable<T> HeapSort_Enqueue<T>(IEnumerable<T> inputs, IComparer<T>? comparer = null)
    {
        var heap = new Heap<T>(comparer);

        foreach (var input in inputs)
        {
            heap.Push(input);
        }

        foreach (var element in DrainHeap(heap))
        {
            yield return element;
        }
    }

    [Theory]
    [MemberData(nameof(GetRandomStringArrays))]
    public static void KMaxElements_String(string[] elements)
    {
        const int k = 5;
        var expected = elements.OrderByDescending(e => e, _stringComparer).Take(k);
        var actual = KMaxElements(elements, k, _stringComparer);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(GetRandomIntArrays))]
    public static void KMaxElements_Int(int[] elements)
    {
        const int k = 5;
        var expected = elements.OrderByDescending(e => e).Take(k);
        var actual = KMaxElements(elements, k);
        Assert.Equal(expected, actual);
    }

    private static IEnumerable<T> KMaxElements<T>(T[] elements, int k, IComparer<T>? comparer = null)
    {
        var heap = new Heap<T>(comparer);
        comparer = heap.Comparer;

        var heapSize = Math.Min(k, elements.Length);
        for (var i = 0; i < heapSize; i++)
        {
            var element = elements[i];
            heap.Push(element);
        }

        for (var i = k; i < elements.Length; i++)
        {
            var element = elements[i];
            var dequeued = heap.PushPop(element);
            Assert.True(comparer.Compare(dequeued, element) <= 0);
            Assert.Equal(k, heap.Count);
        }

        foreach (var element in DrainHeap(heap).Reverse())
        {
            yield return element;
        }
    }

    private static IEnumerable<T> DrainHeap<T>(Heap<T> heap)
    {
        while (heap.Count > 0)
        {
            Assert.True(heap.TryPeek(out var element));
            Assert.True(heap.TryPop(out var element2));
            Assert.Equal(element, element2);
            yield return element;
        }

        Assert.False(heap.TryPeek(out _));
    }

    public static IEnumerable<object[]> GetRandomStringArrays() => GenerateMemberData(random => GenArray(GenString, random));
    public static IEnumerable<object[]> GetRandomIntArrays() => GenerateMemberData(random => GenArray(GenInt, random));

    private static IEnumerable<object[]> GenerateMemberData<T>(Func<Random, T> genElement)
    {
        var random = new Random(Seed);
        for (var i = 0; i < MaxTest; i++)
        {
            yield return [genElement(random)!];
        }
        ;
    }

    private static T[] GenArray<T>(Func<Random, T> genElement, Random random)
    {
        const int maxArraySize = 100;
        var arraySize = random.Next(maxArraySize);
        var array = new T[arraySize];
        for (var i = 0; i < arraySize; i++)
        {
            array[i] = genElement(random);
        }

        return array;
    }

    private static int GenInt(Random random) => random.Next();

    private static string GenString(Random random)
    {
        const int maxSize = 50;
        var size = random.Next(maxSize);
        var buffer = new byte[size];
        random.NextBytes(buffer);
        return Convert.ToBase64String(buffer);
    }
}