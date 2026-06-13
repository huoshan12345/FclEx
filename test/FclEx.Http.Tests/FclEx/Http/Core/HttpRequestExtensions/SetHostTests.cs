namespace FclEx.Http.Core.HttpRequestExtensions;

public class SetHostTests
{
    public static readonly string[] Hosts =
    [
        "localhost",
        "www.baidu.com",
        "127.0.0.1",
        "220.181.112.244",
    ];

    public static readonly int[] Ports = [80, 8080, 1234];

    public static readonly TheoryData<string, int, string, string, int, string> HostPortsPair = Hosts
        .SelectMany(m => Ports, (i, j) => (h: i, p: j)).SelectMany((i, j) =>
        (
            i.h,
            i.p,
            $"{i.h}:{i.p}",
            j.h,
            j.p,
            $"{j.h}:{j.p}"
        )).ToTheoryData();


    [Theory]
    [MemberData(nameof(HostPortsPair))]
    public void Host_WhenValueContainsPort_SetsHostAndPort(string host, int port, string hp, string newHost, int newPort, string newHp)
    {
        var request = HttpRequest.Get("http://" + host);
        Assert.Equal(host, request.Host);
        Assert.Equal(80, request.Port);

        request.Host(hp);
        Assert.Equal(host, request.Host);
        Assert.Equal(port, request.Port);

        request.Host(newHp);
        Assert.Equal(newHost, request.Host);
        Assert.Equal(newPort, request.Port);
    }

    [Fact]
    public void Host_WhenRequestUriIsRelativeAndSchemeIsSet_SetsHostAndPort()
    {
        var request = HttpRequest.Get("/teacher/app/clean-redis-cache")
            .Scheme("https")
            .Host("betassapinew.knowbox.cn:9002");

        Assert.Equal("https", request.Scheme);
        Assert.Equal("betassapinew.knowbox.cn", request.Host);
        Assert.Equal(9002, request.Port);
    }
}
