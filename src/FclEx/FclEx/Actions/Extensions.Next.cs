using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dawn;
using FclEx.Utils;

namespace FclEx.Actions
{
    partial class Extensions
    {
        public static IAction<TNext> Next<T, TNext>(this IAction<T> action, Func<T, IAction<TNext>> next)
        {
            return new NextAction<T, TNext>(action, next);
        }

        public static IAction<TNext> Next<T, TNext>(this IAction<T> action, Func<IAction<TNext>> next)
        {
            return action.Next(_ => next());
        }

        public static IAction<TNext> Next<T, TNext>(this IAction<T> action, IAction<TNext> next)
        {
            return action.Next(_ => next);
        }

        public static IAction<TNext> Next<T1, T2, TNext>(this IAction<(T1, T2)> action, Func<T1, T2, IAction<TNext>> next)
        {
            return action.Next(m => next(m.Item1, m.Item2));
        }

        public static IAction<T> TryNext<T>(this IAction<T> action, Func<T, IAction<T>?> next,
            bool errorWhenNextNull = false, bool prevWhenNextError = true)
        {
            return new NextAction<T>(action, next, errorWhenNextNull, prevWhenNextError);
        }

        public static IAction<T> NextIf<T>(this IAction<T> action, Func<T, bool> condition, Func<T, IAction<T>> @true, Func<T, IAction<T>> @false)
        {
            Guard.Argument(condition, nameof(condition)).NotNull();
            Guard.Argument(@true, nameof(@true)).NotNull();
            Guard.Argument(@false, nameof(@false)).NotNull();

            return action.Next(t => condition(t) ? @true(t) : @false(t));
        }

        public static IAction<T> NextIf<T>(this IAction<T> action, Func<T, bool> condition, Func<T, IAction<T>> next)
        {
            return action.NextIf<T>(condition, next, m => new SuccessAction<T>(m));
        }

        public static IAction<TNext> NextIf<T, TNext>(this IAction<T> action, Func<T, bool> condition, Func<T, IAction<TNext>> @true, Func<T, IAction<TNext>> @false)
        {
            Guard.Argument(condition, nameof(condition)).NotNull();
            Guard.Argument(@true, nameof(@true)).NotNull();
            Guard.Argument(@false, nameof(@false)).NotNull();

            return action.Next(t => condition(t) ? @true(t) : @false(t));
        }

        public static IAction<Unit> NextIf<T>(this IAction<T> action, Func<T, bool> condition, Func<T, IAction<Unit>> next)
        {
            Guard.Argument(condition, nameof(condition)).NotNull();
            Guard.Argument(next, nameof(next)).NotNull();

            return action.Next(t => condition(t) ? next(t) : new SuccessAction<Unit>(default));
        }

        public static IAction<Unit> Do<T>(this IAction<T> action, Action<T> next)
        {
            return action.Next(r => CommonAction.Create(t => next(r), excuteSafely: false));
        }

        public static IAction<Unit> Do<T>(this IAction<T> action, Func<T, Task> next)
        {
            return action.Next(r => CommonAction.Create(t => next(r), excuteSafely: false));
        }
    }
}