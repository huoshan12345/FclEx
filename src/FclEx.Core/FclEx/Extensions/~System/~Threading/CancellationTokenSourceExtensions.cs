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
}