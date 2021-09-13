using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

namespace FclEx.Benchmarks
{
    [MemoryDiagnoser]
    public class CreateObjectTest
    {
        private static readonly Type _type = typeof(List<>);

        [Benchmark]
        public void CreateInstance()
        {
            Activator.CreateInstance(_type.MakeGenericType(typeof(int)), 4);
        }

        [Benchmark]
        public void Ctor()
        {
            var ctor = _type.MakeGenericType(typeof(int)).GetConstructor(new[] { typeof(int) });
            ctor.Invoke(new object[] { 4 });
        }

        [Benchmark]
        public void CreateObject()
        {
            _type.MakeGenericType(typeof(int)).CreateObject(4);
        }
    }
}
