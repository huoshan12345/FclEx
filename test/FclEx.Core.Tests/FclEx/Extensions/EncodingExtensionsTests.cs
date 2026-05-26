using System.Text;

namespace FclEx.Extensions;

public class EncodingExtensionsTests
{
    [Fact]
    public void Utf8WithoutBom_ShouldReturnUtf8EncodingWithoutPreamble()
    {
        var encoding = Encoding.Utf8WithoutBom;

        Assert.Equal("utf-8", encoding.WebName);
        Assert.Empty(encoding.GetPreamble());
    }

    [Fact]
    public void Utf8WithoutBom_ShouldReturnCachedInstance()
    {
        var first = Encoding.Utf8WithoutBom;
        var second = Encoding.Utf8WithoutBom;

        Assert.Same(first, second);
    }
}
