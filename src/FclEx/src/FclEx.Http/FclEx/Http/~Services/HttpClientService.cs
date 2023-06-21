namespace FclEx.Http;

public class HttpClientService : AbstractHttpClientService
{
    protected HttpClientOptions _options;

    public static HttpClientService Default { get; } = new() { UseCookie = false };

    protected override Task ExecuteAsyncInternal(HttpRequest request, HttpResponse response, CancellationToken token)
    {
        var httpClient = CreateClient();
        return ExecuteAsyncInternal(httpClient, request, response, token);
    }

    public HttpClientService(HttpClientOptions? options = null)
    {
        _options = options ?? HttpClientOptions.Default;
    }

    public override IWebProxy? Proxy
    {
        get => _options.Proxy;
        set
        {
            if (IWebProxyEqualityComparer.Instance.Equals(_options.Proxy, value))
                return;

            _options = _options with { Proxy = value };
        }
    }

    public override void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    protected static readonly ConcurrentDictionary<HttpClientOptions, IHttpClientFactory> Factories = new(HttpClientOptionsEqualityComparer.Instance);

    protected internal static IHttpClientFactory GetFactory(HttpClientOptions options)
    {
        return Factories.GetOrAdd(options, m => new ServiceCollection()
            .AddHttpClientWithPolly(string.Empty, options)
            .Services
            .BuildServiceProvider()
            .GetRequiredService<IHttpClientFactory>());
    }

    protected internal virtual HttpClient CreateClient()
    {
        return GetFactory(_options).CreateClient();
    }

    public static HttpClientService Create(HttpClientOptions? options = null, bool useCookie = true, ILoggerFactory? loggerFactory = null)
    {
        return new HttpClientService(options)
        {
            UseCookie = useCookie,
            Logger = loggerFactory?.CreateLogger<HttpClientService>()
        };
    }

    public static HttpClientService Create(bool useCookie, ILoggerFactory? loggerFactory = null)
    {
        return Create(HttpClientOptions.Default, useCookie, loggerFactory);
    }

    public static HttpClientService Create(Action<HttpClientOptions> configureOptions, bool useCookie = true, ILoggerFactory? loggerFactory = null)
    {
        var options = new HttpClientOptions();
        configureOptions(options);
        return Create(options, useCookie, loggerFactory);
    }

    public static HttpClientService Create(IWebProxy? proxy, bool useCookie = true, ILoggerFactory? loggerFactory = null)
    {
        return Create(m => m.Proxy = proxy, useCookie, loggerFactory);
    }

    public static HttpClientService Create(Uri? proxy, bool useCookie = true, ILoggerFactory? loggerFactory = null)
    {
        return Create(m => m.Proxy = WebProxyHelper.Create(proxy), useCookie, loggerFactory);
    }

    public static HttpClientService Create(string? proxy, bool useCookie = true, ILoggerFactory? loggerFactory = null)
    {
        return Create(m => m.Proxy = WebProxyHelper.Create(proxy), useCookie, loggerFactory);
    }
}