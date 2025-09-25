namespace FclEx.Http.Core.HttpRequestExtensions;

public class HttpContentTypeTests
{
    [RetryFact]
    public async Task ReadAsStream_Test()
    {
        var response = await HttpRequest.Get("http://baidu.com")
            .ReadAsStream()
            .SendAsync()
            .ThrowIfError();

        Assert.Empty(response.ResponseBytes);
        Assert.Empty(response.ResponseString);
        Assert.IsType<HttpResponseStream>(response.ResponseStream);
#if NET5_0_OR_GREATER
        await
#endif
        using var stream = response.ResponseStream;
        var reader = new StreamReader(stream);
        var text = await reader.ReadToEndAsync();

        Assert.Contains("baidu.com", text);
    }
}