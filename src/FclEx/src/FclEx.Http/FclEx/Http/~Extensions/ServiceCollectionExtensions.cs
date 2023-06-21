namespace FclEx.Http;

public static class ServiceCollectionExtensions
{
    public static IHttpClientBuilder AddHttpClientWithPolly(this IServiceCollection services, string name, HttpClientOptions? options = null)
    {
        options ??= HttpClientOptions.Default;
        return services.AddHttpClient(name, httpClient =>
            {
                httpClient.Timeout = options.TotalTimeout;
                httpClient.BaseAddress = options.BaseAddress;
                httpClient.DefaultRequestVersion = options.HttpVersion;
                httpClient.DefaultVersionPolicy = options.HttpVersionPolicy;
            })
            .ConfigurePrimaryHttpMessageHandler(() => HttpClientHelper.CreateSocketsHttpHandler(options))
            .AddRetryPolicy(options.ExecutionTimeout, options.RetryCount, options.AutoUpdateTotalTimeout, options.SleepDurationProvider);
    }
}