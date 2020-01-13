using System;
using System.Threading.Tasks;

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
                return OperateResult.CreateSuccess<T>(result.Result.Result, result.Elapsed);
            else if (!result.Successful)
                return OperateResult.CreateError<T>(result.Exception, result.Elapsed);
            else return result.Result;
        }

        public static IOperateResult<T> Unwrap<T>(this IOperateResult<IOperateResult<T>> result)
        {
            if (result.Successful && result.Result.Successful)
                return OperateResult.CreateSuccess<T>(result.Result.Result, result.Elapsed);
            else if (!result.Successful)
                return OperateResult.CreateError<T>(result.Exception, result.Elapsed);
            else return result.Result;
        }

        public static OperateResult Unwrap(this IOperateResult<OperateResult> result)
        {
            if (result.Successful && result.Result.Successful)
                return OperateResult.CreateSuccess(result.Elapsed);
            else if (!result.Successful)
                return OperateResult.CreateError(result.Exception, result.Elapsed);
            else return result.Result;
        }

        public static IOperateResult Unwrap(this OperateResult<IOperateResult> result)
        {
            var (successful, elapsed, innerResult, exception) = result;
            return successful
                ? innerResult.WithElapsed(elapsed)
                : OperateResult.CreateError(exception, elapsed);
        }

        public static OperateResult ToUntyped(this IOperateResult result)
        {
            return result.Successful
                ? new OperateResult(result.Elapsed)
                : new OperateResult(result.Code, result.Exception, result.Elapsed);
        }

        public static async Task<OperateResult> ToUntyped(this Task<IOperateResult> task)
        {
            return (await task.DonotCapture()).ToUntyped();
        }

        public static async Task<OperateResult<T>> ToExplicit<T>(this Task<IOperateResult> task)
        {
            return (await task.DonotCapture()).ToExplicit<T>();
        }

        public static bool IsObjErr<T>(this IOperateResult result, out T item)
        {
            if (result.Exception is ObjectException<T> ex)
            {
                item = ex.Target;
                return true;
            }
            else
            {
                item = default;
                return false;
            }
        }

        public static bool IsObjErr<T>(this IOperateResult result, Func<T, bool> predicate)
        {
            return result.Exception is ObjectException<T> ex && predicate(ex.Target);
        }
    }
}
