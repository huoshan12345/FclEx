namespace FclEx.Utils;

partial class OperationResultExtensions
{
    public static OperationResult<T> When<T>(this OperationResult<T> result, Func<T, bool> condition, Action<OperationResult<T>> action)
    {
        Check.NotNull(condition);
        Check.NotNull(action);
    
        if (result.IsSuccess && condition(result.Value))
            action(result);
        return result;
    }

    public static OperationResult<T> WhenResult<T>(this OperationResult<T> result, Func<OperationResult<T>, bool> condition, Action<OperationResult<T>> action)
    {
        Check.NotNull(condition);
        Check.NotNull(action);

        if (condition(result))
            action(result);
        return result;
    }

    public static OperationResult<T> OnSucceeded<T>(this OperationResult<T> result, Action<OperationResult<T>> action)
    {
        return result.WhenResult(m => m.IsSuccess, action);
    }

    public static OperationResult<T> OnFailed<T>(this OperationResult<T> result, Action<OperationResult<T>> action)
    {
        return result.WhenResult(m => m.IsError, action);
    }

    public static OperationResult<T> OnFaulted<T>(this OperationResult<T> result, Action<OperationResult<T>> action)
    {
        return result.WhenResult(m => m.IsFaulted(), action);
    }

    public static OperationResult<T> OnCanceled<T>(this OperationResult<T> result, Action<OperationResult<T>> action)
    {
        return result.WhenResult(m => m.IsCanceled(), action);
    }

    public static OperationResult<T> OnValue<T>(this OperationResult<T> result, Action<T> action)
    {
        Check.NotNull(action);
        return result.OnSucceeded(m => action(m.Value!));
    }

    public static OperationResult<T> OnValue<T>(this OperationResult<T> result, Action<T, TimeSpan> action)
    {
        Check.NotNull(action);
        return result.OnSucceeded(m => action(m.Value!, m.Elapsed));
    }

    public static OperationResult<T> OnException<T>(this OperationResult<T> result, Action<Exception> action)
    {
        Check.NotNull(action);
        return result.OnFailed(t => action(t.Exception!));
    }

    public static OperationResult<T> OnException<T>(this OperationResult<T> result, Action<Exception, TimeSpan> action)
    {
        Check.NotNull(action);
        return result.OnFailed(t => action(t.Exception!, t.Elapsed));
    }

    public static OperationResult WithoutValue<T>(this OperationResult<T> result) => result;

    public static OperationResult<T> Elapsed<T>(this OperationResult<T> result, TimeSpan span)
    {
        return result.IsSuccess
            ? (result.Value, span)
            : (result.Exception!, span);
    }

    public static OperationResult<T> ThrowIfError<T>(this OperationResult<T> result)
    {
        if (result.IsError)
            result.Exception.ReThrow();
        return result;
    }

    public static OperationResult<T> Flatten<T>(this OperationResult<OperationResult<T>> result)
    {
        return result.IsSuccess
            ? result.Value.Elapsed(result.Elapsed)
            : (result.Exception, result.Elapsed);
    }

    public static OperationResult<TResult> MapValue<T, TResult>(this OperationResult<T> result, Func<T, TResult> map)
    {
        Check.NotNull(map);

        return result.IsSuccess
            ? (map(result.Value), result.Elapsed)
            : (result.Exception, result.Elapsed);
    }

    public static OperationResult<TResult> Then<T, TResult>(this OperationResult<T> result, Func<T, OperationResult<TResult>> next)
    {
        Check.NotNull(next);

        return result.IsSuccess
            ? next(result.Value)
            : (result.Exception, result.Elapsed);
    }

    public static Task<OperationResult<TResult>> Then<T, TResult>(this OperationResult<T> result, Func<T, Task<OperationResult<TResult>>> next)
    {
        Check.NotNull(next);

        return result.IsSuccess
            ? next(result.Value)
            : Operation.Error<TResult>(result.Exception, result.Elapsed);
    }

    public static OperationResult<TResult> ThenResult<T, TResult>(this OperationResult<T> result, Func<OperationResult<T>, OperationResult<TResult>> next)
    {
        Check.NotNull(next);

        return next(result);
    }

    public static OperationResult<TResult> ThenResult<T, TResult>(this OperationResult<T> result, Func<OperationResult<T>, TResult> next)
    {
        Check.NotNull(next);

        return next(result);
    }

    public static T UnwrapOr<T>(this OperationResult<T> result, T defaultValue)
    {
        return result.IsSuccess ? result.Value : defaultValue;
    }

    public static T Unwrap<T>(this OperationResult<T> result)
    {
        if (result.IsError)
            result.Exception.ReThrow();

        return result.Value;
    }

    public static T FromObjectError<T>(this IOperationResult result) where T : notnull
    {
        Check.NotNull(result);

        if (result.IsObjectError<T>(static (_, _) => true, out var value))
            return value;

        throw new InvalidOperationException($"The result is not an object error of type '{typeof(T).LongName()}'");
    }

    public static T FromObjectError<T>(this IOperationResult<T> result) where T : notnull
    {
        Check.NotNull(result);

        return result.CastTo<IOperationResult>().FromObjectError<T>();
    }

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

    public static bool IsSuccess<T>(this OperationResult<T> result, Func<T, bool> condition)
    {
        Check.NotNull(condition);

        return result.IsSuccess && condition(result.Value);
    }

    public static OperationResult<TNext> ThenIf<T, TNext>(this OperationResult<T> result, Func<T, bool> condition,
        Func<T, OperationResult<TNext>> @true, Func<T, OperationResult<TNext>> @false)
    {
        Check.NotNull(condition);
        Check.NotNull(@true);
        Check.NotNull(@false);

        return result.Then(t => condition(t) ? @true(t) : @false(t));
    }

    public static OperationResult<T> ThenIf<T>(this OperationResult<T> result, Func<T, bool> condition, Func<T, OperationResult<T>> next)
    {
        Check.NotNull(next);

        return result.ThenIf(condition, next, m => Operation.Success(m));
    }

    public static OperationResult ThenIf<T>(this OperationResult<T> result, Func<T, bool> condition, Func<T, OperationResult> next)
    {
        Check.NotNull(next);

        return result.ThenIf(condition, next, _ => Operation.Success(Unit.Default));
    }

    public static OperationResult<T> Fallback<T>(this OperationResult<T> result, Func<OperationResult<T>> fallback)
    {
        Check.NotNull(fallback);

        return result.IsError
            ? fallback()
            : result;
    }

    public static OperationResult<T> Fallback<T>(this OperationResult<T> result, Func<T> fallback)
    {
        Check.NotNull(fallback);

        return result.IsError
            ? Operation.Success(fallback())
            : result;
    }

    public static OperationResult<T> Fallback<T>(this OperationResult<T> result, T fallback)
    {
        return result.IsError
            ? Operation.Success(fallback)
            : result;
    }

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

    public static OperationResult<(T, TNext)> ThenWith<T, TNext>(this OperationResult<T> result, Func<T, TNext> next)
    {
        Check.NotNull(next);

        return result.MapValue(m => (m, next(m)));
    }

    public static OperationResult<(T, TNext)> ThenWith<T, TNext>(this OperationResult<T> result, Func<T, OperationResult<TNext>> next)
    {
        Check.NotNull(next);

        return result.Then(m => next(m).MapValue(x => (m, x)));
    }
}
