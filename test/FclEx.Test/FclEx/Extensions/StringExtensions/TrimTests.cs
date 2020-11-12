using System;
using Xunit;

namespace FclEx.Extensions.StringExtensions
{
    public class TrimTests
    {
        [Fact]
        public void TrimStart_Test()
        {
            string str = null;
            Assert.Throws<ArgumentNullException>(() => str.TrimStart(""));

            str = "aa_xx";
            var newStr = str.TrimStart("aa");
            Assert.Equal("_xx", newStr);

            newStr = str.TrimStart("_");
            Assert.Equal(str, newStr);

            newStr = str.TrimStart("");
            Assert.Equal(str, newStr);
        }

        [Fact]
        public void TrimEnd_Test()
        {
            string str = null;
            Assert.Throws<ArgumentNullException>(() => str.TrimEnd(""));

            str = "aa_xx";
            var newStr = str.TrimEnd("xx");
            Assert.Equal("aa_", newStr);

            newStr = str.TrimEnd("_");
            Assert.Equal(str, newStr);

            newStr = str.TrimEnd("");
            Assert.Equal(str, newStr);
        }
    }
}
