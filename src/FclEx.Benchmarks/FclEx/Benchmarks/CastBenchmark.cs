using BenchmarkDotNet.Attributes;
using FclEx.Benchmark;
using FclEx.TypeCasters;
using static FclEx.Benchmark.Variable;

namespace FclEx.Benchmarks
{
    [MemoryDiagnoser]
    public class CastBenchmark
    {
        private static readonly CommonTypeCaster CommonTypeCaster = CommonTypeCaster.Instance;
        private static readonly ExpressionTypeCaster ExpressionTypeCaster = ExpressionTypeCaster.Instance;
        private static readonly DelegateTypeCaster DelegateTypeCaster = DelegateTypeCaster.Instance;
        private static readonly DynamicTypeCaster DynamicTypeCaster = DynamicTypeCaster.Instance;

        [BenchmarkCategory("Int_Object"), Benchmark]
        public void Int_Object_Common()
        {
            var actual = CommonTypeCaster.CastTo<int, object>(IntNumber);
        }

        [BenchmarkCategory("Int_Object"), Benchmark]
        public void Int_Object_Expression()
        {
            var actual = ExpressionTypeCaster.CastTo<int, object>(IntNumber);
        }

        [BenchmarkCategory("Int_Object"), Benchmark]
        public void Int_Object_Delegate()
        {
            var actual = DelegateTypeCaster.CastTo<int, object>(IntNumber);
        }

        [BenchmarkCategory("Int_Object"), Benchmark]
        public void Int_Object_Dynamic()
        {
            var actual = DynamicTypeCaster.CastTo<int, object>(IntNumber);
        }


        [BenchmarkCategory("Object_Int"), Benchmark]
        public void Object_Int_Common()
        {
            var actual = CommonTypeCaster.CastTo<object, int>(IntObj);
        }

        [BenchmarkCategory("Object_Int"), Benchmark]
        public void Object_Int_Expression()
        {
            var actual = ExpressionTypeCaster.CastTo<object, int>(IntObj);
        }

        [BenchmarkCategory("Object_Int"), Benchmark]
        public void Object_Int_Delegate()
        {
            var actual = DelegateTypeCaster.CastTo<object, int>(IntObj);
        }

        [BenchmarkCategory("Object_Int"), Benchmark]
        public void Object_Int_Dynamic()
        {
            var actual = DynamicTypeCaster.CastTo<object, int>(IntObj);
        }


        [BenchmarkCategory("Enum_Int"), Benchmark]
        public void Enum_Int_Common()
        {
            var actual = CommonTypeCaster.CastTo<IntEnum, int>(Variable.IntEnum);
        }

        [BenchmarkCategory("Enum_Int"), Benchmark]
        public void Enum_Int_Expression()
        {
            var actual = ExpressionTypeCaster.CastTo<IntEnum, int>(Variable.IntEnum);
        }

        [BenchmarkCategory("Enum_Int"), Benchmark]
        public void Enum_Int_Delegate()
        {
            var actual = DelegateTypeCaster.CastTo<IntEnum, int>(Variable.IntEnum);
        }

        [BenchmarkCategory("Enum_Int"), Benchmark]
        public void Enum_Int_Dynamic()
        {
            var actual = DynamicTypeCaster.CastTo<IntEnum, int>(Variable.IntEnum);
        }
    }
}
