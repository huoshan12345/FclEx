namespace FclEx.Logging;

/// <summary>Collects log messages and their active scopes in memory for inspection.</summary>
/// <remarks>
/// Writes, snapshots, and clearing are synchronized. Standalone instances maintain independent async scopes.
/// Log state, exceptions, and scope values are retained by reference rather than deep copied.
/// </remarks>
public class CollectingLogger : ILogger
{
    private readonly List<LogEntry> _entries;
    private readonly object _lock;
    private readonly string _category;
    private readonly Func<IExternalScopeProvider> _getScopeProvider;

    /// <summary>Creates a standalone collector with its own entries and scope context.</summary>
    /// <param name="category">The category stored in every entry; defaults to an empty string.</param>
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

    /// <summary>Returns a snapshot of the collected entries in insertion order.</summary>
    /// <remarks>
    /// Subsequent writes and clearing do not change the returned list. Loggers created by the same
    /// <see cref="CollectingLoggerProvider"/> share all entries, including those from other categories.
    /// </remarks>
    public IReadOnlyList<LogEntry> Entries
    {
        get { lock (_lock) return _entries.ToList(); }
    }

    /// <summary>Removes collected entries without ending active scopes or changing earlier snapshots.</summary>
    /// <remarks>For a provider-created logger, this clears entries from all categories in that provider.</remarks>
    public void Clear()
    {
        lock (_lock) _entries.Clear();
    }

    /// <summary>Pushes a scope onto the current async context until the returned handle is disposed.</summary>
    /// <typeparam name="TState">The scope state type.</typeparam>
    /// <param name="state">The scope value retained in entries written while the scope is active.</param>
    /// <returns>A handle that restores the previous scope when disposed. Dispose nested scopes in reverse order.</returns>
    public IDisposable BeginScope<TState>(TState state) where TState : notnull
        => _getScopeProvider().Push(state);

    /// <summary>Determines whether the collector accepts the specified level.</summary>
    /// <param name="logLevel">The level to check.</param>
    /// <returns><see langword="false"/> for <see cref="LogLevel.None"/>; otherwise <see langword="true"/>.</returns>
    /// <remarks>When used through a logger factory, the factory also applies its configured filters.</remarks>
    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    /// <summary>Formats and stores a log entry with a snapshot of the active scope stack.</summary>
    /// <typeparam name="TState">The log state type.</typeparam>
    /// <param name="logLevel">The entry level. <see cref="LogLevel.None"/> is ignored without invoking the formatter.</param>
    /// <param name="eventId">The event identifier.</param>
    /// <param name="state">The original log state, retained without copying.</param>
    /// <param name="exception">The optional exception associated with the entry.</param>
    /// <param name="formatter">Produces the stored message from the state and exception.</param>
    /// <remarks>Formatter exceptions propagate and no entry is added for that call.</remarks>
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
