namespace FclEx.Actions;

public static class ErrorAction
{
    /// <summary>
    /// Creates an action that always fails with the given message.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="error">The failure message.</param>
    /// <param name="elapsed">The elapsed time assigned to the result.</param>
    /// <returns>An action that always returns an error result.</returns>
    public static IAction<T> Create<T>(string error, TimeSpan elapsed = default)
    {
        return new ErrorAction<T>(error, elapsed);
    }

    /// <summary>
    /// Creates an action that always fails with the given exception.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="exception">The exception returned by the action.</param>
    /// <param name="elapsed">The elapsed time assigned to the result.</param>
    /// <returns>An action that always returns an error result.</returns>
    public static IAction<T> Create<T>(Exception exception, TimeSpan elapsed = default)
    {
        return new ErrorAction<T>(exception, elapsed);
    }
}

public class ErrorAction<T> : IAction<T>
{
    private readonly OperationResult<T> _result;

    /// <summary>
    /// Creates an action that always fails with the given message.
    /// </summary>
    /// <param name="error">The failure message.</param>
    /// <param name="elapsed">The elapsed time assigned to the result.</param>
    public ErrorAction(string error, TimeSpan elapsed = default)
    {
        _result = Operation.Error<T>(error, elapsed);
    }

    /// <summary>
    /// Creates an action that always fails with the given exception.
    /// </summary>
    /// <param name="exception">The exception returned by the action.</param>
    /// <param name="elapsed">The elapsed time assigned to the result.</param>
    public ErrorAction(Exception exception, TimeSpan elapsed = default)
    {
        _result = Operation.Error<T>(exception, elapsed);
    }

    /// <summary>
    /// Returns the configured error result.
    /// </summary>
    /// <param name="token">Ignored by this action.</param>
    /// <returns>The configured error result.</returns>
    public Task<OperationResult<T>> ExecuteAsync(CancellationToken token = default)
    {
        return _result;
    }
}
