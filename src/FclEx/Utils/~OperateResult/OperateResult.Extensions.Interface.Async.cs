using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace FclEx.Utils
{
    public static partial class OperateResultExtensions
    {
        public static Task<IOperateResult> Ok(this Task<IOperateResult> @this, Action<TimeSpan> action)
        {
            return @this.On(r => r.Successful, t => action(t.Elapsed));
        }

        public static async Task<OperateResult> ToUntyped(this Task<IOperateResult> task)
        {
            return (await task.DonotCapture()).ToUntyped();
        }

        public static async Task<OperateResult<T>> ToExplicit<T>(this Task<IOperateResult> task)
        {
            return (await task.DonotCapture()).ToExplicit<T>();
        }
    }
}
