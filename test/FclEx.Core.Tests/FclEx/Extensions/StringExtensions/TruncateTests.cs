using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FclEx.Extensions.StringExtensions;

public class TruncateTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void NullOrEmpty_Return_Empty(string? str)
    {
        Assert.Equal("", str.Truncate(10));
    }

    [Fact]
    public void ExceededLength_Return_Self()
    {
        var random = new Random(0);

        for (var i = 1; i < 100; i++)
        {
            var str = random.NextString(i);

            for (var j = 0; j < 5; j++)
            {
                Assert.Equal(str, str.Truncate(i + j));
            }
        }
    }

    [Fact]
    public void LessThanLength_Return_SubString()
    {
        var random = new Random(0);

        for (var i = 2; i < 100; i++)
        {
            var str = random.NextString(i);

            for (var j = 1; j < i; j++)
            {
                Assert.Equal(str[..(i - j)] + "...", str.Truncate(i - j));
            }
        }
    }
}