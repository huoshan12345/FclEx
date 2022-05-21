using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using FclEx.Extensions;

namespace FclEx.Benchmarks
{
    [MemoryDiagnoser]
    public class GetDefaultTest
    {
        public static IEnumerable<object[]> Cases => new[]
        {
            typeof(int),
            typeof(string),
            typeof(DateTime),
            typeof(List<int>),
        }.Select(m => new object[] { m }).ToArray();

        [Benchmark]
        [ArgumentsSource(nameof(Cases))]
        public void DefaultValue(Type type)
        {
            type.DefaultValue();
        }

        [Benchmark]
        [ArgumentsSource(nameof(Cases))]
        public void DefaultValueByExp(Type type)
        {
            type.DefaultValueByExp();
        }
    }
}
