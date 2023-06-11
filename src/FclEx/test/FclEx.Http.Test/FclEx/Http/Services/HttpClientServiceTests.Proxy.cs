namespace FclEx.Http.Services;

partial class HttpClientServiceTests
{
    public static IWebProxy[] ProxyList { get; } =
    {
        GlobalConstants.DefaultProxy
    };

    public static string[] Urls { get; } =
    {
        "https://www.google.com/",
        "https://www.instagram.com/",
        "https://www.baidu.com/",
    };

    public static IEnumerable<object[]> Cases { get; } = ProxyList.SelectMany(m => Urls, (x, y) => new object[] { x, y });

    [Theory]
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


        var http = new HttpClientService(true, proxy);
        var res = await HttpReq.Get(url)
            .ConnectTimeout(TimeSpan.FromSeconds(10))
            .SendAsync(http);
        AssertExt.False(res.HasError, () => res.Exception!.ToString());
    }
}