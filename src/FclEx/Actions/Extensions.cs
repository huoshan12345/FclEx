using System;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Utils;

namespace FclEx.Actions
{
    public static class Extensions
    {
        public static IAction<T2> Map<T, T2>(this IAction<T> action, Func<T, T2> map)
        {
            return new MapAction<T, T2>(action, map);
        }

        public static IAction<T2> Bind<T, T2>(this IAction<T> action, Func<T, OperateResult<T2>> map)
        {
            return new BindAction<T, T2>(action, map);
        }

        public static IAction<(T Cur, TNext Next)> Union<T, TNext>(this IAction<T> action, Func<T, IAction<TNext>> next)
        {
            return new UnionAction<T, TNext>(action, next);
        }

        public static IAction<(T Cur, TNext Next)> Union<T, TNext>(this IAction<T> action, Func<T, OperateResult<TNext>> next)
        {
            return new UnionAction<T, TNext>(action, m => CommonAction.Create(() => next(m)));
        }

        public static IAction<(T1, T2, TNext)> Union<T1, T2, TNext>(this IAction<(T1, T2)> action, Func<T1, T2, IAction<TNext>> next)
        {
            return new UnionAction<(T1, T2), TNext>(action, m => next(m.Item1, m.Item2)).Map(m => (m.Item1.Item1, m.Item1.Item2, m.Item2));
        }

        public static IAction<TNext> Next<T, TNext>(this IAction<T> action, Func<T, IAction<TNext>> next)
        {
            return new UnionAction<T, TNext>(action, next).Map(m => m.Item2);
        }

        public static IAction<TNext> Next<T, TNext>(this IAction<T> action, Func<IAction<TNext>> next)
        {
            return action.Next(_ => next());
        }

        public static IAction<TNext> Next<T1, T2, TNext>(this IAction<(T1, T2)> action, Func<T1, T2, IAction<TNext>> next)
        {
            return new UnionAction<(T1, T2), TNext>(action, m => next(m.Item1, m.Item2)).Map(m => m.Item2);
        }

        public static Task<OperateResult> RunAsync<T>(this IAction<T> action, CancellationToken token = default)
        {
            return action.ExecuteAsync(token).ToUntyped();
        }

        public static Task<OperateResult<T>> YieldAsync<T>(this IAction<T> action, CancellationToken token = default)
        {
            return action.ExecuteAsync(token).Implement();
        }

        public static IAction<TNext> Next<T, TNext>(this IAction<T> action, IAction<TNext> next)
        {
            return action.Next(_ => next);
        }

    }
}