using Microsoft.Extensions.Logging;

namespace FclEx.Serilog;

/// <summary>
/// Adapts Serilog to Microsoft logging without external scope support, preserving event properties when scope names collide.
/// </summary>
/// <remarks>
/// Logger ownership follows the constructor's dispose argument. On supported targets, asynchronous disposal
/// delegates to the native provider so asynchronous sink cleanup can complete.
/// See <see href="https://github.com/serilog/serilog-extensions-logging/pull/272"/>.
/// </remarks>
public class SerilogLoggerProviderWithoutExternalScope : ILoggerProvider, ILogEventEnricher
#if NET6_0_OR_GREATER
    , IAsyncDisposable
#endif
{
    private readonly SerilogLoggerProvider _innerProvider;

    /// <summary>Creates a provider that forwards logging and owns the logger only when requested.</summary>
    /// <param name="logger">The destination logger, or null to use <see cref="Log.Logger"/>.</param>
    /// <param name="dispose">Whether provider disposal disposes the supplied logger, or closes the global logger when logger is null.</param>
    public SerilogLoggerProviderWithoutExternalScope(global::Serilog.ILogger? logger = null, bool dispose = false)
    {
        _innerProvider = new SerilogLoggerProvider(logger, dispose);
    }

    /// <summary>Creates a Microsoft logger using the supplied category as its Serilog SourceContext.</summary>
    /// <param name="categoryName">The category attached to events emitted through the returned logger.</param>
    /// <returns>A logger backed by the native Serilog provider and its provider-managed scopes.</returns>
    public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
    {
        return _innerProvider.CreateLogger(categoryName);
    }

    /// <summary>Delegates event enrichment from provider-managed scopes to the native provider.</summary>
    /// <param name="logEvent">The event to enrich in place.</param>
    /// <param name="propertyFactory">Creates event properties from scope values.</param>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        _innerProvider.Enrich(logEvent, propertyFactory);
    }

    /// <summary>Synchronously disposes the native provider and any logger it owns.</summary>
    /// <remarks>Use asynchronous disposal on supported targets when owned sinks require asynchronous cleanup.</remarks>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _innerProvider.Dispose();
    }

#if NET6_0_OR_GREATER
    /// <summary>Asynchronously disposes the native provider and awaits cleanup of any logger it owns.</summary>
    /// <returns>An operation that completes when provider and owned logger cleanup has finished.</returns>
    /// <remarks>When constructed with dispose: false, the destination logger remains owned by its caller.</remarks>
    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return _innerProvider.DisposeAsync();
    }
#endif
}
