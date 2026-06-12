namespace FclEx.Http.Core.HttpRequestExtensions;

public class DumpTests
{
    [Theory]
    [InlineData("/relative/path?x=1")]
    [InlineData("relative/path?x=1")]
    public void GetUri_WhenRequestUriIsRelative_ReturnsRelativeUri(string uri)
    {
        var actual = HttpRequest.Get(uri).GetUri();

        Assert.False(actual.IsAbsoluteUri);
        Assert.Equal(uri, actual.ToString());
    }

    [Fact]
    public void Dump_WhenRequestUriIsRelativeAndServiceIsProvided_DoesNotThrow()
    {
        var request = HttpRequest.Get("/relative/path?x=1")
            .AddHeader("X-Test", "yes");
        using var service = HttpClientService.Create(true);

        var dump = request.Dump(service);

        Assert.Contains("GET /relative/path?x=1", dump);
        Assert.Contains("X-Test: yes", dump);
    }

    [Fact]
    public void Dump_WithExplicitCookies_AppendsCookieHeader()
    {
        var request = HttpRequest.Post("https://example.com/api")
            .AddHeader("X-Test", "yes");
        var cookies = new[]
        {
            new Cookie("sid", "abc"),
            new Cookie("theme", "dark"),
        };

        var dump = request.Dump(cookies);

        Assert.Contains("POST https://example.com/api", dump);
        Assert.Contains("X-Test: yes", dump);
        Assert.Contains("Cookie: sid=abc; theme=dark", dump);
    }

    [Fact]
    public void Dump_WithNoCookies_DoesNotAppendCookieHeader()
    {
        var request = HttpRequest.Get("https://example.com/api");

        var dump = request.Dump([]);

        Assert.Contains("GET https://example.com/api", dump);
        Assert.DoesNotContain("Cookie:", dump);
    }
}
