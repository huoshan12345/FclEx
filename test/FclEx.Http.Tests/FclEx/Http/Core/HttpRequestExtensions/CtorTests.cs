using System.Net.Http;

namespace FclEx.Http.Core.HttpRequestExtensions;

public class CtorTests
{
    public static string[] Urls { get; } =
    [
        "https://www.cnblogs.com/armfly/p/9378170.html",
        "/parent/change-old-passwd",
    ];

    public static HttpMethod[] Methods { get; } =
    [
        HttpMethod.Get,
        HttpMethod.Post,
        HttpMethod.Put,
        HttpMethod.Delete,
        HttpMethod.Head,
        HttpMethod.Options,
    ];

    public static IEnumerable<object[]> CtorCases { get; } =
        Urls.SelectMany(m => Methods, (u, m) => new object[] { u, m });

    [Theory]
    [MemberData(nameof(CtorCases))]
    public void TestCtor(string url, HttpMethod method)
    {
        var request = HttpRequest.Create(url, method);
        request.Host("localhost");
        var realUrl = request.GetUri();
    }

    [Fact]
    public void Ctor_WithUserInfo()
    {
        var request = HttpRequest.Get("http://tom:tom123@localhost/api/save");
        Assert.Equal("tom", request.UserName);
        Assert.Equal("tom123", request.Password);

        request.BasicAuth(request.UserName, request.Password);

        Assert.True(request.Headers.TryGet(HttpHeaderNames.Authorization, out var auth));
        Assert.Equal("Basic dG9tOnRvbTEyMw==", auth);
    }
}