using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Polly;
using Polly.Retry;

namespace FclEx.Actions
{
    public interface IAbstractAction<T> : IAction<T>
    {
        ILogger Logger { get; }

        Task<OperateResult<T>> ExecuteInternalAsync(CancellationToken token = default);
        Task<OperateResult<T>> HandleCancellationAsync(Exception ex) => OperateResult.CreateCancel<T>(ex);
        Task<OperateResult<T>> HandleErrorAsync(Exception ex) => OperateResult.CreateError<T>(ex);

        async Task<OperateResult<T>> IAction<T>.ExecuteAsync(CancellationToken token)
        {
            var action = GetType().ShortName();

            if (Logger.IsEnabled(LogLevel.Trace))
                Logger.LogTrace($"[Action={action}]Begin");

            var future = CommonAction.Create(ExecuteInternalAsync, true)
                .NextResult(r =>
                {
                    return r.Successful
                        ? (IAction<T>)new SuccessAction<T>(r.Result!, r.Elapsed)
                        : r.IsCancelErr()
                            ? (IAction<T>)CommonAction.Create(t => HandleCancellationAsync(r.Exception!), true)
                            : (IAction<T>)CommonAction.Create(t => HandleErrorAsync(r.Exception!), true);
                });

            var result = await future.ExecuteAsync(token).DonotCapture();

            if (Logger.IsEnabled(LogLevel.Trace))
                Logger.LogTrace($"[Action={action}]End, after {result.Elapsed.TotalMilliseconds:f3} ms]");

            return result;
        }
    }
}
