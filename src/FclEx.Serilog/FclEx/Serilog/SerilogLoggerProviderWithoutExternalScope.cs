using Microsoft.Extensions.Logging;

namespace FclEx.Serilog;

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
