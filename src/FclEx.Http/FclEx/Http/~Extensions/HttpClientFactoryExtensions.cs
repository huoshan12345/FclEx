namespace FclEx.Http;

public static class HttpClientFactoryExtensions
{
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
