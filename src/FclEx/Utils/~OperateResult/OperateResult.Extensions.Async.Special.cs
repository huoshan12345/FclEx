using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace FclEx.Utils
{
    public static partial class OperateResultExtensions
    {
        public static Task<OperateResult> Ok(this Task<OperateResult> @this, Action<TimeSpan> action)
        {
            return @this.Ok<IUnit, OperateResult>((o, t) => action(t));
        }

        public static Task<OperateResult> Ok(this Task<OperateResult> @this, Func<TimeSpan, Task> action)
        {
            return @this.Ok<IUnit, OperateResult>((o, t) => action(t));
        }

        public static Task<OperateResult<T>> Ok<T>(this Task<OperateResult<T>> @this, Action<T, TimeSpan> action)
        {
            return @this.Ok<T, OperateResult<T>>(action);
        }

        public static Task<OperateResult<T>> Ok<T>(this Task<OperateResult<T>> @this, Action<T> action)
        {
            return @this.Ok<T, OperateResult<T>>((o, t) => action(o));
        }

        public static Task<OperateResult<T>> Ok<T>(this Task<OperateResult<T>> @this, Func<T, TimeSpan, Task> action)
        {
            return @this.Ok<T, OperateResult<T>>(action);
        }

        public static Task<OperateResult<T>> Ok<T>(this Task<OperateResult<T>> @this, Func<T, Task> action)
        {
            return @this.Ok<T, OperateResult<T>>((o, t) => action(o));
        }
    }
}
