namespace FclEx.RabbitMQ;

public interface IMessageProcessor<in T> : IAsyncDisposable where T : RabbitMqProcessorOptions
{
    Task InitializeAsync(T settings);
}