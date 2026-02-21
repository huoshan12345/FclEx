namespace FclEx.Logging;

public class CollectingLoggerProvider : ILoggerProvider
{
    public List<LogEntry> Entries { get; } = [];

    public ILogger CreateLogger(string categoryName)
    {
        return new CollectingLogger(categoryName, Entries);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
