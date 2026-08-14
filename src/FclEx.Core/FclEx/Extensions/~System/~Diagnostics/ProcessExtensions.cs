namespace FclEx.Extensions;

public static class ProcessExtensions
{
#if !NET5_0_OR_GREATER
    /// <summary>
    /// Waits asynchronously for the process to exit.
    /// </summary>
    /// <param name="process">The process to wait for.</param>
    /// <param name="cancellationToken">A cancellation token that cancels waiting but does not terminate the process.</param>
    /// <returns>A Task representing waiting for the process to end.</returns>
    public static async Task WaitForExitAsync(this Process process, CancellationToken cancellationToken = default)
    {
        if (process.HasExited)
            return;

        var tcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        void OnExited(object? sender, EventArgs args) => tcs.TrySetResult(null);

        process.EnableRaisingEvents = true;
        process.Exited += OnExited;

        try
        {
            if (process.HasExited)
                return;

            using var registration = cancellationToken.Register(() => tcs.TrySetCanceled());
            await tcs.Task;
        }
        finally
        {
            process.Exited -= OnExited;
        }
    }
#endif
}
