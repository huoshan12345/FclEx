namespace FclEx.Utils;

/// <summary>
/// Represents the result of an operation, including a success/failure indicator,
/// an optional value, an optional exception, and the elapsed time.
/// </summary>
public interface IOperationResult
{
    /// <summary>
    /// Gets the code associated with the operation result. Typically used for error codes.
    /// </summary>
    public int Code { get; }

    /// <summary>
    /// Gets the exception that occurred during the operation, if any.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Gets the time elapsed during the operation.
    /// </summary>
    public TimeSpan Elapsed { get; }

    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Exception))]
    public bool Success { get; }

    /// <summary>
    /// Gets a value indicating whether the operation resulted in an error.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Exception))]
    public bool Error { get; }
}

/// <summary>
/// Represents the result of an operation, including a success/failure indicator,
/// an optional value, an optional exception, and the elapsed time.
/// </summary>
/// <typeparam name="T">The type of the value returned by the operation.</typeparam>
public interface IOperationResult<out T> : IOperationResult
{
    /// <summary>
    /// Gets the value returned by the operation, if successful.
    /// </summary>
    public T? Value { get; }
}