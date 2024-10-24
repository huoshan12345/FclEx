namespace FclEx.Actions;

partial class ActionExtensions
{
    // don't add this method, otherwise there will be a conflict or unexpected method selecting.
    //public static IAction<TNext> Next<T, TNext>(this IAction<T> action, TNext result)
    //{
    //    return action.Next<T, TNext>(Operate.CreateSuccess(result));
    //}

    public static IAction<TNext> Next<T, TNext>(this IAction<T> action, OperateResult<TNext> result)
    {
        return action.Next<T, TNext>(_ => new ResultAction<TNext>(result));
    }

    public static IAction<TNext> Next<T, TNext>(this IAction<T> action, IAction<TNext> next)
    {
        Check.NotNull(next);
        return action.Next(_ => next);
    }

    public static IAction<TNext> Next<T, TNext>(this IAction<T> action, Func<T, IAction<TNext>> next)
    {
        return new NextAction<T, TNext>(action, next);
    }

    public static IAction<TNext> Next<T, TNext>(this IAction<T> action, Func<T, Task<TNext>> next, bool executeSafely = true)
    {
        return action.Next(r => CommonAction.Create(t => next(r), executeSafely));
    }

    public static IAction<TNext> Next<T, TNext>(this IAction<T> action, Func<T, Task<OperateResult<TNext>>> next, bool executeSafely = true)
    {
        return action.Next(r => CommonAction.Create(t => next(r), executeSafely));
    }

    public static IAction<TNext> Next<T1, T2, TNext>(this IAction<(T1, T2)> action, Func<T1, T2, IAction<TNext>> next)
    {
        return action.Next(m => next(m.Item1, m.Item2));
    }

    public static IAction<Unit> Next<T>(this IAction<T> action, Action<T> next, bool executeSafely = true)
    {
        return action.Next(r => CommonAction.Create(t => next(r), executeSafely));
    }

    public static IAction<Unit> Next<T>(this IAction<T> action, Func<T, Task> next, bool executeSafely = true)
    {
        return action.Next(r => CommonAction.Create(t => next(r), executeSafely));
    }

    public static IAction<Unit> Next<T>(this IAction<T> action, Func<T, OperateResult> next, bool executeSafely = true)
    {
        return action.Next(r => CommonAction.Create(t => next(r), executeSafely));
    }

    public static IAction<Unit> Next<T>(this IAction<T> action, Func<T, Task<OperateResult>> next, bool executeSafely = true)
    {
        return action.Next(r => CommonAction.Create(t => next(r), executeSafely));
    }


    public static IAction<T> NextIf<T>(this IAction<T> action, Func<T, bool> condition, Func<T, IAction<T>> next)
    {
        return action.NextIf<T, T>(condition, next, m => new SuccessAction<T>(m));
    }

    public static IAction<TNext> NextIf<T, TNext>(this IAction<T> action, Func<T, bool> condition, Func<T, IAction<TNext>> @true, Func<T, IAction<TNext>> @false)
    {
        Check.NotNull(condition);
        Check.NotNull(@true);
        Check.NotNull(@false);

        return action.Next(t => condition(t) ? @true(t) : @false(t));
    }

    public static IAction<Unit> NextIf<T>(this IAction<T> action, Func<T, bool> condition, Func<T, IAction<Unit>> next)
    {
        Check.NotNull(condition);
        Check.NotNull(next);

        return action.Next(t => condition(t) ? next(t) : new SuccessAction<Unit>(default));
    }


    public static IAction<T> TryNext<T>(this IAction<T> action, Func<T, IAction<T>?> func)
    {
        return action.Next(m => func(m) ?? ResultAction.Create(m));
    }
}