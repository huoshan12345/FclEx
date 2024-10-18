namespace FclEx.Extensions;

public static class QueueExtensions
{
    public static Queue<T> Enqueue<T>(this Queue<T> queue, IEnumerable<T> enumerable)
    {
        foreach (var item in enumerable)
        {
            queue.Enqueue(item);
        }
        return queue;
    }

    public static IEnumerable<T> Dequeue<T>(this Queue<T> queue, int chunkSize)
    {
        for (var i = 0; i < chunkSize && queue.Count > 0; i++)
        {
            yield return queue.Dequeue();
        }
    }
}