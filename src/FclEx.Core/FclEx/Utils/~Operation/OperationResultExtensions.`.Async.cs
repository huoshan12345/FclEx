namespace FclEx.Utils;

public static partial class OperationResultExtensions
{
    public static Task<OperationResult<T>> When<T>(this Task<OperationResult<T>> result, Func<T, bool> condition, Func<T, Task> action)
    {
        Check.NotNull(condition);
        Check.NotNull(action);

        return result.WhenResult(r => r.IsSuccess && condition(r.Value), r => action(r.Value!));
    }

    public static Task<OperationResult<T>> When<T>(this Task<OperationResult<T>> result, Func<T, bool> condition, Action<T> action)
    {
        Check.NotNull(condition);
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
        Check.NotNull(condition);
        Check.NotNull(action);

        return result.WhenResult(condition, r =>
        {
            action(r);
            return Task.CompletedTask;
        });
    }

    public static Task<OperationResult<T>> OnResult<T>(this Task<OperationResult<T>> task, Action<OperationResult<T>> action)
    {
        Check.NotNull(action);

        return task.Then(action);
    }

    public static Task<OperationResult<T>> OnResult<T>(this Task<OperationResult<T>> task, Func<OperationResult<T>, Task> action)
    {
        Check.NotNull(action);

        return task.Then(action);
    }

    public static Task<OperationResult<T>> OnSucceeded<T>(this Task<OperationResult<T>> task, Action<OperationResult<T>> action)
    {
        Check.NotNull(action);

        return task.ThenIf(action, m => m.IsSuccess);
    }

    public static Task<OperationResult<T>> OnSucceeded<T>(this Task<OperationResult<T>> task, Func<OperationResult<T>, Task> action)
    {
        Check.NotNull(action);

        return task.ThenIf(action, m => m.IsSuccess);
    }

    public static Task<OperationResult<T>> OnFailed<T>(this Task<OperationResult<T>> task, Action<OperationResult<T>> action)
    {
        Check.NotNull(action);

        return task.ThenIf(action, m => m.IsError);
    }

    public static Task<OperationResult<T>> OnFailed<T>(this Task<OperationResult<T>> task, Func<OperationResult<T>, Task> action)
    {
        Check.NotNull(action);

        return task.ThenIf(action, m => m.IsError);
    }

    public static Task<OperationResult<T>> OnFaulted<T>(this Task<OperationResult<T>> task, Action<OperationResult<T>> action)
    {
        Check.NotNull(action);

        return task.ThenIf(action, m => m.IsFaulted());
    }

    public static Task<OperationResult<T>> OnFaulted<T>(this Task<OperationResult<T>> task, Func<OperationResult<T>, Task> action)
    {
        Check.NotNull(action);

        return task.ThenIf(action, m => m.IsFaulted());
    }

    public static Task<OperationResult<T>> OnCanceled<T>(this Task<OperationResult<T>> task, Action<OperationResult<T>> action)
    {
        Check.NotNull(action);

        return task.ThenIf(action, m => m.IsCanceled());
    }

    public static Task<OperationResult<T>> OnCanceled<T>(this Task<OperationResult<T>> task, Func<OperationResult<T>, Task> action)
    {
        Check.NotNull(action);

        return task.ThenIf(action, m => m.IsCanceled());
    }

    public static Task<OperationResult<T>> OnValue<T>(this Task<OperationResult<T>> task, Action<T> action)
    {
        Check.NotNull(action);

        return task.OnSucceeded(m => action(m.Value!));
    }

    public static Task<OperationResult<T>> OnValue<T>(this Task<OperationResult<T>> task, Func<T, Task> action)
    {
        Check.NotNull(action);

        return task.OnSucceeded(m => action(m.Value!));
    }

    public static Task<OperationResult<T>> OnValue<T>(this Task<OperationResult<T>> task, Action<T, TimeSpan> action)
    {
        Check.NotNull(action);

        return task.OnSucceeded(m => action(m.Value!, m.Elapsed));
    }

    public static Task<OperationResult<T>> OnValue<T>(this Task<OperationResult<T>> task, Func<T, TimeSpan, Task> action)
    {
        Check.NotNull(action);

        return task.OnSucceeded(m => action(m.Value!, m.Elapsed));
    }

    public static Task<OperationResult<T>> OnException<T>(this Task<OperationResult<T>> task, Action<Exception> action)
    {
        Check.NotNull(action);

        return task.OnFailed(t => action(t.Exception!));
    }

    public static Task<OperationResult<T>> OnException<T>(this Task<OperationResult<T>> task, Action<Exception, TimeSpan> action)
    {
        Check.NotNull(action);

        return task.OnFailed(t => action(t.Exception!, t.Elapsed));
    }

    public static Task<OperationResult<T>> OnException<T>(this Task<OperationResult<T>> task, Func<Exception, TimeSpan, Task> action)
    {
        Check.NotNull(action);

        return task.OnFailed(t => action(t.Exception!, t.Elapsed));
    }

    public static Task<OperationResult<T>> OnException<T>(this Task<OperationResult<T>> task, Func<Exception, Task> action)
    {
        Check.NotNull(action);

        return task.OnFailed(t => action(t.Exception!));
    }

    public static async Task<OperationResult<T>> ThrowIfError<T>(this Task<OperationResult<T>> task)
    {
        var result = await task.NoCapture();
        return result.Unwrap();
    }

    public static async Task<OperationResult> WithoutValue<T>(this Task<OperationResult<T>> task)
    {
        var result = await task.NoCapture();
        return result.WithoutValue();
    }

    public static Task<OperationResult<TNext>> Then<T, TNext>(this Task<OperationResult<T>> task, Func<T, Task<OperationResult<TNext>>> next)
    {
        Check.NotNull(next);

        return ThenAsync();

        async Task<OperationResult<TNext>> ThenAsync()
        {
            var result = await Normalize(task).NoCapture();
            if (result.IsError)
                return result.Cast<TNext>();

            var nextResult = await next(result.Value).NoCapture();
            return nextResult.IsSuccess
                ? (nextResult.Value, result.Elapsed + nextResult.Elapsed)
                : (nextResult.Exception, result.Elapsed + nextResult.Elapsed);
        }
    }

    public static Task<OperationResult<TNext>> Then<T, TNext>(this Task<OperationResult<T>> task, Func<T, OperationResult<TNext>> next)
    {
        Check.NotNull(next);

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
        Check.NotNull(next);

        return task.Then(m => Operation.Success(next(m)));
    }

    public static Task<OperationResult<TNext>> ThenResult<T, TNext>(this Task<OperationResult<T>> task, Func<OperationResult<T>, Task<OperationResult<TNext>>> next)
    {
        Check.NotNull(next);

        return ThenResultAsync();

        async Task<OperationResult<TNext>> ThenResultAsync()
        {
            var result = await Normalize(task).NoCapture();
            var nextResult = await next(result).NoCapture();
            return nextResult.IsSuccess
                ? (nextResult.Value, result.Elapsed + nextResult.Elapsed)
                : (nextResult.Exception, result.Elapsed + nextResult.Elapsed);
        }
    }

    public static Task<OperationResult<TNext>> ThenResult<T, TNext>(this Task<OperationResult<T>> task, Func<OperationResult<T>, OperationResult<TNext>> next)
    {
        Check.NotNull(next);

        return task.ThenResult(m => Task.FromResult(next(m)));
    }

    public static Task<OperationResult<TNext>> ThenResult<T, TNext>(this Task<OperationResult<T>> task, Func<OperationResult<T>, TNext> next)
    {
        Check.NotNull(next);

        return task.ThenResult(m => Operation.Success(next(m)));
    }

    public static async Task<T> Unwrap<T>(this Task<OperationResult<T>> task)
    {
        var result = await task.NoCapture();
        return result.Unwrap();
    }

    public static async Task<T> UnwrapOr<T>(this Task<OperationResult<T>> task, T defaultValue)
    {
        var result = await task.NoCapture();
        return result.UnwrapOr(defaultValue);
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
        Check.NotNull(next);

        return task.ThenIf(condition, next, m => Operation.Success(m));
    }

    public static Task<OperationResult> ThenIf<T>(this Task<OperationResult<T>> task, Func<T, bool> condition, Func<T, Task<OperationResult>> next)
    {
        Check.NotNull(next);

        return task.ThenIf(condition, next, _ => Operation.Success(Unit.Default));
    }

    public static IAction<T[]> ToAction<T>(this IEnumerable<Task<OperationResult<T>>> tasks, bool parallel)
    {
        Check.NotNull(tasks);

        var actions = tasks.Select(m => Operation.Action(t => m));
        return parallel ? actions.CombineInParallel() : actions.CombineInSeries();
    }

    public static Task<OperationResult<T>> Fallback<T>(this Task<OperationResult<T>> result, Func<OperationResult<T>, Task<OperationResult<T>>> fallback)
    {
        Check.NotNull(fallback);

        return FallbackAsync();

        async Task<OperationResult<T>> FallbackAsync()
        {
            var r = await Normalize(result).NoCapture();
            if (r.IsSuccess)
                return r;

            var fallbackResult = await fallback(r).NoCapture();
            return fallbackResult.IsSuccess
                ? (fallbackResult.Value, r.Elapsed + fallbackResult.Elapsed)
                : (fallbackResult.Exception, r.Elapsed + fallbackResult.Elapsed);
        }
    }

    public static Task<OperationResult<T>> Fallback<T>(this Task<OperationResult<T>> result, Func<OperationResult<T>, OperationResult<T>> fallback)
    {
        Check.NotNull(fallback);

        return result.Fallback(r => Task.FromResult(fallback(r)));
    }

    public static Task<OperationResult<T>> Fallback<T>(this Task<OperationResult<T>> result, Func<OperationResult<T>, T> fallback)
    {
        Check.NotNull(fallback);

        return result.Fallback(r => Task.FromResult(Operation.Success(fallback(r))));
    }

    public static Task<OperationResult<T>> Fallback<T>(this Task<OperationResult<T>> result, T fallback)
    {
        return result.Fallback(_ => Task.FromResult(Operation.Success(fallback)));
    }

    public static Task<OperationResult<TNext>> Then<T, TNext>(this Task<OperationResult<T>> task, Func<T, Task<TNext>> next)
    {
        Check.NotNull(next);

        return task.Then<T, TNext>(m => Operation.ExecuteAsync(() => next(m)));
    }

    public static Task<OperationResult<(T, TNext)>> ThenWith<T, TNext>(this Task<OperationResult<T>> task, Func<T, Task<TNext>> next)
    {
        Check.NotNull(next);

        return task.Then<T, (T, TNext)>(m => Operation.ExecuteAsync(() => next(m).Then(x => (m, x))));
    }

    public static Task<OperationResult<(T, TNext)>> ThenWith<T, TNext>(this Task<OperationResult<T>> task, Func<T, Task<OperationResult<TNext>>> next)
    {
        Check.NotNull(next);

        return task.Then<T, (T, TNext)>(async m =>
        {
            var nextResult = await next(m).NoCapture();
            return nextResult.MapValue(x => (m, x));
        });
    }

    public static Task<OperationResult<T[]>> Merge<T, TResults>(this Task<TResults> task) where TResults : IEnumerable<OperationResult<T>>
    {
        var watch = ValueStopwatch.StartNew();
        return task.Then(m => m.Merge().Elapsed(watch.GetElapsedTime()));
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

    private static async Task<OperationResult<T>> Normalize<T>(Task<OperationResult<T>> task)
    {
        task = Check.NotNull(task);

        var watch = ValueStopwatch.StartNew();
        try
        {
            return await task.NoCapture();
        }
        catch (Exception exception)
        {
            return Operation.Error<T>(exception, watch.GetElapsedTime());
        }
    }
}
