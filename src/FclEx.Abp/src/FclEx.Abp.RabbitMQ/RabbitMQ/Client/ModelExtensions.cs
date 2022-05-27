using System.Collections.Generic;
using FclEx;
using FclEx.Extensions;

namespace RabbitMQ.Client
{
    public static class ModelExtensions
    {
        public static void ExchangeDeclareWithAlternate(this IModel channel, string exchange, string type, bool durable,
            bool autoDelete, IDictionary<string, object> arguments, string alternateExchange, bool isDelayed)
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
                var ae = alternateExchange.IsNullOrEmpty() ? $"{exchange}.Alternate" : alternateExchange;
                arguments[FclExAbpRabbitMqConstants.AlternateExchange] = ae;
                channel.ExchangeDeclare(exchange, type, durable, autoDelete, arguments);
                channel.ExchangeDeclare(ae, "fanout", durable, autoDelete);
                var queue = channel.QueueDeclare($"{ae}.Unrouted", durable, false, autoDelete);
                channel.QueueBind(queue.QueueName, ae, "");
            }
        }

        public static AutoCloseableModel CreateChannel(this IConnection con)
        {
            return new AutoCloseableModel(con.CreateModel());
        }
    }
}
