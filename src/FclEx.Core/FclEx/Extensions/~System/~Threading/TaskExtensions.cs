namespace FclEx.Extensions;

public static partial class TaskExtensions
{
    private static readonly Task<Unit> TaskUnit = Task.FromResult(Unit.Default);

    [MethodImpl(AggressiveInlining)]
    public static bool IsSuccessful(this Task task)
    {
        return task is { IsFaulted: false, IsCanceled: false, Status: TaskStatus.RanToCompletion };
    }

    [MethodImpl(AggressiveInlining)]
    public static ConfiguredTaskAwaitable NoCapture(this Task task)
    {
        return task.ConfigureAwait(false);
    }

    [MethodImpl(AggressiveInlining)]
    public static ConfiguredTaskAwaitable<T> NoCapture<T>(this Task<T> task)
    {
        return task.ConfigureAwait(false);
    }

    [MethodImpl(AggressiveInlining)]
    public static ValueTask<T> ToValueTask<T>(this Task<T> task) => new(task);

    [MethodImpl(AggressiveInlining)]
    public static Task<Unit> ToTaskUnit(this Task task) => task.Then(() => TaskUnit);

    extension(Task)
    {
        /// <summary>
        /// Delays the specified time, but does not throw an exception if the cancellation token is canceled.
        /// </summary>
        /// <param name="delay">The time to delay.</param>
        /// <param name="cancellationToken">The cancellation token to observe.</param>
        /// <returns>A task that represents the delay operation.</returns>
        public static async Task DelaySafely(TimeSpan delay, CancellationToken cancellationToken = default)
        {
            if (delay.Ticks <= 0)
                return;
            try
            {
                await Task.Delay(delay, cancellationToken).NoCapture();
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        }
    }
}