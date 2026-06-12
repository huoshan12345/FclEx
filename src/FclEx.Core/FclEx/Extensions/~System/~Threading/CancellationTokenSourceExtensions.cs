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
        catch { }
    }

#if !NET5_0_OR_GREATER
    public static Task CancelAsync(this CancellationTokenSource cts)
    {
        cts.Cancel();
        return Task.CompletedTask;
    }
#endif
}