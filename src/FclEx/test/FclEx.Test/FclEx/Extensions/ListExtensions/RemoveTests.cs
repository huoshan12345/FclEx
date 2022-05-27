using System.Linq;
using Xunit;

namespace FclEx.Extensions.ListExtensions
{
    public class RemoveTests
    {
        [Fact]
        public void Remove_Filter_Test()
        {
            var col = Enumerable.Range(1, 10).ToList();
            col.RemoveAll(m => m % 2 != 0);
            Assert.Equal(Enumerable.Range(1, 5).Select(m => m * 2), col);
        }
    }
}
