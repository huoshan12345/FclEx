using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FclEx.Utils
{
    public partial struct OperateResult
    {
        private static bool IsCancelException(Exception ex)
        {
            var t = ex.GetType();
            var isCancel = t == typeof(TaskCanceledException) || t == typeof(OperationCanceledException);
            return isCancel;
        }

        public static OperateResult Success { get; } = CreateSuccess();

        public static OperateResult NotImplemented { get; } = CreateNotImplemented();

        public static OperateResult Cancel { get; } = CreateCancel();

        public static OperateResult CreateCancel(Exception ex, TimeSpan elapsed = default) => new OperateResult(OperateResultCodes.Cancel, ex, elapsed);

        public static OperateResult CreateCancel(TimeSpan elapsed = default) => CreateError(OperateResultCodes.Cancel, "the operate was canceled", elapsed);

        public static OperateResult CreateSuccess(TimeSpan elapsed = default) => new OperateResult(elapsed);

        public static OperateResult CreateError(int code, string error, TimeSpan elapsed = default) => new OperateResult(code, new SimpleException(error), elapsed);

        public static OperateResult CreateError(string error, TimeSpan elapsed = default) => CreateError(OperateResultCodes.FromString, error, elapsed);

        public static OperateResult CreateObjError<T>(T obj, string error, TimeSpan elapsed = default)
        {
            return new OperateResult(OperateResultCodes.FromString, ObjectException.Create(obj, error), elapsed);
        }

        public static OperateResult CreateError(Exception ex, TimeSpan elapsed = default)
        {
            return new OperateResult(IsCancelException(ex) ? OperateResultCodes.Cancel : OperateResultCodes.FromException, ex, elapsed);
        }

        public static OperateResult CreateObjError<T>(T obj, Exception ex, TimeSpan elapsed = default)
        {
            return new OperateResult(IsCancelException(ex) ? OperateResultCodes.Cancel : OperateResultCodes.FromException,
                ObjectException.Create(obj, ex.Message, ex), elapsed);
        }

        public static OperateResult CreateNotImplemented() => CreateError(OperateResultCodes.NotImplemented, "the operate was not implemented", default);
    }
}
