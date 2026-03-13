namespace FclEx.Http.Core.HttpRequestExtensions;

public class HttpContentTypeTests
{
    [RetryFact]
    public async Task ReadAsStream_Test()
    {
        var response = await HttpRequest.Get("https://google.com")
            .ReadAsStream()
            .SendAsync()
            .ThrowIfError();

        Assert.Empty(response.ResponseBytes);
        Assert.Empty(response.ResponseString);
        Assert.IsType<HttpResponseStream>(response.ResponseStream);

        await using var stream = response.ResponseStream;
        var reader = new StreamReader(stream);
        var text = await reader.ReadToEndAsync();

        Assert.Contains("google.com", text);
    }
}