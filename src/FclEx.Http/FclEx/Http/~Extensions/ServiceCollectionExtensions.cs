namespace FclEx.Http;

/// <summary>
/// Service registration helpers for named HTTP clients configured with FclEx handler and Polly options.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers a named <see cref="HttpClient"/> using fixed FclEx client, handler, and retry policy options.
    /// </summary>
    public static IHttpClientBuilder AddHttpClientWithPolly(this IServiceCollection services, string name, HttpClientOptions? options = null)
    {
        options ??= new();
        return services.AddHttpClient(name, httpClient => ConfigureHttpClient(httpClient, options))
            .ConfigurePrimaryHttpMessageHandler(() => HttpClientHelper.CreateSocketsHttpHandler(options.HandlerOptions))
            .AddRetryPolicy(options.RetryPolicyOptions);
    }

    /// <summary>
    /// Registers a named <see cref="HttpClient"/> whose FclEx client, handler, and retry policy options are resolved from the service provider.
    /// </summary>
    public static IHttpClientBuilder AddHttpClientWithPolly(this IServiceCollection services, string name, Func<IServiceProvider, HttpClientOptions> optionsFactory)
    {
        return services.AddHttpClient(name, (serviceProvider, httpClient) =>
        {
            var options = optionsFactory(serviceProvider);
            ConfigureHttpClient(httpClient, options);
        }).ConfigurePrimaryHttpMessageHandler(serviceProvider =>
        {
            var options = optionsFactory(serviceProvider);
            return HttpClientHelper.CreateSocketsHttpHandler(options.HandlerOptions);
        }).AddRetryPolicy(m => optionsFactory(m).RetryPolicyOptions);
    }

    private static void ConfigureHttpClient(HttpClient httpClient, HttpClientOptions options)
    {
        httpClient.Timeout = options.TotalTimeout;
        httpClient.BaseAddress = options.BaseAddress;
#if NET6_0_OR_GREATER
        httpClient.DefaultRequestVersion = options.HttpVersion;
        httpClient.DefaultVersionPolicy = options.HttpVersionPolicy;
#endif
    }
}
