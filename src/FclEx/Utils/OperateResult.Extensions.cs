using System;


namespace FclEx.Utils
{
    public static partial class OperateResultExtensions
    {
        public static bool HasError<T>(this IOperateResult<T> result)
        {
            return !result.Successful;
        }

        public static bool IsStrErr<T>(this IOperateResult<T> r)
        {
            return r.Code == OperateResultCodes.FromString;
        }

        public static bool IsExErr<T>(this IOperateResult<T> r)
        {
            return r.Code == OperateResultCodes.FromException;
        }

        public static bool IsCancelErr<T>(this IOperateResult<T> r)
        {
            return r.Code == OperateResultCodes.Cancel;
        }

        public static IOperateResult<TTarget> ToExplicit<T, TTarget>(this IOperateResult<T> r)
        {
            if (r.Successful)
                throw new InvalidOperationException("cannot convert to explicit when result is successful");
            else
                return new OperateResult<TTarget>(r.Code, r.Exception, r.Elapsed);
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

        public static IOperateResult<T> Error<T>(this IOperateResult<T> @this, Action<Exception> action)
        {
            return @this.On(r => !r.Successful, t => action(t.Exception));
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
