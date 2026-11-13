namespace FclEx.RabbitMQ.Internal;

internal static class AsyncTools
{
    internal static async Task WaitAsync(Task task, CancellationToken token)
    {
        if (task.IsCompleted)
        {
            await task.ConfigureAwait(false);
            return;
        }
        var canceled = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using (token.Register(() => canceled.TrySetResult(true)))
        {
            if (await Task.WhenAny(task, canceled.Task).ConfigureAwait(false) != task)
            {
                Observe(task);
                token.ThrowIfCancellationRequested();
            }
            await task.ConfigureAwait(false);
        }
    }

    internal static void Observe(Task task)
        => _ = task.ContinueWith(t => { _ = t.Exception; }, CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);

    internal static async Task CloseChannelAsync(IChannel channel)
    {
        try
        {
            if (channel.IsOpen)
                await channel.CloseAsync(200, "Disposed", abort: true).ConfigureAwait(false);
        }
        finally
        {
            await channel.DisposeAsync().ConfigureAwait(false);
        }
    }
}
