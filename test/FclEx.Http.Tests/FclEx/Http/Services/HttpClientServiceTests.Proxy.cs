namespace FclEx.Http.Services;

[SuppressMessage("ReSharper", "ReturnTypeCanBeEnumerable.Global")]
partial class HttpClientServiceTests
{
    public static IWebProxy[] ProxyList { get; } =
    [
        DefaultProxy,
    ];

    public static IEnumerable<object[]> Cases { get; } = ProxyList.SelectMany(m => TestUrls, (x, y) => new object[] { x, y });

    [RetryTheory]
    [MemberData(nameof(Cases))]
    public async Task SendAsync_WithProxy_Success(IWebProxy proxy, string url)
    {
        var http = HttpClientService.Create(proxy);
        var response = await HttpRequest.Get(url)
            .ReadHeadersTimeout(TimeSpan.FromSeconds(10))
            .SendAsync(http);
        Assert.False(response.IsError, () => response.Exception!.ToString());
    }
}