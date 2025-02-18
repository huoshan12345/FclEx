namespace FclEx.Serilog;

public static class Types
{
    public static readonly Type MessageTemplateRenderer = typeof(LogEvent).Assembly.GetRequiredType("Serilog.Rendering.MessageTemplateRenderer");
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class Fields
{
    public static readonly FieldInfo MessageTemplate_Text = typeof(MessageTemplate).GetAutoPropertyBackingField(nameof(MessageTemplate.Text));
    public static readonly FieldInfo LogEvent_Level = typeof(LogEvent).GetAutoPropertyBackingField(nameof(LogEvent.Level));
    public static readonly FieldInfo LogEvent_Exception = typeof(LogEvent).GetAutoPropertyBackingField(nameof(LogEvent.Exception));
    public static readonly FieldInfo LogEvent_MessageTemplate = typeof(LogEvent).GetAutoPropertyBackingField(nameof(LogEvent.MessageTemplate));
    public static readonly FieldInfo LoggerConfiguration_Sinks = typeof(LoggerConfiguration).GetRequiredField("_logEventSinks");
}

public static class Methods
{
    public static readonly MethodInfo MessageTemplateRenderer_Render = Types.MessageTemplateRenderer.GetRequiredMethod("Render");
}