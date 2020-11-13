using Dawn;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;

namespace FclEx.Serilog.Sinks.Logstash
{
    public static class LoggerConfigurationLogstashExtensions
    {
        public static LoggerConfiguration Logstash(this LoggerSinkConfiguration config, LogstashSinkOptions options)
        {
            Guard.Argument(options, nameof(options)).NotNull();

            return config.Sink(new LogstashSink(options),
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
