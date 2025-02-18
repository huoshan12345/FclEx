namespace FclEx.Actions;

partial class ActionExtensions
{
    public static IAction<TNext> NextResult<T, TNext>(this IAction<T> action, Func<OperationResult<T>, TNext> next)
    {
        return action.NextResult<T, TNext>(r => Operation.Action(t => next(r)));
    }

    public static IAction<TNext> NextResult<T, TNext>(this IAction<T> action, Func<OperationResult<T>, OperationResult<TNext>> next)
    {
        return action.NextResult<T, TNext>(r => Operation.Action(t => next(r)));
    }
        
    public static IAction<TNext> NextResult<T, TNext>(this IAction<T> action, Func<OperationResult<T>, IAction<TNext>> next)
    {
        Check.NotNull(next);
        return new NextResultAction<T, TNext>(action, next);
    }
        
    public static IAction<TNext> NextResult<T, TNext>(this IAction<T> action, Func<OperationResult<T>, Task<TNext>> next)
    {
        return action.NextResult<T, TNext>(r => Operation.Action(t => next(r)));
    }

    public static IAction<TNext> NextResult<T, TNext>(this IAction<T> action, Func<OperationResult<T>, Task<OperationResult<TNext>>> next)
    {
        return action.NextResult<T, TNext>(r => Operation.Action(t => next(r)));
    }
        

    public static IAction<Unit> NextResult<T>(this IAction<T> action, Func<OperationResult<T>, Task> next)
    {
        return action.NextResult<T, Unit>(r => Operation.Action(t => next(r)));
    }

    public static IAction<Unit> NextResult<T>(this IAction<T> action, Func<OperationResult<T>, Task<OperationResult>> next)
    {
        return action.NextResult<T, Unit>(r => Operation.Action(t => next(r)));
    }

    public static IAction<Unit> NextResult<T>(this IAction<T> action, Func<OperationResult<T>, OperationResult> next)
    {
        return action.NextResult<T, Unit>(r => Operation.Action(t => next(r)));
    }

    public static IAction<Unit> NextResult<T>(this IAction<T> action, Action<OperationResult<T>> next)
    {
        return action.NextResult<T, Unit>(r => Operation.Action(t => next(r)));
    }

    public static IAction<T> NextResultIf<T>(this IAction<T> action, Func<OperationResult<T>, bool> condition,
        Func<OperationResult<T>, IAction<T>> @true, Func<OperationResult<T>, IAction<T>> @false)
    {
        Check.NotNull(condition);
        Check.NotNull(@true);
        Check.NotNull(@false);

        return action.NextResult(t => condition(t) ? @true(t) : @false(t));
    }

    public static IAction<T> NextResultIf<T>(this IAction<T> action, Func<OperationResult<T>, bool> condition, Func<OperationResult<T>, IAction<T>> next)
    {
        return action.NextResultIf<T>(condition, next, m => ResultAction.Create(m));
    }
}