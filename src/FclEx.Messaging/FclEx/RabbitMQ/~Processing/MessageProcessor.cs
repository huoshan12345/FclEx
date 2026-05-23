namespace FclEx.RabbitMQ;

public abstract class MessageProcessor<TSettings> : IMessageProcessor<TSettings> where TSettings : RabbitMqProcessorOptions
{
    private bool _isDisposed;

    protected MessageProcessor(ILoggerFactory? loggerFactory, IMemoryBytesSerializer? serializer)
    {
        _logger = new(() => CreateLogger(loggerFactory));
        Serializer = serializer ?? JsonMemoryBytesSerializer.Instance;
    }

    [MemberNotNull(nameof(ExchangeName))]
    protected TSettings? Settings { get; set; }

    private readonly Lazy<ILogger> _logger;
    protected ILogger Logger => _logger.Value;
    protected IMemoryBytesSerializer Serializer { get; }
    protected ConnectionFactory? Factory { get; set; }
    protected IConnection? Connection { get; set; }
    protected virtual bool AutomaticRecoveryEnabled => true;
    protected string? ExchangeName => Settings?.Exchange.Name;

    protected virtual IEnumerable<LoggerProperty> GetLogProperties() => [];

    private ILogger CreateLogger(ILoggerFactory? loggerFactory)
    {
        var logger = (loggerFactory ?? NullLoggerFactory.Instance).CreateLogger(GetType());
        if (logger.IsNullOrNullLogger() == false)
            logger = new PropertiesLogger(Logger, GetLogProperties());
        return logger;
    }

    public virtual async Task InitializeAsync(TSettings settings)
    {
        Settings = Check.NotNull(settings);

        var conStr = Settings.Connection.ToString();
        Factory = new ConnectionFactory
        {
            Uri = new Uri(conStr),
            AutomaticRecoveryEnabled = AutomaticRecoveryEnabled,
        };
        Connection = await Factory.CreateConnectionAsync();

        await using var disposable = await Connection.CreateAutoCloseableChannelAsync();
        await disposable.Value.ExchangeDeclareAsync(
            exchange: ExchangeName,
            type: Settings.Exchange.Type,
            durable: true,
            autoDelete: false,
            arguments: null);
    }

    protected ValueTask BasicPublishAsync(IChannel channel, string exchange, ReadOnlyMemory<byte> body, string routingKey, IReadOnlyBasicProperties properties)
    {
        Check.NotNull(Settings);

        return channel.BasicPublishAsync(
            exchange: exchange,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: properties.AsBasicProperties(),
            body: body);
    }

    protected ValueTask BasicPublishAsync<T>(IChannel channel, string exchange, T message, string routingKey, IReadOnlyBasicProperties properties)
    {
        Check.NotNull(Settings);
        var body = Serializer.Serialize(message);
        return BasicPublishAsync(channel, exchange, body, routingKey: routingKey, properties);
    }

    protected virtual async ValueTask DisposeActionAsync()
    {
        if (Connection is null)
            return;

        await Connection.CloseAsync();
        await Connection.DisposeAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
            return;

        GC.SuppressFinalize(this);
        await DisposeActionAsync();
        _isDisposed = true;
    }
}