namespace FclEx.Logging;

public class CollectingLogger : ILogger
{
    private readonly string _category;
    private readonly List<LogEntry> _entries;

    public CollectingLogger(string category, List<LogEntry> entries)
    {
        _category = category;
        _entries = entries;
    }

    IDisposable ILogger.BeginScope<TState>(TState state) => Disposable.Empty;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);

        _entries.Add(new LogEntry
        {
            Category = _category,
            LogLevel = logLevel,
            EventId = eventId,
            Message = message,
            Exception = exception
        });
    }
}

public sealed class LogEntry
{
    public string Category { get; init; } = "";
    public LogLevel LogLevel { get; init; }
    public EventId EventId { get; init; }
    public string Message { get; init; } = "";
    public Exception? Exception { get; init; }
}