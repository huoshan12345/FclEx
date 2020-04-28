using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace FclEx.Utils
{
    public static partial class OperateResultExtensions
    {
        public static Task<TResult> Ok<TResult>(this Task<TResult> @this, Action action) where TResult : IOperateResult
        {
            return @this.On(r => r.Successful, t => action());
        }

        public static Task<TResult> Ok<TResult>(this Task<TResult> @this, Action<TimeSpan> action) where TResult : IOperateResult
        {
            return @this.On(r => r.Successful, t => action(t.Elapsed));
        }

        public static Task<TResult> Ok<TResult>(this Task<TResult> @this, Func<TimeSpan, Task> action) where TResult : IOperateResult
        {
            return @this.On(r => r.Successful, t => action(t.Elapsed));
        }

        public static Task<TResult> Error<TResult>(this Task<TResult> @this, Action<Exception> action) where TResult : IOperateResult
        {
            return @this.On(r => r.HasError(), t => action(t.Exception!));
        }

        public static Task<TResult> Error<TResult>(this Task<TResult> @this, Func<Exception, Task> action) where TResult : IOperateResult
        {
            return @this.On(r => r.HasError(), t => action(t.Exception!));
        }

        public static Task<TResult> ThrowIfError<TResult>(this Task<TResult> @this) where TResult : IOperateResult
        {
            return @this.Error(e => e.ReThrow());
        }

        public static Task<TResult> Cancel<TResult>(this Task<TResult> @this, Func<Exception, Task> action) where TResult : IOperateResult
        {
            return @this.On(r => r.IsCancelErr(), t => action(t.Exception!));
        }

        public static Task<TResult> OkResult<TResult>(this Task<TResult> @this, Action<TResult> action) where TResult : IOperateResult
        {
            return @this.On(m => m.Successful, action);
        }

        public static Task<TResult> OkResult<TResult>(this Task<TResult> @this, Func<TResult, Task> action) where TResult : IOperateResult
        {
            return @this.On(m => m.Successful, action);
        }

        public static Task<TResult> ErrorResult<TResult>(this Task<TResult> @this, Action<TResult> action) where TResult : IOperateResult
        {
            return @this.On(r => r.HasError(), action);
        }

        public static Task<TResult> ErrorResult<TResult>(this Task<TResult> @this, Func<TResult, Task> action) where TResult : IOperateResult
        {
            return @this.On(r => r.HasError(), action);
        }

        public static Task<TResult> CancelResult<TResult>(this Task<TResult> @this, Action<TResult> action) where TResult : IOperateResult
        {
            return @this.On(r => r.IsCancelErr(), action);
        }

        public static Task<TResult> CancelResult<TResult>(this Task<TResult> @this, Func<TResult, Task> action) where TResult : IOperateResult
        {
            return @this.On(r => r.IsCancelErr(), action);
        }
    }
}
