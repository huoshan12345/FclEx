using Microsoft.Extensions.Logging;

namespace FclEx.Serilog;

/// <summary>
/// A wrapper around SerilogLoggerProvider that intentionally excludes the ISupportExternalScope interface.<br/>
/// This is a workaround to avoid Property in LogEvent to be overriden by scope.<br/>
/// See details on <see href="https://github.com/serilog/serilog-extensions-logging/pull/272" />
/// </summary>
public class SerilogLoggerProviderWithoutExternalScope : ILoggerProvider, ILogEventEnricher
{
    private readonly SerilogLoggerProvider _innerProvider;

    public SerilogLoggerProviderWithoutExternalScope(global::Serilog.ILogger? logger = null, bool dispose = false)
    {
        _innerProvider = new SerilogLoggerProvider(logger, dispose);
    }

    public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
    {
        return _innerProvider.CreateLogger(categoryName);
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        _innerProvider.Enrich(logEvent, propertyFactory);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _innerProvider.Dispose();
    }
}
