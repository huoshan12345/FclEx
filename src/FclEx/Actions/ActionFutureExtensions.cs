using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;
using FclEx.Utils;

namespace FclEx.Http.Actions
{
    public static class ActionFutureExtensions
    {
        public static T GetPossibleResultObject<T>(this IOperateResult result)
        {
            var (successful, _, obj, ex) = result.ToExplicit<T>();
            if (successful) return obj;
            else if (ex is ObjectException<T> objEx) return objEx.Target;
            return default;
        }

        public static IActionFuture PushAction(this IActionFuture future, IActor action)
        {
            Guard.Argument(action, nameof(action)).NotNull();
            return future.PushAction(objs => action);
        }

        public static IActionFuture PushActions(this IActionFuture future, IEnumerable<IActor> actions)
        {
            foreach (var action in actions)
            {
                PushAction(future, action);
            }
            return future;
        }

        public static IActionFuture PushAction(this IActionFuture future, int dependentResultIndex,
            Func<IOperateResult, IActor> func)
        {
            Guard.Argument(func, nameof(func)).NotNull();
            return future.PushAction((IOperateResult[] objs) => func(objs[dependentResultIndex]));
        }

        public static IActionFuture PushAction<TResult>(this IActionFuture future, int dependentResultIndex,
            Func<TResult, IActor> func)
        {
            Guard.Argument(func, nameof(func)).NotNull();
            return future.PushAction(dependentResultIndex, r =>
            {
                var obj = r.GetPossibleResultObject<TResult>();
                return func(obj);
            });
        }

        public static IActionFuture PushAction<TResult>(this IActionFuture future, Func<TResult, IActor> func)
        {
            Guard.Argument(func, nameof(func)).NotNull();
            return PushAction<TResult>(future, future.Count - 1, func);
        }
        public static IActionFuture PushActionIf<TResult>(this IActionFuture future, Func<TResult, bool> predicate,
            Func<TResult, IActor> func)
        {
            Guard.Argument(predicate, nameof(predicate)).NotNull();
            return PushAction<TResult>(future, o => predicate(o) ? func(o) : null);
        }

        public static IActionFuture PushActionIf(this IActionFuture future,
            Func<IOperateResult, bool> predicate, int dependentIndex, Func<IOperateResult, IActor> func)
        {
            Guard.Argument(predicate, nameof(predicate)).NotNull();

            var deptIndex = future.Count - 1;
            return future.PushAction((IOperateResult[] objs) =>
            {
                var last = objs[deptIndex];
                if (!predicate(last)) return null;
                var dependent = objs[dependentIndex];
                return func(dependent);
            });
        }

        public static IActionFuture PushActionIf<TLastResult, TDependentResult>(this IActionFuture future,
            Func<TLastResult, bool> predicate, int dependentIndex, Func<TDependentResult, IActor> func)
        {
            Guard.Argument(predicate, nameof(predicate)).NotNull();
            return future.PushActionIf(r =>
            {
                var lastObj = r.GetPossibleResultObject<TLastResult>();
                return predicate(lastObj);
            }, dependentIndex,
            r =>
            {
                var dependentObj = r.GetPossibleResultObject<TDependentResult>();
                return func(dependentObj);
            });
        }

        public static IActionFuture PushActionIf(this IActionFuture future, Func<IOperateResult, bool> predicate,
            Func<IOperateResult, IActor> func)
        {
            Guard.Argument(predicate, nameof(predicate)).NotNull();
            var deptIndex = future.Count - 1;
            return future.PushAction((IOperateResult[] objs) =>
            {
                var last = objs[deptIndex];
                if (!predicate(last)) return null;
                return func(last);
            });
        }
    }
}
