using Microsoft.Extensions.Logging;

namespace FclEx.Serilog;

/// <summary>Delegates level conversion between Serilog and Microsoft logging to the Serilog provider.</summary>
public static class LogEventLevelExtensions
{
    /// <summary>Converts a Microsoft logging level using Serilog's provider mapping.</summary>
    /// <param name="logLevel">The level to convert.</param>
    /// <returns>The mapped Serilog level, with Trace corresponding to Verbose and Critical to Fatal.</returns>
    public static LogEventLevel ToSerilogLevel(this LogLevel logLevel)
    {
        return LevelConvert.ToSerilogLevel(logLevel);
    }

    /// <summary>Converts a Serilog level to the corresponding Microsoft logging level.</summary>
    /// <param name="logEventLevel">The Serilog level to convert.</param>
    /// <returns>The provider mapping, with Verbose corresponding to Trace and Fatal to Critical.</returns>
    public static LogLevel ToExtensionsLevel(this LogEventLevel logEventLevel)
    {
        return LevelConvert.ToExtensionsLevel(logEventLevel);
    }
}
