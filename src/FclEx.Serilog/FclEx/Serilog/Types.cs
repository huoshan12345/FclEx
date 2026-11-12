namespace FclEx.Serilog;

/// <summary>Cached Serilog implementation types used by reflection-based helpers.</summary>
/// <remarks>These descriptors depend on Serilog's private implementation and the package version used by FclEx.</remarks>
public static class Types
{
    /// <summary>The internal Serilog message-template renderer type resolved on first initialization.</summary>
    public static readonly Type MessageTemplateRenderer = typeof(LogEvent).Assembly.GetRequiredType("Serilog.Rendering.MessageTemplateRenderer");
}