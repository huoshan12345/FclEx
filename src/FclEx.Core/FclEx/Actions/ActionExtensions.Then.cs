namespace FclEx.Actions;

partial class ActionExtensions
{
    /// <summary>
    /// Runs the next action after this action succeeds.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="TNext">The next value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="next">Creates the next action from the successful value.</param>
    /// <returns>An action that returns the next action result.</returns>
    /// <remarks>The next action is not created when the source action fails.</remarks>
    public static IAction<TNext> Then<T, TNext>(this IAction<T> action, Func<T, IAction<TNext>> next)
    {
        return new ThenAction<T, TNext>(action, next);
    }

    /// <summary>
    /// Runs the given action after this action succeeds.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="TNext">The next value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="next">The next action to run after success.</param>
    /// <returns>An action that returns the next action result.</returns>
    public static IAction<TNext> Then<T, TNext>(this IAction<T> action, IAction<TNext> next)
    {
        Check.NotNull(next);
        return action.Then(_ => next);
    }

    /// <summary>
    /// Returns the given result after this action succeeds.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="TNext">The next value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="next">The result returned after the source action succeeds.</param>
    /// <returns>An action that returns <paramref name="next"/> after success.</returns>
    public static IAction<TNext> Then<T, TNext>(this IAction<T> action, OperationResult<TNext> next)
    {
        return action.Then(r => Operation.Action(t => next));
    }

    /// <summary>
    /// Runs a result factory after this action succeeds.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="TNext">The next value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="next">Creates the next result from the successful value.</param>
    /// <returns>An action that returns the result produced by <paramref name="next"/>.</returns>
    public static IAction<TNext> Then<T, TNext>(this IAction<T> action, Func<T, OperationResult<TNext>> next)
    {
        Check.NotNull(next);
        return action.Then(r => Operation.Action(t => next(r)));
    }

    /// <summary>
    /// Runs an async value factory after this action succeeds.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="TNext">The next value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="next">Creates the next value asynchronously from the successful value.</param>
    /// <returns>An action that wraps the produced value in a successful result.</returns>
    public static IAction<TNext> Then<T, TNext>(this IAction<T> action, Func<T, Task<TNext>> next)
    {
        Check.NotNull(next);
        return action.Then(r => Operation.Action(t => next(r)));
    }

    /// <summary>
    /// Runs an async result factory after this action succeeds.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="TNext">The next value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="next">Creates the next result asynchronously from the successful value.</param>
    /// <returns>An action that returns the async result produced by <paramref name="next"/>.</returns>
    public static IAction<TNext> Then<T, TNext>(this IAction<T> action, Func<T, Task<OperationResult<TNext>>> next)
    {
        Check.NotNull(next);
        return action.Then(r => Operation.Action(t => next(r)));
    }

    /// <summary>
    /// Runs the next action when the successful value matches the condition.
    /// </summary>
    /// <typeparam name="T">The action value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="condition">The condition evaluated only for successful values.</param>
    /// <param name="next">Creates the next action when the condition matches.</param>
    /// <returns>An action that preserves the successful value when the condition does not match.</returns>
    public static IAction<T> ThenIf<T>(this IAction<T> action, Func<T, bool> condition, Func<T, IAction<T>> next)
    {
        return action.ThenIf(condition, next, m => SuccessAction.Create(m));
    }

    /// <summary>
    /// Chooses the next action based on the successful value.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="TNext">The selected next value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="condition">The condition evaluated only for successful values.</param>
    /// <param name="true">Creates the next action when the condition matches.</param>
    /// <param name="false">Creates the next action when the condition does not match.</param>
    /// <returns>An action that returns the selected next action result.</returns>
    public static IAction<TNext> ThenIf<T, TNext>(this IAction<T> action, Func<T, bool> condition,
        Func<T, IAction<TNext>> @true, Func<T, IAction<TNext>> @false)
    {
        Check.NotNull(condition);
        Check.NotNull(@true);
        Check.NotNull(@false);

        return action.Then(t => condition(t) ? @true(t) : @false(t));
    }

    /// <summary>
    /// Runs the next unit action when the successful value matches the condition.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="condition">The condition evaluated only for successful values.</param>
    /// <param name="next">Creates the unit action when the condition matches.</param>
    /// <returns>An action that returns <see cref="Unit"/>.</returns>
    public static IAction<Unit> ThenIf<T>(this IAction<T> action, Func<T, bool> condition, Func<T, IAction<Unit>> next)
    {
        return action.ThenIf(condition, next, _ => SuccessAction.Create(Unit.Default));
    }

    /// <summary>
    /// Runs an optional next action, preserving the value when no action is returned.
    /// </summary>
    /// <typeparam name="T">The action value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="next">Creates an optional next action from the successful value.</param>
    /// <returns>An action that runs the optional action or preserves the successful value.</returns>
    /// <remarks>A <see langword="null"/> return from <paramref name="next"/> is treated as no-op.</remarks>
    public static IAction<T> ThenOptional<T>(this IAction<T> action, Func<T, IAction<T>?> next)
    {
        Check.NotNull(next);
        return action.Then(m => next(m) ?? SuccessAction.Create(m));
    }

    /// <summary>
    /// Runs a collection of next actions after this action succeeds.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="TNext">The next value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="next">Creates the next actions from the successful value.</param>
    /// <param name="parallel">Whether to run the next actions in parallel.</param>
    /// <returns>An action that returns all next action values.</returns>
    public static IAction<TNext[]> Then<T, TNext>(this IAction<T> action, Func<T, IEnumerable<IAction<TNext>>> next, bool parallel = true)
    {
        Check.NotNull(next);

        return action.Then(m => parallel
            ? next(m).CombineInParallel()
            : next(m).CombineInSeries());
    }

    /// <summary>
    /// Runs a collection of result factories after this action succeeds.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="TNext">The next value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="next">Creates the next results from the successful value.</param>
    /// <param name="parallel">Whether to evaluate the result actions in parallel.</param>
    /// <returns>An action that returns all successful values.</returns>
    public static IAction<TNext[]> Then<T, TNext>(this IAction<T> action, Func<T, IEnumerable<OperationResult<TNext>>> next, bool parallel = true)
    {
        Check.NotNull(next);
        return action.Then(m => next(m).Select(x => Operation.Action(t => x)), parallel);
    }

    /// <summary>
    /// Runs a collection of async results after this action succeeds.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="TNext">The next value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="next">Creates the next async results from the successful value.</param>
    /// <param name="parallel">Whether to wait for the result actions in parallel.</param>
    /// <returns>An action that returns all successful values.</returns>
    public static IAction<TNext[]> Then<T, TNext>(this IAction<T> action, Func<T, IEnumerable<Task<OperationResult<TNext>>>> next, bool parallel = true)
    {
        Check.NotNull(next);
        return action.Then(m => next(m).Select(x => Operation.Action(t => x)), parallel);
    }
}
