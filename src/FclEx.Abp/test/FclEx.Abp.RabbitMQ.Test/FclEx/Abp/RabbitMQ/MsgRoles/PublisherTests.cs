using System;
using System.Linq;
using FclEx.Abp.RabbitMQ.MsgRoles.Testers;
using Xunit;

namespace FclEx.Abp.RabbitMQ.MsgRoles
{
    public class PublisherTests
    {
        public static ExchangeSettings DefaultExchange { get; } = new ExchangeSettings
        {
            Name = "test.publisher",
            Type = "topic",
            IsDelayed = true,
            AlternateName = null
        };

        private static TestPublisher CreatePublisher()
        {
            var connection = GlobalConstants.RmqConnection;
            return new TestPublisher(new PublisherSettings(connection, DefaultExchange));
        }

        [Fact]
        public void Publish_Test()
        {
            using var publisher = CreatePublisher();
            publisher.Publish("test", "test");
        }

        [Fact]
        public void Publish_Serially_Test()
        {
            using var publisher = CreatePublisher();
            for (var i = 0; i < 10; i++)
            {
                publisher.Publish("test", "test");
            }
        }

        [Fact]
        public void Publish_List_Test()
        {
            using var publisher = CreatePublisher();
            publisher.Publish(Enumerable.Range(1, 10).Select(m => "test"), "test");
        }

        [Fact]
        public void Publish_Multi_Test()
        {
            using var publisher1 = CreatePublisher();
            using var publisher2 = CreatePublisher();
            publisher1.Publish("test", "test");
            publisher2.Publish("test", "test");
        }
    }
}
