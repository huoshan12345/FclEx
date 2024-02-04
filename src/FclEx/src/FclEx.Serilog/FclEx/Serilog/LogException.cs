namespace FclEx.Serilog;

public class LogException : Exception
{
    public LogEventLevel Level { get; }

    public LogException(string message, LogEventLevel level, Exception? inner = null) : base(message, inner)
    {
        Level = level;
    }
}