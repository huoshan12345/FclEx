namespace FclEx.Logging;

public class CollectingLogger : ILogger
{
    private readonly List<LogEntry> _entries;
    private readonly object _lock;
    private readonly string _category;
    private readonly Func<IExternalScopeProvider> _getScopeProvider;

    public CollectingLogger(string category = "")
        : this(category, [], new object(), CreateScopeProviderAccessor()) { }

    internal CollectingLogger(string category, List<LogEntry> entries, object @lock,
        Func<IExternalScopeProvider> getScopeProvider)
    {
        _category = category;
        _entries = entries;
        _lock = @lock;
        _getScopeProvider = getScopeProvider;
    }

    public IReadOnlyList<LogEntry> Entries
    {
        get { lock (_lock) return _entries.ToList(); }
    }

    public void Clear()
    {
        lock (_lock) _entries.Clear();
    }

    public IDisposable BeginScope<TState>(TState state) where TState : notnull
        => _getScopeProvider().Push(state);

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
        Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var scopes = new List<object?>();
        _getScopeProvider().ForEachScope((scope, list) => list.Add(scope), scopes);
        var entry = new LogEntry(_category, logLevel, eventId,
            formatter(state, exception), exception, state)
        {
            Scopes = scopes.ToArray()
        };
        lock (_lock) _entries.Add(entry);
    }

    private static Func<IExternalScopeProvider> CreateScopeProviderAccessor()
    {
        var scopeProvider = new LoggerExternalScopeProvider();
        return () => scopeProvider;
    }
}
