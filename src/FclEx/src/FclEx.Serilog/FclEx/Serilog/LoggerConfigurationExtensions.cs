using FclEx.Serilog.Sinks;
using Serilog;
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
}