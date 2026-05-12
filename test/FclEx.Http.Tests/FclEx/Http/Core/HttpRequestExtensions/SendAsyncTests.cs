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
            m.IPVersionPolicy = ipv6
                ? IPVersionPolicy.OnlyIPv6
                : IPVersionPolicy.OnlyIPv4;
            m.ConnectTimeout = TimeSpan.FromSeconds(3);
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
    public async Task Form_Test()
    {
        if (HasApiServer == false)
            return;

        var random = new Random(1024);
        var expected = Enumerable.Range(1, 3).ToDictionary(m => m.ToString(), m => random.NextString(5));
        var response = await HttpRequest.Post("api/post")
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
        var response = await HttpRequest.Post("api/post")
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

        var response = await HttpRequest.Post("api/charset")
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
        var response = await HttpRequest.Get("api/redirect")
            .AddQueryParam("u", url)
            .EnsureSuccessStatusCode()
            .SendAsync(TestHttp)
            .ThrowIfError();

        Assert.Equal(2, response.RedirectUris.Count);
        Assert.Equal(url, response.LastUri().ToString());
    }
}