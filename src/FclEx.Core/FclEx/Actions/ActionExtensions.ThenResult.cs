namespace FclEx.Actions;

partial class ActionExtensions
{
    public static IAction<TNext> ThenResult<T, TNext>(this IAction<T> action, Func<OperationResult<T>, IAction<TNext>> next)
    {
        Check.NotNull(next);
        return new ThenResultAction<T, TNext>(action, next);
    }

    public static IAction<TNext> ThenResult<T, TNext>(this IAction<T> action, Func<OperationResult<T>, TNext> next)
    {
        return action.ThenResult<T, TNext>(r => Operation.Action(t => next(r)));
    }

    public static IAction<TNext> ThenResult<T, TNext>(this IAction<T> action, Func<OperationResult<T>, OperationResult<TNext>> next)
    {
        return action.ThenResult<T, TNext>(r => Operation.Action(t => next(r)));
    }

    public static IAction<TNext> ThenResult<T, TNext>(this IAction<T> action, Func<OperationResult<T>, Task<TNext>> next)
    {
        return action.ThenResult<T, TNext>(r => Operation.Action(t => next(r)));
    }

    public static IAction<TNext> ThenResult<T, TNext>(this IAction<T> action, Func<OperationResult<T>, Task<OperationResult<TNext>>> next)
    {
        return action.ThenResult<T, TNext>(r => Operation.Action(t => next(r)));
    }

    public static IAction<Unit> ThenResult<T>(this IAction<T> action, Func<OperationResult<T>, Task> next)
    {
        return action.ThenResult<T, Unit>(r => Operation.Action(t => next(r)));
    }

    public static IAction<Unit> ThenResult<T>(this IAction<T> action, Func<OperationResult<T>, Task<OperationResult>> next)
    {
        return action.ThenResult<T, Unit>(r => Operation.Action(t => next(r)));
    }

    public static IAction<Unit> ThenResult<T>(this IAction<T> action, Func<OperationResult<T>, OperationResult> next)
    {
        return action.ThenResult<T, Unit>(r => Operation.Action(t => next(r)));
    }

    public static IAction<Unit> ThenResult<T>(this IAction<T> action, Action<OperationResult<T>> next)
    {
        return action.ThenResult<T, Unit>(r => Operation.Action(t => next(r)));
    }

    public static IAction<T> ThenResultIf<T>(this IAction<T> action, Func<OperationResult<T>, bool> condition,
        Func<OperationResult<T>, IAction<T>> @true, Func<OperationResult<T>, IAction<T>> @false)
    {
        Check.NotNull(condition);
        Check.NotNull(@true);
        Check.NotNull(@false);

        return action.ThenResult(t => condition(t) ? @true(t) : @false(t));
    }

    public static IAction<T> ThenResultIf<T>(this IAction<T> action, Func<OperationResult<T>, bool> condition, Func<OperationResult<T>, IAction<T>> next)
    {
        return action.ThenResultIf<T>(condition, next, m => ResultAction.Create(m.Elapsed(default)));
    }
}
