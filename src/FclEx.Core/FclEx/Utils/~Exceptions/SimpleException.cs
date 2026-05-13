namespace FclEx.Utils;

/// <summary>
/// A simplified exception class that optionally omits stack trace information for performance
/// and provides cleaner error message output.
/// </summary>
public class SimpleException : Exception
{
    private readonly bool _noStackTrace;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleException"/> class with the specified error message.
    /// </summary>
    /// <param name="msg">The error message that explains the reason for the exception.</param>
    /// <param name="noStackTrace">If true, the stack trace will be omitted from the exception.</param>
    [StackTraceHidden]
    public SimpleException(string? msg, bool noStackTrace = true) : this(msg, null, noStackTrace)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleException"/> class with the specified error message and inner exception.
    /// </summary>
    /// <param name="msg">The error message that explains the reason for the exception.</param>
    /// <param name="inner">The exception that is the cause of the current exception.</param>
    /// <param name="noStackTrace">If true, the stack trace will be omitted from the exception.</param>
    [StackTraceHidden]
    public SimpleException(string? msg, Exception? inner, bool noStackTrace = true) : base(msg, inner)
    {
        _noStackTrace = noStackTrace;

        if (noStackTrace == false)
            StackTrace = new StackTrace(true).ToString();
    }

    /// <summary>
    /// Gets the stack trace string for this exception.
    /// Returns null if the exception was created with noStackTrace set to true.
    /// </summary>
    public override string? StackTrace => _noStackTrace ? null : base.StackTrace ?? field;

    /// <summary>
    /// Returns a string representation of the exception.
    /// If the exception is just a message (has no inner exception or stack trace),
    /// only the Message property is returned.
    /// </summary>
    /// <returns>A string representation of the exception.</returns>
    public override string ToString()
    {
        return this.IsJustMessage()
            ? Message.IfEmpty(nameof(SimpleException))
            : base.ToString();
    }
}