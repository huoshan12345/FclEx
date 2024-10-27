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
    public void SizeOf()
    {
        var size = UnsafeHelper.SizeOf<T>();
    }

    [Benchmark]
    public void SizeOf2()
    {
        var size = UnsafeHelper.SizeOf2<T>();
    }
}