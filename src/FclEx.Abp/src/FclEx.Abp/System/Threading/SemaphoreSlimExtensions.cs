using System;
using System.Collections.Generic;
using System.Text;
using FclEx;
using FclEx.Extensions;

namespace System.Threading;

public static class SemaphoreSlimExtensions
{
    public static async Task<bool> WaitAsync(this SemaphoreSlim semaphore, int count,
        TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        for (var i = 0; i < count; ++i)
        {
            var flag = await semaphore.WaitAsync(timeout, cancellationToken).IgnoreSyncContext();
            if (!flag) return false; // timeout
        }
        return true;
    }

    public static bool IsEmpty(this SemaphoreSlim semaphore) => semaphore.CurrentCount == 0;
}