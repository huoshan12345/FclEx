using System;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Utils;

namespace FclEx.Actions
{
    public class CommonAction
    {
        public static CommonAction<T> Create<T>(Func<CancellationToken, T> func)
        {
            return new CommonAction<T>(t => func(t).ToTask());
        }

        public static CommonAction<T> Create<T>(Func<CancellationToken, Task<T>> func)
        {
            return new CommonAction<T>(func);
        }

        public static CommonAction<T> Create<T>(Func<T> func)
        {
            return new CommonAction<T>(t => func().ToTask());
        }

        public static WrappedCommonAction<T> Create<T>(Func<CancellationToken, Task<OperateResult<T>>> func)
        {
            return new WrappedCommonAction<T>(func);
        }

        public static WrappedCommonAction<T> Create<T>(Func<CancellationToken, OperateResult<T>> func)
        {
            return new WrappedCommonAction<T>(t => func(t).ToTask());
        }

        public static CommonAction<T> Create<T>(Func<Task<T>> func)
        {
            return new CommonAction<T>(t => func());
        }

        public static WrappedCommonAction<T> Create<T>(Func<Task<OperateResult<T>>> func)
        {
            return new WrappedCommonAction<T>(t => func());
        }

        public static WrappedCommonAction<T> Create<T>(Func<OperateResult<T>> func)
        {
            return new WrappedCommonAction<T>(t => func().ToTask());
        }
    }

    public readonly struct CommonAction<T> : IAction<T>
    {
        private readonly Func<CancellationToken, Task<T>> _func;

        public CommonAction(Func<CancellationToken, Task<T>> func)
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
