namespace FclEx.Serilog;

/// <summary>Connects a text writer to Serilog's process-wide diagnostic output.</summary>
public static class TextWriterExtensions
{
    /// <summary>Routes SelfLog diagnostics to a writer until the returned handle is disposed.</summary>
    /// <param name="tw">The caller-owned diagnostic writer; use a synchronized writer when diagnostics can be concurrent.</param>
    /// <returns>A handle that disables SelfLog without disposing the writer.</returns>
    /// <remarks>SelfLog is global. This replaces the previous destination, and disposal disables the current destination rather than restoring an earlier one.</remarks>
    public static IDisposable SetSelfLog(this TextWriter tw)
    {
        SelfLog.Enable(tw);
        return Disposable.Create(SelfLog.Disable);
    }
}
