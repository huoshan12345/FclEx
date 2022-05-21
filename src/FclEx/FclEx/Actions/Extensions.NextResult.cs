using System;
using System.Threading.Tasks;
using FclEx;
using FclEx.Utils;

namespace FclEx.Actions
{
    partial class Extensions
    {
        public static IAction<TNext> NextResult<T, TNext>(this IAction<T> action, Func<OperateResult<T>, TNext> next, bool excuteSafely = true)
        {
            return action.NextResult<T, TNext>(r => CommonAction.Create(t => next(r), excuteSafely));
        }

        public static IAction<TNext> NextResult<T, TNext>(this IAction<T> action, Func<OperateResult<T>, OperateResult<TNext>> next, bool excuteSafely = true)
        {
            return action.NextResult<T, TNext>(r => CommonAction.Create(t => next(r), excuteSafely));
        }
        
        public static IAction<TNext> NextResult<T, TNext>(this IAction<T> action, Func<OperateResult<T>, IAction<TNext>> next)
        {
            Check.NotNull(next);
            return new NextResultAction<T, TNext>(action, next);
        }
        
        public static IAction<TNext> NextResult<T, TNext>(this IAction<T> action, Func<OperateResult<T>, Task<TNext>> next, bool excuteSafely = true)
        {
            return action.NextResult<T, TNext>(r => CommonAction.Create(t => next(r), excuteSafely));
        }

        public static IAction<TNext> NextResult<T, TNext>(this IAction<T> action, Func<OperateResult<T>, Task<OperateResult<TNext>>> next, bool excuteSafely = true)
        {
            return action.NextResult<T, TNext>(r => CommonAction.Create(t => next(r), excuteSafely));
        }
        

        public static IAction<Unit> NextResult<T>(this IAction<T> action, Func<OperateResult<T>, Task> next, bool excuteSafely = true)
        {
            return action.NextResult<T, Unit>(r => CommonAction.Create(t => next(r), excuteSafely));
        }

        public static IAction<Unit> NextResult<T>(this IAction<T> action, Func<OperateResult<T>, Task<OperateResult>> next, bool excuteSafely = true)
        {
            return action.NextResult<T, Unit>(r => CommonAction.Create(t => next(r), excuteSafely));
        }

        public static IAction<Unit> NextResult<T>(this IAction<T> action, Func<OperateResult<T>, OperateResult> next, bool excuteSafely = true)
        {
            return action.NextResult<T, Unit>(r => CommonAction.Create(t => next(r), excuteSafely));
        }

        public static IAction<Unit> NextResult<T>(this IAction<T> action, Action<OperateResult<T>> next, bool excuteSafely = true)
        {
            return action.NextResult<T, Unit>(r => CommonAction.Create(t => next(r), excuteSafely));
        }
    }
}
