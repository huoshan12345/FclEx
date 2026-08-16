namespace FclEx.Utils;

/// <summary>
/// Represents an error carried primarily as a string message, typically when an API such as
/// <see cref="OperationResult{T}"/> needs to expose textual failure information as an exception.
/// </summary>
public class SimpleException : Exception
{
    private readonly bool _noStackTrace;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleException"/> class with the specified error message.
    /// </summary>
    /// <param name="msg">The error message that explains the reason for the exception.</param>
    /// <param name="noStackTrace">
    /// <see langword="true"/> to hide the stack trace and use message-only formatting when possible;
    /// otherwise, <see langword="false"/>. This controls the exception's presentation, not whether
    /// the runtime records throw information.
    /// </param>
    [StackTraceHidden]
    public SimpleException(string? msg, bool noStackTrace = true) : this(msg, null, noStackTrace)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleException"/> class with the specified error message and inner exception.
    /// </summary>
    /// <param name="msg">The error message that explains the reason for the exception.</param>
    /// <param name="inner">The exception that is the cause of the current exception.</param>
    /// <param name="noStackTrace">
    /// <see langword="true"/> to hide the stack trace and use message-only formatting when possible;
    /// otherwise, <see langword="false"/>. This controls the exception's presentation, not whether
    /// the runtime records throw information.
    /// </param>
    [StackTraceHidden]
    public SimpleException(string? msg, Exception? inner, bool noStackTrace = true) : base(msg, inner)
    {
        _noStackTrace = noStackTrace;

        if (noStackTrace == false)
            StackTrace = new StackTrace(true).ToString();
    }

    /// <summary>
    /// Gets the stack trace exposed by this exception, or <see langword="null"/> when it was configured
    /// for message-only presentation.
    /// </summary>
    public override string? StackTrace => _noStackTrace ? null : base.StackTrace ?? field;

    /// <summary>
    /// Returns the message alone when the exception contains no additional diagnostic information;
    /// otherwise, returns the standard exception representation.
    /// </summary>
    /// <returns>A string representation of the exception.</returns>
    public override string ToString()
    {
        return this.IsJustMessage()
            ? Message.IfEmpty(nameof(SimpleException))
            : base.ToString();
    }
}
