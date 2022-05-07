using System;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Utils;
using Microsoft.Extensions.Logging;

namespace FclEx.Actions
{
    public interface IAbstractAction<T> : IAction<T>
    {
        ILogger Logger { get; }

        Task<OperateResult<T>> ExecuteAsyncBody(CancellationToken token = default);
        Task<OperateResult<T>> HandleCancellationAsync(Exception ex) => OperateResult.CreateCancel<T>(ex);
        Task<OperateResult<T>> HandleErrorAsync(Exception ex) => OperateResult.CreateError<T>(ex);

        async Task<OperateResult<T>> IAction<T>.ExecuteAsync(CancellationToken token)
        {
            var time = ValueStopwatch.StartNew();

            if (Logger.IsEnabled(LogLevel.Trace))
                Logger.LogTrace($"[{GetName()}]Begin");

            var future = CommonAction.Create(ExecuteAsyncBody, true)
                .NextResult<T, T>(r => r.Successful
                    ? new SuccessAction<T>(r.Result, r.Elapsed)
                    : r.IsCancelErr()
                        ? CommonAction.Create(t => HandleCancellationAsync(r.Exception), true)
                        : CommonAction.Create(t => HandleErrorAsync(r.Exception), true));

            var result = await future.ExecuteAsync(token).DonotCapture();
            result = result.WithElapsed(time.GetElapsedTime());

            if (Logger.IsEnabled(LogLevel.Trace))
                Logger.LogTrace($"[{GetName()}]End, after {result.Elapsed.TotalMilliseconds:f3} ms]");

            return result;
        }
    }
}
