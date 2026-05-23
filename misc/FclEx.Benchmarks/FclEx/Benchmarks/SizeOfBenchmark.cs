using System.Runtime.CompilerServices;

namespace FclEx.Benchmarks;

[GenericTypeArguments(typeof(int))]
[GenericTypeArguments(typeof(string))]
[GenericTypeArguments(typeof(DateTimeOffset))]
[GenericTypeArguments(typeof(ValueTuple<string, int, DateTimeOffset>))]
[MemoryDiagnoser]
[MinInvokeCount(10)]
public class SizeOfBenchmark<T>
{
    [Benchmark(Baseline = true)]
    public void Unsafe_SizeOf()
    {
        var size = Unsafe.SizeOf<T>();
    }

    [Benchmark]
    public void UnsafeHelper_SizeOf()
    {
        var size = UnsafeHelper.SizeOf<T>();
    }
}