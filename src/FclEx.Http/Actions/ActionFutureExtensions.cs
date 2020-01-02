using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;
using FclEx.Utils;

namespace FclEx.Http.Actions
{
    public static class ActionFutureExtensions
    {
        public static IActionFuture PushAction(this IActionFuture future, IActor action)
        {
            Guard.Argument(action, nameof(action)).NotNull();
            return future.PushAction(objs => action);
        }

        public static IActionFuture PushAction<TResult>(this IActionFuture future, int dependentResultIndex,
            Func<TResult, IActor> func)
        {
            Guard.Argument(func, nameof(func)).NotNull();
            return future.PushAction((IOperateResult[] objs) =>
            {
                var r = objs[dependentResultIndex];
                return func(r.ToExplicit<TResult>().Result);
            });
        }

        public static IActionFuture PushAction<TResult>(this IActionFuture future, Func<TResult, IActor> func)
        {
            Guard.Argument(func, nameof(func)).NotNull();
            return PushAction<TResult>(future, future.Count - 1, func);
        }

        public static IActionFuture PushActions(this IActionFuture future, IEnumerable<IActor> actions)
        {
            foreach (var action in actions)
            {
                PushAction(future, action);
            }
            return future;
        }

        public static IActionFuture PushActionIf<TResult>(this IActionFuture future, Func<TResult, bool> predicate,
            Func<TResult, IAction> func)
        {
            Guard.Argument(predicate, nameof(predicate)).NotNull();
            return PushAction<TResult>(future, o => predicate(o) ? func(o) : null);
        }

        public static IActionFuture PushActionIf<TLastResult, TDependentResult>(this IActionFuture future, Func<TLastResult, bool> predicate, int dependentIndex,
            Func<TDependentResult, IAction> func)
        {
            Guard.Argument(predicate, nameof(predicate)).NotNull();
            return future.PushAction((IOperateResult[] objs) =>
            {
                var last = objs.Last().CastTo<TLastResult>();
                if (!predicate(last)) return null;
                var dependent = objs[dependentIndex].ToExplicit<TDependentResult>().Result;
                return func(dependent);
            });
        }
    }
}
