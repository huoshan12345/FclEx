using Serilog;
using Serilog.Configuration;
using Serilog.Events;

namespace FclEx.Serilog;

public static class LoggerSinkConfigurationExtensions
{
    public static LoggerConfiguration Slack(
        this LoggerSinkConfiguration configuration,
        string token,
        string channel,
        LogEventLevel restrictedToMinimumLevel = LogEventLevel.Information,
        int batchSizeLimit = 10,
        TimeSpan? period = null)
    {
        Check.NotEmpty(token);
        Check.NotEmpty(channel);

        period ??= TimeSpan.FromSeconds(1);
        var sink = new SlackSink(token, channel);
        var batchingOptions = new BatchingOptions
        {
            BatchSizeLimit = batchSizeLimit,
            BufferingTimeLimit = period.Value,
            EagerlyEmitFirstEvent = true,
            QueueLimit = 10000,
        };

        return configuration.Sink(sink, batchingOptions, restrictedToMinimumLevel);
    }
}