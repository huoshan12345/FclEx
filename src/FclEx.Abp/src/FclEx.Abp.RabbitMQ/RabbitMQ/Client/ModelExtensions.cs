using System.Collections.Generic;
using FclEx;
using FclEx.Extensions;

namespace RabbitMQ.Client;

public static class ModelExtensions
{
    public static void ExchangeDeclareWithAlternate(this IModel channel, string exchange, string type, bool durable,
        bool autoDelete, IDictionary<string, object>? arguments, bool isDelayed)
    {
        arguments ??= new Dictionary<string, object>();
        // AlternateExchange and DelayExchange can not work together
        if (isDelayed)
        {
            arguments[FclExAbpRabbitMqConstants.HeaderOfDelayType] = type;
            channel.ExchangeDeclare(exchange, FclExAbpRabbitMqConstants.DelayExchange, durable, autoDelete, arguments);
        }
        else
        {
            channel.ExchangeDeclare(exchange, type, durable, autoDelete, arguments);
        }
    }

    public static AutoCloseableModel CreateChannel(this IConnection con)
    {
        return new AutoCloseableModel(con.CreateModel());
    }
}