namespace FclEx.RabbitMQ;

[SuppressMessage("ReSharper", "UnusedTypeParameter")]
public interface IMessageConsumer<T, in TSettings> : IMessageProcessor<TSettings> where TSettings : ConsumerSettings;

public interface IMessageConsumer<T> : IMessageConsumer<T, ConsumerSettings>;