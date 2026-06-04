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
}
