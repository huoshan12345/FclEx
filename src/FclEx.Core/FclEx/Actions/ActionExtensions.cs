namespace FclEx.Actions;

public static partial class ActionExtensions
{
    public static IAction<T2> Map<T, T2>(this IAction<T> action, Func<T, T2> map)
    {
        return new MapAction<T, T2>(action, map);
    }

    public static IAction<T2> Bind<T, T2>(this IAction<T> action, Func<T, OperationResult<T2>> map)
    {
        return new BindAction<T, T2>(action, map);
    }

    public static Task<OperationResult> RunAsync<T>(this IAction<T> action, CancellationToken token = default)
    {
        return action.ExecuteAsync(token).WithoutValue();
    }

    public static IAction<T> RepeatOnce<T>(this IAction<T> actor, Func<T, bool> condition)
    {
        return actor.Next(t => condition(t) ? actor : new SuccessAction<T>(t));
    }

    public static IAction<T> ErrorIf<T>(this IAction<T> action, Func<T, bool> condition, Func<T, string> errorFunc)
    {
        Check.NotNull(condition);
        Check.NotNull(errorFunc);
        return action.Next(t => condition(t)
            ? (IAction<T>)new ErrorAction<T>(errorFunc(t))
            : new SuccessAction<T>(t));
    }

    public static IAction<T> OneByOne<T>(this IEnumerable<IAction<T>> actions)
    {
        IAction<T> seed = new SuccessAction<T>(default!);
        return actions.Aggregate(seed, (sum, next) => sum.Next(next), m => m);
    }

    public static Task<OperationResult<T>> ExecuteAsync<T>(this IAction<T> action, CancellationToken token = default)
    {
        return action.ExecuteAsync(token);
    }

    public static IAction<T> InsertIf<T, TNext>(this IAction<T> action, Func<T, bool> condition, Func<T, IAction<TNext>> next)
    {
        Check.NotNull(condition);
        Check.NotNull(next);

        return action.Next(t => condition(t)
            ? next(t).Map(m => t)
            : new SuccessAction<T>(t));
    }

    public static IAction<Unit> Untype<T>(this IAction<T> action)
    {
        return action.Map(m => default(Unit));
    }

    public static IAction<T> RepeatUntil<T>(this IAction<T> actor, Func<T, bool>? until, TimeSpan delay = default, TimeSpan? timeout = null)
    {
        return Operation.Action<T>(async t =>
        {
            using var cts = t.WithTimeout(timeout > TimeSpan.Zero ? timeout : null);
            while (!cts.IsCancellationRequested)
            {
                var r = await actor.ExecuteAsync(t);
                if (!r.Success)
                    return r;

                if (until != null && until(r.Value!))
                    return r;

                await TaskHelper.Delay(delay, t);
            }
            return Operation.Cancel<T>();
        });
    }

    public static IAction<T> RepeatUntil<T>(this IAction<T> actor, Func<T, bool>? until, int delayInSeconds = default, int? timeoutInSeconds = null)
    {
        return actor.RepeatUntil(until, TimeSpan.FromSeconds(delayInSeconds), timeoutInSeconds.HasValue ? TimeSpan.FromSeconds(timeoutInSeconds.Value) : null);
    }

    public static IAction<T> Error<T>(this IAction<T> action, Func<T, string> errorFunc)
    {
        Check.NotNull(errorFunc);
        return action.Next(t => new ErrorAction<T>(errorFunc(t)));
    }

    public static IAction<TNext> Error<T, TNext>(this IAction<T> action, Func<T, string> errorFunc)
    {
        Check.NotNull(errorFunc);
        return action.Next(t => new ErrorAction<T>(errorFunc(t))).Map(m => default(TNext))!;
    }

    public static IAction<T> Error<T>(this IAction<T> action, string? error)
    {
        return action.Error(_ => error ?? string.Empty);
    }

    public static IAction<TNext> Error<T, TNext>(this IAction<T> action, string? error)
    {
        return action.Error<T, TNext>(_ => error ?? string.Empty);
    }

    public static IAction<T> Error<T>(this IAction<T> action, Action<Exception> onError)
    {
        Check.NotNull(onError);
        return action.NextResultIf(r => r.Error, r => Operation.Action(t => onError(r.Exception!)).Next(r));
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
                .NextResult(m => m.Elapsed(watch.GetElapsedTime()));

            if (result.Success)
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

    public static IAction<T> SetError<T>(this IAction<T> action, Func<Exception, Exception> func)
    {
        return action.NextResultIf(m => m.Error, m => Operation.ErrorAction<T>(func(m.Exception!), m.Elapsed));
    }

    public static IAction<T> SetError<T>(this IAction<T> action, Func<string, string> func)
    {
        return action.SetError(e => e.SetMessage(func(e.Message)));
    }
}