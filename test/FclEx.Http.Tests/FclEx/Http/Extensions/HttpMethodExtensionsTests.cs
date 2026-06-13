namespace FclEx.Http.Extensions;

public class HttpMethodExtensionsTests
{
    [Theory]
    [InlineData("GET")]
    [InlineData("get")]
    [InlineData("GeT")]
    public void IsGet_WhenMethodTextDiffersOnlyByCase_ReturnsTrue(string method)
    {
        Assert.True(new HttpMethod(method).IsGet());
    }

    [Theory]
    [InlineData("POST")]
    [InlineData("HEAD")]
    [InlineData("GETX")]
    public void IsGet_WhenMethodTextIsNotExactlyGet_ReturnsFalse(string method)
    {
        Assert.False(new HttpMethod(method).IsGet());
    }
}
