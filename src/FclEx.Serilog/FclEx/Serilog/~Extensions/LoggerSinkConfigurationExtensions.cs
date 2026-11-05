namespace FclEx.Serilog;

/// <summary>Configures owning sink wrappers and buffered New Relic or Logstash delivery.</summary>
public static class LoggerSinkConfigurationExtensions
{
    /// <summary>Creates one owning sink wrapper around the configured group of sinks.</summary>
    /// <param name="configuration">The parent logger's sink configuration.</param>
    /// <param name="wrapSink">Creates a wrapper that takes ownership of the supplied aggregate sink.</param>
    /// <param name="configureWrappedSink">Registers the sinks owned by that aggregate.</param>
    /// <returns>The parent logger configuration.</returns>
    /// <remarks>
    /// The returned wrapper is registered directly. It must forward disposal to its aggregate sink,
    /// including asynchronous disposal when needed, and forward ISetLoggingFailureListener to support
    /// fallback chains. The aggregate disposes each registered sink once and attempts all cleanups even
    /// if one fails. Construction failures release previously registered sinks before propagating.
    /// No additional layer compensates for a wrapper that does not fulfill this ownership contract.
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
        var capturing = new LoggerConfiguration();
        var sinks = Fields.LoggerConfiguration_Sinks.GetRequiredValue<List<ILogEventSink>>(capturing);
        AggregateSink? aggregate = null;
        try
        {
            configureWrappedSink(capturing.WriteTo);
            aggregate = new AggregateSink(sinks.ToArray());
            var wrapper = wrapSink(aggregate)
                ?? throw new InvalidOperationException("The sink wrapper factory returned null.");
            return configuration.Sink(wrapper);
        }
        catch (Exception configurationError)
        {
            try
            {
                // Failed construction still owns the sinks; cleanup may need to await asynchronous resources.
                var cleanup = aggregate ?? new AggregateSink(sinks.ToArray());
                Task.Run(async () => await cleanup.DisposeAsync().ConfigureAwait(false)).GetAwaiter().GetResult();
            }
            catch (Exception cleanupError)
            {
                throw new AggregateException("Sink configuration and cleanup both failed.", configurationError, cleanupError);
            }
            throw;
        }
    }

    /// <summary>Sends buffered log events to New Relic Logs in gzip-compressed JSON batches.</summary>
    /// <param name="loggerSinkConfiguration">The logger configuration.</param>
    /// <param name="formatter">The formatter used for each event in the JSON batch, or null to use the default JSON formatter.</param>
    /// <param name="endpointUrl">The NewRelic Logs API endpoint URL. Default is set to https://log-api.newrelic.com/log/v1 located in the US.</param>
    /// <param name="licenseKey">The request license key, or null to read NEW_RELIC_LICENSE_KEY when this method is called.</param>
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


    /// <summary>Registers buffered TCP or UDP delivery to Logstash using the supplied endpoint and batching options.</summary>
    /// <param name="config">The parent sink configuration.</param>
    /// <param name="options">Settings captured at registration; the referenced level switch remains live.</param>
    /// <returns>The parent logger configuration.</returns>
    /// <remarks>Batches are not emitted eagerly. Transport and formatting failures propagate to Serilog batching for retry.</remarks>
    /// <exception cref="ArgumentNullException">Either argument is null.</exception>
    /// <exception cref="ArgumentException">The endpoint, timeout, or batching options are invalid.</exception>
    /// <exception cref="UriFormatException">The endpoint is not a valid absolute URI.</exception>
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

    /// <summary>Registers buffered Logstash delivery with default formatter and batching settings.</summary>
    /// <param name="config">The parent sink configuration.</param>
    /// <param name="uri">An absolute tcp:// or udp:// endpoint containing a host and explicit port.</param>
    /// <returns>The parent logger configuration.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="config"/> or <paramref name="uri"/> is null.</exception>
    /// <exception cref="ArgumentException">The endpoint is empty or unsupported.</exception>
    /// <exception cref="UriFormatException">The endpoint is not a valid absolute URI.</exception>
    public static LoggerConfiguration Logstash(this LoggerSinkConfiguration config, string uri)
    {
        var options = new LogstashSinkOptions(uri);
        return config.Logstash(options);
    }
}
