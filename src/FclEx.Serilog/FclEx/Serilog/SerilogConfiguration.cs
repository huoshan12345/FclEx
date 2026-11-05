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
        // The filter runs after construction, when this variable refers to the configured root logger.
        ILogger? logger = null;

        // Context loggers retain their root's thresholds and switches, so each root owns its own cache.
        var sourceLoggers = new ConcurrentDictionary<string, ILogger>(StringComparer.Ordinal);

        // Create the factory delegate once to avoid allocating it for each log event, including cache hits.
        Func<string, ILogger> createSourceLogger = source => (logger ?? Logger.None).ForContext(Constants.SourceContext, source);

        // Register this filter first so subsequent event filters and sink restrictions see the exception level.
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
                    sourceLogger = sourceLoggers.GetOrAdd(source, createSourceLogger);
                }

                // Recheck the final level with source overrides and the current level-switch values.
                // Events rejected by the original level check never reach this filter.
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
