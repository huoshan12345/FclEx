using System;
using System.Threading.Tasks;
using FclEx.Helpers;
using OperateResult = FclEx.Utils.OperateResult<FclEx.Utils.IUnit>;

namespace FclEx.Utils
{
    public static partial class OperateUtil
    {
        public static OperateResult Success { get; } = CreateSuccess(TimeSpan.Zero);

        public static OperateResult NotImplemented { get; } = CreateNotImplemented();

        public static OperateResult<T> CreateNotImplemented<T>() => CreateError<T>(OperateResultCodes.NotImplemented, "the operate was not implemented", default);

        public static OperateResult<T> CreateCancel<T>(Exception ex, TimeSpan elapsed) => new OperateResult<T>(OperateResultCodes.Cancel, ex, elapsed);

        public static OperateResult<T> CreateCancel<T>(TimeSpan elapsed) => CreateError<T>(OperateResultCodes.Cancel, "the operate was canceled", elapsed);

        public static OperateResult<T> CreateSuccess<T>(T item, TimeSpan elapsed) => new OperateResult<T>(item, elapsed);

        public static OperateResult<T> CreateError<T>(int code, string error, TimeSpan elapsed) => new OperateResult<T>(code, new SimpleException(error), elapsed);

        public static OperateResult<T> CreateError<T>(string error, TimeSpan elapsed) => CreateError<T>(OperateResultCodes.FromString, error, elapsed);

        public static OperateResult<T> CreateError<T>(Exception ex, TimeSpan elapsed)
        {
            var t = ex.GetType();
            var isCancel = t == typeof(TaskCanceledException) || t == typeof(OperationCanceledException);
            return new OperateResult<T>(isCancel ? OperateResultCodes.Cancel : OperateResultCodes.FromException, ex, elapsed);
        }

        public static OperateResult CreateCancel(Exception ex, TimeSpan elapsed) => CreateCancel<IUnit>(ex, elapsed);

        public static OperateResult CreateCancel(TimeSpan elapsed) => CreateCancel<IUnit>(elapsed);

        public static OperateResult CreateSuccess(TimeSpan elapsed) => CreateSuccess(default(IUnit), elapsed);

        public static OperateResult CreateError(int code, string error, TimeSpan elapsed) => CreateError<IUnit>(code, error, elapsed);

        public static OperateResult CreateError(string error, TimeSpan elapsed) => CreateError(OperateResultCodes.FromString, error, elapsed);

        public static OperateResult CreateError(Exception ex, TimeSpan elapsed) => CreateError<IUnit>(ex, elapsed);

        public static OperateResult CreateNotImplemented() => CreateNotImplemented<IUnit>();
    }
}
