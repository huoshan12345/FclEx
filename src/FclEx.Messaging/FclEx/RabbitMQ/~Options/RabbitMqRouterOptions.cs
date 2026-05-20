// ReSharper disable MemberCanBeProtected.Global

namespace FclEx.RabbitMQ;

public class RabbitMqRouterOptions : RabbitMqConsumerOptions
{
    public RabbitMqExchangeOptions TargetExchange { get; init; }

    public RabbitMqRouterOptions(
        RabbitMqConnectionOptions connection,
        RabbitMqExchangeOptions exchange,
        RabbitMqQueueOptions queue,
        RabbitMqExchangeOptions targetExchange)
        : base(connection, exchange, queue)
    {
        TargetExchange = targetExchange;
    }

    public RabbitMqRouterOptions()
    {
        TargetExchange = new RabbitMqExchangeOptions();
    }
}