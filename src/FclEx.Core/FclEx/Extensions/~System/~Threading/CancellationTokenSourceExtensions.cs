namespace FclEx.Extensions;

public static class CancellationTokenSourceExtensions
{
    /// <summary>Attempts to request cancellation without throwing when the source has already been disposed.</summary>
    /// <param name="cts">The cancellation source to cancel.</param>
    /// <returns>
    /// <see langword="true"/> when <see cref="CancellationTokenSource.Cancel()"/> was invoked; <see langword="false"/>
    /// when cancellation had already been observed or the source was disposed.
    /// </returns>
    /// <remarks>
    /// The result is a concurrency-sensitive snapshot. A <see langword="false"/> result must not be used to infer that
    /// another thread cannot cancel the source immediately afterward. Exceptions from registered cancellation callbacks
    /// are deliberately propagated.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="cts"/> is <see langword="null"/>.</exception>
    public static bool TryCancel(this CancellationTokenSource cts)
    {
        Check.NotNull(cts);

        try
        {
            if (cts.IsCancellationRequested)
                return false;

            cts.Cancel();
            return true;
        }
        catch (ObjectDisposedException)
        {
            return false;
        }
    }

#if !NET5_0_OR_GREATER
    public static Task CancelAsync(this CancellationTokenSource cts)
    {
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        ThreadPool.QueueUserWorkItem(_ =>
        {
            try
            {
                cts.Cancel();
                tcs.SetResult();
            }
            catch (Exception ex)
            {
                // ex is already an AggregateException (if multiple callbacks threw),
                // so set it directly instead of letting Task wrap/unwrap it again.
                tcs.SetException(ex);
            }
        });
        return tcs.Task;
    }
#endif
}
