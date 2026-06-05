namespace FclEx.Http;

public static partial class HttpClientBuilderExtensions
{
    public static IHttpClientBuilder AddRetryPolicy(this IHttpClientBuilder builder, HttpClientRetryPolicyOptions options)
    {
        if (options.AutoUpdateTotalTimeout)
        {
            builder.ConfigureHttpClient(httpClient =>
            {
                var totalTimeout = ComputeMaxTimeout(options);
                if (httpClient.Timeout < totalTimeout)
                    httpClient.Timeout = totalTimeout;
            });
        }

        builder.AddPolicyHandler(PollyHelper.GetHttpRetryPolicy(options.RetryCount, options.SleepDurationProvider));
        builder.AddPolicyHandler(PollyHelper.GetTimeoutPolicy(options.ExecutionTimeout));
        builder.AddPolicyHandler(PollyHelper.GetConnectTimeoutPolicy(options.RetryCount));
        builder.AddPolicyHandler(PollyHelper.GetIORetryPolicy(options.RetryCount));
        return builder;
    }

    public static IHttpClientBuilder AddHttpMessageHandlerBy<TDependency>(this IHttpClientBuilder builder, Func<TDependency, DelegatingHandler> configureHandler)
        where TDependency : class
    {
        return builder.AddHttpMessageHandler(m => configureHandler(m.GetRequiredService<TDependency>()));
    }

    public static IHttpClientBuilder AddPolicyHandlerBy<TDependency>(
        this IHttpClientBuilder builder,
        Func<IServiceProvider, TDependency> dependencyFactory,
        Func<TDependency, HttpRequestMessage, IAsyncPolicy<HttpResponseMessage>> policyFactory)
        where TDependency : class
    {
        return builder.AddPolicyHandler((serviceProvider, request) =>
        {
            var dependency = dependencyFactory(serviceProvider);
            return policyFactory(dependency, request);
        });
    }

    public static IHttpClientBuilder AddPolicyHandlerBy<TDependency>(
        this IHttpClientBuilder builder,
        Func<TDependency, HttpRequestMessage, IAsyncPolicy<HttpResponseMessage>> policyFactory)
        where TDependency : class
    {
        return builder.AddPolicyHandlerBy(s => s.GetRequiredService<TDependency>(), policyFactory);
    }

    public static IHttpClientBuilder AddRetryPolicy(this IHttpClientBuilder builder, Func<IServiceProvider, HttpClientRetryPolicyOptions> optionsFactory)
    {
        builder.ConfigureHttpClient((serviceProvider, httpClient) =>
        {
            var options = optionsFactory(serviceProvider);
            if (options.AutoUpdateTotalTimeout == false)
                return;

            var totalTimeout = ComputeMaxTimeout(options);
            if (httpClient.Timeout < totalTimeout)
                httpClient.Timeout = totalTimeout;
        });

        builder.AddPolicyHandlerBy(optionsFactory, (options, _) => PollyHelper.GetHttpRetryPolicy(options.RetryCount, options.SleepDurationProvider));
        builder.AddPolicyHandlerBy(optionsFactory, (options, _) => PollyHelper.GetTimeoutPolicy(options.ExecutionTimeout));
        builder.AddPolicyHandlerBy(optionsFactory, (options, _) => PollyHelper.GetConnectTimeoutPolicy(options.RetryCount));
        builder.AddPolicyHandlerBy(optionsFactory, (options, _) => PollyHelper.GetIORetryPolicy(options.RetryCount));
        return builder;
    }

    private static TimeSpan ComputeMaxTimeout(HttpClientRetryPolicyOptions options)
    {
        var timeout = options.ExecutionTimeout;

        var totalTimeout = timeout + TimeSpan.FromSeconds(1);
        for (var i = 0; i < options.RetryCount; i++)
        {
            totalTimeout += timeout;
            totalTimeout += options.SleepDurationProvider(i + 1);
        }

        return totalTimeout;
    }
}