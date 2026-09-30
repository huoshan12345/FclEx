namespace FclEx.Logging;

public sealed class CollectingLoggerProvider : ILoggerProvider, ISupportExternalScope
{
    private readonly List<LogEntry> _entries = [];
    private readonly object _lock = new();
    private IExternalScopeProvider _scopeProvider = new LoggerExternalScopeProvider();

    public CollectingLogger Logger => new("", _entries, _lock, () => _scopeProvider);

    public IReadOnlyList<LogEntry> Entries => Logger.Entries;

    public ILogger CreateLogger(string categoryName)
        => new CollectingLogger(categoryName, _entries, _lock, () => _scopeProvider);

    public void SetScopeProvider(IExternalScopeProvider scopeProvider)
    {
        _scopeProvider = Check.NotNull(scopeProvider);
    }

    public void Dispose() { }
}
