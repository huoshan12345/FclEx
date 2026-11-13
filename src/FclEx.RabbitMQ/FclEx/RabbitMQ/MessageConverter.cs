namespace FclEx.RabbitMQ;

/// <summary>Adapts a delegate to a one-input, one-output message converter.</summary>
public sealed class MessageConverter<TInput, TOutput> : IMessageConverter<TInput, TOutput>
{
    private readonly Func<ReceivedMessage<TInput>, CancellationToken, Task<TOutput>> _convert;

    /// <summary>Creates a converter from a non-null asynchronous delegate.</summary>
    public MessageConverter(Func<ReceivedMessage<TInput>, CancellationToken, Task<TOutput>> convert)
    {
        _convert = convert ?? throw new ArgumentNullException(nameof(convert));
    }

    /// <inheritdoc />
    public Task<TOutput> ConvertAsync(ReceivedMessage<TInput> input, CancellationToken cancellationToken)
        => _convert(input, cancellationToken);
}
