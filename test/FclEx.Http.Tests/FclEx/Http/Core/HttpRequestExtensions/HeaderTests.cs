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
}