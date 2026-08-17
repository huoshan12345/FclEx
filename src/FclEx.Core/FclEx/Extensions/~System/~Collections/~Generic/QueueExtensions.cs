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

    /// <summary>
    /// Immediately removes and returns up to <paramref name="count"/> items from the front of the queue.
    /// </summary>
    /// <remarks>The returned array is materialized before this method returns; enumerating it has no further effect on the queue.</remarks>
    public static T[] Dequeue<T>(this Queue<T> queue, int count)
    {
        Check.NotNull(queue);
        Check.NotNegative(count);

        var itemCount = Math.Min(count, queue.Count);
        var result = new T[itemCount];
        for (var i = 0; i < itemCount; i++)
        {
            result[i] = queue.Dequeue();
        }
        return result;
    }
}
