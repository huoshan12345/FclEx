using System;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Utils;

namespace FclEx.Actions
{
    public readonly struct WrappedCommonAction<T> : IAction<T>
    {
        private readonly Func<CancellationToken, Task<OperateResult<T>>> _func;

        public WrappedCommonAction(Func<CancellationToken, Task<OperateResult<T>>> func)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
        }

        public async Task<OperateResult<T>> ExecuteAsync(CancellationToken token = default)
        {
            var func = _func;
            return await OperateResult.ExcuteAsync(() => func(token)).DonotCapture();
        }
    }
}