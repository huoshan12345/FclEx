using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;

namespace FclEx.Serilog.Sinks.Logstash
{
    public static class LoggerConfigurationLogstashExtensions
    {
        public static LoggerConfiguration Logstash(this LoggerSinkConfiguration config, LogstashSinkOptions options)
        {
            Check.NotNull(config);
            Check.NotNull(options);

            var op = new PeriodicBatchingSinkOptions
            {
                EagerlyEmitFirstEvent = false,
                BatchSizeLimit = options.BatchSizeLimit,
                Period = options.Period,
                QueueLimit = options.QueueLimit
            };
            return config.Sink(
                logEventSink: new PeriodicBatchingSink(new LogstashSink(options), op),
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
}
