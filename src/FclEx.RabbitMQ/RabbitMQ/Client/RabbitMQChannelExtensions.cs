using FclEx.RabbitMQ;

namespace RabbitMQ.Client;

/// <summary>Ensures durable RabbitMQ topology without deleting or purging existing resources.</summary>
public static class RabbitMQChannelExtensions
{
    /// <summary>Declares a durable standard exchange, or does nothing for an explicit predeclared reference.</summary>
    /// <remarks>Incompatible existing declarations fail with the broker's channel error. The default exchange is referenced, never declared.</remarks>
    public static Task EnsureExchangeAsync(this IChannel channel, RabbitMQExchangeOptions options, CancellationToken cancellationToken = default)
    {
        OptionsSnapshot.NotNull(channel, nameof(channel));
        var snapshot = OptionsSnapshot.Exchange(options);
        return snapshot.Declare
            ? channel.ExchangeDeclareAsync(snapshot.Name, snapshot.Type, durable: true, autoDelete: false,
                arguments: HeaderSnapshot.Copy(snapshot.Arguments), cancellationToken: cancellationToken)
            : Task.CompletedTask;
    }

    /// <summary>Declares the source quorum queue and its independent direct DLX and quorum dead-letter queue.</summary>
    /// <remarks>Sets at-least-once dead lettering and reject-publish overflow. Retry settings require RabbitMQ 4.3+.
    /// Existing conflicting definitions fail; this operation never recreates queues or removes messages.</remarks>
    public static async Task EnsureQueueAsync(this IChannel channel, RabbitMQQueueOptions options, CancellationToken cancellationToken = default)
    {
        OptionsSnapshot.NotNull(channel, nameof(channel));
        var snapshot = OptionsSnapshot.Queue(options);
        var dead = snapshot.DeadLetter;
        await channel.ExchangeDeclareAsync(dead.ExchangeName!, ExchangeType.Direct, durable: true, autoDelete: false, cancellationToken: cancellationToken).ConfigureAwait(false);
        await channel.QueueDeclareAsync(dead.QueueName!, durable: true, exclusive: false, autoDelete: false,
            arguments: new Dictionary<string, object?> { ["x-queue-type"] = "quorum" }, cancellationToken: cancellationToken).ConfigureAwait(false);
        await channel.QueueBindAsync(dead.QueueName!, dead.ExchangeName!, dead.RoutingKey!, cancellationToken: cancellationToken).ConfigureAwait(false);
        var arguments = HeaderSnapshot.Copy(snapshot.Arguments);
        arguments["x-queue-type"] = "quorum";
        arguments["x-dead-letter-exchange"] = dead.ExchangeName;
        arguments["x-dead-letter-routing-key"] = dead.RoutingKey;
        arguments["x-dead-letter-strategy"] = "at-least-once";
        arguments["x-overflow"] = "reject-publish";
        arguments["x-delivery-limit"] = snapshot.Retry.DeliveryLimit;
        if (snapshot.MaxLength.HasValue) arguments["x-max-length"] = snapshot.MaxLength.Value;
        if (snapshot.MaxLengthBytes.HasValue) arguments["x-max-length-bytes"] = snapshot.MaxLengthBytes.Value;
        if (snapshot.Retry.Enabled)
        {
            arguments["x-delayed-retry-type"] = "all";
            arguments["x-delayed-retry-min"] = OptionsSnapshot.Milliseconds(snapshot.Retry.MinDelay);
            arguments["x-delayed-retry-max"] = OptionsSnapshot.Milliseconds(snapshot.Retry.MaxDelay);
        }
        else
            arguments["x-delayed-retry-type"] = "disabled";
        await channel.QueueDeclareAsync(snapshot.Name, durable: true, exclusive: false, autoDelete: false,
            arguments: arguments, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Ensures the source exchange and binds an already declared queue to it.</summary>
    /// <remarks>Does not declare the queue; call EnsureQueueAsync first. Explicit bindings to the default exchange are rejected.</remarks>
    public static async Task EnsureBindingAsync(this IChannel channel, string queueName, RabbitMQBindingOptions options, CancellationToken cancellationToken = default)
    {
        OptionsSnapshot.NotNull(channel, nameof(channel));
        OptionsSnapshot.QueueName(queueName, nameof(queueName));
        var snapshot = OptionsSnapshot.Binding(options);
        await channel.EnsureExchangeAsync(snapshot.Exchange, cancellationToken).ConfigureAwait(false);
        await channel.QueueBindAsync(queueName, snapshot.Exchange.Name, snapshot.RoutingKey,
            arguments: HeaderSnapshot.Copy(snapshot.Arguments), cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}
