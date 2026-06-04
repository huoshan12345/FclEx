namespace FclEx.Http;

public readonly record struct HttpClientContext(
    HttpClient Client,
    IAsyncPolicy<HttpResponseMessage> Policy,
    bool DisposeHttpClient) : IDisposable
{
    public void Dispose()
    {
        if (DisposeHttpClient)
            Client.Dispose();
    }
}