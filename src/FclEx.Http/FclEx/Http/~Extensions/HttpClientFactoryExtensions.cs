namespace FclEx.Http;

/// <summary>
/// Extensions for creating FclEx HTTP services from <see cref="IHttpClientFactory"/>.
/// </summary>
public static class HttpClientFactoryExtensions
{
    /// <summary>
    /// Creates an <see cref="IHttpService"/> that obtains named clients from an <see cref="IHttpClientFactory"/>.
    /// The service does not dispose clients returned by the factory.
    /// </summary>
    public static IHttpService CreateHttpService(
        this IHttpClientFactory httpClientFactory,
        string? name = null,
        ILoggerFactory? loggerFactory = null,
        HttpClientOptions? options = null,
        bool useCookie = true)
    {
        name ??= nameof(HttpClientService);
        return HttpClientService.Create(
            httpClientProvider: () => httpClientFactory.CreateClient(name),
            disposeHttpClient: false,
            options: options,
            useCookie: useCookie,
            loggerFactory: loggerFactory);
    }

}
