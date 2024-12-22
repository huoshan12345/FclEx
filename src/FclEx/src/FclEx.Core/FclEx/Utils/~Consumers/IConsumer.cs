namespace FclEx.Utils;

public interface IConsumer<in T> : IDisposable
{
    bool IsComplete { get; }
    int Count { get; }
    ConsumerCounter Counter { get; }
    Task StartAsync(bool clear = false);
    void Add(T item);
    void CompleteAdding();
    void Stop();
}

public interface IExceptionListener<out TSelf, out T>
{
    event EventHandler<TSelf, T> ExceptionHandler;
}

public interface ICancellationListener<out TSelf, out T>
{
    event EventHandler<TSelf, T> CancellationHandler;
}

public interface IAsyncConsumer<out TSelf, out T>
{
    event AsyncEventHandler<TSelf, T> ConsumingHandler;
}

public interface IDiscardListener<out TSelf, out T>
{
    event EventHandler<TSelf, T> DiscardHandler;
}

public static class ConsumerExtensions
{
    public static void AddRange<T>(this IConsumer<T> consumer, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            consumer.Add(item);
        }
    }
}