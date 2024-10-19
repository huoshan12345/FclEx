namespace FclEx.RabbitMQ;

public abstract class MessageRouter<TInput, TOutput> : MessageConsumer<TInput, RouterSettings>
{
    protected virtual IMessageConverter<TInput, TOutput> Converter { get; }
    protected static Type OutputType { get; } = typeof(TOutput);

    protected MessageRouter(IMessageConverter<TInput, TOutput> converter,
        IMemoryBytesSerializer? serializer = null,
        ILoggerFactory? loggerFactory = null)
        : base(serializer, loggerFactory)
    {
        Converter = converter;
    }

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
            (nameof(InputType), InputType.ShortName()),
            (nameof(OutputType), OutputType.ShortName()),
        ];
    }

    public override void Init(RouterSettings settings)
    {
        base.Init(settings);

        var s = Settings!;
        Channel!.ExchangeDeclareWithAlternate(
            exchange: s.TargetExchange.Name,
            type: Settings!.TargetExchange.Type,
            durable: true,
            autoDelete: false,
            arguments: null!,
            isDelayed: s.TargetExchange.IsDelayed);
    }

    protected virtual async Task<OperateResult> RouteAsync(BasicDeliverEventArgs args, TInput input)
    {
        var output = await ConvertAsync(args, input).IgnoreSyncContext();
        return await RouteAsync(args, input, output).IgnoreSyncContext();
    }

    protected virtual Task<OperateResult> RouteAsync(BasicDeliverEventArgs args, TInput input, TOutput output)
    {
        var props = args.BasicProperties;
        if (output is not null)
        {
            var bytes = Serializer.Serialize(output);
            var key = GetRoutingKey(props, output);

            Channel.BasicPublish(
                exchange: Settings!.TargetExchange.Name,
                routingKey: key,
                basicProperties: props,
                body: bytes);
        }
        else
        {
            Logger.LogDebug("Null output has been discarded");
        }
        return Operate.Success.ToTask();
    }

    protected override Task<OperateResult> ConsumeInternalAsync(BasicDeliverEventArgs args, TInput message)
    {
        return RouteAsync(args, message);
    }

    protected virtual Task<TOutput> ConvertAsync(BasicDeliverEventArgs args, TInput input)
    {
        return Converter.ConvertAsync(input);
    }

    protected abstract string GetRoutingKey(IBasicProperties props, TOutput output);
}

public abstract class MessageRouter<TInput, TOutput, TOutputs> : MessageRouter<TInput, TOutput>
    where TOutputs : ICollection<TOutput>
{
    protected new IMessageConverter<TInput, TOutputs> Converter { get; }

    protected MessageRouter(
        IMessageConverter<TInput, TOutputs> converter,
        IMemoryBytesSerializer? serializer = null,
        ILoggerFactory? loggerFactory = null)
        : base(null!, serializer, loggerFactory)
    {
        Converter = converter;
    }


    protected override async Task<OperateResult> RouteAsync(BasicDeliverEventArgs args, TInput input)
    {
        var props = args.BasicProperties;
        var outputs = await ConvertAsync(args, input).IgnoreSyncContext();
        Logger.LogTrace($"Outputted {outputs.Count} items");

        var results = new List<OperateResult>();
        foreach (var output in outputs)
        {
            await RouteAsync(args, input, output)
                .Do(r => true, r => results.Add(r))
                .IgnoreSyncContext();
        }
        return results.Merge();
    }

    protected new virtual Task<TOutputs> ConvertAsync(BasicDeliverEventArgs args, TInput input)
    {
        return Converter.ConvertAsync(input);
    }
}