using Microsoft.Extensions.Logging;
using ILogger = Serilog.ILogger;

namespace FclEx.Serilog;

public delegate void ConfigureAction(LoggerConfiguration configuration, SerilogConfiguration serilogConfiguration);

public class SerilogConfiguration
{
    private readonly List<ILogEventExcluder> _excluders = [];
    private readonly List<ConfigureAction> _actions = [];

    public LogEventLevel MinimumLevel { get; set; } = LogEventLevel.Information;
    public bool FormatException { get; set; } = true;
    public Action<ILoggingBuilder>? LoggingBuilderConfigure { get; set; }

    public SerilogConfiguration()
    {
        _actions.Add((m, n) =>
        {
            m.MinimumLevel.Is(MinimumLevel)
                .Destructure.UsingAttributes()
                .Enrich.FromLogContext()
                .Filter.ByExcluding(x => _excluders.Any(y => y.ShouldExclude(x)));
        });
    }

    public SerilogConfiguration Configure(ConfigureAction action)
    {
        _actions.Add(action);
        return this;
    }

    public SerilogConfiguration Exclude(ILogEventExcluder excluder)
    {
        _excluders.Add(excluder);
        return this;
    }

    public ILogger CreateLogger()
    {
        var configuration = new LoggerConfiguration();

        // 不能用 foreach，因为在循环体内部可能会修改 _actions
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < _actions.Count; i++)
        {
            var action = _actions[i];
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

    public static SerilogConfiguration Configure(this SerilogConfiguration configuration, Action<LoggerConfiguration> action)
    {
        return configuration.Configure((m, _) => action(m));
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

}