namespace FclEx.RabbitMQ;

public class RabbitMqProcessorOptions
{
    public RabbitMqConnectionOptions Connection { get; init; }
    public RabbitMqExchangeOptions Exchange { get; init; }

    public RabbitMqProcessorOptions() : this(new(), new())
    {
    }

    public RabbitMqProcessorOptions(RabbitMqConnectionOptions connection, RabbitMqExchangeOptions exchange)
    {
        Connection = connection;
        Exchange = exchange;
    }
}