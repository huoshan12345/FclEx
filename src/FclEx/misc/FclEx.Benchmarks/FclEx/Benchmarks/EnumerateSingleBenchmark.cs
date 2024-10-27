using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using FclEx.Extensions;

namespace FclEx.Benchmarks;

internal readonly struct SingleSequence<T> : IEnumerable<T>
{
    private readonly T _value;

    public SingleSequence(T value)
    {
        _value = value;
    }

    public IEnumerator<T> GetEnumerator() => new SingleEnumerator(in this);
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private struct SingleEnumerator : IEnumerator<T>
    {
        private readonly SingleSequence<T> _parent;
        private bool _couldMove;
        public SingleEnumerator(in SingleSequence<T> parent)
        {
            _parent = parent;
            _couldMove = true;
        }

        public T Current => _parent._value;
        object? IEnumerator.Current => Current;

        public void Dispose() { }

        public bool MoveNext()
        {
            if (!_couldMove)
                return false;

            _couldMove = false;
            return true;
        }
        public void Reset()
        {
            _couldMove = true;
        }
    }
}

public static class Extensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<T> SingleSequence<T>(this T value)
    {
        return new SingleSequence<T>(value);
    }
}


[MemoryDiagnoser]
public class EnumerateSingleBenchmark
{
    [Params(0, 10)]
    public int Number { get; set; }

    [Benchmark]
    public double Array()
    {
        return Sum(new[] { Number });
    }

    [Benchmark]
    public double Yield()
    {
        return Sum(Number.Yield());
    }

    [Benchmark]
    public double Repeat()
    {
        return Sum(Enumerable.Repeat(Number, 1));
    }

    [Benchmark]
    public double SingleSequence()
    {
        return Sum(Number.SingleSequence());
    }

    private static double Sum(IEnumerable<int> values)
    {
        var result = 0;
        foreach (var value in values)
        {
            result += value;
        }
        return result;
    }
}