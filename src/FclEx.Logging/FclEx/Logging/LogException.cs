namespace FclEx.Logging;

public class LogException : Exception
{
    public LogLevel Level { get; }

    public LogException(string message, LogLevel level, Exception? inner = null) : base(message, inner)
    {
        Level = level;
    }
}