using System;
using System.Linq;
using Xunit;

namespace FclEx.Test.Extensions
{
    public class ArrayExtensionsTests
    {
        [Fact]
        public void Segments_Test()
        {
            var arr = Enumerable.Range(1, 10).ToArray();
            var size = 4;
            var segments = arr.Segments(size).ToList();

            Assert.Equal(0, segments[0].Offset);
            Assert.Equal(size, segments[0].Count);

            Assert.Equal(4, segments[1].Offset);
            Assert.Equal(size, segments[1].Count);

            Assert.Equal(8, segments[2].Offset);
            Assert.Equal(2, segments[2].Count);
        }

        [Fact]
        public void Segments_Null_Test()
        {
            int[] arr = null;
            Assert.Throws<ArgumentNullException>(() => arr.Segments(4).ToList());
        }

        [Fact]
        public void Segments_InvalidSize_Test()
        {
            var arr = Enumerable.Range(1, 10).ToArray();
            Assert.Throws<ArgumentOutOfRangeException>(() => arr.Segments(0).ToList());
        }
    }
}
