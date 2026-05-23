using System.Linq.Expressions;

namespace FclEx.Benchmarks;

[MemoryDiagnoser]
public class DefaultValueBenchmark
{
    public static IEnumerable<object[]> Cases => new[]
    {
        typeof(int),
        typeof(string),
        typeof(DateTime),
        typeof(List<int>),
    }.Select(m => new object[] { m }).ToArray();

    [Benchmark]
    [ArgumentsSource(nameof(Cases))]
    public void DefaultValue(Type type)
    {
        type.DefaultValue();
    }

    [Benchmark]
    [ArgumentsSource(nameof(Cases))]
    public void DefaultValue_Expression(Type type)
    {
        Impl(type);
        return;

        static object? Impl(Type type)
        {
            var @default = Expression.Default(type);
            var convert = Expression.Convert(@default, typeof(object));
            var lambda = Expression.Lambda<Func<object?>>(convert);
            return lambda.Compile()();
        }
    }

}