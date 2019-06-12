using System;
using System.Threading.Tasks;

namespace FclEx.Utils
{
    public static partial class OperateUtil
    {
        public static OperateResult Success { get; } = CreateSuccess(TimeSpan.Zero);

        public static OperateResult NotImplemented { get; } = CreateNotImplemented();

        public static OperateResult CreateCancel(Exception ex, TimeSpan elapsed = default) => new OperateResult(OperateResultCodes.Cancel, ex, elapsed);

        public static OperateResult CreateCancel(TimeSpan elapsed = default) => CreateError(OperateResultCodes.Cancel, "the operate was canceled", elapsed);

        public static OperateResult CreateSuccess(TimeSpan elapsed = default) => new OperateResult(elapsed);

        public static OperateResult CreateError(int code, string error, TimeSpan elapsed = default) => new OperateResult(code, new SimpleException(error), elapsed);

        public static OperateResult CreateError(string error, TimeSpan elapsed = default) => CreateError(OperateResultCodes.FromString, error, elapsed);

        public static OperateResult CreateError(Exception ex, TimeSpan elapsed = default)
        {
            var t = ex.GetType();
            var isCancel = t == typeof(TaskCanceledException) || t == typeof(OperationCanceledException);
            return new OperateResult(isCancel ? OperateResultCodes.Cancel : OperateResultCodes.FromException, ex, elapsed);
        }

        public static OperateResult CreateNotImplemented() => CreateError(OperateResultCodes.NotImplemented, "the operate was not implemented", default);
    }
}
