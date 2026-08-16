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

    public override string ToString() => InnerException!.ToFormattedString();
}
