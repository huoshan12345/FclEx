namespace FclEx.RabbitMQ;

[SuppressMessage("ReSharper", "ConvertToPrimaryConstructor")]
public abstract class MessageRouter<TInput, TOutput> : MessageConsumer<TInput, RouterSettings>, IMessageRouter<TInput, TOutput>
{
    protected MessageRouter(
        ILoggerFactory? loggerFactory,
        IMemoryBytesSerializer? serializer,
        IMessageConverter<TInput, TOutput> converter)
        : base(loggerFactory, serializer)
    {
        Converter = converter;
    }

    protected IMessageConverter<TInput, TOutput> Converter { get; }
    protected static Type RouteMessageType { get; } = typeof(TOutput);

    protected override IEnumerable<LoggerProperty> GetLogProperties()
    {
        var s = Settings!;
        return
        [
            ("RouterType", GetType().ShortName()),
            (nameof(Settings.Queue), s.Queue.Name),
            (nameof(Settings.Queue.BindKeys), s.Queue.BindKeys),
            (nameof(Settings.Exchange), s.Exchange.Name),
            (nameof(Settings.TargetExchange), s.TargetExchange.Name),
            (nameof(MessageType), MessageType.ShortName()),
            (nameof(RouteMessageType), RouteMessageType.ShortName()),
        ];
    }

    public override async Task InitializeAsync(RouterSettings settings)
    {
        await base.InitializeAsync(settings);

        Check.NotNull(Settings);
        Check.NotNull(Channel);

        await Channel.ExchangeDeclareAsync(
             exchange: Settings.TargetExchange.Name,
             type: Settings.TargetExchange.Type,
             durable: true,
             autoDelete: false,
             arguments: null,
             isDelayed: Settings.TargetExchange.IsDelayed);
    }

    protected virtual async Task<OperateResult> RouteAsync(BasicDeliverEventArgs args, TInput input)
    {
        var output = await ConvertAsync(args, input).IgnoreSyncContext();
        return await RouteAsync(args, input, output).IgnoreSyncContext();
    }

    protected virtual async Task<OperateResult> RouteAsync(BasicDeliverEventArgs args, TInput input, TOutput output)
    {
        Check.NotNull(Channel);
        Check.NotNull(Settings);

        var properties = args.BasicProperties;
        if (output is not null)
        {
            var key = GetRoutingKey(properties, output);
            await BasicPublishAsync(Channel, Settings.TargetExchange.Name, output, key, properties);
        }
        else
        {
            Logger.LogDebug("Null output has been discarded");
        }
        return Operate.Success;
    }

    protected override Task<OperateResult> ConsumeActionAsync(BasicDeliverEventArgs args, TInput message)
    {
        return RouteAsync(args, message);
    }

    protected virtual Task<TOutput> ConvertAsync(BasicDeliverEventArgs args, TInput input)
    {
        return Converter.ConvertAsync(input);
    }

    protected abstract string GetRoutingKey(IReadOnlyBasicProperties properties, TOutput output);
}