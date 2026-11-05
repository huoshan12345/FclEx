namespace FclEx.Serilog;

/// <summary>Shared names and output templates for FclEx Serilog integrations.</summary>
public static class Constants
{
    /// <summary>A text-sink template containing the timestamp and offset, three-character uppercase level, source, rendered message, and exception.</summary>
    public const string DefaultOutputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss zzz} {Level:u3}] [{SourceContext}] {Message:l}{NewLine}{Exception}";
    /// <summary>Serilog's SourceContext property name, used for logger categories and source filtering.</summary>
    public const string SourceContext = global::Serilog.Core.Constants.SourceContextPropertyName;
}
