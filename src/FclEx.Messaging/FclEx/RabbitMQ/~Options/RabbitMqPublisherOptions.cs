namespace FclEx.RabbitMQ;

public class RabbitMqPublisherOptions : RabbitMqProcessorOptions
{
    public RabbitMqPublisherOptions()
    {
    }

    public RabbitMqPublisherOptions(RabbitMqConnectionOptions connection, RabbitMqExchangeOptions exchange)
        : base(connection, exchange)
    {
    }
}