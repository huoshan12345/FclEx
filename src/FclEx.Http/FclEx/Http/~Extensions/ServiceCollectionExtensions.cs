namespace FclEx.Http;

public static class ServiceCollectionExtensions
{
    public static IHttpClientBuilder AddHttpClientWithPolly(this IServiceCollection services, string name, HttpClientOptions? options = null)
    {
        options ??= new();
        return services.AddHttpClient(name, httpClient => ConfigureHttpClient(httpClient, options))
            .ConfigurePrimaryHttpMessageHandler(() => HttpClientHelper.CreateSocketsHttpHandler(options.HandlerOptions))
            .AddRetryPolicy(options.RetryPolicyOptions);
    }

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