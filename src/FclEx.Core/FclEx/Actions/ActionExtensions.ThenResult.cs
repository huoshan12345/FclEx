namespace FclEx.Actions;

partial class ActionExtensions
{
    /// <summary>
    /// Runs the next action with the full operation result.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="TNext">The next value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="next">Creates the next action from the full operation result.</param>
    /// <returns>An action that returns the next action result.</returns>
    /// <remarks>The next action is created for both success and failure results.</remarks>
    public static IAction<TNext> ThenResult<T, TNext>(this IAction<T> action, Func<OperationResult<T>, IAction<TNext>> next)
    {
        Check.NotNull(next);
        return new ThenResultAction<T, TNext>(action, next);
    }

    /// <summary>
    /// Maps the full operation result to a value.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="TNext">The next value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="next">Creates the next value from the full operation result.</param>
    /// <returns>An action that wraps the produced value in a successful result.</returns>
    public static IAction<TNext> ThenResult<T, TNext>(this IAction<T> action, Func<OperationResult<T>, TNext> next)
    {
        return action.ThenResult<T, TNext>(r => Operation.Action(t => next(r)));
    }

    /// <summary>
    /// Maps the full operation result to another result.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="TNext">The next value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="next">Creates the next result from the full operation result.</param>
    /// <returns>An action that returns the result produced by <paramref name="next"/>.</returns>
    public static IAction<TNext> ThenResult<T, TNext>(this IAction<T> action, Func<OperationResult<T>, OperationResult<TNext>> next)
    {
        return action.ThenResult<T, TNext>(r => Operation.Action(t => next(r)));
    }

    /// <summary>
    /// Maps the full operation result to an async value.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="TNext">The next value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="next">Creates the next value asynchronously from the full operation result.</param>
    /// <returns>An action that wraps the produced value in a successful result.</returns>
    public static IAction<TNext> ThenResult<T, TNext>(this IAction<T> action, Func<OperationResult<T>, Task<TNext>> next)
    {
        return action.ThenResult<T, TNext>(r => Operation.Action(t => next(r)));
    }

    /// <summary>
    /// Maps the full operation result to an async result.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="TNext">The next value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="next">Creates the next result asynchronously from the full operation result.</param>
    /// <returns>An action that returns the async result produced by <paramref name="next"/>.</returns>
    public static IAction<TNext> ThenResult<T, TNext>(this IAction<T> action, Func<OperationResult<T>, Task<OperationResult<TNext>>> next)
    {
        return action.ThenResult<T, TNext>(r => Operation.Action(t => next(r)));
    }

    /// <summary>
    /// Runs an async callback with the full operation result.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="next">The async callback invoked with the full operation result.</param>
    /// <returns>An action that returns <see cref="Unit"/>.</returns>
    public static IAction<Unit> ThenResult<T>(this IAction<T> action, Func<OperationResult<T>, Task> next)
    {
        return action.ThenResult<T, Unit>(r => Operation.Action(t => next(r)));
    }

    /// <summary>
    /// Maps the full operation result to an async unit result.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="next">Creates a unit result asynchronously from the full operation result.</param>
    /// <returns>An action that returns <see cref="Unit"/>.</returns>
    public static IAction<Unit> ThenResult<T>(this IAction<T> action, Func<OperationResult<T>, Task<OperationResult>> next)
    {
        return action.ThenResult<T, Unit>(r => Operation.Action(t => next(r)));
    }

    /// <summary>
    /// Maps the full operation result to a unit result.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="next">Creates a unit result from the full operation result.</param>
    /// <returns>An action that returns <see cref="Unit"/>.</returns>
    public static IAction<Unit> ThenResult<T>(this IAction<T> action, Func<OperationResult<T>, OperationResult> next)
    {
        return action.ThenResult<T, Unit>(r => Operation.Action(t => next(r)));
    }

    /// <summary>
    /// Runs a callback with the full operation result.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="next">The callback invoked with the full operation result.</param>
    /// <returns>An action that returns <see cref="Unit"/>.</returns>
    public static IAction<Unit> ThenResult<T>(this IAction<T> action, Action<OperationResult<T>> next)
    {
        return action.ThenResult<T, Unit>(r => Operation.Action(t => next(r)));
    }

    /// <summary>
    /// Chooses the next action based on the full operation result.
    /// </summary>
    /// <typeparam name="T">The action value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="condition">The condition evaluated against the full operation result.</param>
    /// <param name="true">Creates the next action when the condition matches.</param>
    /// <param name="false">Creates the next action when the condition does not match.</param>
    /// <returns>An action that returns the selected action result.</returns>
    public static IAction<T> ThenResultIf<T>(this IAction<T> action, Func<OperationResult<T>, bool> condition,
        Func<OperationResult<T>, IAction<T>> @true, Func<OperationResult<T>, IAction<T>> @false)
    {
        Check.NotNull(condition);
        Check.NotNull(@true);
        Check.NotNull(@false);

        return action.ThenResult(t => condition(t) ? @true(t) : @false(t));
    }

    /// <summary>
    /// Runs the next action when the full operation result matches the condition.
    /// </summary>
    /// <typeparam name="T">The action value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="condition">The condition evaluated against the full operation result.</param>
    /// <param name="next">Creates the next action when the condition matches.</param>
    /// <returns>An action that preserves the original result when the condition does not match.</returns>
    public static IAction<T> ThenResultIf<T>(this IAction<T> action, Func<OperationResult<T>, bool> condition, Func<OperationResult<T>, IAction<T>> next)
    {
        return action.ThenResultIf<T>(condition, next, m => ResultAction.Create(m.Elapsed(default)));
    }
}
