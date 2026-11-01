namespace FclEx.Http.Core;

public class HttpResponseExtensionsTests
{
    [Theory]
    [InlineData("text/html")]
    [InlineData("TEXT/HTML")]
    [InlineData("Text/Html; charset=utf-8")]
    public void IsHtml_MatchesMediaTypeCaseInsensitively(string contentType)
    {
        var response = new HttpResponse(HttpRequest.Get("https://example.com/"));
        response.Headers.Add(HttpHeaderNames.ContentType, contentType);

        Assert.True(response.IsHtml());
    }

    [Theory]
    [InlineData("text/xml")]
    [InlineData("application/xml")]
    [InlineData("APPLICATION/XML; charset=utf-8")]
    [InlineData("application/soap+xml")]
    [InlineData("image/svg+xml")]
    public void IsXml_RecognizesStandardXmlMediaTypes(string contentType)
    {
        var response = new HttpResponse(HttpRequest.Get("https://example.com/"));
        response.Headers.Add(HttpHeaderNames.ContentType, contentType);

        Assert.True(response.IsXml());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("invalid header")]
    [InlineData("application/json")]
    [InlineData("application/xml-dtd")]
    public void ContentTypeChecks_RejectMissingInvalidAndUnrelatedTypes(string? contentType)
    {
        var response = new HttpResponse(HttpRequest.Get("https://example.com/"));
        if (contentType is not null)
            response.Headers.Add(HttpHeaderNames.ContentType, contentType);

        Assert.False(response.IsHtml());
        Assert.False(response.IsXml());
    }

    [Fact]
    public void HasContentType_PredicateReceivesLastParseableMediaTypeWithoutParameters()
    {
        var response = new HttpResponse(HttpRequest.Get("https://example.com/"));
        response.Headers.Add(HttpHeaderNames.ContentType, "text/plain");
        response.Headers.Add(HttpHeaderNames.ContentType, "application/json; charset=utf-8");
        response.Headers.Add(HttpHeaderNames.ContentType, "invalid header");

        Assert.True(response.HasContentType(type => type == "application/json"));
        var missing = new HttpResponse(HttpRequest.Get("https://example.com/"));
        Assert.False(missing.HasContentType(_ => throw new InvalidOperationException()));
    }
}
