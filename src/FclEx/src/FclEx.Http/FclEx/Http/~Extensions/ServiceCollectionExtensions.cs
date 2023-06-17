namespace FclEx.Http;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSingletonBy<T, TDependency>(this IServiceCollection services, Func<TDependency, T> func)
        where TDependency : class
        where T : class
    {
        return services.AddSingleton(s => func(s.GetRequiredService<TDependency>()));
    }

    public static IServiceCollection AddSingletonBy<T, TDependency1, TDependency2>(this IServiceCollection services, Func<TDependency1, TDependency2, T> func)
        where TDependency1 : class
        where TDependency2 : class
        where T : class
    {
        return services.AddSingleton(s => func(s.GetRequiredService<TDependency1>(), s.GetRequiredService<TDependency2>()));
    }

    public static IHttpClientBuilder AddHttpClientWithPolly(this IServiceCollection services, string name, HttpClientOptions? options = null)
    {
        options ??= HttpClientOptions.Default;
        return services.AddHttpClient(name, httpClient => httpClient.Timeout = options.TotalTimeout)
            .ConfigurePrimaryHttpMessageHandler(() => HttpClientHelper.CreateSocketsHttpHandler(options.ConnectTimeout))
            .AddRetryPolicy(options.ExecutionTimeout, options.RetryCount, options.AutoUpdateTotalTimeout, options.SleepDurationProvider);
    }
}