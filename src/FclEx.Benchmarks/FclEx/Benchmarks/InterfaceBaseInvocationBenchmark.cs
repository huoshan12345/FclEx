using BenchmarkDotNet.Attributes;
using InterfaceBaseInvoke;

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

        public class ClassDelegate : I1
        {
            public int Compute(int number) => _classDelegate.BaseByDelegate<I1, int>(m => m.Compute(0));
        }

        public class ClassDynamicMethod : I1
        {
            public int Compute(int number) => _classDelegate.BaseByDynamicMethod<I1, int>(m => m.Compute(0));
        }

        public class ClassIL : I1
        {
            public int Compute(int number) => _classDelegate.Base<I1>().Compute(0);
        }

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

        private static readonly ClassDynamicMethod _classDynamicMethod = new();
        private static readonly ClassDelegate _classDelegate = new();
        private static readonly ClassIL _classIL = new();
        private static readonly C2 _c2 = new();

        [Benchmark(Baseline = true)]
        public void Base()
        {
            var r1 = _c2.Compute(0);
        }

        [Benchmark]
        public void BaseByDelegate()
        {
            var r1 = _classDelegate.Compute(0);
        }

        [Benchmark]
        public void BaseByDynamicMethod()
        {
            var r1 = _classDynamicMethod.Compute(0);
        }

        [Benchmark]
        public void BaseByDyIL()
        {
            var r1 = _classIL.Compute(0);
        }
    }
}
