using System.Net.NetworkInformation;
using System.Web;
using FclEx.Http.Tests;

namespace FclEx.Http.Core.HttpRequestTests;

internal sealed class HttpEventListener : EventListener
{
    private readonly ITestOutputHelper _output;

    public HttpEventListener(ITestOutputHelper output)
    {
        _output = output;
    }

    protected override void OnEventSourceCreated(EventSource eventSource)
    {
        // Allow internal HTTP logging
        if (eventSource.Name is "Private.InternalDiagnostics.System.Net.Http" or "System.Net.Http")
        {
            EnableEvents(eventSource, EventLevel.LogAlways);
        }
    }

    protected override void OnEventWritten(EventWrittenEventArgs eventData)
    {
        var time = eventData.TimeStamp.SpecifyKind(DateTimeKind.Local);
        var sb = new StringBuilder().Append($"[{time:HH:mm:ss.ffffff}][{eventData.EventName}] ");

        foreach (var ((name, item), _, isFirst, _) in eventData.PayloadNames.EmptyIfNull().Zip(eventData.Payload.EmptyIfNull()).IndexExt())
        {
            if (isFirst == false)
                sb.Append(", ");
            sb.Append(name).Append(": ").Append(item);
        }
        _output.WriteLine(sb.ToString());
    }
}

public class SendAsyncTests : IAssemblyFixture<GlobalFixture>
{
    public static string[] Urls =>
    [
        "https://www.baidu.com/",
        "https://www.qq.com/",
        "https://www.google.com.hk/"
    ];

    public static IEnumerable<object[]> Cases => Urls
        .Select(m => new object[] { m });

    private readonly ITestOutputHelper _output;

    public SendAsyncTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private static readonly bool _supportsIPv6 = NetworkInterface.GetAllNetworkInterfaces()
        .First()
        .Supports(NetworkInterfaceComponent.IPv6);

    [LocalOnlyTheory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Get_IPVersion_Test(bool ipv6)
    {
        if (ipv6 && _supportsIPv6 == false)
            return;

        using var x = _output.SetConsole();

        // Keep the listener around while you want the logging to continue, dispose it after.
        using var listener = new HttpEventListener(_output);

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
        var res = await HttpRequest.Get(url)
            .ReadHeadersTimeout(TimeSpan.FromSeconds(10))
            .SendAsync(http)
            .IgnoreSyncContext();
        res.ThrowIfError();
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task Get_Test(string url)
    {
        var res = await HttpRequest.Get(url)
            .ReadHeadersTimeout(TimeSpan.FromSeconds(5))
            .SendAsync()
            .IgnoreSyncContext();
        res.ThrowIfError();
    }

    [Fact]
    public async Task Form_Test()
    {
        var random = new Random(1024);
        var expected = Enumerable.Range(1, 3).ToDictionary(m => m.ToString(), m => random.NextString(5));
        var res = await HttpRequest.Post("api/post")
            .AddData(expected!)
            .ReadHeadersTimeout(TimeSpan.FromSeconds(5))
            .SendAsync(TestHttp)
            .ThrowIfError()
            .IgnoreSyncContext();

        Assert.False(res.HasError);
        var body = res.ResponseString;
        Assert.NotNull(body);
        var actual = HttpUtility.ParseQueryString(body).ToDictionary();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task Json_Test()
    {
        var list = Enumerable.Range(1, 10).ToList();
        var res = await HttpRequest.Post("api/post")
            .JsonContent(list)
            .SendAsync(TestHttp)
            .ThrowIfError()
            .IgnoreSyncContext();
        Assert.False(res.HasError);
        var body = res.ResponseString.ToJToken();
        Assert.NotNull(body);
        var actual = body.ToObject<List<int>>();
        Assert.True(list.SequenceEqual(actual!));
    }
}