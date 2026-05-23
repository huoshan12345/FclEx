namespace RabbitMQ.Client;

public static class RabbitMqConstants
{
    /// <summary>
    /// Defines the built-in RabbitMQ exchange type names.
    /// </summary>
    public static class ExchangeTypes
    {
        /// <summary>
        /// Routes messages to queues whose binding key exactly matches the message routing key.
        /// </summary>
        public const string Direct = "direct";

        /// <summary>
        /// Routes messages to all bound queues, ignoring the message routing key.
        /// </summary>
        public const string Fanout = "fanout";

        /// <summary>
        /// Routes messages based on matching message header values instead of the routing key.
        /// </summary>
        public const string Headers = "headers";

        /// <summary>
        /// Routes messages to queues whose binding pattern matches the message routing key.
        /// </summary>
        public const string Topic = "topic";
    }
}