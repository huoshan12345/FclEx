using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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

    private static readonly ConcurrentDictionary<string, ILogger> _sourceLoggers = new(StringComparer.Ordinal);

    /// <summary>Creates a logger with the configured enrichers, filters, sinks, and exception handling.</summary>
    /// <returns>The configured Serilog logger.</returns>
    /// <remarks>
    /// The original event level must pass Serilog's initial minimum-level check and any upstream filters.
    /// For events carrying a <see cref="LogException"/>, its level is applied before event filters and
    /// rechecked against the logger's current minimum level, including source overrides and level switches.
    /// <see cref="LogLevel.None"/> suppresses the event. A higher exception level cannot restore an event
    /// rejected before exception-level processing. Upstream Microsoft logger factory filters still evaluate
    /// the original call level.
    /// </remarks>
    public ILogger CreateLogger()
    {
        ILogger? logger = null;

        var configuration = new LoggerConfiguration()
            .Filter.ByIncludingOnly(logEvent =>
            {
                if (logEvent.Exception is not LogException logException)
                    return true;

                if (logException.Level == LogLevel.None)
                    return false;

                logEvent.SetLevel(logException.Level.ToSerilogLevel());

                // ReSharper disable once AccessToModifiedClosure
                var sourceLogger = logger ?? Logger.None;

                if (logEvent.GetSourceContext() is { } source)
                {
                    sourceLogger = _sourceLoggers.GetOrAdd(source, m => sourceLogger.ForContext(Constants.SourceContext, m));
                }

                return sourceLogger.IsEnabled(logEvent.Level);
            });

        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < _actions.Count; i++)
        {
            var action = _actions[i];
            action.Invoke(configuration, this);
        }

        logger = configuration
            .WrapAllSinks(m => new LogEventMutateSink(m, FormatException ? x => x.FormatException() : null))
            .CreateLogger();
        return logger;
    }
}
