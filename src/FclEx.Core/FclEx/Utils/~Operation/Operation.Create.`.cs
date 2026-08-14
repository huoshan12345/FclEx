namespace FclEx.Utils;

public static partial class Operation
{
    /// <summary>
    /// Creates a typed not-implemented error result.
    /// </summary>
    /// <typeparam name="T">The result value type.</typeparam>
    /// <returns>An error result containing a <see cref="NotImplementedException"/>.</returns>
    public static OperationResult<T> NotImplemented<T>() => Error<T>(new NotImplementedException().SetStackTrace());

    /// <summary>
    /// Creates a typed canceled result from an exception.
    /// </summary>
    /// <typeparam name="T">The result value type.</typeparam>
    /// <param name="exception">The exception describing the cancellation.</param>
    /// <param name="elapsed">The elapsed time to store in the result.</param>
    /// <returns>An error result whose exception is an <see cref="OperationCanceledException"/>. Non-cancellation exceptions are wrapped as the inner exception.</returns>
    public static OperationResult<T> Cancel<T>(Exception exception, TimeSpan elapsed = default)
    {
        Check.NotNull(exception);
        return Error<T>(exception is OperationCanceledException ? exception : new OperationCanceledException(exception.Message, exception).SetStackTrace(), elapsed);
    }

    /// <summary>
    /// Creates a typed canceled result.
    /// </summary>
    /// <typeparam name="T">The result value type.</typeparam>
    /// <param name="elapsed">The elapsed time to store in the result.</param>
    /// <returns>An error result whose exception is an <see cref="OperationCanceledException"/>.</returns>
    public static OperationResult<T> Cancel<T>(TimeSpan elapsed = default) => Error<T>(new OperationCanceledException().SetStackTrace(), elapsed);

    /// <summary>
    /// Creates a typed success result.
    /// </summary>
    /// <typeparam name="T">The result value type.</typeparam>
    /// <param name="value">The success value, which cannot be <see langword="null"/>.</param>
    /// <param name="elapsed">The elapsed time to store in the result.</param>
    /// <returns>A success result containing <paramref name="value"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static OperationResult<T> Success<T>(T value, TimeSpan elapsed = default) => OperationResult<T>.FromSuccess(value, elapsed);

    /// <summary>
    /// Creates a typed error result.
    /// </summary>
    /// <typeparam name="T">The result value type.</typeparam>
    /// <param name="exception">The exception to store in the result.</param>
    /// <param name="elapsed">The elapsed time to store in the result.</param>
    /// <returns>An error result containing <paramref name="exception"/>.</returns>
    public static OperationResult<T> Error<T>(Exception exception, TimeSpan elapsed = default)
    {
        Check.NotNull(exception);
        return new(exception, elapsed);
    }

    /// <summary>
    /// Creates a typed error result from a message.
    /// </summary>
    /// <typeparam name="T">The result value type.</typeparam>
    /// <param name="error">The error message.</param>
    /// <param name="elapsed">The elapsed time to store in the result.</param>
    /// <returns>An error result containing a <see cref="SimpleException"/>.</returns>
    public static OperationResult<T> Error<T>(string error, TimeSpan elapsed = default) => Error<T>(new SimpleException(Check.NotNull(error)), elapsed);

    /// <summary>
    /// Creates an error result whose exception carries the input object that caused the error.
    /// </summary>
    /// <typeparam name="T">The input object type and result value type.</typeparam>
    /// <param name="value">The object associated with the error.</param>
    /// <param name="error">The error message.</param>
    /// <param name="elapsed">The elapsed time to store in the result.</param>
    /// <returns>An error result containing an object-associated exception.</returns>
    public static OperationResult<T> ObjectError<T>(T value, string error, TimeSpan elapsed = default) where T : notnull
    {
        Check.NotNull(value);
        Check.NotNull(error);

        return new(ObjectException.Create(value, error).SetStackTrace(), elapsed);
    }

    /// <summary>
    /// Creates an error result whose exception carries the input object that caused the error.
    /// </summary>
    /// <typeparam name="T">The input object type and result value type.</typeparam>
    /// <param name="value">The object associated with the error.</param>
    /// <param name="exception">The exception associated with the object.</param>
    /// <param name="elapsed">The elapsed time to store in the result.</param>
    /// <returns>An error result containing an object-associated exception.</returns>
    public static OperationResult<T> ObjectError<T>(T value, Exception exception, TimeSpan elapsed = default) where T : notnull
    {
        Check.NotNull(value);
        Check.NotNull(exception);

        return new(ObjectException.Create(value, exception.Message, exception).SetStackTrace(), elapsed);
    }

    /// <summary>
    /// Creates an error result whose exception carries the input object that caused the error while returning a different result type.
    /// </summary>
    /// <typeparam name="T">The input object type.</typeparam>
    /// <typeparam name="TResult">The result value type.</typeparam>
    /// <param name="value">The object associated with the error.</param>
    /// <param name="exception">The exception associated with the object.</param>
    /// <param name="elapsed">The elapsed time to store in the result.</param>
    /// <returns>An error result containing an object-associated exception.</returns>
    public static OperationResult<TResult> ObjectError<T, TResult>(T value, Exception exception, TimeSpan elapsed = default) where T : notnull
    {
        Check.NotNull(value);
        Check.NotNull(exception);

        var objectException = ObjectException.Create(value, exception.Message, exception).SetStackTrace();
        return new(objectException, elapsed);
    }
}
