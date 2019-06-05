using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FclEx.Utils;

namespace FclEx
{
    public static class ExcuteResultExtensions
    {
        public static bool HasError(this IExcuteResult result)
        {
            return !result.Successful;
        }

        public static ExcuteResult Unwrap(this ExcuteResult<ExcuteResult> result)
        {
            if (result.Successful && result.Result.Successful) return ExcuteResult.CreateSuccess(result.Elapsed);
            else if (!result.Successful) return result;
            else return result.Result;
        }

        public static ExcuteResult<T> Unwrap<T>(this ExcuteResult<ExcuteResult<T>> result)
        {
            if (result.Successful && result.Result.Successful) return ExcuteResult.CreateSuccess(result.Result.Result, result.Elapsed);
            else if (!result.Successful) return result.ToExplicit<T>();
            else return result.Result;
        }

        public static bool IsStrErr(this IExcuteResult r)
        {
            return r.Code == ExcuteResultCodes.FromString;
        }

        public static bool IsExErr(this IExcuteResult r)
        {
            return r.Code == ExcuteResultCodes.FromException;
        }

        public static ExcuteResult Ok(this ExcuteResult @this, Action action)
        {
            return @this.Ok(t => action());
        }

        public static ExcuteResult Ok(this ExcuteResult @this, Action<TimeSpan> action)
        {
            return @this.On(r => r.Successful, t => action(t.Elapsed));
        }

        public static ExcuteResult Error(this ExcuteResult @this, Action<Exception> action)
        {
            return @this.On(r => !r.Successful, t => action(t.Exception));
        }

        public static ExcuteResult ThrowIfError(this ExcuteResult @this)
        {
            return @this.Error(e => e.ReThrow());
        }

        public static ExcuteResult<T> Ok<T>(this ExcuteResult<T> @this, Action<T> action)
        {
            return @this.Ok((r, t) => action(r));
        }

        public static ExcuteResult<T> Ok<T>(this ExcuteResult<T> @this, Action<T, TimeSpan> action)
        {
            return @this.On(r => r.Successful, t => action(t.Result, t.Elapsed));
        }

        public static ExcuteResult<T> Error<T>(this ExcuteResult<T> @this, Action<Exception> action)
        {
            return @this.On(r => !r.Successful, t => action(t.Exception));
        }

        public static ExcuteResult<T> ThrowIfError<T>(this ExcuteResult<T> @this)
        {
            return @this.Error(e => e.ReThrow());
        }

        public static ExcuteResult ExErr(this ExcuteResult @this, Action<Exception> action)
        {
            return @this.On(r => !r.Successful && r.IsExErr(), t => action(t.Exception));
        }

        public static ExcuteResult NonExErr(this ExcuteResult @this, Action<Exception> action)
        {
            return @this.On(r => !r.Successful && !r.IsExErr(), t => action(t.Exception));
        }

        public static ExcuteResult<T> OkResult<T>(this ExcuteResult<T> @this, Action<ExcuteResult<T>> action)
        {
            return @this.On(m => m.Successful, action);
        }

        public static ExcuteResult<T> ErrorResult<T>(this ExcuteResult<T> @this, Action<ExcuteResult<T>> action)
        {
            return @this.On(m => !m.Successful, action);
        }

        public static ExcuteResult OkResult(this ExcuteResult @this, Action<ExcuteResult> action)
        {
            return @this.On(m => m.Successful, action);
        }

        public static ExcuteResult ErrorResult(this ExcuteResult @this, Action<ExcuteResult> action)
        {
            return @this.On(m => !m.Successful, action);
        }
    }
}
