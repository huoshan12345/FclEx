namespace FclEx.Http.Extensions;

public class HttpStatusCodeExtensionsTests
{
    [Theory]
    [InlineData(100, true, false, false, false, false, HttpStatusCodeClass.Informational)]
    [InlineData(199, true, false, false, false, false, HttpStatusCodeClass.Informational)]
    [InlineData(200, false, true, false, false, false, HttpStatusCodeClass.Successful)]
    [InlineData(299, false, true, false, false, false, HttpStatusCodeClass.Successful)]
    [InlineData(300, false, false, true, false, false, HttpStatusCodeClass.Redirection)]
    [InlineData(399, false, false, true, false, false, HttpStatusCodeClass.Redirection)]
    [InlineData(400, false, false, false, true, false, HttpStatusCodeClass.ClientError)]
    [InlineData(499, false, false, false, true, false, HttpStatusCodeClass.ClientError)]
    [InlineData(500, false, false, false, false, true, HttpStatusCodeClass.ServerError)]
    [InlineData(599, false, false, false, false, true, HttpStatusCodeClass.ServerError)]
    [InlineData(600, false, false, false, false, false, HttpStatusCodeClass.Unknown)]
    [InlineData(99, false, false, false, false, false, HttpStatusCodeClass.Unknown)]
    [InlineData(0, false, false, false, false, false, HttpStatusCodeClass.Unknown)]
    [InlineData(-1, false, false, false, false, false, HttpStatusCodeClass.Unknown)]
    public void StatusClassMethods_ClassifyByHttpStatusCodeRange(
        int value,
        bool isInfo,
        bool isSuccess,
        bool isRedirection,
        bool isClientError,
        bool isServerError,
        HttpStatusCodeClass codeType)
    {
        var code = (HttpStatusCode)value;

        Assert.Equal(isInfo, code.IsInfo());
        Assert.Equal(isSuccess, code.IsSuccess());
        Assert.Equal(isRedirection, code.IsRedirection());
        Assert.Equal(isClientError, code.IsClientError());
        Assert.Equal(isServerError, code.IsServerError());
        Assert.Equal(codeType, code.GetStatusCodeClass());
    }

    [Theory]
    [InlineData(301, false)]
    [InlineData(302, false)]
    [InlineData(303, false)]
    [InlineData(307, true)]
    [InlineData(308, true)]
    public void PreservesMethodAndContent_ReturnsTrueOnlyFor307And308(int value, bool expected)
    {
        var statusCode = (HttpStatusCode)value;

        Assert.Equal(expected, statusCode.PreservesMethodAndContent());
    }

    [Fact]
    public void ToPairString_ReturnsNameAndNumericValue()
    {
        Assert.Equal("NotFound/404", HttpStatusCode.NotFound.ToPairString());
    }

    [Fact]
    public void EnsureSuccess_WhenStatusCodeIsFailure_ThrowsWithStatusCode()
    {
        var uri = new Uri("https://example.com/items");

        var ex = Assert.Throws<HttpRequestException>(() =>
            HttpStatusCode.BadGateway.EnsureSuccess(uri, "POST"));

        Assert.Equal(HttpStatusCode.BadGateway, ex.StatusCode);
        Assert.Contains("BadGateway/502", ex.Message);
        Assert.Contains("POST https://example.com/items", ex.Message);
    }

    [Fact]
    public void EnsureSuccess_WhenStatusCodeIsSuccess_DoesNotThrow()
    {
        HttpStatusCode.NoContent.EnsureSuccess(new Uri("https://example.com/items"), "DELETE");
    }
}
