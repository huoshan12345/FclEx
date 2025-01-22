namespace FclEx.Http.Services;

[SuppressMessage("ReSharper", "ReturnTypeCanBeEnumerable.Global")]
partial class HttpClientServiceTests
{
    public static IWebProxy[] ProxyList { get; } =
    [
        DefaultProxy
    ];

    public static string[] Urls { get; } =
    [
        "https://www.google.com/",
        "https://www.instagram.com/",
        "https://www.baidu.com/"
    ];

    public static IEnumerable<object[]> Cases { get; } = ProxyList.SelectMany(m => Urls, (x, y) => new object[] { x, y });

    [RetryTheory]
    [MemberData(nameof(Cases))]
    public async Task SendAsync_WithProxy_Success(IWebProxy proxy, string url)
    {
        //var client = new HttpClient(new SocketsHttpHandler
        //{
        //    Proxy = proxy,
        //    UseProxy = true,
        //});
        //var response = await client.GetAsync(url);
        //Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var http = HttpClientService.Create(proxy);
        var res = await HttpRequest.Get(url)
            .ReadHeadersTimeout(TimeSpan.FromSeconds(10))
            .SendAsync(http);
        AssertEx.False(res.HasError, () => res.Exception!.ToString());
    }
}