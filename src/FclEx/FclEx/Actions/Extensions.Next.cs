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

        public static IAction<Unit> Next<T>(this IAction<T> action, Action<T> next)
        {
            return action.Next(r => CommonAction.Create(t => next(r), excuteSafely: false));
        }

        public static IAction<Unit> Next<T>(this IAction<T> action, Func<T, Task> next)
        {
            return action.Next(r => CommonAction.Create(t => next(r), excuteSafely: false));
        }

        public static IAction<TNext> Next<T1, T2, TNext>(this IAction<(T1, T2)> action, Func<T1, T2, IAction<TNext>> next)
        {
            return action.Next(m => next(m.Item1, m.Item2));
        }

        public static IAction<T> TryNext<T>(this IAction<T> action, Func<T, IAction<T>> next,
            bool errorWhenNextNull = false, bool prevWhenNextError = true)
        {
            return new NextAction<T>(action, next, errorWhenNextNull, prevWhenNextError);
        }
    }
}