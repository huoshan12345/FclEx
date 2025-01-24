namespace FclEx.Serilog;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class Fields
{
    public static readonly FieldInfo MessageTemplate_Text = typeof(MessageTemplate).GetAutoPropertyBackingField(nameof(MessageTemplate.Text));
    public static readonly FieldInfo LogEvent_Level = typeof(LogEvent).GetAutoPropertyBackingField(nameof(LogEvent.Level));
    public static readonly FieldInfo LogEvent_Exception = typeof(LogEvent).GetAutoPropertyBackingField(nameof(LogEvent.Exception));
    public static readonly FieldInfo LoggerConfiguration_Sinks = typeof(LoggerConfiguration).GetRequiredField("_logEventSinks");
}