namespace FclEx.Logging;

public sealed class CollectingLoggerProvider : ILoggerProvider, ISupportExternalScope
{
    private readonly List<LogEntry> _entries = [];
    private readonly object _lock = new();
    private IExternalScopeProvider _scopeProvider = new LoggerExternalScopeProvider();

    public IReadOnlyList<LogEntry> Entries
    {
        get { lock (_lock) return _entries.ToList(); }
    }

    public void Clear()
    {
        lock (_lock) _entries.Clear();
    }

    public ILogger CreateLogger(string categoryName)
        => new CollectingLogger(categoryName, _entries, _lock, () => _scopeProvider);

    public void SetScopeProvider(IExternalScopeProvider scopeProvider)
    {
        _scopeProvider = Check.NotNull(scopeProvider);
    }

    public void Dispose() { }
}
