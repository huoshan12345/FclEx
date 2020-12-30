using System.Collections.Generic;
using System.Linq;
using MoreLinq.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace FclEx.Extensions.ValueTupleExtensions
{
    public class FirstValidTests
    {
        private readonly ITestOutputHelper _output;

        public FirstValidTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private void Print(int index, IEnumerable<string> items, string result)
        {
            _output.WriteLine($"[{index}]({items.Select(ToStr).JoinWith(", ")}) => {ToStr(result)}");

            static string ToStr(string m) => m switch
            {
                null => "null",
                "" => "empty",
                _ => m
            };
        }

        [Fact]
        public void TwoElements_Test()
        {
            foreach (var (i, items) in new[] { "test", "test2", string.Empty, null }.ToVariations(2).Index())
            {
                Assert.Equal(2, items.Count);
                var result = (items[0], items[1]).FirstValid();
                Assert.Equal(items.FirstOrDefault(m => m.IsValid()), result);
                Print(i, items, result);
            }
        }

        [Fact]
        public void ThreeElements_Test()
        {
            foreach (var (i, items) in new[] { "test", "test2", "test3", string.Empty, null }.ToVariations(3).Index())
            {
                Assert.Equal(3, items.Count);
                var result = (items[0], items[1], items[2]).FirstValid();
                Assert.Equal(items.FirstOrDefault(m => m.IsValid()), result);
                Print(i, items, result);
            }
        }

        [Fact]
        public void FourElements_Test()
        {
            foreach (var (i, items) in new[] { "test", "test2", "test3", "test4", string.Empty, null }.ToVariations(4).Index())
            {
                Assert.Equal(4, items.Count);
                var result = (items[0], items[1], items[2], items[3]).FirstValid();
                Assert.Equal(items.FirstOrDefault(m => m.IsValid()), result);
                Print(i, items, result);
            }
        }
    }
}
