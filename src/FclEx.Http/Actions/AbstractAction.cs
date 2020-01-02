using System;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Polly;
using Polly.Retry;

namespace FclEx.Http.Actions
{
    public abstract class AbstractAction : IAction
    {
        public virtual ILogger Logger { get; } = NullLogger.Instance;

        protected virtual int RetryTimes { get; } = 2;
        protected virtual TimeSpan RetryDelay { get; } = TimeSpan.Zero;
        protected string ActionName => GetType().GetDescription();
        protected virtual AsyncRetryPolicy<IOperateResult> RetryPolicy { get; }

        protected AbstractAction()
        {
            RetryPolicy = Policy<IOperateResult>
                .Handle<Exception>()
                .WaitAndRetryAsync(RetryTimes, i => RetryDelay);
        }

        protected abstract Task<IOperateResult> ExecuteInternalAsync(CancellationToken token = default);

        protected virtual Task<IOperateResult> HandleCancellationAsync(Exception ex)
        {
            return OperateResult.CreateCancel(ex);
        }

        protected virtual Task<IOperateResult> HandleExceptionAsync(Exception ex)
        {
            return OperateResult.CreateError(ex);
        }

        public async Task<IOperateResult> ExecuteAsync(CancellationToken token = default)
        {
            var watch = ValueStopwatch.StartNew();
            IOperateResult result;
            try
            {
                if (Logger.IsEnabled(LogLevel.Trace))
                    Logger.LogTrace($"[Action={ActionName} Begin]");
                result = await RetryPolicy.ExecuteAsync(() => ExecuteInternalAsync(token))
                    .ThrowIfError()
                    .DonotCapture();
            }
            catch (TaskCanceledException ex)
            {
                if (Logger.IsEnabled(LogLevel.Trace))
                    Logger.LogTrace(ex, $"[Action={ActionName} Canceled]");
                result = await HandleCancellationAsync(ex);
            }
            catch (Exception ex)
            {
                if (Logger.IsEnabled(LogLevel.Trace))
                    Logger.LogTrace(ex, $"[Action={ActionName}, Error={ex.Message}]");
                result = await HandleExceptionAsync(ex);
            }

            var time = watch.GetElapsedTime();
            if (Logger.IsEnabled(LogLevel.Trace))
                Logger.LogTrace($"[Action={ActionName} End, {time.TotalMilliseconds:f3} ms]");
            return result.WithElapsed(time);
        }
    }
}
