namespace FclEx.RabbitMQ;

public interface IMessageProcessor<in T> : IAsyncDisposable where T : ProcessorSettings
{
    Task InitializeAsync(T settings);
}