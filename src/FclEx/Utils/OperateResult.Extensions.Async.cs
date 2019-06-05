using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace FclEx.Utils
{
    public static partial class OperateResultExtensions
    {
        public static Task<IOperateResult<T>> Ok<T>(this Task<IOperateResult<T>> @this, Action<T, TimeSpan> action)
        {
            return @this.On(r => r.Successful, t => action(t.Result, t.Elapsed));
        }

        public static Task<OperateResult> Ok(this Task<OperateResult> @this, Action<TimeSpan> action)
        {
            return @this.On(r => r.Successful, t => action(t.Elapsed));
        }

        public static Task<IOperateResult<T>> Ok<T>(this Task<IOperateResult<T>> @this, Func<T, TimeSpan, Task> action)
        {
            return @this.On(r => r.Successful, t => action(t.Result, t.Elapsed));
        }

        public static Task<OperateResult> Ok(this Task<OperateResult> @this, Func<TimeSpan, Task> action)
        {
            return @this.On(r => r.Successful, t => action(t.Elapsed));
        }

        public static Task<IOperateResult<T>> Error<T>(this Task<IOperateResult<T>> @this, Action<Exception> action)
        {
            return @this.On(r => !r.Successful, t => action(t.Exception));
        }

        public static Task<OperateResult> Error(this Task<OperateResult> @this, Action<Exception> action)
        {
            return @this.On(r => !r.Successful, t => action(t.Exception));
        }

        public static Task<IOperateResult<T>> Error<T>(this Task<IOperateResult<T>> @this, Func<Exception, Task> action)
        {
            return @this.On(r => !r.Successful, t => action(t.Exception));
        }

        public static Task<OperateResult> Error(this Task<OperateResult> @this, Func<Exception, Task> action)
        {
            return @this.On(r => !r.Successful, t => action(t.Exception));
        }

        public static Task<IOperateResult<T>> ThrowIfError<T>(this Task<IOperateResult<T>> @this)
        {
            return @this.Error(e => e.ReThrow());
        }

        public static Task<OperateResult> ThrowIfError(this Task<OperateResult> @this)
        {
            return @this.Error(e => e.ReThrow());
        }

        public static IOperateResult<T> Cancel<T>(this IOperateResult<T> @this, Func<Exception, Task> action)
        {
            return @this.On(r => r.IsCancelErr(), t => action(t.Exception));
        }

        public static OperateResult Cancel(this OperateResult @this, Func<Exception, Task> action)
        {
            return @this.On(r => r.IsCancelErr(), t => action(t.Exception));
        }

        public static Task<IOperateResult<T>> OkResult<T>(this Task<IOperateResult<T>> @this, Action<IOperateResult<T>> action)
        {
            return @this.On(m => m.Successful, action);
        }

        public static Task<OperateResult> OkResult(this Task<OperateResult> @this, Action<OperateResult> action)
        {
            return @this.On(m => m.Successful, action);
        }

        public static Task<IOperateResult<T>> ErrorResult<T>(this Task<IOperateResult<T>> @this, Action<IOperateResult<T>> action)
        {
            return @this.On(m => !m.Successful, action);
        }

        public static Task<OperateResult> ErrorResult(this Task<OperateResult> @this, Action<OperateResult> action)
        {
            return @this.On(m => !m.Successful, action);
        }

        public static Task<IOperateResult<T>> OkResult<T>(this Task<IOperateResult<T>> @this, Func<IOperateResult<T>, Task> action)
        {
            return @this.On(m => m.Successful, action);
        }

        public static Task<OperateResult> OkResult(this Task<OperateResult> @this, Func<OperateResult, Task> action)
        {
            return @this.On(m => m.Successful, action);
        }

        public static Task<IOperateResult<T>> CancelResult<T>(this Task<IOperateResult<T>> @this, Func<IOperateResult<T>, Task> action)
        {
            return @this.On(r => r.IsCancelErr(), action);
        }

        public static Task<OperateResult> CancelResult<T>(this Task<OperateResult> @this, Func<OperateResult, Task> action)
        {
            return @this.On(r => r.IsCancelErr(), action);
        }

        public static Task<IOperateResult<T>> ErrorResult<T>(this Task<IOperateResult<T>> @this, Func<IOperateResult<T>, Task> action)
        {
            return @this.On(m => !m.Successful, action);
        }

        public static Task<OperateResult> ErrorResult<T>(this Task<OperateResult> @this, Func<OperateResult, Task> action)
        {
            return @this.On(m => !m.Successful, action);
        }
    }
}
