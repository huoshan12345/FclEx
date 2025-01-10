namespace FclEx.RabbitMQ;

public abstract class MessageConsumer<T, TSettings> : MessageProcessor<TSettings>, IMessageConsumer<T, TSettings>
    where TSettings : ConsumerSettings
{
    public delegate Task<OperationResult> ConsumeHandler(BasicDeliverEventArgs props, T input);
    public delegate Task<OperationResult> ConsumeErrorHandler(BasicDeliverEventArgs props, T input, Exception exception);

    protected MessageConsumer(ILoggerFactory? loggerFactory, IMemoryBytesSerializer? serializer) : base(loggerFactory, serializer)
    {
    }

    protected static Type MessageType { get; } = typeof(T);

    protected IChannel? Channel { get; set; }
    protected AsyncEventingBasicConsumer? Consumer { get; set; }
    protected virtual TimeSpan ProcessInterval => TimeSpan.Zero;
    protected virtual int MaxRetryTimes => 2;

    public override async Task InitializeAsync(TSettings settings)
    {
        await base.InitializeAsync(settings);

        Check.NotNull(Settings);
        Check.NotNull(Connection);

        Channel = await Connection.CreateChannelAsync();
        var queue = await Channel.QueueDeclareAsync(queue: settings.Queue.Name,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        foreach (var key in Settings.Queue.BindKeys.Append(queue.QueueName)) // bind queue.QueueName for PushBack
        {
            await Channel.QueueBindAsync(
                queue: queue.QueueName,
                exchange: Settings.Exchange.Name,
                routingKey: key);
        }

        await Channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: Settings.Queue.PrefetchCount,
            global: false);

        Consumer = new AsyncEventingBasicConsumer(Channel);
        Consumer.ReceivedAsync += (sender, args) => ConsumeAsync(args);
        await Channel.BasicConsumeAsync(
            queue: Settings.Queue.Name,
            autoAck: false,
            consumer: Consumer);

        Logger.LogInformation("Started an instance");
    }

    protected override IEnumerable<LoggerProperty> GetLogProperties()
    {
        return
        [
            ("ConsumerType", GetType().ShortName()),
            (nameof(Settings.Queue), Settings!.Queue.Name),
            (nameof(Settings.Queue.BindKeys), Settings!.Queue.BindKeys),
            (nameof(Settings.Exchange), Settings!.Exchange.Name),
            (nameof(MessageType), MessageType.ShortName()),
        ];
    }

    protected async Task PushBackAsync(BasicDeliverEventArgs args)
    {
        Check.NotNull(Settings);
        Check.NotNull(Channel);

        // we cannot publish to the default exchange whose name is empty because it is not a delay exchange.
        await BasicPublishAsync(Channel, ExchangeName, args.Body, args.RoutingKey, args.BasicProperties);
        Logger.LogTrace("Push back successfully");
    }

    protected virtual async Task ConsumeAsync(BasicDeliverEventArgs args)
    {
        Check.NotNull(Channel);

        var properties = args.BasicProperties;
        var watch = ValueStopwatch.StartNew();
        var disposable = Logger.PushProperty(
            (nameof(properties.MessageId), properties.MessageId),
            (nameof(args.RoutingKey), args.RoutingKey)
        );

        T? obj = default;
        try
        {
            obj = await DeserializeAsync(args).IgnoreSyncContext();
        }
        catch (Exception ex)
        {
            await OnDeserializeDiscardAsync(args, ex).IgnoreSyncContext();
            await Channel.BasicAckAsync(deliveryTag: args.DeliveryTag, multiple: false);
            return;
        }

        Exception? exception = default;
        try
        {
            var result = await ConsumeActionAsync(args, obj)
                .Ok(t => Logger.LogTrace("Consume successfully"))
                .Error(e => exception = e)
                .IgnoreSyncContext();

            if (result.Success)
                return;
        }
        catch (Exception ex)
        {
            Logger.LogError($"An error occured when consuming: {ex.Message}", ex);
            exception = ex;
        }
        finally
        {
            Logger.LogTrace($"Consume finished, it takes {watch.GetElapsedTime().TotalSeconds:f3} seconds");
            disposable.Dispose();

            await TaskHelper.Delay(ProcessInterval).IgnoreSyncContext();
            await Channel.BasicAckAsync(deliveryTag: args.DeliveryTag, multiple: false);
        }

        properties.IncreaseErrorTimes();
        await OnConsumeErrorAsync(args, obj, exception!).IgnoreSyncContext();
    }

    protected virtual async Task OnConsumeErrorAsync(BasicDeliverEventArgs args, T input, Exception exception)
    {
        var properties = args.BasicProperties;
        var errorTimes = properties.GetErrorTimes();

        try
        {
            if (errorTimes <= MaxRetryTimes)
            {
                await OnConsumeRetryAsync(args, input, exception).IgnoreSyncContext();
            }
            else
            {
                await OnConsumeDiscardAsync(args, input, exception).IgnoreSyncContext();
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"An error occured when handle consuming error: {ex.Message}", ex);
        }
    }

    protected virtual Task<T> DeserializeAsync(BasicDeliverEventArgs args)
    {
        var obj = Serializer.Deserialize<T>(args.Body);
        return obj.ToTask()!;
    }

    protected virtual Task OnDeserializeDiscardAsync(BasicDeliverEventArgs args, Exception ex)
    {
        Logger.LogError($"The item will be discarded due to an error occured when deserialize: {ex.Message}", ex);
        return Task.CompletedTask;
    }

    protected abstract Task<OperationResult> ConsumeActionAsync(BasicDeliverEventArgs args, T message);

    protected virtual async Task OnConsumeRetryAsync(BasicDeliverEventArgs args, T input, Exception exception)
    {
        var properties = args.BasicProperties;
        var delay = (int)properties.GetDelay().TotalSeconds;
        using (Logger.PushProperty(
                   ("ErrorTimes", properties.GetErrorTimes()),
                   ("DelaySeconds", delay)
               ))
        {
            Logger.LogWarning(exception, $"The item will be re-queued to retry after {delay} seconds due to: {exception.Message}");
            await PushBackAsync(args);
        }
    }

    protected virtual Task OnConsumeDiscardAsync(BasicDeliverEventArgs args, T input, Exception exception)
    {
        Logger.LogError(exception, "The item will be discarded due to: " + exception.Message);
        return Task.CompletedTask;
    }
}

public abstract class MessageConsumer<T> : MessageConsumer<T, ConsumerSettings>, IMessageConsumer<T>
{
    protected MessageConsumer(ILoggerFactory? loggerFactory, IMemoryBytesSerializer? serializer) : base(loggerFactory, serializer)
    {
    }
}