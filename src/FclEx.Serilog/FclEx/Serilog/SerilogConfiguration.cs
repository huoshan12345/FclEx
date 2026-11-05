using Microsoft.Extensions.Logging;
using ILogger = Serilog.ILogger;

namespace FclEx.Serilog;

public class SerilogConfiguration
{
    private readonly List<ILogEventExcluder> _excluders = [];
    private readonly List<Action<LoggerConfiguration>> _actions = [];

    public LogEventLevel MinimumLevel { get; set; } = LogEventLevel.Information;
    /// <summary>Whether to wrap normalized exceptions for consistent formatting before dispatching to WriteTo sinks. Defaults to true.</summary>
    /// <remarks>Exception-level handling and duplicate-message unwrapping still run when this is false.</remarks>
    public bool FormatException { get; set; } = true;
    /// <summary>Configures Microsoft's logging builder during service registration, before the logger is created.</summary>
    /// <remarks>
    /// Assigning this property replaces previously registered callbacks.
    /// <see cref="SerilogConfigurationExtensions.ConfigureLogging"/> and
    /// <see cref="SerilogConfigurationExtensions.ConfigureLoggingBuilder"/> append callbacks in registration order.
    /// </remarks>
    public Action<ILoggingBuilder>? LoggingBuilderConfigure { get; set; }

    public SerilogConfiguration()
    {
        _actions.Add(m =>
        {
            m.MinimumLevel.Is(MinimumLevel)
                .Destructure.UsingAttributes(x => x.RespectLogPropertyIgnoreAttribute = true)
                .Enrich.FromLogContext()
                .Filter.ByExcluding(x => _excluders.Any(y => y.ShouldExclude(x)));
        });
    }

    /// <summary>Appends a Serilog configuration action to run whenever a root logger is created.</summary>
    /// <param name="action">The action that configures that root logger's pipeline.</param>
    /// <returns>The same configuration for further customization.</returns>
    /// <remarks>
    /// Actions run in registration order after the defaults. For independent loggers in multiple containers,
    /// create disposable sinks inside these actions. Configure Microsoft logging separately with
    /// <see cref="SerilogConfigurationExtensions.ConfigureLogging"/> before registering services.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="action"/> is null.</exception>
    public SerilogConfiguration Configure(Action<LoggerConfiguration> action)
    {
        Check.NotNull(action);
        _actions.Add(action);
        return this;
    }

    public SerilogConfiguration Exclude(ILogEventExcluder excluder)
    {
        _excluders.Add(excluder);
        return this;
    }

    /// <summary>Creates a logger with the configured enrichers, filters, sinks, and exception handling.</summary>
    /// <returns>A new root logger owned by the caller.</returns>
    /// <remarks>
    /// The caller must dispose the logger to flush and release its sinks. Use asynchronous disposal
    /// on supported targets when sinks require asynchronous cleanup.
    /// If a configuration action fails, registered sinks are disposed before the error is propagated.
    /// The original event level must pass Serilog's initial minimum-level check and any upstream filters.
    /// For events carrying a <see cref="LogException"/>, its level is applied before event filters and
    /// rechecked against the logger's current minimum level, including source overrides and level switches.
    /// <see cref="LogLevel.None"/> suppresses the event. A higher exception level cannot restore an event
    /// rejected before exception-level processing. Upstream Microsoft logger factory filters still evaluate
    /// the original call level.
    /// After event filters, exception unwrapping and optional formatting run once before dispatching to
    /// the configured WriteTo sinks. Nested exception levels are not reapplied for subsequent sinks.
    /// </remarks>
    /// <exception cref="AggregateException">Configuration fails and cleanup of the registered sinks also fails.</exception>
    public Logger CreateLogger()
    {
        // The filter runs after construction, when this variable refers to the configured root logger.
        Logger? logger = null;

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

        try
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _actions.Count; i++)
            {
                var action = _actions[i];
                action.Invoke(configuration);
            }
        }
        catch (Exception configurationError)
        {
            try
            {
                // Complete a temporary pipeline to release sinks registered before the action failed.
                var failedLogger = configuration.CreateLogger();
#if NET6_0_OR_GREATER
                // Cleanup can require asynchronous sinks, even though construction is synchronous.
                Task.Run(async () => await failedLogger.DisposeAsync()).GetAwaiter().GetResult();
#else
                failedLogger.Dispose();
#endif
            }
            catch (Exception cleanupError)
            {
                throw new AggregateException("Logger configuration and sink cleanup both failed.", configurationError, cleanupError);
            }
            throw;
        }

        var sinks = Fields.LoggerConfiguration_Sinks.GetRequiredValue<List<ILogEventSink>>(configuration);

        // Normalize once before fan-out, so later sinks cannot unwrap another exception or change its level.
        // The group preserves each sink's restrictions, failure isolation, disposal, and failure notifications.
        var aggregateSink = SinkPipeline.CreateGroup(sinks);

        // The mutation sink owns this group directly; no extra disposal-forwarding layer is needed.
        var mutationSink = new LogEventMutateSink(aggregateSink, FormatException ? logEvent => logEvent.FormatException() : null);

        // The wrapper now owns the captured sinks; remove the original registrations to avoid double dispatch/disposal.
        sinks.Clear();
        configuration.WriteTo.Sink(mutationSink);
        logger = configuration.CreateLogger();
        return logger;
    }
}
