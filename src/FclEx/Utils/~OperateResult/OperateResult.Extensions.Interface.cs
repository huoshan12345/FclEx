using System;
using System.Diagnostics.CodeAnalysis;
using Dawn;

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

        public static IOperateResult Unwrap(this OperateResult<IOperateResult> result)
        {
            var (successful, elapsed, innerResult, exception) = result;
            return successful
                ? innerResult.WithElapsed(elapsed)
                : OperateResult.CreateError(exception!, elapsed);
        }

        public static OperateResult ToUntyped(this IOperateResult result)
        {
            return result.Successful
                ? new OperateResult(result.Elapsed)
                : new OperateResult(result.Code, result.Exception, result.Elapsed);
        }

        public static bool IsObjErr<T>(this IOperateResult result, [MaybeNull] out T item)
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

        public static IOperateResult<TDest> Map<T, TDest>(this IOperateResult<T> result, Func<T, TDest> func)
        {
            Guard.Argument(func, nameof(func)).NotNull();
            return result.Successful
                ? OperateResult.CreateSuccess(func(result.Result))
                : result.ToExplicit<TDest>();
        }

        public static IOperateResult<TDest> Bind<T, TDest>(this IOperateResult<T> result, Func<T, OperateResult<TDest>> func)
        {
            Guard.Argument(func, nameof(func)).NotNull();
            return result.Successful
                ? func(result.Result)
                : result.ToExplicit<TDest>();
        }
    }
}
