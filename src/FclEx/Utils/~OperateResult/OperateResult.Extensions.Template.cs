using System;
using Dawn;


namespace FclEx.Utils
{
    public static partial class OperateResultExtensions
    {
        public static OperateResult<T> Ok<T>(this OperateResult<T> @this, Action<T, TimeSpan> action)
        {
            return @this.On(r => r.Successful, t => action(t.Result, t.Elapsed));
        }

        public static OperateResult<T> Ok<T>(this OperateResult<T> @this, Action<T> action)
        {
            return @this.Ok((r, t) => action(r));
        }

        public static OperateResult Unwrap(this OperateResult<OperateResult> result)
        {
            var (successful, elapsed, innerResult, exception) = result;
            return successful
                ? innerResult.WithElapsed(elapsed)
                : OperateResult.CreateError(exception!, elapsed);
        }

        public static OperateResult<T> Unwrap<T>(this OperateResult<OperateResult<T>> result)
        {
            var (successful, elapsed, innerResult, exception) = result;
            return successful
                ? innerResult.WithElapsed(elapsed)
                : OperateResult.CreateError<T>(exception!, elapsed);
        }

        public static OperateResult<TDest> Map<T, TDest>(this OperateResult<T> result, Func<T, TDest> func)
        {
            Guard.Argument(func, nameof(func)).NotNull();
            return result.Successful
                ? OperateResult.CreateSuccess(func(result.Result))
                : result.ToExplicit<TDest>();
        }

        public static OperateResult<TDest> Bind<T, TDest>(this OperateResult<T> result, Func<T, OperateResult<TDest>> func)
        {
            Guard.Argument(func, nameof(func)).NotNull();
            return result.Successful
                ? func(result.Result)
                : result.ToExplicit<TDest>();
        }
    }
}
