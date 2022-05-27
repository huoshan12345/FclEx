using System;
using System.Threading;

namespace FclEx.Extensions
{
    public static class CancellationTokenExtensions
    {
        public static CancellationTokenSource WithTimeout(this CancellationToken token, TimeSpan? timeout)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            if (timeout.HasValue)
            {
                cts.CancelAfter(timeout.Value);
            }
            return cts;
        }
    }
}
