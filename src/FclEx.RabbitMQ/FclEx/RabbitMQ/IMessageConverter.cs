namespace FclEx.RabbitMQ;

/// <summary>Converts one received value into one output value without publishing or acknowledging it.</summary>
/// <typeparam name="TInput">The deserialized input type.</typeparam>
/// <typeparam name="TOutput">The output type, including null when allowed by the configured serializer.</typeparam>
public interface IMessageConverter<TInput, TOutput>
{
    /// <summary>Converts the input. Exceptions enter the input queue's retry policy.</summary>
    /// <param name="input">The received value and copied, read-only metadata.</param>
    /// <param name="cancellationToken">Signals that this invocation should stop; honoring it leaves incomplete work unacknowledged.</param>
    /// <returns>The output value. Null is a value, not a request to filter the input.</returns>
    Task<TOutput> ConvertAsync(ReceivedMessage<TInput> input, CancellationToken cancellationToken);
}
