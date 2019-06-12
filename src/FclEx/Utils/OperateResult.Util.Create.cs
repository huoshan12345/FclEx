using System;
using System.Threading.Tasks;

namespace FclEx.Utils
{
    public static partial class OperateUtil
    {
        public static OperateResult<T> CreateNotImplemented<T>() => CreateError<T>(OperateResultCodes.NotImplemented, "the operate was not implemented", default);

        public static OperateResult<T> CreateCancel<T>(Exception ex, TimeSpan elapsed = default) => new OperateResult<T>(OperateResultCodes.Cancel, ex, elapsed);

        public static OperateResult<T> CreateCancel<T>(TimeSpan elapsed = default) => CreateError<T>(OperateResultCodes.Cancel, "the operate was canceled", elapsed);

        public static OperateResult<T> CreateSuccess<T>(T item, TimeSpan elapsed = default) => new OperateResult<T>(item, elapsed);

        public static OperateResult<T> CreateError<T>(int code, string error, TimeSpan elapsed = default) => new OperateResult<T>(code, new SimpleException(error), elapsed);

        public static OperateResult<T> CreateError<T>(string error, TimeSpan elapsed = default) => CreateError<T>(OperateResultCodes.FromString, error, elapsed);

        public static OperateResult<T> CreateError<T>(Exception ex, TimeSpan elapsed = default)
        {
            var t = ex.GetType();
            var isCancel = t == typeof(TaskCanceledException) || t == typeof(OperationCanceledException);
            return new OperateResult<T>(isCancel ? OperateResultCodes.Cancel : OperateResultCodes.FromException, ex, elapsed);
        }
    }
}
