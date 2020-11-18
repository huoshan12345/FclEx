using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Polly;
using Polly.Retry;

namespace FclEx.Actions
{
    public abstract class AbstractAction<T> : IAction<T>
    {
        private ILogger _logger = NullLogger.Instance;

        [AllowNull]
        public ILogger Logger
        {
            get => _logger;
            protected set
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (value != null)
                    _logger = value;
            }
        }

        protected virtual int RetryTimes { get; } = 2;
        protected virtual TimeSpan RetryDelay { get; } = TimeSpan.Zero;
        protected string ActionName => GetType().GetDescription();
        protected virtual AsyncRetryPolicy<OperateResult<T>> RetryPolicy { get; }

        protected AbstractAction()
        {
            RetryPolicy = Policy<OperateResult<T>>
                .Handle<Exception>()
                .WaitAndRetryAsync(RetryTimes, i => RetryDelay);
        }

        protected abstract Task<OperateResult<T>> ExecuteInternalAsync(CancellationToken token = default);
        protected virtual Task<OperateResult<T>> HandleCancellationAsync(Exception ex) => OperateResult.CreateCancel<T>(ex);
        protected virtual Task<OperateResult<T>> HandleErrorAsync(Exception ex) => OperateResult.CreateError<T>(ex);

        public async Task<OperateResult<T>> ExecuteAsync(CancellationToken token = default)
        {
            var watch = ValueStopwatch.StartNew();
            OperateResult<T> result;
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
                result = await HandleErrorAsync(ex);
            }

            var time = watch.GetElapsedTime();
            if (Logger.IsEnabled(LogLevel.Trace))
                Logger.LogTrace($"[Action={ActionName} End, {time.TotalMilliseconds:f3} ms]");
            return result.WithElapsed(time);
        }
    }

    public abstract class AbstractAction : AbstractAction<Unit>
    {
    }
}
