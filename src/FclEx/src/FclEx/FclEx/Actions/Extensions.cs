using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FclEx;
using FclEx.Helpers;

namespace FclEx.Actions;

public static partial class Extensions
{
    public static IAction<T2> Map<T, T2>(this IAction<T> action, Func<T, T2> map)
    {
        return new MapAction<T, T2>(action, map);
    }

    public static IAction<T2> Bind<T, T2>(this IAction<T> action, Func<T, OperateResult<T2>> map)
    {
        return new BindAction<T, T2>(action, map);
    }

    public static Task<OperateResult> RunAsync<T>(this IAction<T> action, CancellationToken token = default)
    {
        return action.ExecuteAsync(token).Untype();
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

    public static Task<OperateResult<T>> ExecuteAsync<T>(this IAction<T> action, CancellationToken token = default)
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

    public static IAction<T> RepeatUntil<T>(this IAction<T> actor, Func<T, bool>? until, TimeSpan delay = default, TimeSpan? timeout = null, bool excuteSafely = true)
    {
        return CommonAction.Create<T>(async t =>
        {
            using var cts = t.WithTimeout(timeout > TimeSpan.Zero ? timeout : null);
            while (!cts.IsCancellationRequested)
            {
                var r = await actor.ExecuteAsync(t).DonotCapture();
                if (!r.Success)
                    return r;

                if (until != null && until(r.Value!))
                    return r;

                await TaskHelper.Delay(delay, t);
            }
            return Operate.CreateCancel<T>();
        }, excuteSafely);
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

    public static IAction<T> Error<T>(this IAction<T> action, Action<Exception> onError, bool excuteSafely = true)
    {
        Check.NotNull(onError);
        return action.NextResultIf(r => r.Error, r => CommonAction.Create(t => onError(r.Exception!), excuteSafely).Next(r));
    }
}