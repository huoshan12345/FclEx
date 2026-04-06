namespace FclEx.Http.Services;

[SuppressMessage("ReSharper", "ReturnTypeCanBeEnumerable.Global")]
partial class HttpClientServiceTests
{
    public static readonly IWebProxy[] ProxyList = [DefaultProxy];

    public static readonly TheoryData<IWebProxy, string> Cases = ProxyList.CrossJoin(TestUrls).ToTheoryData();

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