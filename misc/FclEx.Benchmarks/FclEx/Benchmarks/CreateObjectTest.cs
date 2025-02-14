namespace FclEx.Benchmarks;

[MemoryDiagnoser]
public class CreateObjectTest
{
    private static readonly Type _type = typeof(List<>);

    [Benchmark]
    public void CreateInstance()
    {
        Activator.CreateInstance(_type.MakeGenericType<int>(), 4);
    }

    [Benchmark]
    public void Ctor()
    {
        var ctor = _type.MakeGenericType<int>().GetConstructor([typeof(int)])!;
        ctor.Invoke([4]);
    }

    [Benchmark]
    public void CreateObject()
    {
        _type.MakeGenericType<int>().CreateObject(4);
    }
}