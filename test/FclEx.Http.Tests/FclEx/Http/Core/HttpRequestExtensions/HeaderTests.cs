namespace FclEx.Http.Core.HttpRequestExtensions;

[SuppressMessage("ReSharper", "CollectionNeverUpdated.Local")]
public class HeaderTests
{
    [Fact]
    public void AddHeader_WithEnumerableOverloads_AcceptsEmptyCollectionsAndExistingHeaders()
    {
        var request = HttpRequest.Get("http://localhost");
        {
            var pairs = new List<KeyValuePair<string, string>>();
            request.AddHeader(pairs);
        }
        {
            var pairs = new List<KeyValuePair<string, List<string>>>();
            request.AddHeader(pairs);
        }
        {
            request.AddHeader(request.Headers);
        }
    }

    [Fact]
    public void AddHeader_WithDictionary_AddsHeadersToRequest()
    {
        var headers = new Dictionary<string, string>
        {
            { "Content-Type", "application/json" },
            { "Authorization", "Bearer token123" },
            { "Accept", "application/json" },
        };

        var request = HttpRequest.Get("http://localhost");
        var result = request.AddHeader(headers);


        Assert.Same(request, result);
        Assert.Equal(3, request.Headers.Count);
        Assert.Equal("application/json", request.Headers.Get("Content-Type"));
        Assert.Equal("Bearer token123", request.Headers.Get("Authorization"));
        Assert.Equal("application/json", request.Headers.Get("Accept"));
    }

    [Fact]
    public void AddHeader_WithEmptyDictionary_ReturnsRequestUnchanged()
    {
        var headers = new Dictionary<string, string>();
        var request = HttpRequest.Get("http://localhost");
        var result = request.AddHeader(headers);

        Assert.Same(request, result);
        Assert.Empty(request.Headers);
    }

    [Fact]
    public void AddHeader_WithNullDictionary_ReturnsRequestUnchanged()
    {
        Dictionary<string, string> headers = null!;
        var request = HttpRequest.Get("http://localhost");
        var result = request.AddHeader(headers);
        Assert.Same(request, result);
        Assert.Empty(request.Headers);
    }

    [Fact]
    public void AddHeader_WithEmptyValues_AddsEmptyHeaderValues()
    {

        var headers = new Dictionary<string, string>
        {
            { "Content-Type", "application/json" },
            { "Authorization", "" },
            { "Accept", "application/json" },
        };
        var request = HttpRequest.Get("http://localhost");
        var result = request.AddHeader(headers);

        Assert.Same(request, result);
        Assert.Equal(3, request.Headers.Count);
        Assert.Equal("application/json", request.Headers.Get("Content-Type"));
        Assert.Equal("", request.Headers.Get("Authorization"));
        Assert.Equal("application/json", request.Headers.Get("Accept"));
    }

    [Fact]
    public void AddHeader_WithMultiValueDictionary_AddsMultipleHeadersForSameKey()
    {

        var multiValueHeaders = new Dictionary<string, List<string>>
        {
            { "Accept", ["application/json", "text/plain"] },
            { "X-Custom", ["value1", "value2", "value3"] }
        };
        var request = HttpRequest.Get("http://localhost");
        var result = request.AddHeader(multiValueHeaders);

        Assert.Same(request, result);
        Assert.Equal(5, request.Headers.Count);

        var acceptHeaders = request.Headers
            .Where(x => x.Key == "Accept")
            .Select(x => x.Value)
            .ToList();

        Assert.Equal(2, acceptHeaders.Count);
        Assert.Contains("application/json", acceptHeaders);
        Assert.Contains("text/plain", acceptHeaders);

        var customHeaders = request.Headers
            .Where(x => x.Key == "X-Custom")
            .Select(x => x.Value)
            .ToList();

        Assert.Equal(3, customHeaders.Count);
        Assert.Contains("value1", customHeaders);
        Assert.Contains("value2", customHeaders);
        Assert.Contains("value3", customHeaders);
    }

    [Fact]
    public void AddHeader_WithMultiValueDictionaryContainingEmptyValues_AddsEmptyHeaderValues()
    {

        var multiValueHeaders = new Dictionary<string, List<string>>
        {
            { "Accept", ["application/json", "", "text/plain"] }
        };
        var request = HttpRequest.Get("http://localhost");
        var result = request.AddHeader(multiValueHeaders);

        Assert.Same(request, result);
        Assert.Equal(3, request.Headers.Count);

        var acceptHeaders = request.Headers
            .Where(x => x.Key == "Accept")
            .Select(x => x.Value)
            .ToList();

        Assert.Equal(3, acceptHeaders.Count);
        Assert.Contains("application/json", acceptHeaders);
        Assert.Contains("", acceptHeaders);
        Assert.Contains("text/plain", acceptHeaders);
    }

    [Fact]
    public void AddHeader_WithEmptyMultiValueDictionary_ReturnsRequestUnchanged()
    {

        var multiValueHeaders = new Dictionary<string, List<string>>();
        var request = HttpRequest.Get("http://localhost");
        var result = request.AddHeader(multiValueHeaders);

        Assert.Same(request, result);
        Assert.Empty(request.Headers);
    }

    [Fact]
    public void TryAddHeader_WhenHeaderExists_DoesNotAddAnotherValue()
    {
        var request = HttpRequest.Get("http://localhost")
            .AddHeader("X-Trace", "existing");

        var result = request.TryAddHeader("X-Trace", "fallback");

        Assert.Same(request, result);
        Assert.Equal(["existing"], request.Headers.GetValues("X-Trace"));
    }

    [Fact]
    public void TryAddHeader_WhenHeaderIsMissing_AddsValue()
    {
        var request = HttpRequest.Get("http://localhost");

        var result = request.TryAddHeader("X-Trace", "fallback");

        Assert.Same(request, result);
        Assert.Equal("fallback", request.Headers.Get("X-Trace"));
    }

    [Fact]
    public void TrySetHeader_WhenHeaderExists_DoesNotOverwriteValue()
    {
        var request = HttpRequest.Get("http://localhost")
            .AddHeader("X-Trace", "existing")
            .AddHeader("X-Trace", "second");

        var result = request.TrySetHeader("X-Trace", "fallback");

        Assert.Same(request, result);
        Assert.Equal(["existing", "second"], request.Headers.GetValues("X-Trace"));
    }

    [Fact]
    public void TrySetHeader_WhenHeaderIsMissing_SetsValue()
    {
        var request = HttpRequest.Get("http://localhost");

        var result = request.TrySetHeader("X-Trace", "fallback");

        Assert.Same(request, result);
        Assert.Equal("fallback", request.Headers.Get("X-Trace"));
    }

    [Theory]
    [InlineData("Authorization:Bearer token:with:colon", ":", "Authorization", "Bearer token:with:colon")]
    [InlineData("X-Trace=abc=123", "=", "X-Trace", "abc=123")]
    [InlineData("X-Empty:", ":", "X-Empty", "")]
    [InlineData("X-Flag", ":", "X-Flag", "")]
    public void AddHeaderLine_ParsesLineUsingFirstSeparatorOnly(string line, string separator, string expectedKey, string expectedValue)
    {
        var request = HttpRequest.Get("http://localhost");

        var result = request.AddHeaderLine(line, separator);

        Assert.Same(request, result);
        Assert.Equal(expectedValue, request.Headers.Get(expectedKey));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void AddHeaderLine_WhenSeparatorIsEmpty_Throws(string? separator)
    {
        var request = HttpRequest.Get("http://localhost");

        Assert.ThrowsAny<ArgumentException>(() => request.AddHeaderLine("X-Trace:abc", separator!));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void AddHeaderLine_WhenPairIsEmpty_Throws(string? pair)
    {
        var request = HttpRequest.Get("http://localhost");

        Assert.ThrowsAny<ArgumentException>(() => request.AddHeaderLine(pair!));
    }

    [Fact]
    public void AcceptCompress_WhenEncodingsAreOmitted_UsesDefaultHandlerEncodings()
    {
        var request = HttpRequest.Get("http://localhost");

        var result = request.AcceptCompress();

        Assert.Same(request, result);
#if NET5_0_OR_GREATER
        Assert.Equal("gzip, deflate, br", request.Headers.Get(HttpHeaderNames.AcceptEncoding));
#else
        Assert.Equal("gzip", request.Headers.Get(HttpHeaderNames.AcceptEncoding));
#endif
    }

    [Fact]
    public void AcceptCompress_WhenEncodingsAreSpecified_UsesProvidedEncodings()
    {
        var request = HttpRequest.Get("http://localhost");

        var result = request.AcceptCompress(["zstd", "gzip"]);

        Assert.Same(request, result);
        Assert.Equal("zstd, gzip", request.Headers.Get(HttpHeaderNames.AcceptEncoding));
    }

    [Fact]
    public void AcceptChinese_SetsChineseAcceptLanguageHeader()
    {
        var request = HttpRequest.Get("http://localhost");

        var result = request.AcceptChinese();

        Assert.Same(request, result);
        Assert.Equal("zh-CN,zh;q=0.8", request.Headers.Get(HttpHeaderNames.AcceptLanguage));
    }

    [Fact]
    public void SetHeader_ReplacesExistingValuesAndReturnsRequest()
    {
        var request = HttpRequest.Get("http://localhost")
            .AddHeader("X-Trace", "one")
            .AddHeader("X-Trace", "two");

        var result = request.SetHeader("X-Trace", "three");

        Assert.Same(request, result);
        Assert.Equal(["three"], request.Headers.GetValues("X-Trace"));
    }

    [Fact]
    public void RemoveHeader_RemovesHeaderAndReturnsRequest()
    {
        var request = HttpRequest.Get("http://localhost")
            .AddHeader("X-Trace", "one");

        var result = request.RemoveHeader("X-Trace");

        Assert.Same(request, result);
        Assert.False(request.Headers.ContainsKey("X-Trace"));
    }

    [Fact]
    public void AddCookiesAndSetCookies_WriteCookieHeaderValues()
    {
        var request = HttpRequest.Get("http://localhost");
        var cookies = new[]
        {
            new Cookie("sid", "abc"),
            new Cookie("theme", "dark"),
        };

        request.AddCookies("lang=en");
        request.AddCookies(cookies);
        Assert.Equal(["lang=en", "sid=abc; theme=dark"], request.Headers.GetValues(HttpHeaderNames.Cookie));

        request.SetCookies(cookies);
        Assert.Equal(["sid=abc; theme=dark"], request.Headers.GetValues(HttpHeaderNames.Cookie));
    }

    [Fact]
    public void HeaderShortcutMethods_SetExpectedHeaderValues()
    {
        var request = HttpRequest.Get("http://localhost")
            .AcceptUtf8()
            .Ajax()
            .Referrer("https://referrer.example")
            .UserAgent("test-agent");

        Assert.Equal("utf-8", request.Headers.Get(HttpHeaderNames.AcceptCharset));
        Assert.Equal("XMLHttpRequest", request.Headers.Get(HttpHeaderNames.XRequestedWith));
        Assert.Equal("https://referrer.example", request.Referrer);
        Assert.Equal("test-agent", request.UserAgent);
    }

    [Fact]
    public void TryReferrerAndTryUserAgent_DoNotOverwriteExistingValues()
    {
        var request = HttpRequest.Get("http://localhost")
            .Referrer("https://first.example")
            .UserAgent("first-agent");

        request.TryReferrer("https://second.example")
            .TryUserAgent("second-agent");

        Assert.Equal("https://first.example", request.Referrer);
        Assert.Equal("first-agent", request.UserAgent);
    }

    [Fact]
    public void TryReferrerAndTryUserAgent_SetMissingValues()
    {
        var request = HttpRequest.Get("http://localhost");

        request.TryReferrer("https://first.example")
            .TryUserAgent("first-agent");

        Assert.Equal("https://first.example", request.Referrer);
        Assert.Equal("first-agent", request.UserAgent);
    }

    [Theory]
    [InlineData(true, CompressionMethod.GZip)]
    [InlineData(false, CompressionMethod.None)]
    public void UseGZip_UpdatesCompressionMethod(bool gzip, CompressionMethod expected)
    {
        var request = HttpRequest.Get("http://localhost");

        var result = request.UseGZip(gzip, CompressionLevel.Fastest);

        Assert.Same(request, result);
        Assert.Equal(expected, request.CompressionMethod);
        Assert.Equal(CompressionLevel.Fastest, request.CompressionLevel);
    }

    [Fact]
    public void Compression_SetsCompressionMethodAndLevel()
    {
        var request = HttpRequest.Get("http://localhost");

        var result = request.Compression(CompressionMethod.Deflate, CompressionLevel.Fastest);

        Assert.Same(request, result);
        Assert.Equal(CompressionMethod.Deflate, request.CompressionMethod);
        Assert.Equal(CompressionLevel.Fastest, request.CompressionLevel);
    }
}
