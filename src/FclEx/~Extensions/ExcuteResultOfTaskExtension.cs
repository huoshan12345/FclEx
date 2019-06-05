using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FclEx.Utils;
using OperateResult = FclEx.Utils.IOperateResult<FclEx.Utils.IUnit>;

namespace FclEx
{
    [Obsolete("使用" + nameof(OperateResult))]
    public static class ExcuteResultOfTaskExtension
    {
        public static Task<ExcuteResult> Ok(this Task<ExcuteResult> @this, Action action)
        {
            return @this.Ok(t => action());
        }

        public static Task<ExcuteResult> Ok(this Task<ExcuteResult> @this, Action<TimeSpan> action)
        {
            return @this.On(r => r.Successful, t => action(t.Elapsed));
        }

        public static Task<ExcuteResult> Error(this Task<ExcuteResult> @this, Action<Exception> action)
        {
            return @this.On(r => !r.Successful, t => action(t.Exception));
        }

        public static Task<ExcuteResult> ThrowIfError(this Task<ExcuteResult> @this)
        {
            return @this.Error(e => e.ReThrow());
        }

        public static Task<ExcuteResult> Ok(this Task<ExcuteResult> @this, Func<Task> action)
        {
            return @this.Ok(t => action());
        }

        public static Task<ExcuteResult> Ok(this Task<ExcuteResult> @this, Func<TimeSpan, Task> action)
        {
            return @this.On(r => r.Successful, t => action(t.Elapsed));
        }

        public static Task<ExcuteResult> Error(this Task<ExcuteResult> @this, Func<Exception, Task> action)
        {
            return @this.On(r => !r.Successful, t => action(t.Exception));
        }

        public static Task<ExcuteResult<T>> Ok<T>(this Task<ExcuteResult<T>> @this, Action<T> action)
        {
            return @this.Ok((r, t) => action(r));
        }

        public static Task<ExcuteResult<T>> Ok<T>(this Task<ExcuteResult<T>> @this, Action<T, TimeSpan> action)
        {
            return @this.On(r => r.Successful, t => action(t.Result, t.Elapsed));
        }

        public static Task<ExcuteResult<T>> Error<T>(this Task<ExcuteResult<T>> @this, Action<Exception> action)
        {
            return @this.On(r => !r.Successful, t => action(t.Exception));
        }

        public static Task<ExcuteResult<T>> ThrowIfError<T>(this Task<ExcuteResult<T>> @this)
        {
            return @this.Error(e => e.ReThrow());
        }

        public static Task<ExcuteResult<T>> Ok<T>(this Task<ExcuteResult<T>> @this, Func<T, Task> action)
        {
            return @this.Ok((r, t) => action(r));
        }

        public static Task<ExcuteResult<T>> Ok<T>(this Task<ExcuteResult<T>> @this, Func<T, TimeSpan, Task> action)
        {
            return @this.On(r => r.Successful, t => action(t.Result, t.Elapsed));
        }

        public static Task<ExcuteResult<T>> Error<T>(this Task<ExcuteResult<T>> @this, Func<Exception, Task> action)
        {
            return @this.On(r => !r.Successful, t => action(t.Exception));
        }
    }
}
