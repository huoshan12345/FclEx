namespace FclEx.RabbitMQ;

/// <summary>A prepared queue subscription whose handler determines the settlement of each delivery.</summary>
public sealed class RabbitMqConsumer<T> : IAsyncDisposable
{
    private readonly ConsumerRunner<T> _runner;
    private readonly RabbitMqClient _client;
    internal RabbitMqConsumer(RabbitMqClient client, ConsumerRunner<T> runner) { _client = client; _runner = runner; }

    /// <summary>Gets the declared input queue name.</summary>
    public string QueueName => _runner.QueueName;

    /// <summary>Starts this subscription once and runs until cancellation, disposal, or a fatal subscription/settlement failure.</summary>
    /// <remarks>Handler failures enter the queue's retry/dead-letter policy. Requested cancellation completes normally.
    /// Successful in-flight work is acknowledged while draining; incomplete work is not acknowledged.
    /// The returned task includes bounded shutdown. A stopped instance cannot be restarted.</remarks>
    /// <param name="cancellationToken">Stops new work and requests cancellation of in-flight handlers.</param>
    /// <returns>The complete subscription lifetime, including shutdown and any fatal transport failure.</returns>
    public Task RunAsync(CancellationToken cancellationToken = default) => _runner.RunAsync(cancellationToken);

    /// <summary>Stops consumption and releases the channel without deleting the queue or its failure topology.</summary>
    public async ValueTask DisposeAsync()
    {
        try { await _runner.DisposeAsync().ConfigureAwait(false); }
        finally { _client.Unregister(this); }
    }
}
