using System;
using System.Collections.Generic;
using Dawn;
using FclEx.Http.Actions;
using FclEx.Utils;

namespace FclEx.Http
{
    public static class ActionFutureExtensions
    {
        public static IActionFuture PushAction(this IActionFuture future, IActor action)
        {
            Guard.Argument(action, nameof(action)).NotNull();
            return future.PushAction(objs => action);
        }

        public static IActionFuture PushAction(this IActionFuture future,
            Func<object, IActor> func, int dependentResultIndex)
        {
            Guard.Argument(func, nameof(func)).NotNull();
            return future.PushAction(objs => func(objs[dependentResultIndex]));
        }

        public static IActionFuture PushAction<TResult>(this IActionFuture future,
            Func<TResult, IActor> func, int dependentResultIndex)
        {
            Guard.Argument(func, nameof(func)).NotNull();
            return future.PushAction(objs => func(objs[dependentResultIndex].CastTo<TResult>()));
        }

        public static IActionFuture PushAction(this IActionFuture future, Func<object, IActor> func)
        {
            Guard.Argument(func, nameof(func)).NotNull();
            return PushAction(future, func, future.Count - 1);
        }

        public static IActionFuture PushAction<TResult>(this IActionFuture future, Func<TResult, IActor> func)
        {
            Guard.Argument(func, nameof(func)).NotNull();
            return PushAction<TResult>(future, func, future.Count - 1);
        }

        public static IActionFuture PushActionIf(this IActionFuture future, Func<object, bool> predicate,
            Func<object, IAction> func)
        {
            Guard.Argument(predicate, nameof(predicate)).NotNull();
            return PushAction(future, o => predicate(o) ? func(o) : null);
        }

        public static IActionFuture PushActionIf<TResult>(this IActionFuture future, Func<TResult, bool> predicate,
            Func<TResult, IAction> func)
        {
            Guard.Argument(predicate, nameof(predicate)).NotNull();
            return PushAction<TResult>(future, o => predicate(o) ? func(o) : null);
        }


        public static IActionFuture PushActions(this IActionFuture future, IEnumerable<IActor> actions)
        {
            foreach (var action in actions)
            {
                PushAction(future, action);
            }
            return future;
        }
    }
}
