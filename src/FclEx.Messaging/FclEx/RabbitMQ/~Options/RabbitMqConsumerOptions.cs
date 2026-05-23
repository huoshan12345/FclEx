namespace FclEx.RabbitMQ;

public class RabbitMqConsumerOptions : RabbitMqPublisherOptions
{
    public RabbitMqQueueOptions RabbitMqQueue { get; init; }

    public RabbitMqConsumerOptions(RabbitMqConnectionOptions connection, RabbitMqExchangeOptions exchange, RabbitMqQueueOptions queue)
        : base(connection, exchange)
    {
        RabbitMqQueue = queue;
    }

    public RabbitMqConsumerOptions()
    {
        RabbitMqQueue = new RabbitMqQueueOptions();
    }
}