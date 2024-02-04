using FclEx.Serilog.Sinks;
using Serilog.Configuration;

namespace FclEx.Serilog;

public static class LoggerConfigurationExtensions
{
    public static LoggerConfiguration Wrap(this LoggerSinkConfiguration configuration,
        Func<ILogEventSink, ILogEventSink> wrapSink, Action<LoggerSinkConfiguration> configureWrappedSink)
    {
        return LoggerSinkConfiguration.Wrap(configuration, wrapSink, configureWrappedSink, LevelAlias.Minimum, null);
    }

    public static LoggerConfiguration FormatException(this LoggerSinkConfiguration configuration, Action<LoggerSinkConfiguration> configureWrappedSink)
    {
        return configuration.Wrap(m => new FormatExceptionSink(m), configureWrappedSink);
    }

    /// <summary>
    /// </summary>
    /// <param name="loggerSinkConfiguration">The logger configuration.</param>
    /// <param name="formatter">Supplies culture-specific formatting information.</param>
    /// <param name="endpointUrl">The NewRelic Logs API endpoint URL. Default is set to https://log-api.newrelic.com/log/v1 located in the US.</param>
    /// <param name="licenseKey">New Relic APM License key. </param>
    /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink.</param>
    /// <param name="batchSizeLimit">The maximum number of events to include in a single batch. Default is 1000 entries.</param>
    /// <param name="period">The time to wait between checking for event batches. TimeSpan with a default value of 2 seconds.</param>
    /// <returns></returns>
    public static LoggerConfiguration NewRelic(
        this LoggerSinkConfiguration loggerSinkConfiguration,
        ITextFormatter formatter,
        string endpointUrl = NewRelicSink.DefaultEndpoint,
        string? licenseKey = null,
        LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
        int batchSizeLimit = NewRelicSink.DefaultBatchSizeLimit,
        TimeSpan? period = null)
    {
        ArgumentNullException.ThrowIfNull(loggerSinkConfiguration);
        ArgumentNullException.ThrowIfNull(formatter);

        if (loggerSinkConfiguration == null)
            throw new ArgumentNullException(nameof(loggerSinkConfiguration));

        licenseKey ??= Environment.GetEnvironmentVariable("NEW_RELIC_LICENSE_KEY");
        period ??= NewRelicSink.DefaultPeriod;

        if (string.IsNullOrWhiteSpace(endpointUrl))
            throw new ArgumentException("NewRelic Logs API endpoint URL must be supplied", nameof(endpointUrl));

        if (string.IsNullOrWhiteSpace(licenseKey))
            throw new ArgumentException("LicenseKey must be supplied", nameof(licenseKey));

        var sink = new NewRelicSink(endpointUrl, licenseKey, formatter);
        var batchingOptions = new PeriodicBatchingSinkOptions
        {
            BatchSizeLimit = batchSizeLimit,
            Period = period.Value,
            EagerlyEmitFirstEvent = true,
            QueueLimit = 10000
        };
        var batchingSink = new PeriodicBatchingSink(sink, batchingOptions);
        return loggerSinkConfiguration.Sink(batchingSink, restrictedToMinimumLevel);
    }
}