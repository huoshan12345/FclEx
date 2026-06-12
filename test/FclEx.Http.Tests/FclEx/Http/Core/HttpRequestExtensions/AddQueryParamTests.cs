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

    [Fact]
    public void AddQueryParam_WithPairs_AddsAllPairsAndReturnsRequest()
    {
        var request = HttpRequest.Get("http://localhost");

        var result = request.AddQueryParam(
        [
            KeyValuePair.Create("name", "alice"),
            KeyValuePair.Create("city", "st john's"),
        ]);

        Assert.Same(request, result);
        Assert.Equal("http://localhost/?name=alice&city=st+john%27s", request.GetUri().AbsoluteUri);
    }

    [Fact]
    public void AddQueryParam_WithMultiValuePairs_AddsEveryValue()
    {
        var request = HttpRequest.Get("http://localhost");

        request.AddQueryParam(
        [
            KeyValuePair.Create("tag", new[] { "one", "two" }),
        ]);

        Assert.Equal("http://localhost/?tag=one&tag=two", request.GetUri().AbsoluteUri);
    }

    [Fact]
    public void AddQueryParam_WithBuilder_AddsBuiltValues()
    {
        var request = HttpRequest.Get("http://localhost");

        var result = request.AddQueryParam(new TestNameValuesBuilder());

        Assert.Same(request, result);
        Assert.Equal("http://localhost/?name=alice&count=3", request.GetUri().AbsoluteUri);
    }

    private sealed class TestNameValuesBuilder : NameValuesBuilder
    {
        [NameValue("name")]
        public string Name { get; } = "alice";

        [NameValue("count")]
        public int Count { get; } = 3;
    }
}
