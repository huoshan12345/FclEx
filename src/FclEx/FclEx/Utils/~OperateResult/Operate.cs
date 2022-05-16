using System;
using FclEx.Helpers;

namespace FclEx.Utils
{
    public static partial class Operate
    {
        public static OperateResult Excute(Action action)
        {
            var watch = ValueStopwatch.StartNew();
            try
            {
                action();
                return watch.GetElapsedTime();
            }
            catch (Exception ex)
            {
                return (ex, watch.GetElapsedTime());
            }
        }

        public static OperateResult<T> Excute<T>(Func<T> action)
        {
            var watch = ValueStopwatch.StartNew();
            try
            {
                var result = action();
                return (result, watch.GetElapsedTime());
            }
            catch (Exception ex)
            {
                return (ex, watch.GetElapsedTime());
            }
        }

        public static OperateResult Excute(Func<OperateResult> action) => Excute<OperateResult>(action).Unwrap();

        public static OperateResult<T> Excute<T>(Func<OperateResult<T>> action) => Excute<OperateResult<T>>(action).Unwrap();

        public static void ExcuteBg(Action action) => TaskHelper.RunBg(() => Excute(action));
        public static void ExcuteBg<T>(Func<T> action) => TaskHelper.RunBg(() => Excute(action));
        public static void ExcuteBg(Func<OperateResult> action) => TaskHelper.RunBg(() => Excute(action));
        public static void ExcuteBg<T>(Func<OperateResult<T>> action) => TaskHelper.RunBg(() => Excute(action));
    }
}
