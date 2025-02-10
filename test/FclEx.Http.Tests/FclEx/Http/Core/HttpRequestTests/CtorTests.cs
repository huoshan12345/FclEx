namespace FclEx.Http.Core.HttpRequestTests;

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
        var request = HttpRequest.Get("http://lijing:lijing@captcha.mooncatling.fun/api/captcha/save");
        Assert.True(request.Headers.TryGetValue("Authorization", out var auth));
        Assert.Equal("Basic bGlqaW5nOmxpamluZw==", auth);
    }
}