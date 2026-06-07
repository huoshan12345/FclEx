namespace FclEx.Utils;

partial class OperationResultExtensions
{
    public static Task<OperationResult<T>> When<T>(this Task<OperationResult<T>> result, Func<T, bool> condition, Func<T, Task> action)
    {
        Check.NotNull(condition);
        Check.NotNull(action);

        return result.WhenResult(r => r.IsSuccess && condition(r.Value), r => action(r.Value!));
    }

    public static Task<OperationResult<T>> When<T>(this Task<OperationResult<T>> result, Func<T, bool> condition, Action<T> action)
    {
        Check.NotNull(action);

        return result.When(condition, v =>
        {
            action(v);
            return Task.CompletedTask;
        });
    }

    public static Task<OperationResult<T>> WhenResult<T>(this Task<OperationResult<T>> result, Func<OperationResult<T>, bool> condition, Func<OperationResult<T>, Task> action)
    {
        Check.NotNull(condition);
        Check.NotNull(action);

        return result.Then(r => condition(r)
            ? action(r).Then(() => r)
            : r);
    }

    public static Task<OperationResult<T>> WhenResult<T>(this Task<OperationResult<T>> result, Func<OperationResult<T>, bool> condition, Action<OperationResult<T>> action)
    {
        Check.NotNull(action);

        return result.WhenResult(condition, r =>
        {
            action(r);
            return Task.CompletedTask;
        });
    }

    public static Task<OperationResult<T>> OnResult<T>(this Task<OperationResult<T>> task, Action<OperationResult<T>> action)
    {
        return task.Then(action);
    }

    public static Task<OperationResult<T>> OnResult<T>(this Task<OperationResult<T>> task, Func<OperationResult<T>, Task> action)
    {
        return task.Then(action);
    }

    public static Task<OperationResult<T>> OnSucceeded<T>(this Task<OperationResult<T>> task, Action<OperationResult<T>> action)
    {
        return task.ThenIf(action, m => m.IsSuccess);
    }

    public static Task<OperationResult<T>> OnSucceeded<T>(this Task<OperationResult<T>> task, Func<OperationResult<T>, Task> action)
    {
        return task.ThenIf(action, m => m.IsSuccess);
    }

    public static Task<OperationResult<T>> OnFailed<T>(this Task<OperationResult<T>> task, Action<OperationResult<T>> action)
    {
        return task.ThenIf(action, m => m.IsError);
    }

    public static Task<OperationResult<T>> OnFailed<T>(this Task<OperationResult<T>> task, Func<OperationResult<T>, Task> action)
    {
        return task.ThenIf(action, m => m.IsError);
    }

    public static Task<OperationResult<T>> OnFaulted<T>(this Task<OperationResult<T>> task, Action<OperationResult<T>> action)
    {
        return task.ThenIf(action, m => m.IsFaulted());
    }

    public static Task<OperationResult<T>> OnFaulted<T>(this Task<OperationResult<T>> task, Func<OperationResult<T>, Task> action)
    {
        return task.ThenIf(action, m => m.IsFaulted());
    }

    public static Task<OperationResult<T>> OnCanceled<T>(this Task<OperationResult<T>> task, Action<OperationResult<T>> action)
    {
        return task.ThenIf(action, m => m.IsCanceled());
    }

    public static Task<OperationResult<T>> OnCanceled<T>(this Task<OperationResult<T>> task, Func<OperationResult<T>, Task> action)
    {
        return task.ThenIf(action, m => m.IsCanceled());
    }

    public static Task<OperationResult<T>> OnValue<T>(this Task<OperationResult<T>> task, Action<T> action)
    {
        return task.OnSucceeded(m => action(m.Value!));
    }

    public static Task<OperationResult<T>> OnValue<T>(this Task<OperationResult<T>> task, Func<T, Task> action)
    {
        return task.OnSucceeded(m => action(m.Value!));
    }

    public static Task<OperationResult<T>> OnValue<T>(this Task<OperationResult<T>> task, Action<T, TimeSpan> action)
    {
        return task.OnSucceeded(m => action(m.Value!, m.Elapsed));
    }

    public static Task<OperationResult<T>> OnValue<T>(this Task<OperationResult<T>> task, Func<T, TimeSpan, Task> action)
    {
        return task.OnSucceeded(m => action(m.Value!, m.Elapsed));
    }

    public static Task<OperationResult<T>> OnException<T>(this Task<OperationResult<T>> task, Action<Exception> action)
    {
        return task.OnFailed(t => action(t.Exception!));
    }

    public static Task<OperationResult<T>> OnException<T>(this Task<OperationResult<T>> task, Action<Exception, TimeSpan> action)
    {
        return task.OnFailed(t => action(t.Exception!, t.Elapsed));
    }

    public static Task<OperationResult<T>> OnException<T>(this Task<OperationResult<T>> task, Func<Exception, TimeSpan, Task> action)
    {
        return task.OnFailed(t => action(t.Exception!, t.Elapsed));
    }

    public static Task<OperationResult<T>> OnException<T>(this Task<OperationResult<T>> task, Func<Exception, Task> action)
    {
        return task.OnFailed(t => action(t.Exception!));
    }

    public static async Task<OperationResult<T>> ThrowIfError<T>(this Task<OperationResult<T>> task)
    {
        var result = await task.ConfigureAwait(false);
        return result.Unwrap();
    }

    public static async Task<OperationResult> WithoutValue<T>(this Task<OperationResult<T>> task)
    {
        var result = await task.ConfigureAwait(false);
        return result.WithoutValue();
    }

    public static Task<OperationResult<TNext>> Then<T, TNext>(this Task<OperationResult<T>> task, Func<T, Task<OperationResult<TNext>>> next)
    {
        return Operation.ExecuteAsync(async () =>
        {
            var result = await task.ConfigureAwait(false);
            if (result.IsError)
                return result.Cast<TNext>();

            var nextResult = await next(result.Value).ConfigureAwait(false);
            return nextResult;
        });
    }

    public static Task<OperationResult<TNext>> Then<T, TNext>(this Task<OperationResult<T>> task, Func<T, OperationResult<TNext>> next)
    {
        return task.Then(m => Task.FromResult(next(m)));
    }

    // this method is sometimes ambiguous with the Task<TNext> Then<T, TNext>(this Task<T> task, Func<T, TNext> next), so we provide MapValue as an alias for it.
    //public static Task<OperationResult<TNext>> Then<T, TNext>(this Task<OperationResult<T>> task, Func<T, TNext> next)
    //{
    //    return task.Then(m => Operation.Success(next(m)));
    //}

    /// <summary>
    /// Alias for Then with <see cref="Func{T, TNext}"/> to avoid ambiguous.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TNext"></typeparam>
    /// <param name="task"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public static Task<OperationResult<TNext>> MapValue<T, TNext>(this Task<OperationResult<T>> task, Func<T, TNext> next)
    {
        return task.Then(m => Operation.Success(next(m)));
    }

    public static Task<OperationResult<TNext>> ThenResult<T, TNext>(this Task<OperationResult<T>> task, Func<OperationResult<T>, Task<OperationResult<TNext>>> next)
    {
        return Operation.ExecuteAsync(async () =>
        {
            var result = await task.ConfigureAwait(false);
            var nextResult = await next(result).ConfigureAwait(false);
            return nextResult;
        });
    }

    public static Task<OperationResult<TNext>> ThenResult<T, TNext>(this Task<OperationResult<T>> task, Func<OperationResult<T>, OperationResult<TNext>> next)
    {
        return task.ThenResult(m => Task.FromResult(next(m)));
    }

    public static Task<OperationResult<TNext>> ThenResult<T, TNext>(this Task<OperationResult<T>> task, Func<OperationResult<T>, TNext> next)
    {
        return task.ThenResult(m => Operation.Success(next(m)));
    }

    public static async Task<T> Unwrap<T>(this Task<OperationResult<T>> task)
    {
        var result = await task.ConfigureAwait(false);
        return result.Unwrap();
    }

    public static async Task<T> Unwrap<T>(this Task<OperationResult<T>> task, T defaultValue)
    {
        var result = await task.ConfigureAwait(false);
        return result.Unwrap(defaultValue);
    }

    public static Task<OperationResult<TNext>> ThenIf<T, TNext>(this Task<OperationResult<T>> task, Func<T, bool> condition,
        Func<T, Task<OperationResult<TNext>>> @true, Func<T, Task<OperationResult<TNext>>> @false)
    {
        Check.NotNull(condition);
        Check.NotNull(@true);
        Check.NotNull(@false);

        return task.Then(t => condition(t) ? @true(t) : @false(t));
    }

    public static Task<OperationResult<T>> ThenIf<T>(this Task<OperationResult<T>> task, Func<T, bool> condition, Func<T, Task<OperationResult<T>>> next)
    {
        return task.ThenIf(condition, next, m => Operation.Success(m));
    }

    public static Task<OperationResult> ThenIf<T>(this Task<OperationResult<T>> task, Func<T, bool> condition, Func<T, Task<OperationResult>> next)
    {
        return task.ThenIf(condition, next, _ => Operation.Success(Unit.Default));
    }

    public static IAction<T[]> ToAction<T>(this IEnumerable<Task<OperationResult<T>>> tasks, bool parallel)
    {
        var actions = tasks.Select(m => Operation.Action(t => m));
        return parallel ? actions.CombineInParallel() : actions.CombineInSeries();
    }

    public static Task<OperationResult<T>> FallBack<T>(this Task<OperationResult<T>> result, Func<OperationResult<T>, Task<OperationResult<T>>> fallback)
    {
        return result.ThenResult(r => r.IsError ? fallback(r) : r);
    }

    public static Task<OperationResult<T>> FallBack<T>(this Task<OperationResult<T>> result, Func<OperationResult<T>, OperationResult<T>> fallback)
    {
        return result.ThenResult(r => r.IsError ? fallback(r) : r);
    }

    public static Task<OperationResult<T>> FallBack<T>(this Task<OperationResult<T>> result, Func<OperationResult<T>, T> fallback)
    {
        return result.ThenResult(r => r.IsError ? fallback(r) : r);
    }

    public static Task<OperationResult<T>> FallBack<T>(this Task<OperationResult<T>> result, T fallback)
    {
        return result.ThenResult(r => r.IsError ? fallback : r);
    }

    public static Task<OperationResult<TNext>> Then<T, TNext>(this Task<OperationResult<T>> task, Func<T, Task<TNext>> next)
    {
        return task.Then<T, TNext>(m => Operation.ExecuteAsync(() => next(m)));
    }

    public static Task<OperationResult<(T, TNext)>> ThenWith<T, TNext>(this Task<OperationResult<T>> task, Func<T, Task<TNext>> next)
    {
        return task.Then<T, (T, TNext)>(m => Operation.ExecuteAsync(() => next(m).Then(x => (m, x))));
    }

    public static Task<OperationResult<(T, TNext)>> ThenWith<T, TNext>(this Task<OperationResult<T>> task, Func<T, Task<OperationResult<TNext>>> next)
    {
        return task.Then<T, (T, TNext)>(m => Operation.ExecuteAsync(() => next(m).MapValue(x => (m, x))));
    }

    public static Task<OperationResult<T[]>> Merge<T, TResults>(this Task<TResults> task) where TResults : IEnumerable<OperationResult<T>>
    {
        return task.Then(m => m.Merge());
    }

    public static Task<OperationResult<T[]>> Merge<T>(this Task<OperationResult<T>[]> task)
    {
        return task.Merge<T, OperationResult<T>[]>();
    }

    public static IAction<T[]> ToAction<T, TResults>(this Task<TResults> task) where TResults : IEnumerable<OperationResult<T>>
    {
        return Operation.Action(t => task.Merge<T, TResults>());
    }

    public static IAction<T[]> ToAction<T>(this Task<OperationResult<T>[]> task)
    {
        return task.ToAction<T, OperationResult<T>[]>();
    }

}
