namespace FclEx.Utils;

public static partial class Operation
{
    /// <summary>
    /// Creates a non-generic not-implemented error result.
    /// </summary>
    /// <returns>An error result containing a <see cref="NotImplementedException"/>.</returns>
    public static OperationResult NotImplemented() => NotImplemented<Unit>();

    /// <summary>
    /// Creates a non-generic canceled result from an exception.
    /// </summary>
    /// <param name="exception">The exception describing the cancellation.</param>
    /// <param name="elapsed">The elapsed time to store in the result.</param>
    /// <returns>An error result whose exception is an <see cref="OperationCanceledException"/>.</returns>
    public static OperationResult Cancel(Exception exception, TimeSpan elapsed = default) => Cancel<Unit>(exception, elapsed);

    /// <summary>
    /// Creates a non-generic canceled result.
    /// </summary>
    /// <param name="elapsed">The elapsed time to store in the result.</param>
    /// <returns>An error result whose exception is an <see cref="OperationCanceledException"/>.</returns>
    public static OperationResult Cancel(TimeSpan elapsed = default) => Cancel<Unit>(elapsed);

    /// <summary>
    /// Creates a non-generic success result.
    /// </summary>
    /// <param name="elapsed">The elapsed time to store in the result.</param>
    /// <returns>A success result with a <see cref="Unit"/> value.</returns>
    public static OperationResult Success(TimeSpan elapsed = default) => Success<Unit>(default, elapsed);

    /// <summary>
    /// Creates a non-generic error result.
    /// </summary>
    /// <param name="exception">The exception to store in the result.</param>
    /// <param name="elapsed">The elapsed time to store in the result.</param>
    /// <returns>An error result containing <paramref name="exception"/>.</returns>
    public static OperationResult Error(Exception exception, TimeSpan elapsed = default) => Error<Unit>(exception, elapsed);

    /// <summary>
    /// Creates a non-generic error result from a message.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <param name="elapsed">The elapsed time to store in the result.</param>
    /// <returns>An error result containing a <see cref="SimpleException"/>.</returns>
    public static OperationResult Error(string error, TimeSpan elapsed = default) => Error<Unit>(error, elapsed);
}
