namespace FclEx.Extensions;

public static class SemaphoreSlimExtensions
{
    /// <summary>
    /// Asynchronously acquires the specified number of permits from the semaphore.
    /// </summary>
    /// <param name="semaphore">The semaphore from which to acquire permits.</param>
    /// <param name="count">The number of permits to acquire. The value must be positive.</param>
    /// <param name="timeout">
    /// The timeout applied independently to each permit acquisition. To impose a timeout on the entire operation,
    /// pass a cancellation token whose source has been configured with the desired overall timeout.
    /// </param>
    /// <param name="cancellationToken">A token that can cancel the entire operation.</param>
    /// <returns><see langword="true" /> when all permits were acquired; otherwise <see langword="false" />.</returns>
    /// <remarks>
    /// If an acquisition times out or the operation is canceled, all permits acquired by this call are released
    /// before the method returns or propagates cancellation.
    /// </remarks>
    public static async Task<bool> WaitAsync(this SemaphoreSlim semaphore, int count, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        Check.NotNull(semaphore);
        Check.GreaterThan(count, 0);

        var acquiredCount = 0;
        try
        {
            for (; acquiredCount < count; acquiredCount++)
            {
                if (await semaphore.WaitAsync(timeout, cancellationToken).ConfigureAwait(false) == false)
                    return false;
            }

            return true;
        }
        finally
        {
            if (acquiredCount < count && acquiredCount > 0)
                semaphore.Release(acquiredCount);
        }
    }

    public static bool IsEmpty(this SemaphoreSlim semaphore) => semaphore.CurrentCount == 0;
}
