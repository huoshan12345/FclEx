using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FclEx.Test.Extensions.TypeExtensions
{
    public class GetAnyElementTypeTests
    {
        public static IEnumerable<object[]> Cases { get; } = new(object, Type)[]
        {
            (new []{1}, typeof(int)),
            (new List<int>(), typeof(int)),
            (Enumerable.Range(1, 2), typeof(int)),
            (new Dictionary<string, int>(), typeof(KeyValuePair<string, int>))
        }.Select(m => new object[] { m.Item1.GetType(), m.Item2 }).ToArray();

        [Theory]
        [MemberData(nameof(Cases))]
        public void Test(Type type, Type expected)
        {
            var t = type.GetAnyElementType();
            Assert.Equal(expected, t);
        }
    }
}
