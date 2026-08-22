namespace FclEx.Http;

/// <summary>
/// Sends <see cref="HttpRequest"/> instances through <see cref="HttpClient"/>.
/// </summary>
/// <remarks>
/// The service builds an <see cref="HttpRequestMessage"/> for each retry attempt, reads headers before content,
/// handles redirects itself, stores response cookies when enabled, and disposes request content after sending.
/// Handler/provider instances are cached by <see cref="HttpClientOptions"/> value.
/// </remarks>
public class HttpClientService : HttpClientServiceBase
{
    private static readonly Lazy<HttpClientService> _default = new(() => new(new HttpClientOptions()) { UseCookie = false });

    /// <summary>
    /// A shared service with cookies disabled and default options.
    /// </summary>
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

    /// <summary>
    /// Initializes a service with the supplied options and optional client provider.
    /// </summary>
    /// <param name="options">HTTP client and retry options. The service copies the options and disables handler auto-redirect.</param>
    /// <param name="httpClientProvider">Optional provider for externally created clients.</param>
    /// <param name="disposeHttpClient">Whether clients returned by <paramref name="httpClientProvider"/> are disposed after each send.</param>
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
        _options = options with { HandlerOptions = options.HandlerOptions with { AllowAutoRedirect = false } };
    }

    /// <inheritdoc />
    public override IWebProxy? Proxy
    {
        get => _options.HandlerOptions.Proxy;
        set
        {
            var options = _options.HandlerOptions;
            if (WebProxyInterfaceEqualityComparer.Instance.Equals(options.Proxy, value))
                return;

            // NOTE: use with keyword to create new instance instead of changing property directly cause it used as key in cache.
            _options = _options with { HandlerOptions = options with { Proxy = value } };
        }
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Maximum number of cached service providers used for distinct <see cref="HttpClientOptions"/> values.
    /// </summary>
    /// <remarks>This value must be changed before the provider cache is first created.</remarks>
    public static int MaxCacheCount
    {
        get;
        set
        {
            Check.Positive(value);

            if (Providers.IsValueCreated)
                throw new InvalidOperationException("Cannot change MaxCacheCount after cache is created.");

            field = value;
        }
    } = ushort.MaxValue;

    protected static readonly Lazy<LfuCache<HttpClientOptions, IServiceProvider>> Providers = new(CreateCache);

    protected static readonly string[] CanceledErrors =
    [
        new TaskCanceledException(Task.CompletedTask).Message,
        new OperationCanceledException(CancellationToken.None).Message,
    ];

    protected static LfuCache<HttpClientOptions, IServiceProvider> CreateCache()
    {
        var cache = new LfuCache<HttpClientOptions, IServiceProvider>(MaxCacheCount, HttpClientOptionsEqualityComparer.Instance);
        cache.EntryRemoved += (_, args) =>
        {
            if (args.Value is IDisposable disposable)
                disposable.Dispose();
        };
        return cache;
    }

    protected internal static IServiceProvider GetProvider(HttpClientOptions options)
    {
        return Providers.Value.GetOrAdd(options, m =>
        {
            var retryOptions = m.RetryPolicyOptions;
            // this policy is created to retry Task.WithTimeout()
            var policy = Policy<HttpResponseMessage>
                .Handle<OperationCanceledException>(IsPureCanceledException)
                .WaitAndRetryAsync(retryOptions.RetryCount, retryOptions.SleepDurationProvider);

            return new ServiceCollection()
                .AddSingleton<IAsyncPolicy<HttpResponseMessage>>(policy)
                .AddHttpClientWithPolly(string.Empty, m)
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
                if (CanceledErrors.Contains(p.Message, StringComparer.Ordinal) == false)
                    return false;

                if (p.InnerException is null)
                    return true;

                p = p.InnerException;
            }
            return false;
        }
    }

    /// <summary>
    /// Creates a service from options.
    /// </summary>
    /// <param name="options">HTTP client and retry options.</param>
    /// <param name="useCookie">Whether the service should store and send cookies.</param>
    /// <param name="loggerFactory">Optional logger factory for the service logger.</param>
    /// <returns>A configured service instance.</returns>
    public static HttpClientService Create(HttpClientOptions? options = null, bool useCookie = true, ILoggerFactory? loggerFactory = null)
    {
        return new HttpClientService(options)
        {
            UseCookie = useCookie,
            Logger = loggerFactory?.CreateLogger<HttpClientService>()
        };
    }

    /// <summary>
    /// Creates a service with default options and explicit cookie behavior.
    /// </summary>
    public static HttpClientService Create(bool useCookie, ILoggerFactory? loggerFactory = null)
    {
        return Create(new HttpClientOptions(), useCookie, loggerFactory);
    }

    /// <summary>
    /// Creates a service after mutating a new <see cref="HttpClientOptions"/> instance.
    /// </summary>
    public static HttpClientService Create(Action<HttpClientOptions> configureOptions, bool useCookie = true, ILoggerFactory? loggerFactory = null)
    {
        var options = new HttpClientOptions();
        configureOptions(options);
        return Create(options, useCookie, loggerFactory);
    }

    /// <summary>
    /// Creates a service that uses the supplied proxy.
    /// </summary>
    public static HttpClientService Create(IWebProxy? proxy, bool useCookie = true, ILoggerFactory? loggerFactory = null)
    {
        return Create(m => m.HandlerOptions.Proxy = proxy, useCookie, loggerFactory);
    }

    /// <summary>
    /// Creates a service that uses a proxy created from the supplied URI.
    /// </summary>
    public static HttpClientService Create(Uri? proxy, bool useCookie = true, ILoggerFactory? loggerFactory = null)
    {
        return Create(WebProxy.Create(proxy), useCookie, loggerFactory);
    }

    /// <summary>
    /// Creates a service that uses a proxy created from the supplied URI string.
    /// </summary>
    public static HttpClientService Create(string? proxy, bool useCookie = true, ILoggerFactory? loggerFactory = null)
    {
        return Create(WebProxy.Create(proxy), useCookie, loggerFactory);
    }

    /// <summary>
    /// Creates a service backed by caller-provided <see cref="HttpClient"/> instances.
    /// </summary>
    /// <param name="httpClientProvider">Provides the client used for each send operation.</param>
    /// <param name="disposeHttpClient">Whether clients returned by <paramref name="httpClientProvider"/> should be disposed after sending.</param>
    /// <param name="options">Retry and request handling options used around the provided client.</param>
    /// <param name="useCookie">Whether the service should store and send cookies.</param>
    /// <param name="loggerFactory">Optional logger factory for the service logger.</param>
    /// <returns>A configured service instance.</returns>
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

    /// <summary>
    /// Disposes all cached providers and clears the option-to-provider cache.
    /// </summary>
    public static void ClearCache()
    {
        if (Providers.IsValueCreated == false)
            return;

        Providers.Value.Clear();
    }
}
