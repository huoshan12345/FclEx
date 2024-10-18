namespace FclEx.Actions;

partial class ActionExtensions
{
    public static IAction<TNext> NextResult<T, TNext>(this IAction<T> action, Func<OperateResult<T>, TNext> next, bool executeSafely = true)
    {
        return action.NextResult<T, TNext>(r => CommonAction.Create(t => next(r), executeSafely));
    }

    public static IAction<TNext> NextResult<T, TNext>(this IAction<T> action, Func<OperateResult<T>, OperateResult<TNext>> next, bool executeSafely = true)
    {
        return action.NextResult<T, TNext>(r => CommonAction.Create(t => next(r), executeSafely));
    }
        
    public static IAction<TNext> NextResult<T, TNext>(this IAction<T> action, Func<OperateResult<T>, IAction<TNext>> next)
    {
        Check.NotNull(next);
        return new NextResultAction<T, TNext>(action, next);
    }
        
    public static IAction<TNext> NextResult<T, TNext>(this IAction<T> action, Func<OperateResult<T>, Task<TNext>> next, bool executeSafely = true)
    {
        return action.NextResult<T, TNext>(r => CommonAction.Create(t => next(r), executeSafely));
    }

    public static IAction<TNext> NextResult<T, TNext>(this IAction<T> action, Func<OperateResult<T>, Task<OperateResult<TNext>>> next, bool executeSafely = true)
    {
        return action.NextResult<T, TNext>(r => CommonAction.Create(t => next(r), executeSafely));
    }
        

    public static IAction<Unit> NextResult<T>(this IAction<T> action, Func<OperateResult<T>, Task> next, bool executeSafely = true)
    {
        return action.NextResult<T, Unit>(r => CommonAction.Create(t => next(r), executeSafely));
    }

    public static IAction<Unit> NextResult<T>(this IAction<T> action, Func<OperateResult<T>, Task<OperateResult>> next, bool executeSafely = true)
    {
        return action.NextResult<T, Unit>(r => CommonAction.Create(t => next(r), executeSafely));
    }

    public static IAction<Unit> NextResult<T>(this IAction<T> action, Func<OperateResult<T>, OperateResult> next, bool executeSafely = true)
    {
        return action.NextResult<T, Unit>(r => CommonAction.Create(t => next(r), executeSafely));
    }

    public static IAction<Unit> NextResult<T>(this IAction<T> action, Action<OperateResult<T>> next, bool executeSafely = true)
    {
        return action.NextResult<T, Unit>(r => CommonAction.Create(t => next(r), executeSafely));
    }

    public static IAction<T> NextResultIf<T>(this IAction<T> action, Func<OperateResult<T>, bool> condition,
        Func<OperateResult<T>, IAction<T>> @true, Func<OperateResult<T>, IAction<T>> @false)
    {
        Check.NotNull(condition);
        Check.NotNull(@true);
        Check.NotNull(@false);

        return action.NextResult(t => condition(t) ? @true(t) : @false(t));
    }

    public static IAction<T> NextResultIf<T>(this IAction<T> action, Func<OperateResult<T>, bool> condition, Func<OperateResult<T>, IAction<T>> next)
    {
        return action.NextResultIf<T>(condition, next, m => ResultAction.Create(m));
    }
}