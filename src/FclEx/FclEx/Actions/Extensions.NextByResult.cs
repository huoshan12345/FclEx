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
        public static IAction<T> NextByResult<T>(this IAction<T> action, Func<OperateResult<T>, IAction<T>> next, bool errorWhenNextNull = true)
        {
            return new NextByResultAction<T>(action, next, errorWhenNextNull);
        }

        public static IAction<TNext> NextByResult<T, TNext>(this IAction<T> action, Func<OperateResult<T>, IAction<TNext>> next)
        {
            Guard.Argument(next, nameof(next)).NotNull();
            return new NextByResultAction<T, TNext>(action, next);
        }

        public static IAction<T> NextByResultIf<T>(this IAction<T> action, Func<OperateResult<T>, bool> condition, Func<OperateResult<T>, IAction<T>> next)
        {
            Guard.Argument(condition, nameof(condition)).NotNull();
            Guard.Argument(next, nameof(next)).NotNull();

            return action.NextByResult(r => condition(r) ? next(r) : new ResultAction<T>(r));
        }
    }
}
