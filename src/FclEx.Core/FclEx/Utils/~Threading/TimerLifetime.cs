namespace FclEx.Utils;

internal sealed class TimerLifetime(global::System.Threading.Timer timer) : IDisposable, IAsyncDisposable
{
    private global::System.Threading.Timer? _timer = timer;
    private Task? _disposeTask;

    public bool IsActive => Volatile.Read(ref _timer) is not null;

    public void Dispose() => _ = BeginDispose();

    public ValueTask DisposeAsync() => new(BeginDispose());

    private Task BeginDispose()
    {
        var disposeTask = Volatile.Read(ref _disposeTask);
        if (disposeTask is not null)
            return disposeTask;

        var timer = Interlocked.Exchange(ref _timer, null);
        if (timer is null)
        {
            var spinner = new SpinWait();
            while ((disposeTask = Volatile.Read(ref _disposeTask)) is null)
                spinner.SpinOnce();
            return disposeTask;
        }

        disposeTask = DisposeAndWaitAsync(timer);
        Volatile.Write(ref _disposeTask, disposeTask);
        return disposeTask;
    }

    private static Task DisposeAndWaitAsync(global::System.Threading.Timer timer)
    {
        var waitHandle = new ManualResetEvent(false);
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        try
        {
            if (timer.Dispose(waitHandle) == false)
            {
                waitHandle.Dispose();
                return Task.CompletedTask;
            }

            ThreadPool.RegisterWaitForSingleObject(
                waitHandle,
                static (state, _) =>
                {
                    var (handle, source) = ((ManualResetEvent Handle, TaskCompletionSource Source))state!;
                    handle.Dispose();
                    source.TrySetResult();
                },
                (waitHandle, completion),
                Timeout.Infinite,
                executeOnlyOnce: true);
            return completion.Task;
        }
        catch
        {
            waitHandle.Dispose();
            throw;
        }
    }
}
