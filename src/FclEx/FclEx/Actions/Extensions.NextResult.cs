using System;
using System.Threading.Tasks;
using Dawn;
using FclEx.Utils;

namespace FclEx.Actions
{
    partial class Extensions
    {
        public static IAction<TNext> NextResult<T, TNext>(this IAction<T> action, OperateResult<TNext> result)
        {
            return action.Next<T, TNext>(_ => new ResultAction<TNext>(result));
        }

        public static IAction<TNext> NextResult<T, TNext>(this IAction<T> action, Func<T, TNext> next)
        {
            return new NextAction<T, TNext>(action, m => CommonAction.Create(t => next(m)));
        }

        public static IAction<TNext> NextResult<T, TNext>(this IAction<T> action, Func<T, OperateResult<TNext>> next)
        {
            return new NextAction<T, TNext>(action, m => CommonAction.Create(t => next(m)));
        }

        public static IAction<TNext> NextResult<T, TNext>(this IAction<T> action, Func<T, Task<TNext>> next)
        {
            return new NextAction<T, TNext>(action, m => CommonAction.Create(t => next(m)));
        }

        public static IAction<TNext> NextResult<T, TNext>(this IAction<T> action, Func<T, Task<OperateResult<TNext>>> next)
        {
            return new NextAction<T, TNext>(action, m => CommonAction.Create(t => next(m)));
        }

        public static IAction<TNext> NextResult<T, TNext>(this IAction<T> action, Func<OperateResult<T>, IAction<TNext>> next)
        {
            Guard.Argument(next, nameof(next)).NotNull();
            return new NextResultAction<T, TNext>(action, next);
        }

        public static IAction<T> NextResultIf<T>(this IAction<T> action, Func<OperateResult<T>, IAction<T>> next, bool errorWhenNextNull = true)
        {
            return new NextResultAction<T>(action, next, errorWhenNextNull);
        }

        public static IAction<T> NextResultIf<T>(this IAction<T> action, Func<OperateResult<T>, bool> condition, Func<OperateResult<T>, IAction<T>> next)
        {
            Guard.Argument(condition, nameof(condition)).NotNull();
            Guard.Argument(next, nameof(next)).NotNull();

            return action.Next<T>(r => condition(r) ? next(r) : new ResultAction<T>(r));
        }

    }
}
