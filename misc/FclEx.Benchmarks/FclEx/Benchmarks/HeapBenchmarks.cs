// ReSharper disable ForCanBeConvertedToForeach
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace FclEx.Benchmarks;

[WarmupCount(5)]
[IterationCount(10)]
[MemoryDiagnoser]
public class HeapBenchmarks
{
    private int[] _data;

    [Params(200, 2000)]
    public int N;

    [Params(10, 100)]
    public int K;

    [GlobalSetup]
    public void Setup()
    {
        var rand = new Random(42);

        _data = new int[N];

        for (var i = 0; i < N; i++)
            _data[i] = rand.Next();
    }

    [Benchmark]
    public int Heap_Push()
    {
        var heap = new Heap<int>();

        for (var i = 0; i < _data.Length; i++)
            heap.Push(_data[i]);

        return heap.Count;
    }

    [Benchmark]
    public PriorityQueue<int, int> Bcl_Push()
    {
        var pq = new PriorityQueue<int, int>();

        for (var i = 0; i < _data.Length; i++)
            pq.Enqueue(_data[i], _data[i]);

        return pq;
    }

    [Benchmark]
    public int Heap_Pop()
    {
        var heap = new Heap<int>(_data);

        var sum = 0;

        while (heap.TryPop(out var v))
            sum += v;

        return sum;
    }

    [Benchmark]
    public int Bcl_Pop()
    {
        var pq = new PriorityQueue<int, int>();

        for (var i = 0; i < _data.Length; i++)
            pq.Enqueue(_data[i], _data[i]);

        var sum = 0;

        while (pq.TryDequeue(out var v, out _))
            sum += v;

        return sum;
    }

    [Benchmark]
    public Heap<int> Heap_Heapify()
    {
        return new Heap<int>(_data);
    }

    [Benchmark]
    public PriorityQueue<int, int> Bcl_Build()
    {
        var pq = new PriorityQueue<int, int>();

        for (var i = 0; i < _data.Length; i++)
            pq.Enqueue(_data[i], _data[i]);

        return pq;
    }

    [Benchmark]
    public int Heap_Mixed()
    {
        var heap = new Heap<int>();

        var sum = 0;

        for (var i = 0; i < _data.Length; i++)
        {
            heap.Push(_data[i]);

            if ((i & 3) == 0)
                sum += heap.Pop();
        }

        while (heap.TryPop(out var v))
            sum += v;

        return sum;
    }

    [Benchmark]
    public int Bcl_Mixed()
    {
        var pq = new PriorityQueue<int, int>();

        var sum = 0;

        for (var i = 0; i < _data.Length; i++)
        {
            pq.Enqueue(_data[i], _data[i]);

            if ((i & 3) == 0)
                sum += pq.Dequeue();
        }

        while (pq.TryDequeue(out var v, out _))
            sum += v;

        return sum;
    }

    [Benchmark]
    public int Heap_TopK()
    {
        var heap = new Heap<int>();

        for (var i = 0; i < K; i++)
            heap.Push(_data[i]);

        for (var i = K; i < _data.Length; i++)
        {
            var v = _data[i];

            if (v > heap.Peek())
                heap.ReplaceTop(v);
        }

        var sum = 0;

        while (heap.TryPop(out var x))
            sum += x;

        return sum;
    }

    [Benchmark]
    public int Bcl_TopK()
    {
        var pq = new PriorityQueue<int, int>();

        for (var i = 0; i < K; i++)
            pq.Enqueue(_data[i], _data[i]);

        for (var i = K; i < _data.Length; i++)
        {
            var v = _data[i];

            pq.TryPeek(out var top, out _);

            if (v > top)
            {
                pq.Dequeue();
                pq.Enqueue(v, v);
            }
        }

        var sum = 0;

        while (pq.TryDequeue(out var v, out _))
            sum += v;

        return sum;
    }
}