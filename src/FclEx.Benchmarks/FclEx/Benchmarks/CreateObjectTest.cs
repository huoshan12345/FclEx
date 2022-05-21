using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using FclEx.Extensions;

namespace FclEx.Benchmarks
{
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
            var ctor = _type.MakeGenericType<int>().GetConstructor(new[] { typeof(int) })!;
            ctor.Invoke(new object[] { 4 });
        }

        [Benchmark]
        public void CreateObject()
        {
            _type.MakeGenericType<int>().CreateObject(4);
        }
    }
}
