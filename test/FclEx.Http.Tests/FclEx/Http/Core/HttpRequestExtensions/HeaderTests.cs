namespace FclEx.Http.Core.HttpRequestExtensions;

[SuppressMessage("ReSharper", "CollectionNeverUpdated.Local")]
public class HeaderTests
{
    [Fact]
    public void AddHeader_IEnumerable_KeyValuePair_Nullability()
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
    public void AddHeader_WithNullValues_HandlesGracefully()
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
    public void AddHeader_WithMultiValueDictionaryContainingNulls_HandlesGracefully()
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
}
