namespace FclEx.Utils;

public static partial class OperationResultExtensions
{
    /// <summary>
    /// Invokes a callback when the result is successful and the successful value satisfies a condition, then returns the original result.
    /// </summary>
    public static OperationResult<T> When<T>(this OperationResult<T> result, Func<T, bool> condition, Action<OperationResult<T>> action)
    {
        Check.NotNull(condition);
        Check.NotNull(action);

        if (result.IsSuccess && condition(result.Value))
            action(result);
        return result;
    }

    /// <summary>
    /// Invokes a callback when the full result satisfies a condition, then returns the original result.
    /// </summary>
    public static OperationResult<T> WhenResult<T>(this OperationResult<T> result, Func<OperationResult<T>, bool> condition, Action<OperationResult<T>> action)
    {
        Check.NotNull(condition);
        Check.NotNull(action);

        if (condition(result))
            action(result);
        return result;
    }

    /// <summary>
    /// Invokes a callback when the result is successful, then returns the original result.
    /// </summary>
    public static OperationResult<T> OnSucceeded<T>(this OperationResult<T> result, Action<OperationResult<T>> action)
    {
        return result.WhenResult(m => m.IsSuccess, action);
    }

    /// <summary>
    /// Invokes a callback when the result is an error, then returns the original result.
    /// </summary>
    public static OperationResult<T> OnFailed<T>(this OperationResult<T> result, Action<OperationResult<T>> action)
    {
        return result.WhenResult(m => m.IsError, action);
    }

    /// <summary>
    /// Invokes a callback when the result is an error other than cancellation, then returns the original result.
    /// </summary>
    public static OperationResult<T> OnFaulted<T>(this OperationResult<T> result, Action<OperationResult<T>> action)
    {
        return result.WhenResult(m => m.IsFaulted(), action);
    }

    /// <summary>
    /// Invokes a callback when the result is canceled, then returns the original result.
    /// </summary>
    public static OperationResult<T> OnCanceled<T>(this OperationResult<T> result, Action<OperationResult<T>> action)
    {
        return result.WhenResult(m => m.IsCanceled(), action);
    }

    /// <summary>
    /// Invokes a callback with the successful value, then returns the original result.
    /// </summary>
    public static OperationResult<T> OnValue<T>(this OperationResult<T> result, Action<T> action)
    {
        Check.NotNull(action);
        return result.OnSucceeded(m => action(m.Value!));
    }

    /// <summary>
    /// Invokes a callback with the successful value and elapsed time, then returns the original result.
    /// </summary>
    public static OperationResult<T> OnValue<T>(this OperationResult<T> result, Action<T, TimeSpan> action)
    {
        Check.NotNull(action);
        return result.OnSucceeded(m => action(m.Value!, m.Elapsed));
    }

    /// <summary>
    /// Invokes a callback with the exception when the result is an error, then returns the original result.
    /// </summary>
    public static OperationResult<T> OnException<T>(this OperationResult<T> result, Action<Exception> action)
    {
        Check.NotNull(action);
        return result.OnFailed(t => action(t.Exception!));
    }

    /// <summary>
    /// Invokes a callback with the exception and elapsed time when the result is an error, then returns the original result.
    /// </summary>
    public static OperationResult<T> OnException<T>(this OperationResult<T> result, Action<Exception, TimeSpan> action)
    {
        Check.NotNull(action);
        return result.OnFailed(t => action(t.Exception!, t.Elapsed));
    }

    /// <summary>
    /// Drops the typed value and returns a non-generic result while preserving success, error, and elapsed time.
    /// </summary>
    public static OperationResult WithoutValue<T>(this OperationResult<T> result) => result;

    /// <summary>
    /// Returns a copy of the result with the elapsed time replaced.
    /// </summary>
    public static OperationResult<T> Elapsed<T>(this OperationResult<T> result, TimeSpan span)
    {
        return result.IsSuccess
            ? (result.Value, span)
            : (result.Exception!, span);
    }

    /// <summary>
    /// Throws the stored exception when the result is an error; otherwise returns the original result.
    /// </summary>
    public static OperationResult<T> ThrowIfError<T>(this OperationResult<T> result)
    {
        if (result.IsError)
            result.Exception.ReThrow();
        return result;
    }

    /// <summary>
    /// Flattens a nested operation result.
    /// </summary>
    /// <remarks>
    /// An outer error wins. For an outer success, a non-default outer elapsed time replaces the inner elapsed time;
    /// a default outer elapsed time is treated as unspecified and the inner elapsed time is preserved.
    /// </remarks>
    public static OperationResult<T> Flatten<T>(this OperationResult<OperationResult<T>> result)
    {
        if (result.IsError)
            return (result.Exception, result.Elapsed);

        return result.Elapsed == default
            ? result.Value
            : result.Value.Elapsed(result.Elapsed);
    }

    /// <summary>
    /// Maps a successful value while preserving error and elapsed time.
    /// </summary>
    /// <remarks>Exceptions thrown by <paramref name="map"/> are not captured into an operation result.</remarks>
    public static OperationResult<TResult> MapValue<T, TResult>(this OperationResult<T> result, Func<T, TResult> map)
    {
        Check.NotNull(map);

        return result.IsSuccess
            ? (map(result.Value), result.Elapsed)
            : (result.Exception, result.Elapsed);
    }

    /// <summary>
    /// Runs the next operation when the result is successful and returns the next result.
    /// </summary>
    /// <remarks>Source and next elapsed times are added. Exceptions thrown by <paramref name="next"/> are not captured.</remarks>
    public static OperationResult<TResult> Then<T, TResult>(this OperationResult<T> result, Func<T, OperationResult<TResult>> next)
    {
        Check.NotNull(next);

        if (result.IsError)
            return (result.Exception, result.Elapsed);

        var nextResult = next(result.Value);
        return nextResult.IsSuccess
            ? (nextResult.Value, result.Elapsed + nextResult.Elapsed)
            : (nextResult.Exception, result.Elapsed + nextResult.Elapsed);
    }

    /// <summary>
    /// Runs the next asynchronous operation when the result is successful and returns the next result.
    /// </summary>
    /// <remarks>Source and next elapsed times are added. Exceptions or faulted tasks from <paramref name="next"/> are not captured.</remarks>
    public static Task<OperationResult<TResult>> Then<T, TResult>(this OperationResult<T> result, Func<T, Task<OperationResult<TResult>>> next)
    {
        Check.NotNull(next);

        return result.IsSuccess
            ? ThenAsync()
            : Operation.Error<TResult>(result.Exception, result.Elapsed);

        async Task<OperationResult<TResult>> ThenAsync()
        {
            var nextResult = await next(result.Value).NoCapture();
            return nextResult.IsSuccess
                ? (nextResult.Value, result.Elapsed + nextResult.Elapsed)
                : (nextResult.Exception, result.Elapsed + nextResult.Elapsed);
        }
    }

    /// <summary>
    /// Runs the next operation with the full source result and returns the next result.
    /// </summary>
    /// <remarks>Source and next elapsed times are added. Exceptions thrown by <paramref name="next"/> are not captured.</remarks>
    public static OperationResult<TResult> ThenResult<T, TResult>(this OperationResult<T> result, Func<OperationResult<T>, OperationResult<TResult>> next)
    {
        Check.NotNull(next);

        var nextResult = next(result);
        return nextResult.IsSuccess
            ? (nextResult.Value, result.Elapsed + nextResult.Elapsed)
            : (nextResult.Exception, result.Elapsed + nextResult.Elapsed);
    }

    /// <summary>
    /// Maps the full source result to a successful value while preserving the source elapsed time.
    /// </summary>
    /// <remarks>Exceptions thrown by <paramref name="next"/> are not captured.</remarks>
    public static OperationResult<TResult> ThenResult<T, TResult>(this OperationResult<T> result, Func<OperationResult<T>, TResult> next)
    {
        Check.NotNull(next);

        return Operation.Success(next(result), result.Elapsed);
    }

    /// <summary>
    /// Returns the successful value, or <paramref name="defaultValue"/> when the result is an error.
    /// </summary>
    public static T UnwrapOr<T>(this OperationResult<T> result, T defaultValue)
    {
        return result.IsSuccess ? result.Value : defaultValue;
    }

    /// <summary>
    /// Returns the successful value, or rethrows the stored exception when the result is an error.
    /// </summary>
    public static T Unwrap<T>(this OperationResult<T> result)
    {
        if (result.IsError)
            result.Exception.ReThrow();

        return result.Value;
    }

    /// <summary>
    /// Extracts the object associated with an object error.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the result is not an object error for <typeparamref name="T"/>.</exception>
    public static T FromObjectError<T>(this IOperationResult result) where T : notnull
    {
        Check.NotNull(result);

        if (result.IsObjectError<T>(static (_, _) => true, out var value))
            return value;

        throw new InvalidOperationException($"The result is not an object error of type '{typeof(T).LongName()}'");
    }

    /// <summary>
    /// Extracts the object associated with an object error from a typed operation result.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the result is not an object error for <typeparamref name="T"/>.</exception>
    public static T FromObjectError<T>(this IOperationResult<T> result) where T : notnull
    {
        Check.NotNull(result);

        return result.CastTo<IOperationResult>().FromObjectError<T>();
    }

    /// <summary>
    /// Tries to get the successful value.
    /// </summary>
    public static bool TryGetValue<T>(this OperationResult<T> result, [NotNullWhen(true)] out T? value)
    {
        if (result.IsSuccess)
        {
            value = result.Value;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Returns whether the result is successful and its value satisfies a condition.
    /// </summary>
    public static bool IsSuccess<T>(this OperationResult<T> result, Func<T, bool> condition)
    {
        Check.NotNull(condition);

        return result.IsSuccess && condition(result.Value);
    }

    /// <summary>
    /// Chooses between two next operations when the source result is successful.
    /// </summary>
    /// <remarks>The selected branch is composed through <see cref="Then{T, TResult}(OperationResult{T}, Func{T, OperationResult{TResult}})"/>.</remarks>
    public static OperationResult<TNext> ThenIf<T, TNext>(this OperationResult<T> result, Func<T, bool> condition,
        Func<T, OperationResult<TNext>> @true, Func<T, OperationResult<TNext>> @false)
    {
        Check.NotNull(condition);
        Check.NotNull(@true);
        Check.NotNull(@false);

        return result.Then(t => condition(t) ? @true(t) : @false(t));
    }

    /// <summary>
    /// Runs the next operation when the source result is successful and its value satisfies a condition; otherwise preserves the value.
    /// </summary>
    public static OperationResult<T> ThenIf<T>(this OperationResult<T> result, Func<T, bool> condition, Func<T, OperationResult<T>> next)
    {
        return result.ThenIf(condition, next, m => Operation.Success(m));
    }

    /// <summary>
    /// Runs the next unit operation when the source result is successful and its value satisfies a condition; otherwise returns unit success.
    /// </summary>
    public static OperationResult ThenIf<T>(this OperationResult<T> result, Func<T, bool> condition, Func<T, OperationResult> next)
    {
        return result.ThenIf(condition, next, _ => Operation.Success(Unit.Default));
    }

    /// <summary>
    /// Returns the original success result or invokes a fallback operation when the result is an error.
    /// </summary>
    /// <remarks>The fallback result is returned as-is; elapsed times are not added by this synchronous overload.</remarks>
    public static OperationResult<T> Fallback<T>(this OperationResult<T> result, Func<OperationResult<T>> fallback)
    {
        Check.NotNull(fallback);

        return result.IsError
            ? fallback()
            : result;
    }

    /// <summary>
    /// Returns the original success value or invokes a fallback value factory when the result is an error.
    /// </summary>
    public static OperationResult<T> Fallback<T>(this OperationResult<T> result, Func<T> fallback)
    {
        Check.NotNull(fallback);

        return result.IsError
            ? Operation.Success(fallback())
            : result;
    }

    /// <summary>
    /// Returns the original success value or the supplied fallback value when the result is an error.
    /// </summary>
    public static OperationResult<T> Fallback<T>(this OperationResult<T> result, T fallback)
    {
        return result.IsError
            ? Operation.Success(fallback)
            : result;
    }

    /// <summary>
    /// Merges multiple typed operation results into a single array-valued result.
    /// </summary>
    /// <remarks>Successful values are collected in order. Elapsed times are summed. One error is returned directly; multiple errors are wrapped in an <see cref="AggregateException"/>.</remarks>
    public static OperationResult<T[]> Merge<T>(this IEnumerable<OperationResult<T>> enumerable)
    {
        Check.NotNull(enumerable);

        var (values, exceptions, time) = enumerable.Aggregate((Values: new List<T>(), Exceptions: new List<Exception>(), Time: TimeSpan.Zero), (seed, m) =>
        {
            var t = seed.Time + m.Elapsed;
            return m.IsSuccess
                ? (seed.Values.Push(m.Value), seed.Exceptions, t)
                : (seed.Values, seed.Exceptions.Push(m.Exception), t);
        });

        return exceptions.Count switch
        {
            0 => (values.ToArray(), time),
            1 => (exceptions[0], time),
            _ => (new AggregateException(exceptions), time),
        };
    }

    /// <summary>
    /// Maps a successful value and returns both the original value and the mapped value.
    /// </summary>
    /// <remarks>Errors are propagated. Exceptions thrown by <paramref name="next"/> are not captured.</remarks>
    public static OperationResult<(T, TNext)> ThenWith<T, TNext>(this OperationResult<T> result, Func<T, TNext> next)
    {
        Check.NotNull(next);

        return result.MapValue(m => (m, next(m)));
    }

    /// <summary>
    /// Runs the next operation when successful and returns both the original value and the next value.
    /// </summary>
    /// <remarks>Source and next elapsed times are added. Exceptions thrown by <paramref name="next"/> are not captured.</remarks>
    public static OperationResult<(T, TNext)> ThenWith<T, TNext>(this OperationResult<T> result, Func<T, OperationResult<TNext>> next)
    {
        Check.NotNull(next);

        return result.Then(m => next(m).MapValue(x => (m, x)));
    }
}
