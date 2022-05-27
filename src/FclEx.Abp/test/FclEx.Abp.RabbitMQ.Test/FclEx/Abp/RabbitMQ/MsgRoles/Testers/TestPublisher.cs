using FclEx.Abp.RabbitMQ.Serializers;
using FclEx.Abp.Serializers;
using Microsoft.Extensions.Logging.Abstractions;
using RabbitMQ.Client;

namespace FclEx.Abp.RabbitMQ.MsgRoles.Testers
{
    public class TestPublisher<T> : Publisher<T>
    {
        protected override bool AutomaticRecoveryEnabled { get; } = false;

        public TestPublisher(PublisherSettings settings)
        {
            Init(settings);
        }

        protected override void DisposeInternal()
        {
            using var channel = Connection.CreateChannel();
            channel.Model.ExchangeDelete(Settings.Exchange.Name);
            base.DisposeInternal();
        }
    }

    public sealed class TestPublisher : TestPublisher<string>
    {
        public TestPublisher(PublisherSettings settings) : base(settings)
        {
        }
    }
}