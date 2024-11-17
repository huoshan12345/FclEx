namespace FclEx.RabbitMQ;

[SuppressMessage("ReSharper", "UnusedTypeParameter")]
public interface IMessageRouter<TInput, TOutput> : IMessageProcessor<RouterSettings>;