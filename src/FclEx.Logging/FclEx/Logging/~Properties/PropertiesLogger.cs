namespace FclEx.Logging;

/// <summary>Wraps a logger with properties that are scoped to each enabled log call.</summary>
/// <remarks>
/// Property definitions are copied at construction. Values are retained by reference rather than deep copied.
/// Lazy factories run once per enabled log call, and every provider sees the same results for that call.
/// The wrapped logger must support structured scopes to include these properties in its output.
/// </remarks>
public class PropertiesLogger : ILogger
{
    private readonly ILogger _logger;
    private readonly LoggerProperty[] _properties;
    private readonly LazyLoggerProperty[] _lazyProperties;

    /// <summary>Creates a wrapper, combining property definitions when the supplied logger is another wrapper.</summary>
    /// <param name="logger">The logger that receives enabled messages and temporary scopes.</param>
    /// <param name="properties">Fixed properties, or null for none. Enumerated once during construction.</param>
    /// <param name="lazyProperties">Lazy property definitions, or null for none. Factories are not invoked during construction.</param>
    /// <remarks>Property order and duplicate keys are preserved; precedence is determined by the logging provider.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="logger"/> is null.</exception>
    public PropertiesLogger(ILogger logger,
        IEnumerable<LoggerProperty>? properties = null,
        IEnumerable<LazyLoggerProperty>? lazyProperties = null)
    {
        Check.NotNull(logger);
        properties ??= [];
        lazyProperties ??= [];
        if (logger is PropertiesLogger inner)
        {
            _logger = inner._logger;
            _properties = inner._properties.Concat(properties).ToArray();
            _lazyProperties = inner._lazyProperties.Concat(lazyProperties).ToArray();
        }
        else
        {
            _logger = logger;
            _properties = properties.ToArray();
            _lazyProperties = lazyProperties.ToArray();
        }
    }

    /// <summary>Writes an enabled message with fixed properties and freshly evaluated lazy properties.</summary>
    /// <typeparam name="TState">The message state type.</typeparam>
    /// <param name="logLevel">The level checked against the wrapped logger before evaluating any lazy property.</param>
    /// <param name="eventId">The event identifier passed to the wrapped logger.</param>
    /// <param name="state">The original message state.</param>
    /// <param name="exception">The optional exception associated with the message.</param>
    /// <param name="formatter">The formatter passed to the wrapped logger.</param>
    /// <remarks>
    /// Lazy factory exceptions propagate without writing a message. Temporary scopes are restored even if
    /// property evaluation or logging throws. Factories used concurrently must provide their own synchronization.
    /// </remarks>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!_logger.IsEnabled(logLevel))
            return;

        using (_logger.PushProperty(_properties))
        using (_logger.PushProperty(_lazyProperties.Select(m => (m.Key, m.ValueFactory()))))
        {
            _logger.Log(logLevel, eventId, state, exception, formatter);
        }
    }

    /// <summary>Returns whether the wrapped logger accepts the specified level.</summary>
    /// <param name="logLevel">The level to check.</param>
    /// <returns>The wrapped logger's result.</returns>
    public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

    /// <summary>Begins an ordinary scope on the wrapped logger without evaluating this wrapper's properties.</summary>
    /// <typeparam name="TState">The scope state type.</typeparam>
    /// <param name="state">The scope state passed through to the wrapped logger.</param>
    /// <returns>The scope handle, or an empty disposable if the wrapped logger does not support scopes.</returns>
    public IDisposable BeginScope<TState>(TState state) where TState : notnull
        => _logger.BeginScope(state) ?? Disposable.Empty;
}
