using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FclEx.Helpers
{
    public class ObjectHelperTests
    {
        public static IEnumerable<int> Range { get; } = Enumerable.Range(1, 5);
        public static IEnumerable<object[]> Cases { get; } = Range.SelectMany(m => Range, (i, j) => new object[] { i, j }).ToArray();


        [Theory]
        [MemberData(nameof(Cases))]
        public void UpdateIfLessThan_Test(int obj, int newObj)
        {
            var old = obj;
            ObjectHelper.UpdateIfLessThan(ref obj, newObj);
            if (old < newObj)
                Assert.Equal(newObj, obj);
            else
                Assert.Equal(old, obj);
        }
    }
}
