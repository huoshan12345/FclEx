using System;
using System.Collections.Generic;
using System.Linq;
using FclEx;
using FclEx.Extensions;

namespace Microsoft.Extensions.Logging;

public class PropertiesLogger : ILogger
{
    private readonly ILogger _logger;
    private readonly IEnumerable<KeyValuePair<string, object>> _properties;
    private readonly IEnumerable<KeyValuePair<string, Func<object>>> _lazyProperties;

    public PropertiesLogger(ILogger logger,
        IEnumerable<KeyValuePair<string, object>>? properties = null,
        IEnumerable<KeyValuePair<string, Func<object>>>? lazyProperties = null)
    {
        lazyProperties ??= Enumerable.Empty<KeyValuePair<string, Func<object>>>();
        properties ??= Enumerable.Empty<KeyValuePair<string, object>>();
        if (logger is PropertiesLogger scopeLogger)
        {
            _logger = scopeLogger._logger;
            _properties = scopeLogger._properties.Concat(properties);
            _lazyProperties = scopeLogger._lazyProperties.Concat(lazyProperties);
        }
        else
        {
            _logger = logger;
            _properties = properties;
            _lazyProperties = lazyProperties;
        }
    }

    public PropertiesLogger(ILogger logger,
        IEnumerable<(string, object)>? properties = null,
        IEnumerable<(string, Func<object>)>? lazyProperties = null)
        : this(logger, properties!.Touch().AsKeyValue(), lazyProperties.Touch().AsKeyValue())
    {

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

    public IDisposable BeginScope<TState>(TState state) => _logger.BeginScope(state);
}