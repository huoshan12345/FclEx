using System;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Utils;

namespace FclEx.Http.Actions
{
    public class CommonActor : IActor
    {
        private readonly Func<CancellationToken, Task> _func;

        public CommonActor(Func<CancellationToken, Task> func)
        {
            _func = func;
        }

        public async Task<IOperateResult> ExecuteAsync(CancellationToken token = default)
        {
            return await OperateResult.ExcuteAsync(() => _func(token)).DonotCapture();
        }

        public static CommonActor<T> Create<T>(Func<CancellationToken, Task<T>> func)
        {
            return new CommonActor<T>(func);
        }

        public static CommonActor Create<T>(Func<T> func)
        {
            return new CommonActor(t =>
            {
                var result = func();
                return Task.FromResult(result);
            });
        }
        public static CommonActor Create(Func<CancellationToken, Task> func)
        {
            return new CommonActor(func);
        }
        public static CommonActor Create(Action action)
        {
            return new CommonActor(t =>
            {
                action();
                return Task.CompletedTask;
            });
        }
    }

    public class CommonActor<T> : IActor
    {
        private readonly Func<CancellationToken, Task<T>> _func;

        public CommonActor(Func<CancellationToken, Task<T>> func)
        {
            _func = func;
        }

        public async Task<IOperateResult> ExecuteAsync(CancellationToken token = default)
        {
            return await OperateResult.ExcuteAsync(() => _func(token)).DonotCapture();
        }
    }
}
