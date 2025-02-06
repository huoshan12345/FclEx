namespace FclEx.Http.Core.HttpRequestExtensions;

public class SetHostTests
{
    public static string[] Hosts { get; } =
    [
        "localhost",
        "www.baidu.com",
        "127.0.0.1",
        "220.181.112.244",
    ];

    public static int[] Ports { get; } = [80, 8080, 1234];

    public static IEnumerable<object[]> HostPortsPair { get; } = Hosts
        .SelectMany(m => Ports, (i, j) => (h: i, p: j)).SelectMany((i, j) => new object[]
        {
            i.h,
            i.p,
            $"{i.h}:{i.p}",
            j.h,
            j.p,
            $"{j.h}:{j.p}",
        })
        .ToArray();


    [Theory]
    [MemberData(nameof(HostPortsPair))]
    public void TestSetHost(string host, int port, string hp, string newHost, int newPort, string newHp)
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
    public void SetHostWithSchemeTest()
    {
        var request = HttpRequest.Get("/teacher/app/clean-redis-cache")
            .Scheme("https")
            .Host("betassapinew.knowbox.cn:9002");

        Assert.Equal("https", request.Scheme);
        Assert.Equal("betassapinew.knowbox.cn", request.Host);
        Assert.Equal(9002, request.Port);
    }
}