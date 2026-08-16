namespace FclEx.Utils;

public static partial class OperationResultExtensions
{
    /// <summary>
    /// Invokes an asynchronous callback when the awaited result is successful and its value satisfies a condition, then returns the result.
    /// </summary>
    public static Task<OperationResult<T>> When<T>(this Task<OperationResult<T>> result, Func<T, bool> condition, Func<T, Task> action)
    {
        Check.NotNull(condition);
        Check.NotNull(action);

        return result.WhenResult(r => r.IsSuccess && condition(r.Value), r => action(r.Value!));
    }

    /// <summary>
    /// Invokes a callback when the awaited result is successful and its value satisfies a condition, then returns the result.
    /// </summary>
    public static Task<OperationResult<T>> When<T>(this Task<OperationResult<T>> result, Func<T, bool> condition, Action<T> action)
    {
        Check.NotNull(action);

        return result.When(condition, v =>
        {
            action(v);
            return Task.CompletedTask;
        });
    }

    /// <summary>
    /// Invokes an asynchronous callback when the awaited full result satisfies a condition, then returns the result.
    /// </summary>
    public static Task<OperationResult<T>> WhenResult<T>(this Task<OperationResult<T>> result, Func<OperationResult<T>, bool> condition, Func<OperationResult<T>, Task> action)
    {
        Check.NotNull(condition);
        Check.NotNull(action);

        return result.Then(r => condition(r)
            ? action(r).Then(() => r)
            : r);
    }

    /// <summary>
    /// Invokes a callback when the awaited full result satisfies a condition, then returns the result.
    /// </summary>
    public static Task<OperationResult<T>> WhenResult<T>(this Task<OperationResult<T>> result, Func<OperationResult<T>, bool> condition, Action<OperationResult<T>> action)
    {
        Check.NotNull(action);

        return result.WhenResult(condition, r =>
        {
            action(r);
            return Task.CompletedTask;
        });
    }

    /// <summary>
    /// Invokes a callback with the awaited result, then returns the result.
    /// </summary>
    public static Task<OperationResult<T>> OnResult<T>(this Task<OperationResult<T>> task, Action<OperationResult<T>> action)
    {
        return task.Then(action);
    }

    /// <summary>
    /// Invokes an asynchronous callback with the awaited result, then returns the result.
    /// </summary>
    public static Task<OperationResult<T>> OnResult<T>(this Task<OperationResult<T>> task, Func<OperationResult<T>, Task> action)
    {
        return task.Then(action);
    }

    /// <summary>
    /// Invokes a callback when the awaited result is successful, then returns the result.
    /// </summary>
    public static Task<OperationResult<T>> OnSucceeded<T>(this Task<OperationResult<T>> task, Action<OperationResult<T>> action)
    {
        Check.NotNull(action);

        return task.ThenIf(action, m => m.IsSuccess);
    }

    /// <summary>
    /// Invokes an asynchronous callback when the awaited result is successful, then returns the result.
    /// </summary>
    public static Task<OperationResult<T>> OnSucceeded<T>(this Task<OperationResult<T>> task, Func<OperationResult<T>, Task> action)
    {
        Check.NotNull(action);

        return task.ThenIf(action, m => m.IsSuccess);
    }

    /// <summary>
    /// Invokes a callback when the awaited result is an error, then returns the result.
    /// </summary>
    public static Task<OperationResult<T>> OnFailed<T>(this Task<OperationResult<T>> task, Action<OperationResult<T>> action)
    {
        Check.NotNull(action);

        return task.ThenIf(action, m => m.IsError);
    }

    /// <summary>
    /// Invokes an asynchronous callback when the awaited result is an error, then returns the result.
    /// </summary>
    public static Task<OperationResult<T>> OnFailed<T>(this Task<OperationResult<T>> task, Func<OperationResult<T>, Task> action)
    {
        Check.NotNull(action);

        return task.ThenIf(action, m => m.IsError);
    }

    /// <summary>
    /// Invokes a callback when the awaited result is an error other than cancellation, then returns the result.
    /// </summary>
    public static Task<OperationResult<T>> OnFaulted<T>(this Task<OperationResult<T>> task, Action<OperationResult<T>> action)
    {
        Check.NotNull(action);

        return task.ThenIf(action, m => m.IsFaulted());
    }

    /// <summary>
    /// Invokes an asynchronous callback when the awaited result is an error other than cancellation, then returns the result.
    /// </summary>
    public static Task<OperationResult<T>> OnFaulted<T>(this Task<OperationResult<T>> task, Func<OperationResult<T>, Task> action)
    {
        Check.NotNull(action);

        return task.ThenIf(action, m => m.IsFaulted());
    }

    /// <summary>
    /// Invokes a callback when the awaited result is canceled, then returns the result.
    /// </summary>
    public static Task<OperationResult<T>> OnCanceled<T>(this Task<OperationResult<T>> task, Action<OperationResult<T>> action)
    {
        Check.NotNull(action);

        return task.ThenIf(action, m => m.IsCanceled());
    }

    /// <summary>
    /// Invokes an asynchronous callback when the awaited result is canceled, then returns the result.
    /// </summary>
    public static Task<OperationResult<T>> OnCanceled<T>(this Task<OperationResult<T>> task, Func<OperationResult<T>, Task> action)
    {
        Check.NotNull(action);

        return task.ThenIf(action, m => m.IsCanceled());
    }

    /// <summary>
    /// Invokes a callback with the successful value, then returns the awaited result.
    /// </summary>
    public static Task<OperationResult<T>> OnValue<T>(this Task<OperationResult<T>> task, Action<T> action)
    {
        Check.NotNull(action);

        return task.OnSucceeded(m => action(m.Value!));
    }

    /// <summary>
    /// Invokes an asynchronous callback with the successful value, then returns the awaited result.
    /// </summary>
    public static Task<OperationResult<T>> OnValue<T>(this Task<OperationResult<T>> task, Func<T, Task> action)
    {
        Check.NotNull(action);

        return task.OnSucceeded(m => action(m.Value!));
    }

    /// <summary>
    /// Invokes a callback with the successful value and elapsed time, then returns the awaited result.
    /// </summary>
    public static Task<OperationResult<T>> OnValue<T>(this Task<OperationResult<T>> task, Action<T, TimeSpan> action)
    {
        Check.NotNull(action);

        return task.OnSucceeded(m => action(m.Value!, m.Elapsed));
    }

    /// <summary>
    /// Invokes an asynchronous callback with the successful value and elapsed time, then returns the awaited result.
    /// </summary>
    public static Task<OperationResult<T>> OnValue<T>(this Task<OperationResult<T>> task, Func<T, TimeSpan, Task> action)
    {
        Check.NotNull(action);

        return task.OnSucceeded(m => action(m.Value!, m.Elapsed));
    }

    /// <summary>
    /// Invokes a callback with the exception when the awaited result is an error, then returns the result.
    /// </summary>
    public static Task<OperationResult<T>> OnException<T>(this Task<OperationResult<T>> task, Action<Exception> action)
    {
        Check.NotNull(action);

        return task.OnFailed(t => action(t.Exception!));
    }

    /// <summary>
    /// Invokes a callback with the exception and elapsed time when the awaited result is an error, then returns the result.
    /// </summary>
    public static Task<OperationResult<T>> OnException<T>(this Task<OperationResult<T>> task, Action<Exception, TimeSpan> action)
    {
        Check.NotNull(action);

        return task.OnFailed(t => action(t.Exception!, t.Elapsed));
    }

    /// <summary>
    /// Invokes an asynchronous callback with the exception and elapsed time when the awaited result is an error, then returns the result.
    /// </summary>
    public static Task<OperationResult<T>> OnException<T>(this Task<OperationResult<T>> task, Func<Exception, TimeSpan, Task> action)
    {
        Check.NotNull(action);

        return task.OnFailed(t => action(t.Exception!, t.Elapsed));
    }

    /// <summary>
    /// Invokes an asynchronous callback with the exception when the awaited result is an error, then returns the result.
    /// </summary>
    public static Task<OperationResult<T>> OnException<T>(this Task<OperationResult<T>> task, Func<Exception, Task> action)
    {
        Check.NotNull(action);

        return task.OnFailed(t => action(t.Exception!));
    }

    /// <summary>
    /// Awaits a result and throws the stored exception when it is an error.
    /// </summary>
    public static async Task<OperationResult<T>> ThrowIfError<T>(this Task<OperationResult<T>> task)
    {
        var result = await task.NoCapture();
        return result.Unwrap();
    }

    /// <summary>
    /// Awaits a typed result and drops its value while preserving success, error, and elapsed time.
    /// </summary>
    public static async Task<OperationResult> WithoutValue<T>(this Task<OperationResult<T>> task)
    {
        var result = await task.NoCapture();
        return result.WithoutValue();
    }

    /// <summary>
    /// Awaits the source result, runs the next asynchronous operation when successful, and returns the next result.
    /// </summary>
    /// <remarks>Source and next elapsed times are added. A faulted or canceled source task is normalized to an error result; exceptions from <paramref name="next"/> are not captured.</remarks>
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
            return nextResult.AddElapsed(result.Elapsed);
        }
    }

    /// <summary>
    /// Awaits the source result, runs the next operation when successful, and returns the next result.
    /// </summary>
    /// <remarks>Source and next elapsed times are added. A faulted or canceled source task is normalized to an error result; exceptions from <paramref name="next"/> are not captured.</remarks>
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
    /// Maps a successful value while preserving error and elapsed time.
    /// </summary>
    /// <remarks>This is an alias for the value-returning <c>Then</c> shape to avoid overload ambiguity.</remarks>
    public static Task<OperationResult<TNext>> MapValue<T, TNext>(this Task<OperationResult<T>> task, Func<T, TNext> next)
    {
        Check.NotNull(next);

        return task.Then(m => Operation.Success(next(m)));
    }

    /// <summary>
    /// Awaits the source result, runs the next asynchronous operation with the full result, and returns the next result.
    /// </summary>
    /// <remarks>Source and next elapsed times are added. A faulted or canceled source task is normalized to an error result; exceptions from <paramref name="next"/> are not captured.</remarks>
    public static Task<OperationResult<TNext>> ThenResult<T, TNext>(this Task<OperationResult<T>> task, Func<OperationResult<T>, Task<OperationResult<TNext>>> next)
    {
        Check.NotNull(next);

        return ThenResultAsync();

        async Task<OperationResult<TNext>> ThenResultAsync()
        {
            var result = await Normalize(task).NoCapture();
            var nextResult = await next(result).NoCapture();
            return nextResult.AddElapsed(result.Elapsed);
        }
    }

    /// <summary>
    /// Awaits the source result, runs the next operation with the full result, and returns the next result.
    /// </summary>
    /// <remarks>Source and next elapsed times are added. A faulted or canceled source task is normalized to an error result; exceptions from <paramref name="next"/> are not captured.</remarks>
    public static Task<OperationResult<TNext>> ThenResult<T, TNext>(this Task<OperationResult<T>> task, Func<OperationResult<T>, OperationResult<TNext>> next)
    {
        Check.NotNull(next);

        return task.ThenResult(m => Task.FromResult(next(m)));
    }

    /// <summary>
    /// Awaits the source result and maps the full result to a successful value.
    /// </summary>
    /// <remarks>A faulted or canceled source task is normalized to an error result; exceptions from <paramref name="next"/> are not captured.</remarks>
    public static Task<OperationResult<TNext>> ThenResult<T, TNext>(this Task<OperationResult<T>> task, Func<OperationResult<T>, TNext> next)
    {
        Check.NotNull(next);

        return task.ThenResult(m => Operation.Success(next(m)));
    }

    /// <summary>
    /// Awaits a result and returns the successful value, or rethrows the stored exception when it is an error.
    /// </summary>
    public static async Task<T> Unwrap<T>(this Task<OperationResult<T>> task)
    {
        var result = await task.NoCapture();
        return result.Unwrap();
    }

    /// <summary>
    /// Awaits a result and returns the successful value, or <paramref name="defaultValue"/> when it is an error.
    /// </summary>
    public static async Task<T> UnwrapOr<T>(this Task<OperationResult<T>> task, T defaultValue)
    {
        var result = await task.NoCapture();
        return result.UnwrapOr(defaultValue);
    }

    /// <summary>
    /// Chooses between two next asynchronous operations when the awaited source result is successful.
    /// </summary>
    public static Task<OperationResult<TNext>> ThenIf<T, TNext>(this Task<OperationResult<T>> task, Func<T, bool> condition,
        Func<T, Task<OperationResult<TNext>>> @true, Func<T, Task<OperationResult<TNext>>> @false)
    {
        Check.NotNull(condition);
        Check.NotNull(@true);
        Check.NotNull(@false);

        return task.Then(t => condition(t) ? @true(t) : @false(t));
    }

    /// <summary>
    /// Runs the next asynchronous operation when the awaited source result is successful and its value satisfies a condition; otherwise preserves the value.
    /// </summary>
    public static Task<OperationResult<T>> ThenIf<T>(this Task<OperationResult<T>> task, Func<T, bool> condition, Func<T, Task<OperationResult<T>>> next)
    {
        return task.ThenIf(condition, next, m => Operation.Success(m));
    }

    /// <summary>
    /// Runs the next unit operation when the awaited source result is successful and its value satisfies a condition; otherwise returns unit success.
    /// </summary>
    public static Task<OperationResult> ThenIf<T>(this Task<OperationResult<T>> task, Func<T, bool> condition, Func<T, Task<OperationResult>> next)
    {
        return task.ThenIf(condition, next, _ => Operation.Success(Unit.Default));
    }

    /// <summary>
    /// Converts asynchronous operation results to actions and combines them.
    /// </summary>
    /// <param name="enumerable">The enumerable items to be converted to actions.</param>
    /// <param name="selector">A function that selects an asynchronous operation result for each item.</param>
    /// <param name="parallel">Whether the generated actions should be combined in parallel.</param>
    public static IAction<TResult[]> ToAction<T, TResult>(this IEnumerable<T> enumerable, Func<T, CancellationToken, Task<OperationResult<TResult>>> selector, bool parallel)
    {
        Check.NotNull(enumerable);
        Check.NotNull(selector);

        var actions = enumerable.Select(m => Operation.Action(t => selector(m, t)));
        return parallel ? actions.CombineInParallel() : actions.CombineInSeries();
    }

    /// <summary>
    /// Returns the original success result or invokes an asynchronous fallback when the awaited result is an error.
    /// </summary>
    /// <remarks>Source and fallback elapsed times are added for fallback execution. A faulted or canceled source task is normalized to an error result.</remarks>
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
            return fallbackResult.AddElapsed(r.Elapsed);
        }
    }

    /// <summary>
    /// Returns the original success result or invokes a fallback when the awaited result is an error.
    /// </summary>
    public static Task<OperationResult<T>> Fallback<T>(this Task<OperationResult<T>> result, Func<OperationResult<T>, OperationResult<T>> fallback)
    {
        Check.NotNull(fallback);

        return result.Fallback(r => Task.FromResult(fallback(r)));
    }

    /// <summary>
    /// Returns the original success value or invokes a fallback value factory when the awaited result is an error.
    /// </summary>
    public static Task<OperationResult<T>> Fallback<T>(this Task<OperationResult<T>> result, Func<OperationResult<T>, T> fallback)
    {
        Check.NotNull(fallback);

        return result.Fallback(r => Task.FromResult(Operation.Success(fallback(r))));
    }

    /// <summary>
    /// Returns the original success value or the supplied fallback value when the awaited result is an error.
    /// </summary>
    public static Task<OperationResult<T>> Fallback<T>(this Task<OperationResult<T>> result, T fallback)
    {
        return result.Fallback(_ => Task.FromResult(Operation.Success(fallback)));
    }

    /// <summary>
    /// Runs an asynchronous value-producing next operation when the awaited source result is successful.
    /// </summary>
    /// <remarks>The next operation is wrapped with <see cref="Operation.ExecuteAsync{T}(Func{Task{T}}, TimeSpan?)"/>, so its exceptions are converted into error results.</remarks>
    public static Task<OperationResult<TNext>> Then<T, TNext>(this Task<OperationResult<T>> task, Func<T, Task<TNext>> next)
    {
        Check.NotNull(next);

        return task.Then<T, TNext>(m => Operation.ExecuteAsync(() => next(m)));
    }

    /// <summary>
    /// Runs an asynchronous value-producing next operation and returns both the original value and the next value.
    /// </summary>
    /// <remarks>The next operation is wrapped with <see cref="Operation.ExecuteAsync{T}(Func{Task{T}}, TimeSpan?)"/>, so its exceptions are converted into error results.</remarks>
    public static Task<OperationResult<(T, TNext)>> ThenWith<T, TNext>(this Task<OperationResult<T>> task, Func<T, Task<TNext>> next)
    {
        Check.NotNull(next);

        return task.Then<T, (T, TNext)>(m => Operation.ExecuteAsync(() => next(m).Then(x => (m, x))));
    }

    /// <summary>
    /// Runs an asynchronous result-producing next operation and returns both the original value and the next value.
    /// </summary>
    /// <remarks>Source and next elapsed times are added. Exceptions or faulted tasks from <paramref name="next"/> are not captured.</remarks>
    public static Task<OperationResult<(T, TNext)>> ThenWith<T, TNext>(this Task<OperationResult<T>> task, Func<T, Task<OperationResult<TNext>>> next)
    {
        Check.NotNull(next);

        return task.Then<T, (T, TNext)>(async m =>
        {
            var nextResult = await next(m).NoCapture();
            return nextResult.MapValue(x => (m, x));
        });
    }

    /// <summary>
    /// Awaits a task that produces operation results and merges them into a single array-valued result.
    /// </summary>
    /// <remarks>The returned elapsed time is the sum of the contained operation results; waiting for the source task is not measured.</remarks>
    public static Task<OperationResult<T[]>> Merge<T, TResults>(this Task<TResults> task) where TResults : IEnumerable<OperationResult<T>>
    {
        return task.Then(m => m.Merge());
    }

    /// <summary>
    /// Awaits a task that produces operation-result arrays and merges them into a single array-valued result.
    /// </summary>
    public static Task<OperationResult<T[]>> Merge<T>(this Task<OperationResult<T>[]> task)
    {
        return task.Merge<T, OperationResult<T>[]>();
    }

    /// <summary>
    /// Converts a task that produces operation results to an action that returns their merged values.
    /// </summary>
    public static IAction<T[]> ToAction<T, TResults>(this Task<TResults> task) where TResults : IEnumerable<OperationResult<T>>
    {
        return Operation.Action(t => task.Merge<T, TResults>());
    }

    /// <summary>
    /// Converts a task that produces operation-result arrays to an action that returns their merged values.
    /// </summary>
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
            // Successful tasks keep non-default inner elapsed; default elapsed is treated as unspecified.
            var result = await task.NoCapture();
            return result.Elapsed == default
                ? result.Elapsed(watch.GetElapsedTime())
                : result;
        }
        catch (Exception exception)
        {
            return Operation.Error<T>(exception, watch.GetElapsedTime());
        }
    }
}
