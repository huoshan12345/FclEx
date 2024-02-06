namespace FclEx.Serilog;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class Fields
{
    public static readonly FieldInfo MessageTemplate_Text = typeof(MessageTemplate).GetRequiredField($"<{nameof(MessageTemplate.Text)}>k__BackingField");
    public static readonly FieldInfo LogEvent_Level = typeof(LogEvent).GetRequiredField($"<{nameof(LogEvent.Level)}>k__BackingField");
    public static readonly FieldInfo LogEvent_Exception = typeof(LogEvent).GetRequiredField($"<{nameof(LogEvent.Exception)}>k__BackingField");
}