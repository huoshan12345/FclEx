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
                .Destructure.UsingAttributes(x => x.RespectLogPropertyIgnoreAttribute = true)
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