namespace FclEx.Actions;

partial class ActionExtensions
{
    /// <summary>
    /// Runs the next action and returns both successful values.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="TNext">The next value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="next">Creates the next action from the successful value.</param>
    /// <returns>An action whose successful value contains both source and next values.</returns>
    /// <remarks>The next action is not created when the source action fails.</remarks>
    public static IAction<(T Cur, TNext Next)> ThenWith<T, TNext>(this IAction<T> action, Func<T, IAction<TNext>> next)
    {
        return new ThenWithAction<T, TNext>(action, next);
    }

    /// <summary>
    /// Runs the next result factory and returns both successful values.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="TNext">The next value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="next">Creates the next result from the successful value.</param>
    /// <returns>An action whose successful value contains both source and next values.</returns>
    public static IAction<(T Cur, TNext Next)> ThenWith<T, TNext>(this IAction<T> action, Func<T, OperationResult<TNext>> next)
    {
        return new ThenWithAction<T, TNext>(action, m => Operation.Action(t => next(m)));
    }

    /// <summary>
    /// Runs the next value factory and returns both successful values.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="TNext">The next value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="next">A function that produces the next value from the source value.</param>
    /// <returns>An action whose successful value contains both source and next values.</returns>
    public static IAction<(T Cur, TNext Next)> ThenWith<T, TNext>(this IAction<T> action, Func<T, TNext> next)
    {
        return action.Then<T, (T, TNext)>(m => Operation.Success((m, next(m))));
    }

    /// <summary>
    /// Runs the next action for a pair and returns all successful values.
    /// </summary>
    /// <typeparam name="T1">The first source value type.</typeparam>
    /// <typeparam name="T2">The second source value type.</typeparam>
    /// <typeparam name="TNext">The next value type.</typeparam>
    /// <param name="action">The source pair action.</param>
    /// <param name="next">Creates the next action from both successful source values.</param>
    /// <returns>An action whose successful value contains all three values.</returns>
    public static IAction<(T1, T2, TNext)> ThenWith<T1, T2, TNext>(this IAction<(T1, T2)> action, Func<T1, T2, IAction<TNext>> next)
    {
        return new ThenWithAction<(T1, T2), TNext>(action, m => next(m.Item1, m.Item2)).MapValue(m => (m.Item1.Item1, m.Item1.Item2, m.Item2));
    }
}
