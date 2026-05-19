namespace FclEx.Actions;

public static class ResultAction
{
    /// <summary>
    /// Creates an action from an existing result.
    /// </summary>
    /// <typeparam name="T">The result value type.</typeparam>
    /// <param name="result">The result returned by the action.</param>
    /// <returns>An action that returns <paramref name="result"/>.</returns>
    public static ResultAction<T> Create<T>(OperationResult<T> result) => new(result);
}

/// <summary>
/// An action that always returns a configured result.
/// </summary>
/// <typeparam name="T">The result value type.</typeparam>
/// <param name="result">The result returned by the action.</param>
public class ResultAction<T>(OperationResult<T> result) : IAction<T>
{
    /// <summary>
    /// Returns the configured result.
    /// </summary>
    /// <param name="token">Ignored by this action.</param>
    /// <returns>The configured result.</returns>
    public Task<OperationResult<T>> ExecuteAsync(CancellationToken token = default)
    {
        return result;
    }
}
