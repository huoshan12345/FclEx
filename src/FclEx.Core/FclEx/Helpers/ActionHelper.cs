namespace FclEx.Helpers;

public static class ActionHelper
{
    internal static Action<Exception> EmptyExpAction { get; } = e => { };

    public static void Try(Action action, int retryTimes = 3, int delaySeconds = 0, Action<Exception>? onFail = null, bool throwOnFail = false)
    {
        Check.NotNull(action);

        var lastEx = default(Exception);
        onFail ??= EmptyExpAction;
        var times = Math.Max(0, retryTimes) + 1;
        for (var i = 1; i <= times; i++)
        {
            try
            {
                action();
                return;
            }
            catch (Exception ex)
            {
                lastEx = ex;
                ThreadHelper.Sleep(delaySeconds);
            }
        }

        if (lastEx == null) return;

        onFail(lastEx);
        if (throwOnFail) throw lastEx;
    }

    public static T? Try<T>(Func<T> action, int retryTimes = 3, int delaySeconds = 0, Func<Exception, T>? onFail = null, bool throwOnFail = false)
    {
        Check.NotNull(action);

        var times = Math.Max(0, retryTimes) + 1;
        var lastEx = default(Exception);
        for (var i = 1; i <= times; i++)
        {
            try
            {
                return action();
            }
            catch (Exception ex)
            {
                lastEx = ex;
                ThreadHelper.Sleep(delaySeconds);
            }
        }
        if (throwOnFail && lastEx != null) throw lastEx;
        return onFail == null || lastEx == null ? default : onFail(lastEx);
    }

    /// <summary>
    /// Executes an asynchronous operation until it succeeds or exhausts its retries.
    /// </summary>
    /// <param name="action">The token-aware asynchronous operation.</param>
    /// <param name="maxRetryCount">The maximum number of retries after the initial attempt.</param>
    /// <param name="retryDelay">The delay before each retry.</param>
    /// <param name="onFailure">An optional observer invoked after the final failed attempt.</param>
    /// <param name="throwOnFailure">Whether to rethrow the final exception after notifying <paramref name="onFailure"/>.</param>
    /// <param name="cancellationToken">Stops the active operation or retry delay. Cancellation is never retried.</param>
    public static async Task TryAsync(
        Func<CancellationToken, Task> action,
        int maxRetryCount = 3,
        TimeSpan retryDelay = default,
        Action<Exception>? onFailure = null,
        bool throwOnFailure = false,
        CancellationToken cancellationToken = default)
    {
        Check.NotNull(action);

        var outcome = await TryAsyncCore(
            async token =>
            {
                await action(token).ConfigureAwait(false);
                return true;
            },
            maxRetryCount,
            retryDelay,
            cancellationToken).ConfigureAwait(false);

        if (outcome.Succeeded)
            return;

        var failure = outcome.Failure!;
        onFailure?.Invoke(failure.SourceException);
        if (throwOnFailure)
            failure.Throw();
    }

    /// <summary>
    /// Executes an asynchronous value-producing operation until it succeeds or exhausts its retries.
    /// </summary>
    /// <typeparam name="T">The operation result type.</typeparam>
    /// <param name="action">The token-aware asynchronous operation.</param>
    /// <param name="maxRetryCount">The maximum number of retries after the initial attempt.</param>
    /// <param name="retryDelay">The delay before each retry.</param>
    /// <param name="fallback">Creates a fallback value from the final exception when it is not rethrown.</param>
    /// <param name="throwOnFailure">Whether to rethrow the final exception instead of returning a fallback.</param>
    /// <param name="defaultValue">The value returned when retries are exhausted and no <paramref name="fallback"/> is supplied.</param>
    /// <param name="cancellationToken">Stops the active operation or retry delay. Cancellation is never retried.</param>
    public static async Task<T> TryAsync<T>(
        Func<CancellationToken, Task<T>> action,
        int maxRetryCount = 3,
        TimeSpan retryDelay = default,
        Func<Exception, T>? fallback = null,
        bool throwOnFailure = false,
        T defaultValue = default!,
        CancellationToken cancellationToken = default)
    {
        Check.NotNull(action);

        var outcome = await TryAsyncCore(
            action,
            maxRetryCount,
            retryDelay,
            cancellationToken).ConfigureAwait(false);

        if (outcome.Succeeded)
            return outcome.Result;

        var failure = outcome.Failure!;
        if (throwOnFailure)
            failure.Throw();

        return fallback is null ? defaultValue : fallback(failure.SourceException);
    }

    private static async Task<(bool Succeeded, T Result, ExceptionDispatchInfo? Failure)> TryAsyncCore<T>(
        Func<CancellationToken, Task<T>> action,
        int maxRetryCount,
        TimeSpan retryDelay,
        CancellationToken cancellationToken)
    {
        Check.NotLessThan(maxRetryCount, 0);
        Check.NotLessThan(retryDelay, TimeSpan.Zero);

        for (long retryCount = 0; ; retryCount++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var result = await action(cancellationToken).ConfigureAwait(false);
                return (true, result, null);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var failure = ExceptionDispatchInfo.Capture(ex);
                if (retryCount >= maxRetryCount)
                    return (false, default!, failure);

                if (retryDelay > TimeSpan.Zero)
                    await Task.Delay(retryDelay, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
