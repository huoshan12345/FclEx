namespace FclEx.Utils;

internal static class CacheNotificationHelper
{
    public static void Notify<TKey, TValue>(
        object sender,
        EventHandler<CacheEntryRemovedEventArgs<TKey, TValue>>? handlers,
        IReadOnlyCollection<CacheEntryRemovedEventArgs<TKey, TValue>> notifications)
        where TKey : notnull
    {
        if (handlers is null || notifications.Count == 0)
            return;

        List<Exception>? exceptions = null;
        foreach (var notification in notifications)
        {
            foreach (EventHandler<CacheEntryRemovedEventArgs<TKey, TValue>> handler in handlers.GetInvocationList())
            {
                try
                {
                    handler(sender, notification);
                }
                catch (Exception ex)
                {
                    (exceptions ??= []).Add(ex);
                }
            }
        }

        if (exceptions is null)
            return;

        if (exceptions.Count == 1)
            ExceptionDispatchInfo.Capture(exceptions[0]).Throw();

        throw new AggregateException("One or more cache entry removal handlers failed.", exceptions);
    }
}
