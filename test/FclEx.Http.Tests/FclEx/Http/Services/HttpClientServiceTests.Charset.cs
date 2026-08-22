namespace FclEx.Http.Services;

public partial class HttpClientServiceTests
{
    [Theory]
    [InlineData("""<meta charset="utf-8">""", "utf-8")]
    [InlineData("<meta charset='gb2312'>", "gb2312")]
    [InlineData("<meta charset=utf-8>", "utf-8")]
    [InlineData("""<meta http-equiv="Content-Type" content="text/html; charset=gb2312">""", "gb2312")]
    [InlineData("<meta content='text/html; charset=utf-8' http-equiv='Content-Type'>", "utf-8")]
    public void GetMetaCharSet_ParsesCommonMetaForms(string html, string expected)
    {
        var actual = HtmlHelper.GetMetaCharSet(html);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void RegexesCharSet_CannotBeMutatedThroughListInterfaces()
    {
        var regexes = Regexes.CharSet;

        Assert.IsType<IReadOnlyList<Regex>>(regexes, exactMatch: false);

        if (regexes is IList<Regex> mutableList)
        {
            Assert.Throws<NotSupportedException>(() => mutableList[0] = new Regex(".*"));
        }
    }

    [Fact]
    public async Task SendAsync_WhenDetectedMetaCharsetIsInvalidAndInvalidCharsetsAreIgnored_FallsBackToUtf8()
    {
        using var service = CreateService(new HtmlResponseHandler("""<meta charset="not-a-charset"><p>ok</p>"""));

        var response = await HttpRequest.Get("https://example.com")
            .DetectCharSet()
            .SendAsync(service);

        Assert.False(response.IsError, response.Exception?.ToString());
        Assert.Equal("utf-8", response.Encoding?.WebName);
        Assert.Contains("ok", response.ResponseString);
    }

    [Fact]
    public async Task SendAsync_WhenDetectedMetaCharsetIsInvalidAndInvalidCharsetsAreNotIgnored_ReturnsError()
    {
        using var service = CreateService(new HtmlResponseHandler("""<meta charset="not-a-charset"><p>ok</p>"""));
        var request = HttpRequest.Get("https://example.com")
            .DetectCharSet();
        request.IgnoreInvalidCharSet = false;

        var response = await request.SendAsync(service);

        Assert.True(response.IsError);
        Assert.IsType<InvalidOperationException>(response.Exception);
        Assert.Contains("not-a-charset", response.Exception.Message);
    }

    [Fact]
    public async Task SendAsync_WhenHeaderCharsetExists_DoesNotUseInvalidMetaCharset()
    {
        using var service = CreateService(new HtmlResponseHandler(
            """<meta charset="not-a-charset"><p>ok</p>""",
            headerCharSet: "utf-8"));
        var request = HttpRequest.Get("https://example.com")
            .DetectCharSet();
        request.IgnoreInvalidCharSet = false;

        var response = await request.SendAsync(service);

        Assert.False(response.IsError, response.Exception?.ToString());
        Assert.Equal("utf-8", response.Encoding?.WebName);
        Assert.Contains("ok", response.ResponseString);
    }

    private sealed class HtmlResponseHandler(string html, string? headerCharSet = null) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var content = new ByteArrayContent(Encoding.UTF8.GetBytes(html));
            content.Headers.ContentType = new MediaTypeHeaderValue("text/html")
            {
                CharSet = headerCharSet,
            };

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                RequestMessage = request,
                Content = content,
            });
        }
    }
}
