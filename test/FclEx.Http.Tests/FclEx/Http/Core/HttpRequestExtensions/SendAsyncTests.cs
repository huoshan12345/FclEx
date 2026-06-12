namespace FclEx.Http.Core.HttpRequestExtensions;

public class SendAsyncTests : HttpServerTests
{
    public static TheoryData<string> TestUrlCases => TestUrls.ToTheoryData();

    public static bool InterfaceHasIpv6Enabled(NetworkInterface @interface)
    {
        try
        {
            if (@interface.Supports(NetworkInterfaceComponent.IPv6) == false)
                return false;

            var addresses = @interface.GetIPProperties()
                .UnicastAddresses
                .Select(m => m.Address)
                .ToArray();

            return addresses.Any(m => m.IsIPv6() && m.IsPrivate() == false);
        }
        catch (NetworkInformationException)
        {
            return false;
        }
    }

    private static readonly bool _supportsIPv6 = NetworkInterface.GetAllNetworkInterfaces()
        .Take(1)
        .Any(InterfaceHasIpv6Enabled);

    [LocalOnlyTheory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Get_IpVersion_Test(bool ipv6)
    {
        if (ipv6 && _supportsIPv6 == false)
            return;

        using var http = HttpClientService.Create(m =>
        {
            m.HandlerOptions = new()
            {
                IPVersionPolicy = ipv6
                    ? IPVersionPolicy.OnlyIPv6
                    : IPVersionPolicy.OnlyIPv4,
                ConnectTimeout = TimeSpan.FromSeconds(3)
            };
        });

        const string ipv4Url = "https://ip4only.me/api/";
        const string ipv6Url = "https://ip6only.me/api/";
        var url = ipv6 ? ipv6Url : ipv4Url;
        var response = await HttpRequest.Get(url)
            .ReadHeadersTimeout(TimeSpan.FromSeconds(10))
            .SendAsync(http);

        response.ThrowIfError();
    }

    [Theory]
    [MemberData(nameof(TestUrlCases))]
    public async Task Get_Test(string url)
    {
        var response = await HttpRequest.Get(url)
            .ReadHeadersTimeout(TimeSpan.FromSeconds(5))
            .SendAsync();

        response.ThrowIfError();
    }

    [Fact]
    public async Task SendAsync_WithOptimisticTimeoutPolicy_PassesPolicyCancellationTokenToService()
    {
        using var service = new DelayedHttpService();
        var policy = Polly.Policy.TimeoutAsync(TimeSpan.FromMilliseconds(20));

        await Assert.ThrowsAsync<TimeoutRejectedException>(() =>
            HttpRequest.Get("https://example.com/").SendAsync(service, policy));

        Assert.True(service.ObservedToken.CanBeCanceled);
        Assert.True(service.ObservedToken.IsCancellationRequested);
    }

    [Fact]
    public async Task Form_Test()
    {
        if (HasApiServer == false)
            return;

        var random = new Random(1024);
        var expected = Enumerable.Range(1, 3).ToDictionary(m => m.ToString(), m => random.NextString(5));
        var response = await HttpRequest.Post(TestApiPaths.Post)
            .AddFormParam(expected)
            .ReadHeadersTimeout(TimeSpan.FromSeconds(5))
            .SendAsync(TestHttp)
            .ThrowIfError();

        Assert.False(response.IsError);
        var body = response.ResponseString;
        Assert.NotNull(body);
        var actual = UriParams.Parse(body)
            .ToDictionary(m => m.Key, m => m.Value);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task Json_Test()
    {
        if (HasApiServer == false)
            return;

        var list = Enumerable.Range(1, 10).ToList();
        var response = await HttpRequest.Post(TestApiPaths.Post)
            .JsonContent(list)
            .SendAsync(TestHttp)
            .ThrowIfError();

        Assert.False(response.IsError);
        var body = response.ResponseString.ToJsonNode();
        Assert.NotNull(body);
        var actual = body.Deserialize<List<int>>();
        Assert.True(list.SequenceEqual(actual!));
    }

    [Theory]
    [InlineData("utf8")]
    [InlineData("utf-8")]
    [InlineData("UTF-8")]
    [InlineData("\"gbk\"")]
    [InlineData("'gbk'")]
    [InlineData("gbk")]
    [InlineData("gb2312")]
    [InlineData("gb18030")]
    [InlineData("big5")]
    [InlineData("ISO-8859-1")]
    public async Task CharSet_Test(string charSet)
    {
        if (HasApiServer == false)
            return;

        var response = await HttpRequest.Post(TestApiPaths.Charset)
            .AddQueryParam("charset", charSet)
            .SendAsync(TestHttp)
            .ThrowIfError();

        Assert.False(response.IsError);
        var contentType = response.Headers.Get(HttpHeaderNames.ContentType).FirstOrDefault();
        Assert.NotNull(contentType);
        Assert.Contains(charSet, contentType);

        var expectedEncoding = GetEncoding(charSet);
        Assert.Equal(expectedEncoding, response.Encoding);

        static Encoding GetEncoding(string charSet)
        {
            charSet = charSet.Trim('\'').Trim('"');
            return charSet switch
            {
                "utf8" => Encoding.UTF8,
                _ => Encoding.GetEncoding(charSet),
            };
        }
    }

    [Fact]
    public async Task Redirection_Test()
    {
        if (HasApiServer == false)
            return;

        var url = TestUrls.First();
        var response = await HttpRequest.Get(TestApiPaths.Redirect)
            .AddQueryParam("u", url)
            .EnsureSuccessStatusCode()
            .SendAsync(TestHttp)
            .ThrowIfError();

        Assert.Equal(2, response.VisitedUris.Count);
        Assert.Equal(url, response.LastUri().ToString());
    }

    private sealed class DelayedHttpService : IHttpService
    {
        public CancellationToken ObservedToken { get; private set; }
        public IWebProxy? Proxy { get; set; }
        public ILogger Logger { get; set; } = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

        public async Task<HttpResponse> SendAsync(HttpRequest request, CancellationToken token = default)
        {
            ObservedToken = token;
            await Task.Delay(TimeSpan.FromSeconds(5), token);
            return new HttpResponse(request);
        }

        public Cookie? GetCookie(Uri uri, string name)
        {
            return null;
        }

        public IReadOnlyCollection<Cookie> GetCookies(Uri uri)
        {
            return [];
        }

        public void AddCookie(Cookie cookie, Uri? uri = null, bool overrideDomain = false)
        {
        }

        public IReadOnlyCollection<Cookie> GetAllCookies()
        {
            return [];
        }

        public void Dispose()
        {
        }
    }
}
