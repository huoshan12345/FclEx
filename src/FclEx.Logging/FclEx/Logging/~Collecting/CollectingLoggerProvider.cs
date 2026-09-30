namespace FclEx.Logging;

/// <summary>注册到 ILoggerFactory / DI,所有类别的日志汇总到同一个列表。</summary>
public sealed class CollectingLoggerProvider : ILoggerProvider
{
    private readonly List<LogEntry> _entries = [];
    private readonly object _lock = new();

    /// <summary>汇总所有类别的日志。</summary>
    public CollectingLogger Logger => new("", _entries, _lock);

    public IReadOnlyList<LogEntry> Entries => Logger.Entries;

    public ILogger CreateLogger(string categoryName) => new CollectingLogger(categoryName, _entries, _lock);

    public void Dispose() { }
}