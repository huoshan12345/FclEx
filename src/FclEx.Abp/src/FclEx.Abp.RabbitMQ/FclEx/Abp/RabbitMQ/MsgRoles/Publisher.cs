using System;
using System.Collections.Generic;

using FclEx.Abp.RabbitMQ.Serializers;
using FclEx.Extensions;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace FclEx.Abp.RabbitMQ.MsgRoles
{
    public abstract class Publisher<TOutput> : MsgProcessor<PublisherSettings>
    {
        protected static Type OutputType { get; } = typeof(TOutput);

        protected Publisher(IMemoryBytesSerializer? serializer = null,
            ILoggerFactory? loggerFactory = null)
            : base(serializer, loggerFactory)
        {
        }

        protected override IEnumerable<(string, object)> GetLogProperties()
        {
            return new (string, object)[]
            {
                ("PublisherType", GetType().ShortName()),
                ("TargetExchange", Settings!.Exchange.Name),
                (nameof(OutputType), OutputType.ShortName()),
            };
        }

        public override void Init(PublisherSettings settings)
        {
            base.Init(settings);
            Logger.LogInformation("Started an instance");
        }

        protected void Publish(IModel channel, OutputMessage<TOutput> msg)
        {
            var props = channel.CreateBasicProperties();
            props.MessageId = msg.Id.ToStringOrEmpty();
            props.SetDelay(msg.Delay);

            var disposable = Logger.PushProperty(
                props.GetNamedValue(m => m.MessageId)!,
                msg.GetNamedValue(m => m.RoutingKey)!
            );
            try
            {
                var body = Serializer.Serialize(msg.Body);
                channel.BasicPublish(
                    exchange: Settings!.Exchange.Name,
                    routingKey: msg.RoutingKey,
                    basicProperties: props,
                    body: body);
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

        public void Publish(OutputMessage<TOutput> msg)
        {
            using var channel = Connection!.CreateChannel();
            Publish(channel.Model, msg);
        }

        public void Publish(IEnumerable<OutputMessage<TOutput>> msgs)
        {
            using var channel = Connection!.CreateChannel();
            foreach (var msg in msgs)
            {
                Publish(channel.Model, msg);
            }
        }
    }
}
