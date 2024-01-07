using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using FclEx.Abp.RabbitMQ.Serializers;
using FclEx.Helpers;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FclEx.Abp.RabbitMQ.MsgRoles
{
    public abstract class AsyncConsumer<TInput, TSettings> : MsgProcessor<TSettings>
        where TSettings : ConsumerSettings
    {
        public delegate Task<OperateResult> ConsumeHandler(BasicDeliverEventArgs props, TInput input);
        public delegate Task<OperateResult> ConsumeErrorHandler(BasicDeliverEventArgs props, TInput input, Exception exception);

        protected static Type InputType { get; } = typeof(TInput);
        protected IModel? Channel { get; set; }
        protected AsyncEventingBasicConsumer? RmqConsumer { get; set; }
        protected virtual TimeSpan ProcessInterval { get; } = TimeSpan.Zero;
        public virtual int MaxRetryTimes { get; } = 2;
        protected sealed override bool DispatchConsumersAsync { get; } = true;

        protected AsyncConsumer(IMemoryBytesSerializer? serializer, ILoggerFactory? loggerFactory = null)
            : base(serializer, loggerFactory)
        {
        }
        
        [MemberNotNull(nameof(Channel))]
        public override void Init(TSettings settings)
        {
            base.Init(settings);
            Channel = Connection!.CreateModel();
            var queue = Channel.QueueDeclare(queue: settings.Queue.Name,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            foreach (var key in settings.Queue.BindKeys.Append(queue.QueueName)) // bind queue.QueueName for PushBack
            {
                Channel.QueueBind(queue: queue.QueueName,
                    exchange: settings.Exchange.Name,
                    routingKey: key);
            }
            Channel.BasicQos(prefetchSize: 0,
                prefetchCount: Settings!.Queue.PrefetchCount,
                global: false);

            RmqConsumer = new AsyncEventingBasicConsumer(Channel);
            RmqConsumer.Received += (sender, args) => ConsumeAsync(args);
            Channel.BasicConsume(queue: Settings!.Queue.Name,
                autoAck: false,
                consumer: RmqConsumer);

            Logger.LogInformation("Started an instance");
        }

        protected override IEnumerable<LoggerProperty> GetLogProperties()
        {
            return new LoggerProperty[]
            {
                ("ConsumerType", GetType().ShortName()),
                (nameof(Settings.Queue), Settings!.Queue.Name),
                (nameof(Settings.Queue.BindKeys), Settings!.Queue.BindKeys),
                (nameof(Settings.Exchange), Settings!.Exchange.Name),
                (nameof(InputType), InputType.ShortName())
            };
        }

        protected void PushBack(BasicDeliverEventArgs args)
        {
            // we cannot publish to the default exchange whose name is empty cause it is not a delay exchange.
            Channel.BasicPublish(
                exchange: Settings!.Exchange.Name,
                routingKey: Settings!.Queue.Name,
                basicProperties: args.BasicProperties,
                body: args.Body);

            Logger.LogTrace("Push back successfully");
        }

        protected virtual async Task ConsumeAsync(BasicDeliverEventArgs args)
        {
            var props = args.BasicProperties;
            var watch = ValueStopwatch.StartNew();
            var disposable = Logger.PushProperty(
                props.GetNamedValue(m => m.MessageId)!,
                args.GetNamedValue(m => m.RoutingKey)!
            );

            TInput? obj = default;
            try
            {
                obj = await DeserializeAsync(args).IgnoreSyncContext();
            }
            catch (Exception ex)
            {
                await OnDeserializeDiscardAsync(args, ex).IgnoreSyncContext();
                Channel!.BasicAck(deliveryTag: args.DeliveryTag, multiple: false);
                return;
            }

            Exception? exception = default;
            try
            {
                var result = await ConsumeInternalAsync(args, obj)
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
                Channel!.BasicAck(deliveryTag: args.DeliveryTag, multiple: false);
            }

            props.IncreErrorTimes();
            await OnConsumeErrorAsync(args, obj, exception!).IgnoreSyncContext();
        }

        protected virtual async Task OnConsumeErrorAsync(BasicDeliverEventArgs args, TInput input, Exception exception)
        {
            var props = args.BasicProperties;
            var errorTimes = props.GetErrorTimes();

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

        protected virtual Task<TInput> DeserializeAsync(BasicDeliverEventArgs args)
        {
            var obj = Serializer.Deserialize<TInput>(args.Body);
            return obj.ToTask()!;
        }

        protected virtual Task OnDeserializeDiscardAsync(BasicDeliverEventArgs args, Exception ex)
        {
            Logger.LogError($"The item will be discarded due to an error occured when deserialize: {ex.Message}", ex);
            return Task.CompletedTask;
        }

        protected abstract Task<OperateResult> ConsumeInternalAsync(BasicDeliverEventArgs args, TInput message);

        protected virtual Task OnConsumeRetryAsync(BasicDeliverEventArgs args, TInput input, Exception exception)
        {
            var delay = (int)args.BasicProperties.GetDelay().TotalSeconds;
            using (Logger.PushProperty(
                    ("ErrorTimes", args.BasicProperties.GetErrorTimes()),
                    ("DelaySeconds", delay)
                ))
            {
                Logger.LogWarning(exception, $"The item will be requeued to retry after {delay} seconds due to: {exception.Message}");
                PushBack(args);
                return Task.CompletedTask;
            }
        }

        protected virtual Task OnConsumeDiscardAsync(BasicDeliverEventArgs args, TInput input, Exception exception)
        {
            Logger.LogError(exception, "The item will be discarded due to: " + exception.Message);
            return Task.CompletedTask;
        }
    }

    public abstract class AsyncConsumer<TMessage> : AsyncConsumer<TMessage, ConsumerSettings>
    {
        protected AsyncConsumer(IMemoryBytesSerializer? serializer, ILoggerFactory? loggerFactory = null)
            : base(serializer, loggerFactory)
        {
        }
    }
}
