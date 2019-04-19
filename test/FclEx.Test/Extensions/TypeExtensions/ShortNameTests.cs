using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FclEx.Test.Extensions.TypeExtensions
{
    public class ShortNameTests
    {
        public static IEnumerable<object[]> Cases { get; } = new (object, string)[]
        {
            (typeof(int), nameof(Int32)),
            (typeof(Dictionary<string, int>), "Dictionary<String, Int32>"),
            (typeof(Dictionary<List<string>, HashSet<int>>), "Dictionary<List<String>, HashSet<Int32>>"),
        }.Select(m => new[] { m.Item1, m.Item2 }).ToArray();

        [Theory]
        [MemberData(nameof(Cases))]
        public void Test(Type type, string expectedName)
        {
            var name = type.ShortName();
            Assert.Equal(expectedName, name);
        }

        [Fact]
        public void TestAllType()
        {
            var allTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(m => m.GetTypes()).ToArray();
            foreach (var type in allTypes)
            {
                var simpleName = type.SimpleName();
                var shortName = type.ShortName();
                if (!type.IsGenericType)
                {
                    Assert.Equal(simpleName, shortName);
                }
            }

        }
    }
}
