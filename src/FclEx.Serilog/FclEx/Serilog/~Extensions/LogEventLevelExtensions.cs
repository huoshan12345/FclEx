using Microsoft.Extensions.Logging;

namespace FclEx.Serilog;

public static class LogEventLevelExtensions
{
    public static LogEventLevel ToSerilogLevel(this LogLevel logLevel)
    {
        return LevelConvert.ToSerilogLevel(logLevel);
    }

    public static LogLevel ToExtensionsLevel(this LogEventLevel logEventLevel)
    {
        return LevelConvert.ToExtensionsLevel(logEventLevel);
    }
}