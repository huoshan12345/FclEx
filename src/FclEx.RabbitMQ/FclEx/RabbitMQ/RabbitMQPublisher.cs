namespace FclEx.RabbitMQ;

/// <summary>Publishes persistent messages to one exchange, waiting for tracked broker confirmations and mandatory returns.</summary>
/// <typeparam name="T">The serialized body type.</typeparam>
/// <remarks>Calls on the channel are serialized. Publication failures or cancellation after sending can have an unknown outcome;
/// reuse a stable ID and idempotent consumers when retrying. No automatic republication is performed.</remarks>
public sealed class RabbitMQPublisher<T> : IAsyncDisposable
{
    private readonly RabbitMQClient _client;
    private readonly IChannel _channel;
    private readonly RabbitMQPublisherOptions _options;
    private readonly ILogger _logger;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly CancellationTokenSource _shutdown = new();
    private readonly object _disposeLock = new();
    private Task? _disposeTask;
    private int _disposing;

    internal RabbitMQPublisher(RabbitMQClient client, IChannel channel, RabbitMQPublisherOptions options)
    {
        _client = client;
        _channel = channel;
        _options = options;
        _logger = client.LoggerFactory.CreateLogger<RabbitMQPublisher<T>>();
    }

    /// <summary>Gets the fixed exchange name; empty denotes the built-in default exchange.</summary>
    public string ExchangeName => _options.Exchange.Name;

    /// <summary>Publishes once and waits for broker acceptance. Does not wait for consumers to execute.</summary>
    /// <param name="message">An envelope whose stable ID is preserved; its body is serialized on each call.</param>
    /// <param name="cancellationToken">Cancels queuing/sending/confirmation; cancellation after sending cannot retract the message.</param>
    /// <remarks>Positive initial delays require RabbitMQ 4.4 and quorum destinations. No binding is created for unknown subscribers.</remarks>
    /// <exception cref="TimeoutException">The configured confirmation timeout expires, including channel queuing time.</exception>
    public async Task PublishAsync(PublishMessage<T> message, CancellationToken cancellationToken = default)
    {
        OptionsSnapshot.NotNull(message, nameof(message));
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();
        var deliveryTime = DeliveryTime(message.Options, _client.UtcNow());
        if (deliveryTime.HasValue) _client.RequireVersion(4, 4);
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _shutdown.Token);
        deadline.CancelAfter(_options.ConfirmationTimeout);
        var entered = false;
        try
        {
            await _gate.WaitAsync(deadline.Token).ConfigureAwait(false);
            entered = true;
            ThrowIfDisposed();
            if (deliveryTime.HasValue) _client.RequireVersion(4, 4);
            var headers = message.CopyHeaders();
            if (deliveryTime.HasValue) headers["x-opt-delivery-time"] = deliveryTime.Value;
            var properties = new BasicProperties
            {
                Persistent = true,
                MessageId = message.MessageId,
                CorrelationId = message.CorrelationId,
                Headers = headers,
            };
            if (ReferenceEquals(_client.Serializer, SerializerPresets.Utf8StringOrJson))
            {
                properties.ContentType = message.Body is string ? "text/plain" : "application/json";
                properties.ContentEncoding = "utf-8";
            }
            var body = _client.Serializer.Serialize(message.Body, typeof(T));
            await _channel.BasicPublishAsync(ExchangeName, message.Options.RoutingKey, mandatory: true,
                basicProperties: properties, body: body, cancellationToken: deadline.Token).ConfigureAwait(false);
            _logger.LogTrace("Published {MessageId} to {Exchange} with {RoutingKey}", message.MessageId, ExchangeName, message.Options.RoutingKey);
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested && !_shutdown.IsCancellationRequested)
        {
            throw new TimeoutException("RabbitMQ publication timed out; broker acceptance may be unknown.", ex);
        }
        finally { if (entered) _gate.Release(); }
    }

    internal static long? DeliveryTime(RabbitMQPublishOptions options, DateTimeOffset now)
    {
        var at = options.DeliverAt ?? (options.Delay.HasValue ? now.Add(options.Delay.Value) : now);
        if (at <= now) return null;
        var epochTicks = checked(at.UtcDateTime.Ticks - 621355968000000000L);
        return checked(epochTicks / TimeSpan.TicksPerMillisecond + (epochTicks % TimeSpan.TicksPerMillisecond == 0 ? 0 : 1));
    }

    private void ThrowIfDisposed()
    {
        if (Volatile.Read(ref _disposing) != 0) throw new ObjectDisposedException(nameof(RabbitMQPublisher<T>));
        _client.ThrowIfDisposed();
    }

    /// <summary>Cancels outstanding publications and closes this publisher's channel, retaining its exchange and messages.</summary>
    public ValueTask DisposeAsync()
    {
        lock (_disposeLock)
        {
            Interlocked.Exchange(ref _disposing, 1);
            return new(_disposeTask ??= DisposeCoreAsync());
        }
    }

    private async Task DisposeCoreAsync()
    {
        _shutdown.Cancel();
        await _gate.WaitAsync().ConfigureAwait(false);
        try { await AsyncTools.CloseChannelAsync(_channel).ConfigureAwait(false); }
        finally { _gate.Release(); _shutdown.Dispose(); _client.Unregister(this); }
    }
}
