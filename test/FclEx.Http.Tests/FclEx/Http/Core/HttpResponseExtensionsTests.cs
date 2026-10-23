namespace FclEx.Http.Core;

public class HttpResponseExtensionsTests
{
    [Theory]
    [InlineData("text/html")]
    [InlineData("TEXT/HTML")]
    public void IsHtml_MatchesMediaTypeCaseInsensitively(string contentType)
    {
        var response = new HttpResponse(HttpRequest.Get("https://example.com/"));
        response.Headers.Add(HttpHeaderNames.ContentType, contentType);

        Assert.True(response.IsHtml());
    }

    [Theory]
    [InlineData("text/xml")]
    [InlineData("application/xml")]
    public void IsXml_RecognizesStandardXmlMediaTypes(string contentType)
    {
        var response = new HttpResponse(HttpRequest.Get("https://example.com/"));
        response.Headers.Add(HttpHeaderNames.ContentType, contentType);

        Assert.True(response.IsXml());
    }
}
