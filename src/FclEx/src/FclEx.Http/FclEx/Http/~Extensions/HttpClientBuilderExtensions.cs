namespace FclEx.Http;

public static class HttpClientBuilderExtensions
{
    public static IHttpClientBuilder AddRetryPolicy(this IHttpClientBuilder builder, TimeSpan? timeout = null, int retryCount = 2, bool autoUpdateTotalTimeout = true, SleepDurationProvider? sleepDurationProvider = null)
    {
        var t = timeout ?? TimeSpan.FromMinutes(1);
        var sleepProvider = sleepDurationProvider ?? (retryAttempt => TimeSpan.FromSeconds(1 + retryAttempt));
        if (autoUpdateTotalTimeout)
        {
            var totalTimeout = t + TimeSpan.FromSeconds(1);
            for (var i = 0; i < retryCount; i++)
            {
                totalTimeout += t;
                totalTimeout += sleepProvider(i + 1);
            }

            builder.ConfigureHttpClient(m =>
            {
                if (m.Timeout < totalTimeout)
                    m.Timeout = totalTimeout;
            });
        }
        builder.AddPolicyHandler(PollyHelper.GetHttpRetryPolicy(retryCount, sleepDurationProvider));
        builder.AddPolicyHandler(PollyHelper.GetTimeoutPolicy(t));
        builder.AddPolicyHandler(PollyHelper.GetConnectTimeoutPolicy(retryCount, sleepDurationProvider));
        builder.AddPolicyHandler(PollyHelper.GetCancelPolicy(retryCount, sleepDurationProvider));
        return builder;
    }
}