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
}
