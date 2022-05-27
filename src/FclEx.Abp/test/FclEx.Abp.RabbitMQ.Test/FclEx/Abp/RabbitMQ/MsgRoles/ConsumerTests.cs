using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Abp.Models;
using FclEx.Abp.RabbitMQ.MsgRoles.Testers;
using FclEx.Extensions;
using FclEx.Helpers;
using FclEx.Utils;
using Xunit;

namespace FclEx.Abp.RabbitMQ.MsgRoles
{
    public class ConsumerTests
    {
        public static ExchangeSettings DefaultExchange { get; } = new ExchangeSettings
        {
            Name = "test.comsumer",
            Type = "topic",
            IsDelayed = true,
            AlternateName = null
        };

        [Fact]
        public async Task Consume_Test()
        {
            var connection = GlobalConstants.RmqConnection;
            using var publisher = new TestPublisher(new PublisherSettings(connection, DefaultExchange));

            var msgList = Enumerable.Range(1, 10).Select(m => (Seq: m, Msg: "msg_" + m)).ToList();
            var list = new List<string>();

            using var semaphore = new SemaphoreSlim(0);
            using var consumer = new TestConsumer(new ConsumerSettings()
            {
                Connection = connection,
                Exchange = DefaultExchange,
                Queue = new QueueSettings
                {
                    Name = "test.comsumer",
                    BindKeys = new[] { "#" },
                }
            }, m =>
            {
                list.Add(m);
                semaphore.Release();
            });

            publisher.Publish(msgList, m => (m.Msg, m.Seq.ToString()));

            var flag = await semaphore.WaitAsync(msgList.Count, TimeSpan.FromSeconds(5));
            Assert.True(flag);

            Assert.Equal(msgList.Select(m => m.Msg), list);
        }

        private async Task ConsumePushBackTest<T>(T valueToPublish, TimeSpan delay = default)
        {
            var connection = GlobalConstants.RmqConnection;
            using var publisher = new TestPublisher<T>(new PublisherSettings(connection, DefaultExchange));

            var name = typeof(T).ShortName();
            var key = nameof(ConsumePushBackTest) + "." + name;
            var list = new List<T>();

            const int retryTimes = 1;
            using var semaphore = new SemaphoreSlim(0);
            using var consumer = new TestConsumer<T>(new ConsumerSettings
            {
                Connection = connection,
                Exchange = DefaultExchange,
                Queue = new QueueSettings
                {
                    Name = "test.comsumer" + "." + name.ToLower(),
                    BindKeys = new[] { key },
                }
            }, m =>
            {
                list.Add(m);
                semaphore.Release();
                return Operate.Cancel;
            }, retryTimes, m => delay);

            publisher.Publish(valueToPublish, key);

            var (flag, t) = await SimpleWatch.DoAsync(() => semaphore.WaitAsync(retryTimes + 1, delay + TimeSpan.FromSeconds(3)));
            Assert.True(flag);
            Assert.True(delay < t, $"expected time: {delay}, actual time: {t}");

            Assert.True(list.Count == retryTimes + 1);
            foreach (var m in list)
            {
                Assert.Equal(valueToPublish, m);
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(3)]
        public async Task Consume_PushBack_String_Test(int delaySeconds)
        {
            await ConsumePushBackTest("test", TimeSpan.FromSeconds(delaySeconds));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(3)]
        public async Task Consume_PushBack_Int_Test(int delaySeconds)
        {
            await ConsumePushBackTest(10, TimeSpan.FromSeconds(delaySeconds));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(3)]
        public async Task Consume_PushBack_Class_Test(int delaySeconds)
        {
            await ConsumePushBackTest(new Person
            {
                Id = 10,
                Name = "Jim",
                Age = 30,
                CoinCount = 5
            }, TimeSpan.FromSeconds(delaySeconds));
        }

        [Fact]
        public async Task Consume_MultiBind_Test()
        {
            var connection = GlobalConstants.RmqConnection;
            using var publisher = new TestPublisher<string>(new PublisherSettings(connection, DefaultExchange));

            var msgList = Enumerable.Range(1, 10).Select(m => (Seq: m, Msg: "msg_" + m)).ToList();
            var list = new List<string>();

            using var semaphore = new SemaphoreSlim(0);
            using var consumer = new TestConsumer<string>(new ConsumerSettings()
            {
                Connection = connection,
                Exchange = DefaultExchange,
                Queue = new QueueSettings
                {
                    Name = "test.comsumer",
                    BindKeys = new[] { "output.0", "output.1" },
                }
            }, m =>
            {
                list.Add(m);
                semaphore.Release();
            });

            publisher.Publish(msgList, m => (m.Msg, "output." + m.Seq % 3));

            var expectedList = msgList.Where(m => m.Seq % 3 != 2).Select(m => m.Msg).ToList();

            var flag = await semaphore.WaitAsync(expectedList.Count, TimeSpan.FromSeconds(5));
            Assert.True(flag);

            Assert.Equal(expectedList, list);
        }
    }
}
