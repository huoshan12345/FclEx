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
        public ILogger Logger { get; } = NullLogger.Instance;

        protected string ActionName => GetType().GetDescription();
        protected virtual AsyncRetryPolicy<IOperateResult> RetryPolicy { get; } = Policy<IOperateResult>
            .Handle<Exception>()
            .WaitAndRetryAsync(2, i => TimeSpan.FromSeconds(0));

        protected abstract Task<IOperateResult> ExecuteInternalAsync(CancellationToken token = default);

        public async Task<IOperateResult> ExecuteAsync(CancellationToken token = default)
        {
            var watch = ValueStopwatch.StartNew();
            try
            {
                if (Logger.IsEnabled(LogLevel.Trace))
                    Logger.LogTrace($"[Action={ActionName} Begin]");
                var result = await RetryPolicy.ExecuteAsync(() => ExecuteInternalAsync(token))
                    .ThrowIfError()
                    .DonotCapture();
                return result.WithElapsed(watch.GetElapsedTime());
            }
            catch (TaskCanceledException ex)
            {
                if (Logger.IsEnabled(LogLevel.Trace))
                    Logger.LogTrace(ex, $"[Action={ActionName} Canceled]");
                return OperateResult.CreateCancel(ex, watch.GetElapsedTime());
            }
            catch (Exception ex)
            {
                if (Logger.IsEnabled(LogLevel.Trace))
                    Logger.LogTrace(ex, $"[Action={ActionName}, Error={ex.Message}]");
                return OperateResult.CreateError(ex, watch.GetElapsedTime());
            }
            finally
            {
                if (Logger.IsEnabled(LogLevel.Trace))
                    Logger.LogTrace($"[Action={ActionName} End, {watch.GetElapsedTime().TotalMilliseconds:f3} ms]");
            }
        }
    }
}
