using System.Text;

namespace FclEx.RabbitMQ;

/// <summary>Owns one RabbitMQ connection and creates independently disposable publishers, consumers, and routers.</summary>
/// <remarks>Requires RabbitMQ 4.3+. Initial delivery scheduling requires 4.4+ on every relevant cluster node.
/// Disposing this client stops its roles before disposing the connection; broker topology is retained.</remarks>
public sealed class RabbitMQClient : IAsyncDisposable
{
    private readonly SemaphoreSlim _lifecycle = new(1, 1);
    private readonly List<IAsyncDisposable> _roles = [];
    private readonly object _rolesLock = new();
    private readonly object _disposeLock = new();
    private Task? _disposeTask;
    private int _disposing;

    internal IConnection Connection { get; }
    internal IMemoryBytesSerializer Serializer { get; }
    internal ILoggerFactory LoggerFactory { get; }
    internal bool AutomaticRecoveryEnabled { get; }
    internal Func<DateTimeOffset> UtcNow { get; }

    internal RabbitMQClient(IConnection connection, IMemoryBytesSerializer serializer, ILoggerFactory loggerFactory,
        bool automaticRecoveryEnabled, Func<DateTimeOffset>? utcNow = null)
    {
        Connection = connection;
        Serializer = serializer;
        LoggerFactory = loggerFactory;
        AutomaticRecoveryEnabled = automaticRecoveryEnabled;
        UtcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
    }

    /// <summary>Connects once using the supplied native factory. Does not dispose the factory or logger factory.</summary>
    /// <param name="factory">Connection, credentials, TLS and recovery configuration; do not mutate it while connecting.</param>
    /// <param name="loggerFactory">Optional logging dependency; credentials and message bodies are never logged by this package.</param>
    /// <param name="serializer">Optional serializer; defaults to verbatim UTF-8 strings and JSON for other values.</param>
    /// <param name="cancellationToken">Cancels initial connection establishment.</param>
    /// <returns>The connected owner. Initial connection and unsupported-version failures propagate.</returns>
    public static async Task<RabbitMQClient> ConnectAsync(ConnectionFactory factory, ILoggerFactory? loggerFactory = null,
        IMemoryBytesSerializer? serializer = null, CancellationToken cancellationToken = default)
    {
        OptionsSnapshot.NotNull(factory, nameof(factory));
        if (factory.AutomaticRecoveryEnabled && !factory.TopologyRecoveryEnabled)
            throw new ArgumentException("Automatic recovery requires topology recovery to restore subscriptions.", nameof(factory));
        var connection = await factory.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);
        var client = new RabbitMQClient(connection, serializer ?? SerializerPresets.Utf8StringOrJson,
            loggerFactory ?? NullLoggerFactory.Instance, factory.AutomaticRecoveryEnabled);
        try
        {
            client.RequireVersion(4, 3);
            return client;
        }
        catch
        {
            await connection.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    /// <summary>Ensures the target exchange and creates a persistent channel with publisher confirms and return tracking.</summary>
    /// <typeparam name="T">The body type serialized by this publisher.</typeparam>
    /// <param name="options">Copied destination and confirmation timeout settings. An exchange declaration never creates unknown subscriber queues.</param>
    /// <param name="cancellationToken">Cancels channel creation and topology declaration.</param>
    /// <returns>A ready publisher, owned by this client and independently disposable.</returns>
    public async Task<RabbitMQPublisher<T>> CreatePublisherAsync<T>(RabbitMQPublisherOptions options, CancellationToken cancellationToken = default)
    {
        var snapshot = OptionsSnapshot.Publisher(options);
        await _lifecycle.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ThrowIfDisposed();
            var publisher = await CreatePublisherCoreAsync<T>(snapshot, cancellationToken).ConfigureAwait(false);
            return await RegisterAsync(publisher).ConfigureAwait(false);
        }
        finally { _lifecycle.Release(); }
    }

    /// <summary>Ensures the queue, its bindings and failure topology, but does not begin consumption.</summary>
    /// <param name="options">Copied subscription configuration. Declaration conflicts fail without deleting existing resources.</param>
    /// <param name="handler">Returns a disposition after completing its work; exceptions retry unless retry is disabled.</param>
    /// <param name="cancellationToken">Cancels resource creation, not the eventual RunAsync lifetime.</param>
    public async Task<RabbitMQConsumer<T>> CreateConsumerAsync<T>(RabbitMQConsumerOptions options,
        Func<ReceivedMessage<T>, CancellationToken, Task<MessageHandlingResult>> handler, CancellationToken cancellationToken = default)
    {
        var snapshot = OptionsSnapshot.Consumer(options);
        OptionsSnapshot.NotNull(handler, nameof(handler));
        await _lifecycle.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ThrowIfDisposed();
            var channel = await CreateConsumerChannelAsync(snapshot, cancellationToken).ConfigureAwait(false);
            ConsumerRunner<T> runner;
            try { runner = new ConsumerRunner<T>(this, channel, snapshot, handler); }
            catch { await AsyncTools.CloseChannelAsync(channel).ConfigureAwait(false); throw; }
            return await RegisterAsync(new RabbitMQConsumer<T>(this, runner)).ConfigureAwait(false);
        }
        finally { _lifecycle.Release(); }
    }

    /// <summary>Prepares all input topology and the target exchange, then creates a one-input, one-output router.</summary>
    /// <param name="options">Copied input, target, default output, and business-header allowlist configuration.</param>
    /// <param name="converter">Converts input without publishing or manually acknowledging it.</param>
    /// <param name="outputOptionsSelector">Optional synchronous per-output routing/scheduling selector; null uses options.Output.</param>
    /// <param name="cancellationToken">Cancels creation. Call RunAsync separately to start consuming.</param>
    public async Task<RabbitMQRouter<TInput, TOutput>> CreateRouterAsync<TInput, TOutput>(RabbitMQRouterOptions options,
        IMessageConverter<TInput, TOutput> converter,
        Func<ReceivedMessage<TInput>, TOutput, RabbitMQPublishOptions>? outputOptionsSelector = null,
        CancellationToken cancellationToken = default)
    {
        var snapshot = OptionsSnapshot.Router(options);
        OptionsSnapshot.NotNull(converter, nameof(converter));
        await _lifecycle.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ThrowIfDisposed();
            var publisher = await CreatePublisherCoreAsync<TOutput>(snapshot.Target, cancellationToken).ConfigureAwait(false);
            try
            {
                var channel = await CreateConsumerChannelAsync(snapshot.Input, cancellationToken).ConfigureAwait(false);
                RabbitMQRouter<TInput, TOutput> router;
                try { router = new(this, channel, snapshot, converter, publisher, outputOptionsSelector); }
                catch { await AsyncTools.CloseChannelAsync(channel).ConfigureAwait(false); throw; }
                return await RegisterAsync(router).ConfigureAwait(false);
            }
            catch
            {
                await publisher.DisposeAsync().ConfigureAwait(false);
                throw;
            }
        }
        finally { _lifecycle.Release(); }
    }

    private async Task<RabbitMQPublisher<T>> CreatePublisherCoreAsync<T>(RabbitMQPublisherOptions options, CancellationToken token)
    {
        RequireVersion(4, 3);
        var channel = await Connection.CreateChannelAsync(new CreateChannelOptions(true, true), token).ConfigureAwait(false);
        try
        {
            await channel.EnsureExchangeAsync(options.Exchange, token).ConfigureAwait(false);
            return new(this, channel, options);
        }
        catch { await AsyncTools.CloseChannelAsync(channel).ConfigureAwait(false); throw; }
    }

    private async Task<IChannel> CreateConsumerChannelAsync(RabbitMQConsumerOptions options, CancellationToken token)
    {
        RequireVersion(4, 3);
        var channel = await Connection.CreateChannelAsync(new CreateChannelOptions(false, false,
            consumerDispatchConcurrency: options.MaxConcurrency), token).ConfigureAwait(false);
        try
        {
            foreach (var binding in options.Bindings)
                await channel.EnsureExchangeAsync(binding.Exchange, token).ConfigureAwait(false);
            await channel.EnsureQueueAsync(options.Queue, token).ConfigureAwait(false);
            foreach (var binding in options.Bindings)
                await channel.EnsureBindingAsync(options.Queue.Name, binding, token).ConfigureAwait(false);
            await channel.BasicQosAsync(0, options.PrefetchCount, global: false, cancellationToken: token).ConfigureAwait(false);
            return channel;
        }
        catch { await AsyncTools.CloseChannelAsync(channel).ConfigureAwait(false); throw; }
    }

    private async Task<T> RegisterAsync<T>(T role) where T : IAsyncDisposable
    {
        if (Volatile.Read(ref _disposing) != 0)
        {
            await role.DisposeAsync().ConfigureAwait(false);
            throw new ObjectDisposedException(nameof(RabbitMQClient));
        }
        lock (_rolesLock) _roles.Add(role);
        return role;
    }

    internal void Unregister(IAsyncDisposable role)
    {
        lock (_rolesLock) _roles.Remove(role);
    }

    internal void RequireVersion(int major, int minor)
    {
        var properties = Connection.ServerProperties;
        var value = properties is not null && properties.TryGetValue("version", out var version) ? version : null;
        var text = value is byte[] bytes ? Encoding.UTF8.GetString(bytes) : value as string;
        var numeric = text is null ? "" : new string(text.TakeWhile(c => char.IsDigit(c) || c == '.').ToArray()).TrimEnd('.');
        if (!Version.TryParse(numeric, out var parsed) || parsed < new Version(major, minor))
            throw new NotSupportedException($"This operation requires RabbitMQ {major}.{minor}+; connected broker reports '{text ?? "unknown"}'.");
    }

    internal void ThrowIfDisposed()
    {
        if (Volatile.Read(ref _disposing) != 0)
            throw new ObjectDisposedException(nameof(RabbitMQClient));
    }

    /// <summary>Stops every owned role and disposes the connection, attempting all cleanup even if one role fails.</summary>
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
        await _lifecycle.WaitAsync().ConfigureAwait(false);
        var errors = new List<Exception>();
        try
        {
            IAsyncDisposable[] roles;
            lock (_rolesLock) { roles = _roles.ToArray(); _roles.Clear(); }
            for (var i = roles.Length - 1; i >= 0; i--)
                try { await roles[i].DisposeAsync().ConfigureAwait(false); }
                catch (Exception ex) { errors.Add(ex); }
            try { await Connection.DisposeAsync().ConfigureAwait(false); }
            catch (Exception ex) { errors.Add(ex); }
        }
        finally { _lifecycle.Release(); }
        if (errors.Count > 0)
            throw new AggregateException("RabbitMQ cleanup failed.", errors);
    }
}
