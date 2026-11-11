namespace FclEx.Serilog;

/// <summary>Cached reflection fields used to mutate Serilog events and inspect registered sinks.</summary>
/// <remarks>These descriptors access implementation details rather than stable Serilog extension points.</remarks>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class FieldInfos
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
