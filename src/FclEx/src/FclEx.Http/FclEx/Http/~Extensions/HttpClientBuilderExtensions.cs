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
        return builder;
    }
}

public class HttpClientOptions
{
    public static readonly SleepDurationProvider DefaultSleepDurationProvider = retryAttempt => TimeSpan.FromSeconds(1 + retryAttempt);

    public TimeSpan ExecutionTimeout { get; set; } = TimeSpan.FromMinutes(1);
    public int RetryCount { get; set; } = 2;

    /// <summary>
    /// Indicates whether or not update <see cref="TotalTimeout"/> when it is less than a total timeout that is computed with <see cref="ExecutionTimeout"/> and <see cref="RetryCount"/>
    /// </summary>
    public bool AutoUpdateTotalTimeout { get; set; } = true;
    public SleepDurationProvider SleepDurationProvider { get; set; } = DefaultSleepDurationProvider;

#if NETSTANDARD2_0
    /// <summary>
    /// Will be used as <see cref="StandardSocketsHttpHandler.ConnectTimeout"/>
    /// </summary>
#else
    /// <summary>
    /// Will be used as <see cref="SocketsHttpHandler.ConnectTimeout"/>
    /// </summary>
#endif
    public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Will be used as <see cref="HttpClient.Timeout"/>
    /// </summary>
    public TimeSpan TotalTimeout { get; set; } = TimeSpan.FromMinutes(2);

    public IpVersionPreference IpVersionPreference { get; set; } = IpVersionPreference.PreferIpV4;

    public static readonly HttpClientOptions Default = new();
}