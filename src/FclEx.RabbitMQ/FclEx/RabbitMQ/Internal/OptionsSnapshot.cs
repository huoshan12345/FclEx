using System.Text;

namespace FclEx.RabbitMQ.Internal;

internal static class OptionsSnapshot
{
    private static readonly HashSet<string> QueueKeys = new(StringComparer.Ordinal)
    {
        "x-queue-type", "x-dead-letter-exchange", "x-dead-letter-routing-key", "x-dead-letter-strategy",
        "x-overflow", "x-delivery-limit", "x-delayed-retry-type", "x-delayed-retry-min", "x-delayed-retry-max",
        "x-max-length", "x-max-length-bytes", "x-message-ttl", "x-expires",
    };

    internal static T NotNull<T>(T? value, string name) where T : class
        => value ?? throw new ArgumentNullException(name);

    internal static string ShortString(string value, string name, bool allowEmpty = true)
    {
        NotNull(value, name);
        if ((!allowEmpty && string.IsNullOrWhiteSpace(value)) || Encoding.UTF8.GetByteCount(value) > 255)
            throw new ArgumentException("The name must fit 255 UTF-8 bytes and must not be blank when required.", name);
        return value;
    }

    internal static string QueueName(string value, string name)
    {
        ShortString(value, name, false);
        if (value.StartsWith("amq.", StringComparison.Ordinal))
            throw new ArgumentException("Application queue names cannot start with amq.", name);
        return value;
    }

    internal static long Milliseconds(TimeSpan value)
        => checked(value.Ticks / TimeSpan.TicksPerMillisecond + (value.Ticks % TimeSpan.TicksPerMillisecond == 0 ? 0 : 1));

    private static TimeSpan Timeout(TimeSpan value, string name)
    {
        if (value <= TimeSpan.Zero || Milliseconds(value) > int.MaxValue)
            throw new ArgumentOutOfRangeException(name, "Timeouts must be positive and at most Int32.MaxValue milliseconds.");
        return TimeSpan.FromMilliseconds(Milliseconds(value));
    }

    internal static RabbitMQExchangeOptions Exchange(RabbitMQExchangeOptions value)
    {
        NotNull(value, nameof(value));
        ShortString(value.Name, nameof(value.Name), !value.Declare);
        if (value.Type is not (ExchangeType.Direct or ExchangeType.Topic or ExchangeType.Fanout or ExchangeType.Headers))
            throw new ArgumentException("Only standard RabbitMQ exchange types are supported.", nameof(value.Type));
        if (value.Declare && value.Name.StartsWith("amq.", StringComparison.Ordinal))
            throw new ArgumentException("Reference predefined exchanges with Declare=false.", nameof(value.Name));
        return new() { Name = value.Name, Type = value.Type, Declare = value.Declare, Arguments = HeaderSnapshot.ReadOnly(NotNull(value.Arguments, nameof(value.Arguments))) };
    }

    internal static RabbitMQBindingOptions Binding(RabbitMQBindingOptions value)
    {
        NotNull(value, nameof(value));
        var exchange = Exchange(value.Exchange);
        if (exchange.Name.Length == 0)
            throw new ArgumentException("The default exchange cannot have explicit queue bindings.", nameof(value.Exchange));
        return new() { Exchange = exchange, RoutingKey = ShortString(value.RoutingKey, nameof(value.RoutingKey)), Arguments = HeaderSnapshot.ReadOnly(NotNull(value.Arguments, nameof(value.Arguments))) };
    }

    internal static RabbitMQQueueOptions Queue(RabbitMQQueueOptions value)
    {
        NotNull(value, nameof(value));
        var name = QueueName(value.Name, nameof(value.Name));
        var retry = NotNull(value.Retry, nameof(value.Retry));
        if (retry.MinDelay <= TimeSpan.Zero || retry.MaxDelay < retry.MinDelay || retry.DeliveryLimit <= 0)
            throw new ArgumentException("Retry delays must be positive and ordered; the delivery limit must be positive.", nameof(value.Retry));
        if (value.MaxLength is <= 0 || value.MaxLengthBytes is <= 0)
            throw new ArgumentException("Queue capacity limits must be positive.", nameof(value));
        var dead = NotNull(value.DeadLetter, nameof(value.DeadLetter));
        var deadExchange = ShortString(dead.ExchangeName ?? name + ".dlx", nameof(dead.ExchangeName), false);
        if (deadExchange.StartsWith("amq.", StringComparison.Ordinal))
            throw new ArgumentException("The owned dead-letter exchange cannot use a predefined name.", nameof(value));
        var deadQueue = QueueName(dead.QueueName ?? name + ".dead", nameof(dead.QueueName));
        if (deadQueue == name)
            throw new ArgumentException("The dead-letter queue must differ from the source queue.", nameof(value));
        var arguments = HeaderSnapshot.Copy(NotNull(value.Arguments, nameof(value.Arguments)));
        if (arguments.Keys.Any(QueueKeys.Contains))
            throw new ArgumentException("Extra arguments cannot override queue type, retry, dead-letter, capacity or expiration settings.", nameof(value.Arguments));
        return new()
        {
            Name = name,
            Retry = new() { Enabled = retry.Enabled, MinDelay = retry.MinDelay, MaxDelay = retry.MaxDelay, DeliveryLimit = retry.DeliveryLimit },
            DeadLetter = new() { ExchangeName = deadExchange, QueueName = deadQueue, RoutingKey = ShortString(dead.RoutingKey ?? name, nameof(dead.RoutingKey)) },
            MaxLength = value.MaxLength,
            MaxLengthBytes = value.MaxLengthBytes,
            Arguments = HeaderSnapshot.ReadOnly(arguments),
        };
    }

    internal static RabbitMQConsumerOptions Consumer(RabbitMQConsumerOptions value)
    {
        NotNull(value, nameof(value));
        if (value.MaxConcurrency == 0 || value.PrefetchCount < value.MaxConcurrency)
            throw new ArgumentException("Concurrency must be positive and prefetch must be at least concurrency.", nameof(value));
        return new()
        {
            Queue = Queue(value.Queue),
            Bindings = NotNull(value.Bindings, nameof(value.Bindings)).Select(Binding).ToArray(),
            MaxConcurrency = value.MaxConcurrency,
            PrefetchCount = value.PrefetchCount,
            ShutdownTimeout = Timeout(value.ShutdownTimeout, nameof(value.ShutdownTimeout)),
        };
    }

    internal static RabbitMQPublisherOptions Publisher(RabbitMQPublisherOptions value)
    {
        NotNull(value, nameof(value));
        return new() { Exchange = Exchange(value.Exchange), ConfirmationTimeout = Timeout(value.ConfirmationTimeout, nameof(value.ConfirmationTimeout)) };
    }

    internal static RabbitMQPublishOptions Publish(RabbitMQPublishOptions value)
    {
        NotNull(value, nameof(value));
        if (value.Delay < TimeSpan.Zero || value.Delay.HasValue && value.DeliverAt.HasValue)
            throw new ArgumentException("Delay must be nonnegative and cannot be combined with DeliverAt.", nameof(value));
        return new() { RoutingKey = ShortString(value.RoutingKey, nameof(value.RoutingKey)), Delay = value.Delay, DeliverAt = value.DeliverAt };
    }

    internal static RabbitMQRouterOptions Router(RabbitMQRouterOptions value)
    {
        NotNull(value, nameof(value));
        var headers = NotNull(value.ForwardedHeaderNames, nameof(value.ForwardedHeaderNames)).ToArray();
        foreach (var name in headers)
        {
            ShortString(name, nameof(value.ForwardedHeaderNames), false);
            if (HeaderSnapshot.IsReserved(name))
                throw new ArgumentException($"Cannot forward transport header '{name}'.", nameof(value));
        }
        return new() { Input = Consumer(value.Input), Target = Publisher(value.Target), Output = Publish(value.Output), ForwardedHeaderNames = headers };
    }
}
