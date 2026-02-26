namespace FclEx.Actions;

public static partial class ActionExtensions
{
    public static IAction<Unit> ToUnit<T>(this IAction<T> action)
    {
        return action.MapValue(m => default(Unit));
    }

    public static IAction<T2> MapValue<T, T2>(this IAction<T> action, Func<T, T2> map)
    {
        return new MapValueAction<T, T2>(action, map);
    }

    public static IAction<T2> MapToResult<T, T2>(this IAction<T> action, Func<T, OperationResult<T2>> map)
    {
        return new MapToResultAction<T, T2>(action, map);
    }

    public static IAction<T> MapError<T>(this IAction<T> action, Func<Exception, Exception> func)
    {
        return action.ThenResultIf(m => m.IsError, m => new ErrorAction<T>(func(m.Exception!)));
    }

    public static IAction<T> MapErrorMessage<T>(this IAction<T> action, Func<string, string> func)
    {
        return action.MapError(e => e.SetMessage(func(e.Message)));
    }

    public static IAction<T> Fail<T>(this IAction<T> action, Func<T, Exception> errorFunc)
    {
        Check.NotNull(errorFunc);
        return action.Then(t => ErrorAction.Create<T>(errorFunc(t)));
    }

    public static IAction<T> Fail<T>(this IAction<T> action, Func<T, string> errorFunc)
    {
        Check.NotNull(errorFunc);
        return action.Then(t => new ErrorAction<T>(errorFunc(t)));
    }

    public static IAction<T> FailIf<T>(this IAction<T> action, Func<T, bool> condition, Func<T, Exception> errorFunc)
    {
        Check.NotNull(condition);
        Check.NotNull(errorFunc);

        return action.Then<T, T>(t => condition(t)
            ? ErrorAction.Create<T>(errorFunc(t))
            : SuccessAction.Create(t));
    }

    public static IAction<T> FailIf<T>(this IAction<T> action, Func<T, bool> condition, Func<T, string> errorFunc)
    {
        Check.NotNull(condition);
        Check.NotNull(errorFunc);

        return action.Then<T, T>(t => condition(t)
            ? ErrorAction.Create<T>(errorFunc(t))
            : SuccessAction.Create(t));
    }

    public static IAction<T> OnFailed<T>(this IAction<T> action, Action<OperationResult<T>> errorAction)
    {
        Check.NotNull(errorAction);
        return action.ThenResult(r => r.OnFailed(e => errorAction(e)));
    }

    public static IAction<T> OnException<T>(this IAction<T> action, Func<Exception, Task> errorAction)
    {
        Check.NotNull(errorAction);
        return action.ThenResult(r => r.OnException(e => errorAction(e)));
    }

    public static IAction<T> OnException<T>(this IAction<T> action, Action<Exception> errorAction)
    {
        Check.NotNull(errorAction);
        return action.ThenResult(r => r.OnException(e => errorAction(e)));
    }

    public static IAction<T> RepeatOnce<T>(this IAction<T> action, Func<T, bool> condition)
    {
        return action.Then(t => condition(t) ? action : SuccessAction.Create(t));
    }

    public static IAction<T> RepeatUntil<T>(this IAction<T> action, Func<T, bool>? until, TimeSpan delay = default, TimeSpan? timeout = null)
    {
        return Operation.Action<T>(async t =>
        {
            using var cts = t.WithTimeout(timeout > TimeSpan.Zero ? timeout : null);
            while (!cts.IsCancellationRequested)
            {
                var r = await action.ExecuteAsync(t);
                if (!r.IsSuccess)
                    return r;

                if (until != null && until(r.Value!))
                    return r;

                await TaskHelper.Delay(delay, t);
            }
            return Operation.Cancel<T>();
        });
    }

    public static IAction<T> RepeatUntil<T>(this IAction<T> action, Func<T, bool>? until, int delayInSeconds = default, int? timeoutInSeconds = null)
    {
        return action.RepeatUntil(until, TimeSpan.FromSeconds(delayInSeconds), timeoutInSeconds.HasValue ? TimeSpan.FromSeconds(timeoutInSeconds.Value) : null);
    }

    public static IAction<T> ExecuteAsync<T>(this IEnumerable<IAction<T>> actions)
    {
        IAction<T> seed = new SuccessAction<T>(default!);
        return actions.Aggregate(seed, (sum, next) => sum.Then(next), m => m);
    }

    public static Task<OperationResult> RunAsync<T>(this IAction<T> action, CancellationToken token = default)
    {
        return action.ExecuteAsync(token).WithoutValue();
    }

    // NOTE: help value type cast to interface.
    public static Task<OperationResult<T>> ExecuteAsync<T>(this IAction<T> action, CancellationToken token = default)
    {
        return action.ExecuteAsync(token);
    }

    public static Task<OperationResult<T>> ExecuteAsync<T>(this IAction<T> action, int retryCount, CancellationToken token)
    {
        return action.ExecuteAsync(retryCount, null, null, token);
    }

    public static Task<OperationResult<T>> ExecuteAsync<T>(this IAction<T> action, int retryCount, Func<OperationResult<T>, bool?>? retryCondition, CancellationToken token)
    {
        return action.ExecuteAsync(retryCount, retryCondition, null, token);
    }

    public static async Task<OperationResult<T>> ExecuteAsync<T>(this IAction<T> action,
        int retryCount,
        Func<OperationResult<T>, bool?>? retryCondition = null,
        Func<int, TimeSpan>? sleepDurationProvider = null,
        CancellationToken token = default)
    {
        var executeCount = Math.Max(1, retryCount + 1);

        var result = Operation.Error<T>("not started");
        var watch = ValueStopwatch.StartNew();
        for (var i = 1; i <= executeCount; i++)
        {
            result = await action.ExecuteAsync(token)
                .ThenResult(m => m.Elapsed(watch.GetElapsedTime()));

            if (result.IsSuccess)
                return result;

            if (retryCondition is not null)
            {
                var condition = retryCondition(result);
                if (condition != true)
                    return result;
            }

            if (sleepDurationProvider is null)
                continue;

            var sleepDuration = sleepDurationProvider.Invoke(i);
            if (sleepDuration > TimeSpan.Zero)
                await Task.Delay(sleepDuration, token);
        }

        return result;
    }

}