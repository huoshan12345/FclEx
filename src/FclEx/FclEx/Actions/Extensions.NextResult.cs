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
        public static IAction<T> NextResult<T>(this IAction<T> action, Func<OperateResult<T>, IAction<T>> next, bool errorWhenNextNull = true)
        {
            return new NextResultAction<T>(action, next, errorWhenNextNull);
        }

        public static IAction<TNext> NextResult<T, TNext>(this IAction<T> action, Func<OperateResult<T>, IAction<TNext>> next)
        {
            Guard.Argument(next, nameof(next)).NotNull();
            return new NextResultAction<T, TNext>(action, next);
        }

        public static IAction<T> NextResultIf<T>(this IAction<T> action, Func<OperateResult<T>, bool> condition, Func<OperateResult<T>, IAction<T>> next)
        {
            Guard.Argument(condition, nameof(condition)).NotNull();
            Guard.Argument(next, nameof(next)).NotNull();

            return action.NextResult(r => condition(r) ? next(r) : new ResultAction<T>(r));
        }
    }
}
