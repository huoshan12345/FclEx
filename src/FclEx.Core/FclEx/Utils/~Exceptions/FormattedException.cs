namespace FclEx.Utils;

/// <summary>
/// Represents an exception that wraps another exception and provides a formatted string representation.
/// </summary>
public class FormattedException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FormattedException"/> class, wrapping the specified exception.
    /// </summary>
    /// <param name="exception">The exception to wrap.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="exception"/> is null.</exception>
    public FormattedException(Exception exception)
        : base(exception?.Message ?? "", exception ?? throw new ArgumentNullException(nameof(exception)))
    {
    }

    /// <summary>
    /// Gets the inner exception that is wrapped by this <see cref="FormattedException"/>.
    /// </summary>
    public Exception Exception => InnerException!;
    public override string ToString() => InnerException!.ToFormattedString();
}
