using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;
using FclEx.TypeCasters;
using FclEx.Utils;

namespace FclEx.Benchmark
{
    [MemoryDiagnoser]
    public class CastTest
    {
        private const IntEnum Enum = IntEnum.Yes;
        private const int Number = 100;
        private static readonly object Obj = 100;
        private static readonly CommonTypeCaster CommonTypeCaster = CommonTypeCaster.Instance;
        private static readonly ExpressionTypeCaster ExpressionTypeCaster = ExpressionTypeCaster.Instance;
        private static readonly DelegateTypeCaster DelegateTypeCaster = DelegateTypeCaster.Instance;

        [BenchmarkCategory("Int_Object"), Benchmark]
        public void Int_Object_Common()
        {
            var actual = CommonTypeCaster.CastTo<int, object>(Number);
            var expected = (object)Number;
            Check.Equal(actual, expected, nameof(Number));
        }

        [BenchmarkCategory("Int_Object"), Benchmark]
        public void Int_Object_Expression()
        {
            var actual = ExpressionTypeCaster.CastTo<int, object>(Number);
            var expected = (object)Number;
            Check.Equal(actual, expected, nameof(Number));
        }

        [BenchmarkCategory("Int_Object"), Benchmark]
        public void Int_Object_Delegate()
        {
            var actual = DelegateTypeCaster.CastTo<int, object>(Number);
            var expected = (object)Number;
            Check.Equal(actual, expected, nameof(Number));
        }


        [BenchmarkCategory("Object_Int"), Benchmark]
        public void Object_Int_Common()
        {
            var actual = CommonTypeCaster.CastTo<object, int>(Obj);
            var expected = (int)Obj;
            Check.Equal(actual, expected, nameof(Number));
        }

        [BenchmarkCategory("Object_Int"), Benchmark]
        public void Object_Int_Expression()
        {
            var actual = ExpressionTypeCaster.CastTo<object, int>(Obj);
            var expected = (int)Obj;
            Check.Equal(actual, expected, nameof(Number));
        }

        [BenchmarkCategory("Object_Int"), Benchmark]
        public void Object_Int_Delegate()
        {
            var actual = DelegateTypeCaster.CastTo<object, int>(Obj);
            var expected = (int)Obj;
            Check.Equal(actual, expected, nameof(Number));
        }

        [BenchmarkCategory("Enum_Int"), Benchmark]
        public void Enum_Int_Common()
        {
            var actual = CommonTypeCaster.CastTo<IntEnum, int>(Enum);
            var expected = (int)Enum;
            Check.Equal(actual, expected, nameof(Enum));
        }
    }
}
