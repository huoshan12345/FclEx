using System;


namespace FclEx.Utils
{
    public static partial class OperateResultExtensions
    {
        public static TResult Ok<TResult>(this TResult @this, Action action) where TResult : IOperateResult
        {
            return @this.On(r => r.Successful, t => action());
        }

        public static OperateResult Ok(this OperateResult @this, Action<TimeSpan> action)
        {
            return @this.On(r => r.Successful, t => action(t.Elapsed));
        }

        public static TResult Error<TResult>(this TResult @this, Action<Exception> action) where TResult : IOperateResult
        {
            return @this.On(r => !r.Successful, t => action(t.Exception!));
        }

        public static TResult StrError<TResult>(this TResult @this, Action<Exception> action) where TResult : IOperateResult
        {
            return @this.On(r => r.IsStrErr(), t => action(t.Exception!));
        }

        public static TResult ExError<TResult>(this TResult @this, Action<Exception> action) where TResult : IOperateResult
        {
            return @this.On(r => r.IsExErr(), t => action(t.Exception!));
        }

        public static TResult NonExError<TResult>(this TResult @this, Action<Exception> action) where TResult : IOperateResult
        {
            return @this.On(r => r.HasError() && !r.IsExErr(), t => action(t.Exception!));
        }

        public static TResult Cancel<TResult>(this TResult @this, Action<Exception> action) where TResult : IOperateResult
        {
            return @this.On(r => r.IsCancelErr(), t => action(t.Exception!));
        }

        public static TResult OkResult<TResult>(this TResult @this, Action<TResult> action) where TResult : IOperateResult
        {
            return @this.On(m => m.Successful, action);
        }

        public static TResult CancelResult<TResult>(this TResult @this, Action<TResult> action) where TResult : IOperateResult
        {
            return @this.On(r => r.IsCancelErr(), action);
        }

        public static TResult ErrorResult<TResult>(this TResult @this, Action<TResult> action) where TResult : IOperateResult
        {
            return @this.On(m => !m.Successful, action);
        }

        public static TResult ThrowIfError<TResult>(this TResult @this) where TResult : IOperateResult
        {
            return @this.Error(e => e.ReThrow());
        }
    }
}
