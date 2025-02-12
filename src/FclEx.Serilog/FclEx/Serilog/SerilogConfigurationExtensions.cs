using Microsoft.Extensions.Logging;

namespace FclEx.Serilog;

public static class SerilogConfigurationExtensions
{
    public static SerilogConfiguration AddCommonExcluders(this SerilogConfiguration configuration)
    {
        return configuration
            .Exclude(ExceptionExcluder.CommonItems)
            .Exclude(MessageExcluder.CommonItems)
            .Exclude(PropertyExcluder.CommonItems)
            .Exclude(SourceExcluder.CommonItems);
    }

    public static SerilogConfiguration Configure(this SerilogConfiguration configuration, Action<LoggerConfiguration> action)
    {
        return configuration.Configure((m, _) => action(m));
    }

    public static SerilogConfiguration ConfigureLoggingBuilder(this SerilogConfiguration configuration, Action<ILoggingBuilder> action)
    {
        return configuration.Configure((m, n) => n.LoggingBuilderConfigure += action);
    }

    public static SerilogConfiguration Exclude(this SerilogConfiguration configuration, IEnumerable<ILogEventExcluder> excluders)
    {
        foreach (var excluder in excluders)
        {
            configuration.Exclude(excluder);
        }
        return configuration;
    }


    public static SerilogConfiguration Exclude(this SerilogConfiguration configuration, Func<LogEvent, bool> predicate)
    {
        return configuration.Exclude(new LogEventExcluder(predicate));
    }

    public static SerilogConfiguration Enrich(this SerilogConfiguration configuration, ILogEventEnricher enricher)
    {
        return configuration.Configure(m => m.Enrich.With(enricher));
    }

    public static SerilogConfiguration WriteTo(this SerilogConfiguration configuration, ILogEventSink sink)
    {
        return configuration.Configure(m => m.WriteTo.Sink(sink));
    }

    public static SerilogConfiguration FormatException(this SerilogConfiguration configuration, bool value)
    {
        configuration.FormatException = value;
        return configuration;
    }

    public static SerilogConfiguration WriteTo(this SerilogConfiguration configuration, Action<LoggerSinkConfiguration> action)
    {
        return configuration.Configure(m => action(m.WriteTo));
    }

    public static SerilogConfiguration ConfigureLogging(this SerilogConfiguration configuration, Action<ILoggingBuilder> action)
    {
        configuration.LoggingBuilderConfigure += action;
        return configuration;
    }
}