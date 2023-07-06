namespace FclEx.Consumers;

public interface IConsumer<in T> : IDisposable
{
    bool IsComplete { get; }
    int Count { get; }
    ILogger Logger { get; set; }
    Counter Counter { get; }
    Task Start(bool clear = false);
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