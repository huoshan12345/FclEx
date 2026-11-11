namespace FclEx.Serilog;

#if NET8_0_OR_GREATER
public static class UnsafeAccessors
{
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = $"<{nameof(MessageTemplate.Text)}>k__BackingField")]
    public static extern ref string MessageTemplate_Text(MessageTemplate obj);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = $"<{nameof(LogEvent.Level)}>k__BackingField")]
    public static extern ref LogEventLevel LogEvent_Level(LogEvent obj);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = $"<{nameof(LogEvent.Exception)}>k__BackingField")]
    public static extern ref Exception? LogEvent_Exception(LogEvent obj);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = $"<{nameof(LogEvent.MessageTemplate)}>k__BackingField")]
    public static extern ref MessageTemplate LogEvent_MessageTemplate(LogEvent obj);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_logEventSinks")]
    public static extern ref List<ILogEventSink> LoggerConfiguration_Sinks(LoggerConfiguration obj);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "Render")]
    public static extern void MessageTemplateRenderer_Render(
        [UnsafeAccessorType("Serilog.Rendering.MessageTemplateRenderer, Serilog")] object? target,
        MessageTemplate messageTemplate,
        IReadOnlyDictionary<string, LogEventPropertyValue> properties,
        TextWriter output,
        string? format,
        IFormatProvider? formatProvider);
}
#endif
