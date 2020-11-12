using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace FclEx.Utils
{
    public static partial class OperateResultExtensions
    {
        public static Task<OperateResult<T>> Ok<T>(this Task<OperateResult<T>> @this, Action<T, TimeSpan> action)
        {
            return @this.On(r => r.Successful, t => action(t.Result!, t.Elapsed));
        }

        public static Task<OperateResult<T>> Ok<T>(this Task<OperateResult<T>> @this, Action<T> action)
        {
            return @this.Ok((r, t) => action(r));
        }

        public static Task<OperateResult<T>> Ok<T>(this Task<OperateResult<T>> @this, Func<T, TimeSpan, Task> action)
        {
            return @this.On(r => r.Successful, t => action(t.Result!, t.Elapsed));
        }

        public static Task<OperateResult<T>> Ok<T>(this Task<OperateResult<T>> @this, Func<T, Task> action)
        {
            return @this.Ok((r, t) => action(r));
        }
    }
}
