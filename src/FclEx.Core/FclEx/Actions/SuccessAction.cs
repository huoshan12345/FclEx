namespace FclEx.Actions;

public static class SuccessAction
{
    /// <summary>
    /// Creates an action that always succeeds with the given value.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The value returned by the action, which cannot be <see langword="null"/>.</param>
    /// <param name="elapsed">The elapsed time assigned to the result.</param>
    /// <returns>An action that always returns a successful result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static IAction<T> Create<T>(T value, TimeSpan elapsed = default)
    {
        return new SuccessAction<T>(value, elapsed);
    }
}

/// <summary>
/// An action that always returns a successful result.
/// </summary>
/// <typeparam name="T">The value type.</typeparam>
public class SuccessAction<T> : IAction<T>
{
    private readonly T _value;
    private readonly TimeSpan _elapsed;

    /// <summary>
    /// Initializes an action that always succeeds with a non-null value.
    /// </summary>
    /// <param name="value">The value returned by the action, which cannot be <see langword="null"/>.</param>
    /// <param name="elapsed">The elapsed time assigned to the result.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public SuccessAction(T value, TimeSpan elapsed = default)
    {
        _value = Check.NotNull(value);
        _elapsed = elapsed;
    }

    /// <summary>
    /// Returns the configured successful result.
    /// </summary>
    /// <param name="token">Ignored by this action.</param>
    /// <returns>The configured successful result.</returns>
    public Task<OperationResult<T>> ExecuteAsync(CancellationToken token = default)
    {
        return Operation.Success(_value, _elapsed);
    }
}
