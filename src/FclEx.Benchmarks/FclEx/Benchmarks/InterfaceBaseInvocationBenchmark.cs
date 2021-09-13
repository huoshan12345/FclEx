using BenchmarkDotNet.Attributes;
using InterfaceBaseInvoke;

namespace FclEx.Benchmarks
{
    public interface IEmptyMethod
    {
        int Compute(int number);
    }

    public interface IDefaultMethod
    {
        int Compute(int number) => number + 1;
    }

    public interface IOverridedMethod : IEmptyMethod
    {
        int IEmptyMethod.Compute(int number) => number + 1;
    }

    public class InheritIOverridedMethodWithDelegate : IOverridedMethod
    {
        public int Compute(int number) => this.BaseByDelegate<IOverridedMethod, int>(m => m.Compute(0));
    }

    public class InheritIOverridedMethodWithDynamicMethod : IOverridedMethod
    {
        public int Compute(int number) => this.BaseByDynamicMethod<IOverridedMethod, int>(m => m.Compute(0));
    }

    public class InheritIOverridedMethodWithIL : IOverridedMethod
    {
        public int Compute(int number) => this.Base<IOverridedMethod>().Compute(0);
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
        private static readonly InheritIOverridedMethodWithDynamicMethod _inheritIOverridedMethodWithDynamicMethod = new();
        private static readonly InheritIOverridedMethodWithDelegate _inheritIOverridedMethodWithDelegate = new();
        private static readonly InheritIOverridedMethodWithIL _inheritIOverridedMethodWithIL = new();
        private static readonly InheritIDefaultMethodWithIL _inheritIDefaultMethodWithIL = new();
        private static readonly Child _child = new();

        [Benchmark(Baseline = true)]
        public void Base()
        {
            var r1 = _child.Compute(0);
        }

        [Benchmark]
        public void Base_Delegate()
        {
            var r1 = _inheritIOverridedMethodWithDelegate.Compute(0);
        }

        [Benchmark]
        public void Base_DynamicMethod()
        {
            var r1 = _inheritIOverridedMethodWithDynamicMethod.Compute(0);
        }

        [Benchmark]
        public void Base_IL()
        {
            var r1 = _inheritIDefaultMethodWithIL.Compute(0);
        }

        [Benchmark]
        public void Base_IL_MultiLevel()
        {
            var r1 = _inheritIOverridedMethodWithIL.Compute(0);
        }
    }
}
