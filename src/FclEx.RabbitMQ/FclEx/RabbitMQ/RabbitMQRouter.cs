namespace FclEx.RabbitMQ;

/// <summary>Converts one input into one output and acknowledges the input only after confirmed output publication.</summary>
/// <remarks>Output confirmation and input acknowledgement are not atomic. A crash between them can duplicate output.
/// Output inherits the input ID when present, its correlation ID, and only explicitly allowlisted business headers.</remarks>
public sealed class RabbitMQRouter<TInput, TOutput> : IAsyncDisposable
{
    private readonly ConsumerRunner<TInput> _runner;
    private readonly RabbitMQPublisher<TOutput> _publisher;
    private readonly RabbitMQClient _client;
    private readonly object _disposeLock = new();
    private Task? _disposeTask;

    internal RabbitMQRouter(RabbitMQClient client, IChannel channel, RabbitMQRouterOptions options,
        IMessageConverter<TInput, TOutput> converter, RabbitMQPublisher<TOutput> publisher,
        Func<ReceivedMessage<TInput>, TOutput, RabbitMQPublishOptions>? selector)
    {
        _client = client;
        _publisher = publisher;
        _runner = new(client, channel, options.Input, async (input, token) =>
        {
            var output = await converter.ConvertAsync(input, token).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            var selected = selector is null ? options.Output : OptionsSnapshot.NotNull(selector(input, output), nameof(selector));
            var message = new PublishMessage<TOutput>(output, selected,
                messageId: string.IsNullOrEmpty(input.MessageId) ? null : input.MessageId,
                correlationId: string.IsNullOrEmpty(input.CorrelationId) ? null : input.CorrelationId,
                headers: input.ForwardHeaders(options.ForwardedHeaderNames));
            await publisher.PublishAsync(message, token).ConfigureAwait(false);
            return MessageHandlingResult.Completed;
        });
    }

    /// <summary>Gets the input queue name.</summary>
    public string QueueName => _runner.QueueName;
    /// <summary>Gets the target exchange name.</summary>
    public string TargetExchangeName => _publisher.ExchangeName;

    /// <summary>Starts routing once; the task represents consumption, confirmed forwarding, and shutdown.</summary>
    public Task RunAsync(CancellationToken cancellationToken = default) => _runner.RunAsync(cancellationToken);

    /// <summary>Stops the input subscription before closing the output publisher. Broker resources are retained.</summary>
    public ValueTask DisposeAsync()
    {
        lock (_disposeLock) return new(_disposeTask ??= DisposeCoreAsync());
    }

    private async Task DisposeCoreAsync()
    {
        try { await _runner.DisposeAsync().ConfigureAwait(false); }
        finally
        {
            try { await _publisher.DisposeAsync().ConfigureAwait(false); }
            finally { _client.Unregister(this); }
        }
    }
}
