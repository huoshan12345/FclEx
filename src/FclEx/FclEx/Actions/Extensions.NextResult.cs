using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dawn;
using FclEx.Utils;

namespace FclEx.Actions
{
    partial class Extensions
    {
        public static IAction<TNext> NextResult<T, TNext>(this IAction<T> action, TNext result)
        {
            return action.Next(_ => new ResultAction<TNext>(OperateResult.CreateSuccess(result)));
        }

        public static IAction<TNext> NextResult<T, TNext>(this IAction<T> action, OperateResult<TNext> result)
        {
            return action.Next(_ => new ResultAction<TNext>(result));
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
    }
}
