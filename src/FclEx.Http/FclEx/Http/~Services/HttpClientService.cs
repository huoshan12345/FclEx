namespace FclEx.Http;

public class HttpClientService : AbstractHttpClientService
{
    private static readonly Lazy<HttpClientService> _default = new(() => new(new HttpClientOptions()) { UseCookie = false });
    public static HttpClientService Default => _default.Value;

    protected HttpClientOptions _options;
    protected readonly Func<HttpClient>? _httpClientProvider;
    protected readonly bool _disposeHttpClient;

    protected internal override HttpClientContext CreateHttpClientContext()
    {
        var dispose = false;
        var provider = GetProvider(_options);
        var policy = provider.GetRequiredService<IAsyncPolicy<HttpResponseMessage>>();
        HttpClient client;
        if (_httpClientProvider is null)
        {
            client = provider.GetRequiredService<IHttpClientFactory>().CreateClient();
        }
        else
        {
            client = _httpClientProvider();
            dispose = _disposeHttpClient;
        }
        return new(client, policy, dispose);
    }

    public HttpClientService(
        HttpClientOptions? options = null,
        Func<HttpClient>? httpClientProvider = null,
        bool disposeHttpClient = true)
    {
        _httpClientProvider = httpClientProvider;
        _disposeHttpClient = disposeHttpClient;
        options ??= new();

        // NOTE: always use with keyword to create new instance cause it used as key in cache.
        // do not try to change property directly or reuse options.
        _options = options with { AllowAutoRedirect = false };
    }

    public override IWebProxy? Proxy
    {
        get => _options.Proxy;
        set
        {
            if (IWebProxyEqualityComparer.Instance.Equals(_options.Proxy, value))
                return;

            // NOTE: use with keyword to create new instance instead of changing property directly cause it used as key in cache.
            _options = _options with { Proxy = value };
        }
    }

    public override void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public static int MaxCacheCount { get; } = ushort.MaxValue;

    protected static readonly Lazy<LfuCache<HttpClientOptions, IServiceProvider>> Providers = new(() => new(Math.Max(1, MaxCacheCount), HttpClientOptionsEqualityComparer.Instance));

    protected static readonly string[] CanceledErrors =
    [
        new TaskCanceledException(Task.CompletedTask).Message,
        new OperationCanceledException(CancellationToken.None).Message,
    ];

    protected internal static IServiceProvider GetProvider(HttpClientOptions options)
    {
        return Providers.Value.GetOrAdd(options, m =>
        {
            // this policy is created to retry Task.WithTimeout()
            var policy = Policy<HttpResponseMessage>
                .Handle<OperationCanceledException>(IsPureCanceledException)
                .WaitAndRetryAsync(options.RetryCount, options.SleepDurationProvider);

            return new ServiceCollection()
                .AddSingleton<IAsyncPolicy<HttpResponseMessage>>(policy)
                .AddHttpClientWithPolly(string.Empty, options)
                .Services
                .Remove(x => x.ServiceType == typeof(IHttpMessageHandlerBuilderFilter)
                             && x.ImplementationType?.FullName == "Microsoft.Extensions.Http.LoggingHttpMessageHandlerBuilderFilter")
                .BuildServiceProvider();
        });

        static bool IsPureCanceledException(Exception? ex)
        {
            var p = ex;
            while (p is OperationCanceledException)
            {
                if (CanceledErrors.Contains(p.Message) == false)
                    return false;

                if (p.InnerException is null)
                    return true;

                p = p.InnerException;
            }
            return false;
        }
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
        return Create(new HttpClientOptions(), useCookie, loggerFactory);
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

    public static HttpClientService Create(
        Func<HttpClient> httpClientProvider,
        bool disposeHttpClient = true,
        HttpClientOptions? options = null,
        bool useCookie = true,
        ILoggerFactory? loggerFactory = null)
    {
        return new HttpClientService(options, httpClientProvider, disposeHttpClient)
        {
            UseCookie = useCookie,
            Logger = loggerFactory?.CreateLogger<HttpClientService>()
        };
    }
}