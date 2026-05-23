namespace FclEx.Extensions;

public static class SemaphoreSlimExtensions
{
    public static async Task<bool> WaitAsync(this SemaphoreSlim semaphore, int count, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        for (var i = 0; i < count; ++i)
        {
            var flag = await semaphore.WaitAsync(timeout, cancellationToken);
            if (flag == false)
                return false; // timeout
        }
        return true;
    }

    public static bool IsEmpty(this SemaphoreSlim semaphore) => semaphore.CurrentCount == 0;
}