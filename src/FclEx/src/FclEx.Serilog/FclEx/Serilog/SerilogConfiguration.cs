using Microsoft.Extensions.Logging;
using ILogger = Serilog.ILogger;

namespace FclEx.Serilog;

public delegate void ConfigureAction(LoggerConfiguration configuration, SerilogConfiguration serilogConfiguration);

public class SerilogConfiguration
{
    public List<ILogEventExcluder> Excluders { get; } = [];
    public List<ConfigureAction> ConfigureActions { get; } = [];
    public LogEventLevel MinimumLevel { get; set; } = LogEventLevel.Information;
    public bool FormatException { get; set; } = true;
    public Action<ILoggingBuilder>? LoggingBuilderConfigure { get; set; }

    public SerilogConfiguration()
    {
        ConfigureActions.Add((m, n) =>
        {
            m.MinimumLevel.Is(MinimumLevel)
                .Destructure.UsingAttributes()
                .Enrich.FromLogContext()
                .Filter.ByExcluding(x => Excluders.Any(y => y.ShouldExclude(x)));
        });
    }

    public ILogger CreateSerilogLogger()
    {
        var configuration = new LoggerConfiguration();
        foreach (var action in ConfigureActions)
        {
            action.Invoke(configuration, this);
        }
        return configuration
            .WrapAllSinks(m => new LogEventMutateSink(m, FormatException ? x => x.FormatException() : null))
            .CreateLogger();
    }
}

public static class SerilogConfigurationExtensions
{
    public static SerilogConfiguration AddCommonExcluders(this SerilogConfiguration configuration)
    {
        return configuration.Exclude(ExceptionExcluder.CommonItems)
             .Exclude(MessageExcluder.CommonItems)
             .Exclude(PropertyExcluder.CommonItems)
             .Exclude(SourceExcluder.CommonItems);
    }

    public static SerilogConfiguration Configure(this SerilogConfiguration configuration, ConfigureAction action)
    {
        configuration.ConfigureActions.Add(action);
        return configuration;
    }

    public static SerilogConfiguration Configure(this SerilogConfiguration configuration, Action<LoggerConfiguration> action)
    {
        return configuration.Configure((m, _) => action(m));
    }

    public static SerilogConfiguration Exclude(this SerilogConfiguration configuration, ILogEventExcluder excluder)
    {
        configuration.Excluders.Add(excluder);
        return configuration;
    }

    public static SerilogConfiguration Exclude(this SerilogConfiguration configuration, IEnumerable<ILogEventExcluder> excluders)
    {
        configuration.Excluders.AddRange(excluders);
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

}