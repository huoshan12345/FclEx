namespace RabbitMQ.Client;

public static class FclExAbpRabbitMqConstants
{
    public const string DefaultExchangeType = "topic";

    public const string Other = nameof(Other);

    public const string HeaderOfErrorTimes = "d-error-times";

    public const string HeaderOfDelayMilli = "x-delay";

    public const string HeaderOfDelayType = "x-delayed-type";

    public const string DelayExchange = "x-delayed-message";

    public const string AlternateExchange = "alternate-exchange";
}