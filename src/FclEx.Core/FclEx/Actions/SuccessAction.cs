namespace FclEx.Actions;

public static class SuccessAction
{
    /// <summary>
    /// Creates an action that always succeeds with the given value.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The value returned by the action.</param>
    /// <param name="elapsed">The elapsed time assigned to the result.</param>
    /// <returns>An action that always returns a successful result.</returns>
    public static IAction<T> Create<T>(T value, TimeSpan elapsed = default)
    {
        return new SuccessAction<T>(value, elapsed);
    }
}

/// <summary>
/// An action that always returns a successful result.
/// </summary>
/// <typeparam name="T">The value type.</typeparam>
/// <param name="value">The value returned by the action.</param>
/// <param name="elapsed">The elapsed time assigned to the result.</param>
public class SuccessAction<T>(T value, TimeSpan elapsed = default) : IAction<T>
{
    /// <summary>
    /// Returns the configured successful result.
    /// </summary>
    /// <param name="token">Ignored by this action.</param>
    /// <returns>The configured successful result.</returns>
    public Task<OperationResult<T>> ExecuteAsync(CancellationToken token = default)
    {
        return Operation.Success(value, elapsed);
    }
}
