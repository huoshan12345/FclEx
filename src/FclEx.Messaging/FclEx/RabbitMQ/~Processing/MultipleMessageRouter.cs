namespace FclEx.RabbitMQ;

public abstract class MultipleMessageRouter<TInput, TOutput> : MessageRouter<TInput, IReadOnlyCollection<TOutput>>
{
    protected MultipleMessageRouter(
        ILoggerFactory? loggerFactory,
        IMemoryBytesSerializer? serializer,
        IMessageConverter<TInput, IReadOnlyCollection<TOutput>> Converter)
        : base(loggerFactory, serializer, Converter)
    {
    }

    protected sealed override string GetRoutingKey(IReadOnlyBasicProperties properties, IReadOnlyCollection<TOutput> output)
    {
        throw new NotSupportedException();
    }

    protected abstract string GetRoutingKey(IReadOnlyBasicProperties properties, TOutput output);

    protected override async Task<OperationResult> RouteAsync(BasicDeliverEventArgs args, TInput input, IReadOnlyCollection<TOutput> output)
    {
        Check.NotNull(Channel);
        Check.NotNull(Settings);

        Logger.LogTrace($"Outputted {output.Count} items");
        var properties = args.BasicProperties;
        foreach (var item in output)
        {
            var key = GetRoutingKey(properties, item);
            await BasicPublishAsync(Channel, Settings.TargetExchange.Name, item, key, properties);
        }
        return Operation.Success();
    }
}