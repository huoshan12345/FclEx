namespace FclEx.Http;

public class HttpClientService : AbstractHttpClientService
{
    protected HttpClientOptions _options;

    public static HttpClientService Default { get; } = new() { UseCookie = false };

    protected internal override HttpClientContext CreateHttpClientContext()
    {
        var provider = GetProvider(_options);
        var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient();
        var policy = provider.GetRequiredService<IAsyncPolicy<HttpResponseMessage>>();
        return new(client, policy);
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

    protected static readonly ConcurrentDictionary<HttpClientOptions, IServiceProvider> Providers = new(HttpClientOptionsEqualityComparer.Instance);

    protected static readonly string[] CanceledErrors =
    [
        new TaskCanceledException(Task.CompletedTask).Message,
        new OperationCanceledException(CancellationToken.None).Message
    ];

    protected internal static IServiceProvider GetProvider(HttpClientOptions options)
    {
        return Providers.GetOrAdd(options, m =>
        {
            var policy = Policy<HttpResponseMessage>
                .Handle<OperationCanceledException>(m => m.InnerException is null && CanceledErrors.Contains(m.Message))
                .WaitAndRetryAsync(options.RetryCount, options.SleepDurationProvider);

            return new ServiceCollection()
                .AddSingleton<IAsyncPolicy<HttpResponseMessage>>(policy)
                .AddHttpClientWithPolly(string.Empty, options)
                .Services
                .Remove(m => m.ServiceType == typeof(IHttpMessageHandlerBuilderFilter)
                             && m.ImplementationType?.FullName == "Microsoft.Extensions.Http.LoggingHttpMessageHandlerBuilderFilter")
                .BuildServiceProvider();
        });
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