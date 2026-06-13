namespace FclEx.Http;

/// <summary>
/// Holds an <see cref="HttpClient"/> and its retry policy for one send operation.
/// </summary>
/// <param name="Client">The client used to send request messages.</param>
/// <param name="Policy">The policy used around each send.</param>
/// <param name="DisposeHttpClient">Whether disposing the context should dispose <paramref name="Client"/>.</param>
public readonly record struct HttpClientContext(
    HttpClient Client,
    IAsyncPolicy<HttpResponseMessage> Policy,
    bool DisposeHttpClient) : IDisposable
{
    /// <summary>
    /// Disposes the client only when <see cref="DisposeHttpClient"/> is true.
    /// </summary>
    public void Dispose()
    {
        if (DisposeHttpClient)
            Client.Dispose();
    }
}
