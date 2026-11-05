namespace FclEx.Serilog;

/// <summary>Cached Serilog implementation types used by reflection-based helpers.</summary>
/// <remarks>These descriptors depend on Serilog's private implementation and the package version used by FclEx.</remarks>
public static class Types
{
    /// <summary>The internal Serilog message-template renderer type resolved on first initialization.</summary>
    public static readonly Type MessageTemplateRenderer = typeof(LogEvent).Assembly.GetRequiredType("Serilog.Rendering.MessageTemplateRenderer");
}

/// <summary>Cached reflection fields used to mutate Serilog events and inspect registered sinks.</summary>
/// <remarks>These descriptors access implementation details rather than stable Serilog extension points.</remarks>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class Fields
{
    /// <summary>The backing field for MessageTemplate.Text.</summary>
    public static readonly FieldInfo MessageTemplate_Text = typeof(MessageTemplate).GetAutoPropertyBackingField(nameof(MessageTemplate.Text));
    /// <summary>The backing field for LogEvent.Level, used for exception-driven level changes.</summary>
    public static readonly FieldInfo LogEvent_Level = typeof(LogEvent).GetAutoPropertyBackingField(nameof(LogEvent.Level));
    /// <summary>The backing field for LogEvent.Exception, used when wrapping or unwrapping exceptions.</summary>
    public static readonly FieldInfo LogEvent_Exception = typeof(LogEvent).GetAutoPropertyBackingField(nameof(LogEvent.Exception));
    /// <summary>The backing field for LogEvent.MessageTemplate, used when replacing an event's template.</summary>
    public static readonly FieldInfo LogEvent_MessageTemplate = typeof(LogEvent).GetAutoPropertyBackingField(nameof(LogEvent.MessageTemplate));
    /// <summary>The private list of WriteTo sinks registered with a LoggerConfiguration; AuditTo sinks use a separate list.</summary>
    public static readonly FieldInfo LoggerConfiguration_Sinks = typeof(LoggerConfiguration).GetRequiredField("_logEventSinks");
}

/// <summary>Cached Serilog implementation methods used by message-rendering helpers.</summary>
/// <remarks>Resolution depends on Serilog's private implementation and may fail with incompatible versions.</remarks>
public static class Methods
{
    /// <summary>The internal renderer accepting a message template, properties, writer, format, and format provider.</summary>
    public static readonly MethodInfo MessageTemplateRenderer_Render = Types.MessageTemplateRenderer.GetRequiredMethod("Render");
}
