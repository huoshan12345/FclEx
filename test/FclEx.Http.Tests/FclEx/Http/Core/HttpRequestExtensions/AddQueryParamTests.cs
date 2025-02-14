namespace FclEx.Http.Core.HttpRequestExtensions;

public class AddQueryParamTests
{
    [Fact]
    public void AddQueryParam_Test()
    {
        var request = HttpRequest.Get("http://localhost")
            .AddQueryParam("path", "index");

        Assert.Equal("http://localhost/?path=index", request.GetUri().AbsoluteUri);
    }

    [Fact]
    public void AddQueryParam_UrlEncode_Test()
    {
        var request = HttpRequest.Get("http://localhost")
            .AddQueryParam("pa=th", "in=dex");

        Assert.Equal("http://localhost/?pa%3dth=in%3ddex", request.GetUri().AbsoluteUri);
    }

    [Fact]
    public void AddQueryParam_NullValue_Test()
    {
        var request = HttpRequest.Get("http://localhost")
            .AddQueryParam("index.html", null);

        Assert.Equal("http://localhost/?index.html=", request.GetUri().AbsoluteUri);
    }

    [Fact]
    public void AddQueryParam_NullKey_Test()
    {
        var request = HttpRequest.Get("http://localhost")
            .AddQueryParam(null, "index.html");

        Assert.Equal("http://localhost/?index.html", request.GetUri().AbsoluteUri);
    }

    [Fact]
    public void AddQueryValue_Test()
    {
        var request = HttpRequest.Get("http://localhost")
            .AddQueryValue("index.html");

        Assert.Equal("http://localhost/?index.html", request.GetUri().AbsoluteUri);
    }
}
