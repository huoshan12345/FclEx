using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Dawn;
using FclEx.Utils;

namespace FclEx.Actions
{
    public static class ActionFutureExtensions
    {
        [return: MaybeNull]
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
            Func<IOperateResult, IActor?> func)
        {
            Guard.Argument(func, nameof(func)).NotNull();
            return future.PushAction((IOperateResult[] objs) => func(objs[dependentResultIndex]));
        }

        public static IActionFuture PushAction<TResult>(this IActionFuture future, int dependentResultIndex,
            Func<TResult, IActor?> func)
        {
            Guard.Argument(func, nameof(func)).NotNull();
            Guard.Argument(func, nameof(func)).NotNull();
            return future.PushAction(dependentResultIndex, r =>
            {
                var obj = r.GetPossibleResultObject<TResult>();
#pragma warning disable CS8604 // Possible null reference argument.
                return func(obj);
#pragma warning restore CS8604 // Possible null reference argument.
            });
        }

        public static IActionFuture PushAction<TResult>(this IActionFuture future, Func<TResult, IActor?> func)
        {
            Guard.Argument(func, nameof(func)).NotNull();
            return PushAction<TResult>(future, future.Count - 1, func);
        }
        public static IActionFuture PushActionIf<TResult>(this IActionFuture future, Func<TResult, bool> predicate,
            Func<TResult, IActor> func)
        {
            Guard.Argument(predicate, nameof(predicate)).NotNull();
            Guard.Argument(func, nameof(func)).NotNull();
            return PushAction<TResult>(future, o => predicate(o) ? func(o) : null);
        }

        public static IActionFuture PushActionIf(this IActionFuture future,
            Func<IOperateResult, bool> predicate, int dependentIndex, Func<IOperateResult, IActor?> func)
        {
            Guard.Argument(predicate, nameof(predicate)).NotNull();
            Guard.Argument(func, nameof(func)).NotNull();

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
            Func<TLastResult, bool> predicate, int dependentIndex, Func<TDependentResult, IActor?> func)
        {
            Guard.Argument(predicate, nameof(predicate)).NotNull();
            Guard.Argument(func, nameof(func)).NotNull();

            return future.PushActionIf(r =>
            {
                var lastObj = r.GetPossibleResultObject<TLastResult>();
#pragma warning disable CS8604 // Possible null reference argument.
                return predicate(lastObj);
#pragma warning restore CS8604 // Possible null reference argument.
            }, dependentIndex,
            r =>
            {
                var dependentObj = r.GetPossibleResultObject<TDependentResult>();
#pragma warning disable CS8604 // Possible null reference argument.
                return func(dependentObj);
#pragma warning restore CS8604 // Possible null reference argument.
            });
        }

        public static IActionFuture PushActionIf(this IActionFuture future, Func<IOperateResult, bool> predicate,
            Func<IOperateResult, IActor?> func)
        {
            Guard.Argument(predicate, nameof(predicate)).NotNull();
            Guard.Argument(func, nameof(func)).NotNull();

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
