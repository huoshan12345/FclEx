using InterfaceBaseInvoke;

namespace FclEx.Benchmarks;

public interface IEmptyMethod
{
    int Compute(int number);
}

public interface IDefaultMethod
{
    int Compute(int number) => number + 1;
}

public interface IOverrideMethod : IEmptyMethod
{
    int IEmptyMethod.Compute(int number) => number + 1;
}

public class InheritIOverrideMethodWithFunctionPointer : IOverrideMethod
{
    public int Compute(int number) => this.BaseByFunctionPointer<IOverrideMethod, int>(m => m.Compute(0));
}

public class InheritIOverrideMethodWithDynamicMethod : IOverrideMethod
{
    public int Compute(int number) => this.BaseByDynamicMethod<IOverrideMethod, int>(m => m.Compute(0));
}

public class InheritIOverrideMethodWithIL : IOverrideMethod
{
    public int Compute(int number) => this.Base<IOverrideMethod>().Compute(0);
}

public class InheritIDefaultMethodWithIL : IDefaultMethod
{
    public int Compute(int number) => this.Base<IDefaultMethod>().Compute(0);
}

public abstract class Ancestor
{
    public abstract int Compute(int number);
}

public class Parent : Ancestor
{
    public override int Compute(int number) => number + 1;
}

public class Child : Parent
{
    public override int Compute(int number) => base.Compute(number) + 1;
}

[MemoryDiagnoser]
[StopOnFirstError]
public class InterfaceBaseInvocationBenchmark
{
    private static readonly InheritIOverrideMethodWithDynamicMethod _inheritIOverrideMethodWithDynamicMethod = new();
    private static readonly InheritIOverrideMethodWithFunctionPointer _inheritIOverrideMethodWithFunctionPointer = new();
    private static readonly InheritIOverrideMethodWithIL _inheritIOverrideMethodWithIL = new();
    private static readonly InheritIDefaultMethodWithIL _inheritIDefaultMethodWithIL = new();
    private static readonly Child _child = new();

    [Benchmark(Baseline = true)]
    public void Class_Base()
    {
        var r1 = _child.Compute(0);
    }

    [Benchmark]
    public void Interface_Base_FunctionPointer()
    {
        var r1 = _inheritIOverrideMethodWithFunctionPointer.Compute(0);
    }

    [Benchmark]
    public void Interface_Base_DynamicMethod()
    {
        var r1 = _inheritIOverrideMethodWithDynamicMethod.Compute(0);
    }

    [Benchmark]
    public void Interface_Base_IL()
    {
        var r1 = _inheritIDefaultMethodWithIL.Compute(0);
    }

    // [Benchmark]
    public void Interface_Base_IL_MultiLevel()
    {
        var r1 = _inheritIOverrideMethodWithIL.Compute(0);
    }
}