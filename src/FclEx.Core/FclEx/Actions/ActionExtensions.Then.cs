namespace FclEx.Actions;

partial class ActionExtensions
{
    public static IAction<TNext> Then<T, TNext>(this IAction<T> action, Func<T, IAction<TNext>> next)
    {
        return new ThenAction<T, TNext>(action, next);
    }

    public static IAction<TNext> Then<T, TNext>(this IAction<T> action, IAction<TNext> next)
    {
        Check.NotNull(next);
        return action.Then(_ => next);
    }

    public static IAction<TNext> Then<T, TNext>(this IAction<T> action, OperationResult<TNext> next)
    {
        return action.Then(r => Operation.Action(t => next));
    }

    public static IAction<TNext> Then<T, TNext>(this IAction<T> action, Func<T, OperationResult<TNext>> next)
    {
        return action.Then(r => Operation.Action(t => next(r)));
    }

    public static IAction<TNext> Then<T, TNext>(this IAction<T> action, Func<T, Task<TNext>> next)
    {
        return action.Then(r => Operation.Action(t => next(r)));
    }

    public static IAction<TNext> Then<T, TNext>(this IAction<T> action, Func<T, Task<OperationResult<TNext>>> next)
    {
        return action.Then(r => Operation.Action(t => next(r)));
    }

    public static IAction<Unit> Then<T>(this IAction<T> action, Action<T> next)
    {
        return action.Then(r => Operation.Action(t => next(r)));
    }

    public static IAction<Unit> Then<T>(this IAction<T> action, Func<T, Task> next)
    {
        return action.Then(r => Operation.Action(t => next(r)));
    }

    public static IAction<T> ThenIf<T>(this IAction<T> action, Func<T, bool> condition, Func<T, IAction<T>> next)
    {
        return action.ThenIf(condition, next, m => SuccessAction.Create(m));
    }

    public static IAction<TNext> ThenIf<T, TNext>(this IAction<T> action, Func<T, bool> condition,
        Func<T, IAction<TNext>> @true, Func<T, IAction<TNext>> @false)
    {
        Check.NotNull(condition);
        Check.NotNull(@true);
        Check.NotNull(@false);

        return action.Then(t => condition(t) ? @true(t) : @false(t));
    }

    public static IAction<T> ThenTry<T>(this IAction<T> action, Func<T, IAction<T>?> func)
    {
        return action.Then(m => func(m) ?? SuccessAction.Create(m));
    }
}