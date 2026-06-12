namespace FclEx.Http.Core.HttpRequestExtensions;

public class AddFormParamTests
{
    [Fact]
    public void AddFormParam_WithStringValue_AddsValueWithoutGenericTypeArgument()
    {
        var request = HttpRequest.Post("https://example.com/api");

        var result = request.AddFormParam("name", "alice");

        Assert.Same(request, result);
        Assert.Equal(["alice"], request.Form.GetValues("name"));
    }

    [Fact]
    public void AddFormParam_WithNullKey_AddsQueryStyleValue()
    {
        var request = HttpRequest.Post("https://example.com/api");

        var result = request.AddFormParam(null, "loose-value");

        Assert.Same(request, result);
        Assert.Equal(["loose-value"], request.Form.GetValues(null));
    }

    [Fact]
    public void AddFormParam_WithGenericValue_ConvertsValueToString()
    {
        var request = HttpRequest.Post("https://example.com/api");

        var result = request.AddFormParam("count", 3);

        Assert.Same(request, result);
        Assert.Equal(["3"], request.Form.GetValues("count"));
    }

    [Fact]
    public void AddFormParam_WithPairs_AddsAllPairs()
    {
        var request = HttpRequest.Post("https://example.com/api");

        request.AddFormParam(
        [
            KeyValuePair.Create("name", "alice"),
            KeyValuePair.Create("city", "st john's"),
        ]);

        Assert.Equal(["alice"], request.Form.GetValues("name"));
        Assert.Equal(["st john's"], request.Form.GetValues("city"));
    }

    [Fact]
    public void AddFormParam_WithMultiValuePairs_AddsEveryValue()
    {
        var request = HttpRequest.Post("https://example.com/api");

        request.AddFormParam(
        [
            KeyValuePair.Create("tag", new[] { "one", "two" }),
        ]);

        Assert.Equal(["one", "two"], request.Form.GetValues("tag"));
    }
}
