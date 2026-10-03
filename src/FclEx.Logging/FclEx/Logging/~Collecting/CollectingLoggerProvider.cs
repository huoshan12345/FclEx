namespace FclEx.Logging;

/// <summary>Collects entries from all created logger categories into one synchronized in-memory store.</summary>
/// <remarks>Supports the external scope context supplied by a logger factory.</remarks>
public sealed class CollectingLoggerProvider : ILoggerProvider, ISupportExternalScope
{
    private readonly List<LogEntry> _entries = [];
    private readonly object _lock = new();
    private IExternalScopeProvider _scopeProvider = new LoggerExternalScopeProvider();

    /// <summary>Returns a snapshot of all collected entries in insertion order.</summary>
    /// <remarks>Later writes and clearing do not change the returned list; entry values are not deep copied.</remarks>
    public IReadOnlyList<LogEntry> Entries
    {
        get { lock (_lock) return _entries.ToList(); }
    }

    /// <summary>Removes entries from every category without ending scopes or changing earlier snapshots.</summary>
    public void Clear()
    {
        lock (_lock) _entries.Clear();
    }

    /// <summary>Creates a logger that shares this provider's entries and current scope provider.</summary>
    /// <param name="categoryName">The category recorded for messages written by the returned logger.</param>
    /// <returns>A new collector associated with the specified category.</returns>
    public ILogger CreateLogger(string categoryName)
        => new CollectingLogger(categoryName, _entries, _lock, () => _scopeProvider);

    /// <summary>Sets the scope provider used by both existing and subsequently created loggers.</summary>
    /// <param name="scopeProvider">The shared scope context, normally supplied by the logger factory.</param>
    /// <exception cref="ArgumentNullException"><paramref name="scopeProvider"/> is null.</exception>
    public void SetScopeProvider(IExternalScopeProvider scopeProvider)
    {
        _scopeProvider = Check.NotNull(scopeProvider);
    }

    /// <summary>Completes disposal without clearing the collected entries.</summary>
    /// <remarks>The provider owns no disposable resources; entries remain available for inspection.</remarks>
    public void Dispose() { }
}
