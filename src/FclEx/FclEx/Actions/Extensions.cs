using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dawn;
using FclEx.Utils;

namespace FclEx.Actions
{
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
            return action.ExecuteAsync(token).ToUntyped();
        }

        public static IAction<T> RepeatOnce<T>(this IAction<T> actor, Func<T, bool> condition)
        {
            return actor.Next(t => condition(t) ? actor : new SuccessAction<T>(t));
        }

        public static IAction<T> ErrorIf<T>(this IAction<T> action, Func<T, bool> condition, Func<T, string> errorFunc)
        {
            Guard.Argument(condition, nameof(condition)).NotNull();
            Guard.Argument(errorFunc, nameof(errorFunc)).NotNull();
            return action.Next(t => condition(t)
                ? (IAction<T>)new ErrorAction<T>(errorFunc(t))
                : new SuccessAction<T>(t));
        }

        public static IAction<T> OneByOne<T>(this IEnumerable<IAction<T>> actions)
        {
            IAction<T> seed = new SuccessAction<T>(default!);
            return actions.Aggregate(seed, (sum, next) => sum.Next(next), m => m);
        }
    }
}