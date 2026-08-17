namespace FclEx.Extensions;

public static class CancellationTokenSourceExtensions
{
    public static void TryCancel(this CancellationTokenSource cts)
    {
        try
        {
            if (cts.IsCancellationRequested)
                return;

            cts.Cancel();
        }
        catch (ObjectDisposedException) { }
    }

#if !NET5_0_OR_GREATER
    public static Task CancelAsync(this CancellationTokenSource cts)
    {
        var tcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        ThreadPool.QueueUserWorkItem(_ =>
        {
            try
            {
                cts.Cancel();
                tcs.SetResult(0);
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