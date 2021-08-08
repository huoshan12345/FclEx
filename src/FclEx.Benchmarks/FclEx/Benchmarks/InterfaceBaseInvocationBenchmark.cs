using BenchmarkDotNet.Attributes;

namespace FclEx.Benchmarks
{
    [MemoryDiagnoser]
    [StopOnFirstError]
    public class InterfaceBaseInvocationBenchmark
    {
        public interface I0
        {
            int Compute(int number);
        }

        public interface I1 : I0
        {
            int I0.Compute(int number) => number + 1;
        }

        public interface I2 : I1
        {
            int I0.Compute(int number) => number + 2;
        }

        public class InheritI2 : I2
        {
        }

        private static readonly InheritI2 _inheritI2 = new();
        private static readonly C2 _c2 = new();

        public abstract class C0
        {
            public abstract int Compute(int number);
        }

        public class C1 : C0
        {
            public override int Compute(int number) => number + 1;
        }

        public class C2 : C1
        {
            public override int Compute(int number) => base.Compute(number) + 1;
        }

        [Benchmark(Baseline = true)]
        public void Base()
        {
            var r1 = _c2.Compute(0);
            var r2 = _c2.Compute(0);
        }

        [Benchmark]
        public void BaseByFunctionPointer()
        {
            var r1 = _inheritI2.Base<I1, int>(m => m.Compute(0));
            var r2 = _inheritI2.Base<I2, int>(m => m.Compute(0));
        }

        [Benchmark]
        public void BaseByDynamicMethod()
        {
            var r1 = _inheritI2.BaseByDynamicMethod<I1, int>(m => m.Compute(0));
            var r2 = _inheritI2.BaseByDynamicMethod<I2, int>(m => m.Compute(0));
        }
    }
}
