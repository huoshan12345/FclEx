using System.Linq;
using Xunit;

namespace FclEx.Extensions.EnumExtensions
{
    public class IsValidTests
    {
        private enum Tester
        {
            No = 0,
            Yes = 1,
        }

        [Fact]
        public void IsValid_Test()
        {
            var values = Enumerable.Range(-5, 10).Cast<Tester>();
            foreach (var value in values)
            {
                Assert.Equal(value == Tester.No || value == Tester.Yes, value.IsValid());
            }
        }

        [Fact]
        public void IsEachValid_Test()
        {
            var values = Enumerable.Range(-5, 10).Cast<Tester>().ToArray();
            Assert.Equal(values.All(m => m == Tester.No || m == Tester.Yes), values.IsEachValid());
        }
    }
}
