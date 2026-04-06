namespace FclEx.Http.Core.HttpRequestExtensions;

public class CtorTests
{
    public static readonly string[] Urls =
    [
        "https://www.cnblogs.com/armfly/p/9378170.html",
        "/parent/change-old-passwd",
    ];

    public static readonly HttpMethod[] Methods =
    [
        HttpMethod.Get,
        HttpMethod.Post,
        HttpMethod.Put,
        HttpMethod.Delete,
        HttpMethod.Head,
        HttpMethod.Options,
    ];

    public static readonly TheoryData<string, HttpMethod> CtorCases = Urls.CrossJoin(Methods).ToTheoryData();

    [Theory]
    [MemberData(nameof(CtorCases))]
    public void TestCtor(string url, HttpMethod method)
    {
        var request = new HttpRequest(new Uri(url, UriKind.RelativeOrAbsolute), method);
        request.Host("localhost");
        var realUrl = request.GetUri();
    }

    [Fact]
    public void Ctor_WithUserInfo()
    {
        var request = new HttpRequest(new Uri("http://tom:tom123@localhost/api/save"), HttpMethod.Get);
        Assert.Equal("tom", request.UserName);
        Assert.Equal("tom123", request.Password);

        request.BasicAuth(request.UserName, request.Password);

        Assert.True(request.Headers.TryGet(HttpHeaderNames.Authorization, out var auth));
        Assert.Equal("Basic dG9tOnRvbTEyMw==", auth);
    }
}