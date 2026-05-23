namespace FclEx.Http;

public static class ServiceCollectionExtensions
{
    public static IHttpClientBuilder AddHttpClientWithPolly(this IServiceCollection services, string name, HttpClientOptions? options = null)
    {
        options ??= new();
        return services.AddHttpClient(name, httpClient =>
            {
                httpClient.Timeout = options.TotalTimeout;
                httpClient.BaseAddress = options.BaseAddress;
#if NET6_0_OR_GREATER
                httpClient.DefaultRequestVersion = options.HttpVersion;
                httpClient.DefaultVersionPolicy = options.HttpVersionPolicy;                
#endif
            })
            .ConfigurePrimaryHttpMessageHandler(() => HttpClientHelper.CreateSocketsHttpHandler(options))
            .AddRetryPolicy(options.ExecutionTimeout, options.RetryCount, options.AutoUpdateTotalTimeout, options.SleepDurationProvider);
    }
}