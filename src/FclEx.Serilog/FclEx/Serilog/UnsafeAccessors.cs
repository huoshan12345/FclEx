namespace FclEx.Serilog;

#if NET8_0_OR_GREATER
/// <summary>Provides private Serilog field references on .NET 8 and later and the internal message renderer on .NET 10 and later.</summary>
/// <remarks>
/// These methods depend on the exact Serilog implementation. Writable references bypass validation and
/// synchronization; complete mutations before sharing events with concurrent sinks. Missing fields or methods
/// can cause MissingFieldException or MissingMethodException with incompatible Serilog or runtime versions.
/// </remarks>
public static class UnsafeAccessors
{
    /// <summary>Accesses the backing storage for MessageTemplate.Text.</summary>
    /// <param name="obj">The template whose text storage is accessed.</param>
    /// <returns>A writable reference to the original template text; assignment does not reparse or replace its tokens.</returns>
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = $"<{nameof(MessageTemplate.Text)}>k__BackingField")]
    public static extern ref string MessageTemplate_Text(MessageTemplate obj);

    /// <summary>Accesses the backing storage for LogEvent.Level.</summary>
    /// <param name="obj">The event to inspect or mutate.</param>
    /// <returns>A writable reference to the level; assignment does not rerun logging filters.</returns>
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = $"<{nameof(LogEvent.Level)}>k__BackingField")]
    public static extern ref LogEventLevel LogEvent_Level(LogEvent obj);

    /// <summary>Accesses the backing storage for LogEvent.Exception.</summary>
    /// <param name="obj">The event to inspect or mutate.</param>
    /// <returns>A writable reference to the exception, including null when no exception is attached.</returns>
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = $"<{nameof(LogEvent.Exception)}>k__BackingField")]
    public static extern ref Exception? LogEvent_Exception(LogEvent obj);

    /// <summary>Accesses the backing storage for LogEvent.MessageTemplate.</summary>
    /// <param name="obj">The event to inspect or mutate.</param>
    /// <returns>A writable reference to the template; assignment retains the existing structured properties.</returns>
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = $"<{nameof(LogEvent.MessageTemplate)}>k__BackingField")]
    public static extern ref MessageTemplate LogEvent_MessageTemplate(LogEvent obj);

    /// <summary>Returns a writable reference to the internal WriteTo sink list.</summary>
    /// <param name="obj">The configuration to inspect before creating a logger.</param>
    /// <returns>A writable reference to the live WriteTo sink list, excluding AuditTo sinks. Removed sinks are not disposed.</returns>
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_logEventSinks")]
    public static extern ref List<ILogEventSink> LoggerConfiguration_Sinks(LoggerConfiguration obj);

#if NET10_0_OR_GREATER
    /// <summary>Writes a message using Serilog's internal message-token renderer without appending a newline.</summary>
    /// <param name="target">Pass null; this parameter identifies the declaring type of the static method.</param>
    /// <param name="messageTemplate">The parsed template to render.</param>
    /// <param name="properties">The structured values substituted for template tokens.</param>
    /// <param name="output">The destination writer; it is not disposed by this method.</param>
    /// <param name="format">Message-token formatting such as l or j, or null for the default.</param>
    /// <param name="formatProvider">The value-formatting provider, or null for Serilog's default.</param>
    /// <remarks>Only supported on .NET 10 and later because .NET 8/9 do not support UnsafeAccessorType.</remarks>
    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "Render")]
    public static extern void MessageTemplateRenderer_Render(
        [UnsafeAccessorType("Serilog.Rendering.MessageTemplateRenderer, Serilog")] object? target,
        MessageTemplate messageTemplate,
        IReadOnlyDictionary<string, LogEventPropertyValue> properties,
        TextWriter output,
        string? format,
        IFormatProvider? formatProvider);
#endif
}
#endif
