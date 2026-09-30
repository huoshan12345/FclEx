namespace FclEx.Logging;

/// <summary>收集日志的 ILogger,用于单元测试断言。线程安全。</summary>
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

    public IEnumerable<LogEntry> At(LogLevel level) => Entries.Where(e => e.Level == level);
    public IEnumerable<LogEntry> Errors => At(LogLevel.Error);
    public IEnumerable<LogEntry> Warnings => At(LogLevel.Warning);

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

/// <summary>直接注入到 ctor(ILogger&lt;T&gt;) 的场景。</summary>
public sealed class CollectingLogger<T> : CollectingLogger, ILogger<T>
{
    public CollectingLogger() : base(typeof(T).FullName ?? typeof(T).Name) { }
}
