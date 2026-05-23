namespace FclEx.RabbitMQ;

public class RabbitMqExchangeOptions
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = ExchangeTypes.Topic;
}