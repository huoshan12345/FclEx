namespace FclEx.Logging;

public class PropertiesLogger : ILogger
{
    private readonly ILogger _logger;
    private readonly IEnumerable<LoggerProperty> _properties;
    private readonly IEnumerable<LazyLoggerProperty> _lazyProperties;

    public PropertiesLogger(ILogger logger,
        IEnumerable<LoggerProperty>? properties = null,
        IEnumerable<LazyLoggerProperty>? lazyProperties = null)
    {
        properties ??= [];
        lazyProperties ??= [];
        if (logger is PropertiesLogger inner)
        {
            _logger = inner._logger;
            _properties = inner._properties.Concat(properties);
            _lazyProperties = inner._lazyProperties.Concat(lazyProperties);
        }
        else
        {
            _logger = logger;
            _properties = properties;
            _lazyProperties = lazyProperties;
        }
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        using (_logger.PushProperty(_properties))
        using (_logger.PushProperty(_lazyProperties.Select(m => (m.Key, m.Value()))))
        {
            _logger.Log(logLevel, eventId, state, exception, formatter);
        }
    }

    public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

    public IDisposable BeginScope<TState>(TState state) where TState : notnull
        => _logger.BeginScope(state) ?? Disposable.Empty;
}