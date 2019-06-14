using System;


namespace FclEx.Utils
{
    public static partial class OperateResultExtensions
    {
        public static OperateResult Ok(this OperateResult @this, Action<TimeSpan> action)
        {
            return @this.Ok<IUnit, OperateResult>((o, t) => action(t));
        }

        public static OperateResult<T> Ok<T>(this OperateResult<T> @this, Action<T, TimeSpan> action)
        {
            return @this.Ok<T, OperateResult<T>>(action);
        }

        public static OperateResult<T> Ok<T>(this OperateResult<T> @this, Action<T> action)
        {
            return @this.Ok<T, OperateResult<T>>((o, t) => action(o));
        }
    }
}
