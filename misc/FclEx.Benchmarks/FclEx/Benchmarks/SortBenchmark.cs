namespace FclEx.Benchmarks;

[MemoryDiagnoser]
public class SortBenchmark
{
    private readonly Person[] _array;

    private static readonly Comparison<Person> _comparisonOfDefault = (x, y) =>
    {
        var cmpOfAge = Comparer<int>.Default.Compare(x.Age, y.Age); // age asc
        if (cmpOfAge != 0) return cmpOfAge;
        var cmpOfName = Comparer<string>.Default.Compare(x.Name, y.Name); // name asc
        if (cmpOfName != 0) return cmpOfName;
        var cmpOfHeight = Comparer<int>.Default.Compare(y.Height, x.Height); // height desc
        return cmpOfHeight;
    };

    private static readonly Comparison<Person> _comparisonOfProp =
        MemberComparerBuilder<Person>
            .Create(m => m.Age)
            .OrderBy(m => m.Name)
            .OrderBy(m => m.Height, true)
            .CreateComparison();

    public SortBenchmark()
    {
        var random = new Random(12345);
        _array = Enumerable.Range(1, 10000).Select(m => new Person()
        {
            Age = random.Next(1, 100),
            Height = random.Next(100, 200),
            Name = random.NextString(10),
        }).ToArray();
    }

    public class Person
    {
        public int Age { get; set; }
        public string? Name { get; set; }
        public int Height { get; set; }
    }

    [BenchmarkCategory(nameof(Comparison_Default)), Benchmark]
    public void Comparison_Default()
    {
        var list = _array.ToList();
        list.Sort(_comparisonOfDefault);
    }

    [BenchmarkCategory(nameof(Comparison_Prop)), Benchmark]
    public void Comparison_Prop()
    {
        var list = _array.ToList();
        list.Sort(_comparisonOfProp);
    }


    [BenchmarkCategory(nameof(OrderBy)), Benchmark]
    public void OrderBy()
    {
        var list = _array
            .OrderBy(m => m.Age)
            .ThenBy(m => m.Name)
            .ThenByDescending(m => m.Height)
            .ToList();
    }
}