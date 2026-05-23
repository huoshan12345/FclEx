namespace RabbitMQ.Client;

/// <summary>
/// Provides constants for well-known RabbitMQ names, types, and argument keys.
/// </summary>
public static class RabbitMqConstants
{
    /// <summary>
    /// Provides well-known RabbitMQ exchange type names.
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

    /// <summary>
    /// Provides well-known RabbitMQ exchange names.
    /// </summary>
    public static class ExchangeNames
    {
        /// <summary>
        /// The default RabbitMQ exchange.
        /// Messages published to this exchange are routed by queue name.
        /// </summary>
        public const string Default = "";
    }

    /// <summary>
    /// Provides well-known RabbitMQ queue argument names.
    /// </summary>
    public static class QueueArgumentNames
    {
        /// <summary>
        /// Sets the message time-to-live in milliseconds.
        /// </summary>
        public const string MessageTtl = "x-message-ttl";

        /// <summary>
        /// Sets the queue expiration time in milliseconds.
        /// </summary>
        public const string Expires = "x-expires";

        /// <summary>
        /// Sets the maximum number of ready messages the queue can hold.
        /// </summary>
        public const string MaxLength = "x-max-length";

        /// <summary>
        /// Sets the maximum total body size, in bytes, the queue can hold.
        /// </summary>
        public const string MaxLengthBytes = "x-max-length-bytes";

        /// <summary>
        /// Sets the overflow behavior when the queue reaches its maximum length.
        /// </summary>
        public const string Overflow = "x-overflow";

        /// <summary>
        /// Sets the exchange to which rejected or expired messages are republished.
        /// </summary>
        public const string DeadLetterExchange = "x-dead-letter-exchange";

        /// <summary>
        /// Sets the routing key used when dead-lettering messages.
        /// </summary>
        public const string DeadLetterRoutingKey = "x-dead-letter-routing-key";

        /// <summary>
        /// Sets the queue type.
        /// </summary>
        public const string QueueType = "x-queue-type";

        /// <summary>
        /// Enables single active consumer mode for the queue.
        /// </summary>
        public const string SingleActiveConsumer = "x-single-active-consumer";
    }

    /// <summary>
    /// Provides well-known RabbitMQ queue type names.
    /// </summary>
    public static class QueueTypes
    {
        /// <summary>
        /// A classic RabbitMQ queue.
        /// </summary>
        public const string Classic = "classic";

        /// <summary>
        /// A replicated RabbitMQ queue based on Raft consensus.
        /// </summary>
        public const string Quorum = "quorum";

        /// <summary>
        /// A RabbitMQ stream queue.
        /// </summary>
        public const string Stream = "stream";
    }

    /// <summary>
    /// Provides well-known RabbitMQ binding argument names.
    /// </summary>
    public static class BindingArgumentNames
    {
        /// <summary>
        /// Sets how header binding arguments are matched by a headers exchange.
        /// </summary>
        public const string Match = "x-match";
    }

    /// <summary>
    /// Provides well-known header matching modes for headers exchanges.
    /// </summary>
    public static class HeaderMatchModes
    {
        /// <summary>
        /// Requires all specified header values to match.
        /// </summary>
        public const string All = "all";

        /// <summary>
        /// Requires at least one specified header value to match.
        /// </summary>
        public const string Any = "any";
    }
}