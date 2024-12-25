namespace FclEx.RabbitMQ;

public class MessagePublisher : MessageProcessor<PublisherSettings>, IMessagePublisher
{
    protected MessagePublisher(ILoggerFactory? loggerFactory, IMemoryBytesSerializer? serializer)
        : base(loggerFactory, serializer)
    {
    }
    
    protected override IEnumerable<LoggerProperty> GetLogProperties()
    {
        return
        [
            ("PublisherType", GetType().ShortName()),
            ("TargetExchange", Settings?.Exchange.Name),
        ];
    }

    public override async Task InitializeAsync(PublisherSettings settings)
    {
        await base.InitializeAsync(settings);
        Logger.LogInformation("Started an instance");
    }

    protected async Task PublishAsync<T>(IChannel channel, RoutingMessage<T> message)
    {
        Check.NotNull(Settings);

        var properties = new BasicProperties
        {
            MessageId = message.Id.ToStringOrEmpty(),
        };
        properties.SetDelay(message.Delay);

        var disposable = Logger.PushProperty(
            (nameof(properties.MessageId), properties.MessageId),
            (nameof(message.RoutingKey), message.RoutingKey)
        );
        try
        {
            await BasicPublishAsync(channel, ExchangeName, message.Body, message.RoutingKey, properties);
            Logger.LogTrace("Publish successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError($"An error occured when publishing: {ex.Message}", ex);
            throw;
        }
        finally
        {
            disposable.Dispose();
        }
    }

    public async Task PublishAsync<T>(IEnumerable<RoutingMessage<T>> msgs)
    {
        Check.NotNull(Connection);

        await using var channel = await Connection.CreateAutoCloseableChannelAsync();
        foreach (var msg in msgs)
        {
            await PublishAsync(channel.Value, msg);
        }
    }
}