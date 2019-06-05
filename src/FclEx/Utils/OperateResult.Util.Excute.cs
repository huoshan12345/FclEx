using System;
using System.Threading.Tasks;
using FclEx.Helpers;
using IOperateResult = FclEx.Utils.IOperateResult<FclEx.Utils.IUnit>;
using OperateResult = FclEx.Utils.OperateResult<FclEx.Utils.IUnit>;

namespace FclEx.Utils
{
    public static partial class OperateUtil
    {
        public static IOperateResult Excute(Action action)
        {
            var watch = ValueStopwatch.StartNew();
            try
            {
                action();
                return CreateSuccess(watch.GetElapsedTime());
            }
            catch (Exception ex)
            {
                return CreateError(ex, watch.GetElapsedTime());
            }
        }

        public static IOperateResult<T> Excute<T>(Func<T> action)
        {
            var watch = ValueStopwatch.StartNew();
            try
            {
                var result = action();
                return CreateSuccess(result, watch.GetElapsedTime());
            }
            catch (Exception ex)
            {
                return CreateError<T>(ex, watch.GetElapsedTime());
            }
        }

        public static IOperateResult Excute(Func<IOperateResult> action) => Excute<IOperateResult>(action).Unwrap();

        public static IOperateResult<T> Excute<T>(Func<IOperateResult<T>> action) => Excute<IOperateResult<T>>(action).Unwrap();

        public static void ExcuteBg(Action action) => TaskHelper.RunBg(() => Excute(action));
        public static void ExcuteBg<T>(Func<T> action) => TaskHelper.RunBg(() => Excute(action));
        public static void ExcuteBg(Func<IOperateResult> action) => TaskHelper.RunBg(() => Excute(action));
        public static void ExcuteBg<T>(Func<IOperateResult<T>> action) => TaskHelper.RunBg(() => Excute(action));
    }
}
