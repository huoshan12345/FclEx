namespace FclEx.Serilog;

public static class LoggerSinkConfigurationExtensions
{
    /// <summary>Wraps a group of sinks while retaining their disposal and failure-notification support.</summary>
    /// <param name="configuration">The parent logger's sink configuration.</param>
    /// <param name="wrapSink">Creates a wrapper around a borrowed sink entry point that exposes emission and failure notifications, but no disposal interfaces.</param>
    /// <param name="configureWrappedSink">Registers the inner sinks owned by the resulting pipeline.</param>
    /// <returns>The parent logger configuration.</returns>
    /// <remarks>
    /// The pipeline disposes the wrapper's own resources, then the inner sinks, each once. Async disposal
    /// prefers IAsyncDisposable and falls back to IDisposable. Failure listeners go through the wrapper
    /// when it supports them, or directly to the inner sinks otherwise. Construction failures release
    /// previously registered sinks. Wrappers must not require ownership or a concrete type for the borrowed sink.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Any argument is null.</exception>
    /// <exception cref="InvalidOperationException">The wrapper factory returns null.</exception>
    /// <exception cref="AggregateException">Construction and cleanup both fail.</exception>
    public static LoggerConfiguration Wrap(
        this LoggerSinkConfiguration configuration,
        Func<ILogEventSink, ILogEventSink> wrapSink,
        Action<LoggerSinkConfiguration> configureWrappedSink)
    {
        Check.NotNull(configuration);
        Check.NotNull(wrapSink);
        Check.NotNull(configureWrappedSink);
        var logEventSink = SinkPipeline.Wrap(wrapSink, configureWrappedSink);
        return configuration.Sink(logEventSink);
    }

    /// <summary>Formats event exceptions once before dispatching to the specified sinks.</summary>
    /// <param name="configuration">The parent logger's sink configuration.</param>
    /// <param name="configureWrappedSink">Configures sinks owned by the formatting wrapper.</param>
    /// <returns>The parent logger configuration for further customization.</returns>
    /// <remarks>Logger disposal releases the inner sinks. Failure notifications are forwarded to support fallback chains. Use asynchronous disposal on supported targets for asynchronous sinks.</remarks>
    /// <exception cref="ArgumentNullException">Either argument is null.</exception>
    public static LoggerConfiguration FormatException(this LoggerSinkConfiguration configuration, Action<LoggerSinkConfiguration> configureWrappedSink)
    {
        return configuration.Wrap(inner => new FormatExceptionSink(inner), configureWrappedSink);
    }

    /// <summary>Sends buffered log events to New Relic Logs in gzip-compressed JSON batches.</summary>
    /// <param name="loggerSinkConfiguration">The logger configuration.</param>
    /// <param name="formatter">The formatter used for each event in the JSON batch, or null to use the default JSON formatter.</param>
    /// <param name="endpointUrl">The NewRelic Logs API endpoint URL. Default is set to https://log-api.newrelic.com/log/v1 located in the US.</param>
    /// <param name="licenseKey">New Relic APM License key. </param>
    /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink.</param>
    /// <param name="batchSizeLimit">The maximum number of events to include in a single batch. Default is 100 entries.</param>
    /// <param name="period">The time to wait between checking for event batches. TimeSpan with a default value of 2 seconds.</param>
    /// <returns>The logger configuration for further customization.</returns>
    /// <remarks>
    /// Serialization failures, request failures, and responses other than HTTP 202 Accepted are handled
    /// by Serilog's batching diagnostics and retry policy. Logger disposal releases the sink's owned HTTP client.
    /// </remarks>
    public static LoggerConfiguration NewRelic(
        this LoggerSinkConfiguration loggerSinkConfiguration,
        string? licenseKey = null,
        string? endpointUrl = NewRelicSink.DefaultEndpoint,
        ITextFormatter? formatter = null,
        LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
        int batchSizeLimit = NewRelicSink.DefaultBatchSizeLimit,
        TimeSpan? period = null)
    {
        Check.NotNull(loggerSinkConfiguration);

        licenseKey ??= Environment.GetEnvironmentVariable("NEW_RELIC_LICENSE_KEY");
        period ??= NewRelicSink.DefaultPeriod;

        if (endpointUrl.IsNullOrEmpty())
            throw new ArgumentException("NewRelic Logs API endpoint URL must be supplied", nameof(endpointUrl));

        if (licenseKey.IsNullOrEmpty())
            throw new ArgumentException("LicenseKey must be supplied", nameof(licenseKey));

        var sink = new NewRelicSink(licenseKey, endpointUrl, formatter);
        var batchingOptions = new BatchingOptions
        {
            BatchSizeLimit = batchSizeLimit,
            BufferingTimeLimit = period.Value,
            EagerlyEmitFirstEvent = true,
            QueueLimit = 10000,
        };
        try
        {
            return loggerSinkConfiguration.Sink(sink, batchingOptions, restrictedToMinimumLevel);
        }
        catch
        {
            sink.Dispose();
            throw;
        }
    }


    public static LoggerConfiguration Logstash(this LoggerSinkConfiguration config, LogstashSinkOptions options)
    {
        Check.NotNull(config);
        Check.NotNull(options);

        var op = new BatchingOptions
        {
            EagerlyEmitFirstEvent = false,
            BatchSizeLimit = options.BatchSizeLimit,
            BufferingTimeLimit = options.Period,
            QueueLimit = options.QueueLimit,
        };
        return config.Sink(new LogstashSink(options), op,
            restrictedToMinimumLevel: options.MinimumLogEventLevel ?? LevelAlias.Minimum,
            levelSwitch: options.LevelSwitch
        );
    }

    public static LoggerConfiguration Logstash(this LoggerSinkConfiguration config, string uri)
    {
        var options = new LogstashSinkOptions(uri);
        return config.Logstash(options);
    }
}
