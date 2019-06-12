using System;


namespace FclEx.Utils
{
    public static partial class OperateResultExtensions
    {
        public static bool HasError(this IOperateResult result)
        {
            return !result.Successful;
        }

        public static bool IsStrErr(this IOperateResult r)
        {
            return r.Code == OperateResultCodes.FromString;
        }

        public static bool IsExErr(this IOperateResult r)
        {
            return r.Code == OperateResultCodes.FromException;
        }

        public static bool IsCancelErr(this IOperateResult r)
        {
            return r.Code == OperateResultCodes.Cancel;
        }

        public static OperateResult<T> Unwrap<T>(this IOperateResult<OperateResult<T>> result)
        {
            if (result.Successful && result.Result.Successful)
                return OperateUtil.CreateSuccess<T>(result.Result.Result, result.Elapsed);
            else if (!result.Successful)
                return OperateUtil.CreateError<T>(result.Exception, result.Elapsed);
            else return result.Result;
        }

        public static IOperateResult<T> Unwrap<T>(this IOperateResult<IOperateResult<T>> result)
        {
            if (result.Successful && result.Result.Successful)
                return OperateUtil.CreateSuccess<T>(result.Result.Result, result.Elapsed);
            else if (!result.Successful)
                return OperateUtil.CreateError<T>(result.Exception, result.Elapsed);
            else return result.Result;
        }

        public static OperateResult Unwrap(this IOperateResult<OperateResult> result)
        {
            if (result.Successful && result.Result.Successful)
                return OperateUtil.CreateSuccess(result.Elapsed);
            else if (!result.Successful)
                return OperateUtil.CreateError(result.Exception, result.Elapsed);
            else return result.Result;
        }

        public static IOperateResult<T> Ok<T>(this IOperateResult<T> @this, Action<T, TimeSpan> action)
        {
            return @this.On(r => r.Successful, t => action(t.Result, t.Elapsed));
        }

        public static OperateResult Ok(this OperateResult @this, Action<TimeSpan> action)
        {
            return @this.On(r => r.Successful, t => action(t.Elapsed));
        }

        public static OperateResult<T> Ok<T>(this OperateResult<T> @this, Action<T> action)
        {
            return @this.On(r => r.Successful, t => action(t.Result));
        }

        public static IOperateResult<T> Error<T>(this IOperateResult<T> @this, Action<Exception> action)
        {
            return @this.On(r => !r.Successful, t => action(t.Exception));
        }

        public static IOperateResult<T> StrError<T>(this IOperateResult<T> @this, Action<Exception> action)
        {
            return @this.On(r => r.IsStrErr(), t => action(t.Exception));
        }

        public static IOperateResult<T> ExError<T>(this IOperateResult<T> @this, Action<Exception> action)
        {
            return @this.On(r => r.IsExErr(), t => action(t.Exception));
        }

        public static IOperateResult<T> NonExError<T>(this IOperateResult<T> @this, Action<Exception> action)
        {
            return @this.On(r => r.HasError() && !r.IsExErr(), t => action(t.Exception));
        }

        public static IOperateResult<T> Cancel<T>(this IOperateResult<T> @this, Action<Exception> action)
        {
            return @this.On(r => r.IsCancelErr(), t => action(t.Exception));
        }

        public static IOperateResult<T> ThrowIfError<T>(this IOperateResult<T> @this)
        {
            return @this.Error(e => e.ReThrow());
        }

        public static IOperateResult<T> OkResult<T>(this IOperateResult<T> @this, Action<IOperateResult<T>> action)
        {
            return @this.On(m => m.Successful, action);
        }

        public static IOperateResult<T> CancelResult<T>(this IOperateResult<T> @this, Action<IOperateResult<T>> action)
        {
            return @this.On(r => r.IsCancelErr(), action);
        }

        public static IOperateResult<T> ErrorResult<T>(this IOperateResult<T> @this, Action<IOperateResult<T>> action)
        {
            return @this.On(m => !m.Successful, action);
        }
    }
}
