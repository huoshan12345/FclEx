namespace FclEx.Extensions;

public static class ConcurrentQueueExtensions
{
    public static void Clear<T>(this ConcurrentQueue<T> queue)
    {
        while (queue.TryDequeue(out _)) { }
    }
}