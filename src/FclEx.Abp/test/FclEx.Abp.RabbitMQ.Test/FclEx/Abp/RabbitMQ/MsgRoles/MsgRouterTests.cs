using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Abp.RabbitMQ.MsgRoles.Testers;
using FclEx.Extensions;
using FclEx.Helpers;
using FclEx.Utils;
using Xunit;

namespace FclEx.Abp.RabbitMQ.MsgRoles
{
    public class MsgRouterTests
    {
        private static string GetRoutingKey(string output)
        {
            if (int.TryParse(output, out var number))
            {
                return GetKey(number, "output.number.");
            }
            else
            {
                var str = output.TrimStart("msg_");
                return GetKeyForStr(str, "output.string.");
            }

            static string GetNumType(int num)
            {
                return (num % 2 == 0) ? "even" : "odd";
            }

            static string GetKey(int num, string prefix)
            {
                return prefix + GetNumType(num);
            }

            static string GetKeyForStr(string str, string prefix)
            {
                if (int.TryParse(str, out var num))
                {
                    return GetKey(num, prefix);
                }
                else
                {
                    var hash = str.GetHashCodeSafely();
                    return prefix + GetNumType(hash);
                }
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Route_Test(bool sameAsInputExchange)
        {
            var inputExchange = new ExchangeSettings
            {
                Name = "test.router.input",
                Type = "topic",
                IsDelayed = true
            };
            var connection = GlobalConstants.RmqConnection;
            using var publisher = new TestPublisher(new PublisherSettings(connection, inputExchange));

            var routerSettings = new RouterSettings(connection, inputExchange, new QueueSettings
            {
                Name = "test.router",
                BindKeys = new[] { "input.#" },
            }, new ExchangeSettings
            {
                Name = sameAsInputExchange ? inputExchange.Name : inputExchange.Name + ".output",
                Type = "topic",
                IsDelayed = true
            });

            using var semaphore = new SemaphoreSlim(0);
            using var router = new TestRouter(routerSettings, GetRoutingKey);

            var evenList = new SortedSet<string>();
            using var evenConsumer = new TestConsumer(new ConsumerSettings(connection,
                routerSettings.TargetExchange, new QueueSettings
                {
                    Name = "test.router.even",
                    BindKeys = new[] { "output.*.even" },
                }), m =>
            {
                evenList.Add(m);
                semaphore.Release();
            });

            var stringList = new SortedSet<string>();

            using var stringConsumer = new TestConsumer(new ConsumerSettings(connection,
                routerSettings.TargetExchange, new QueueSettings
                {
                    Name = "test.router.string",
                    BindKeys = new[] { "output.string.*" },
                }), m =>
            {
                stringList.Add(m);
                semaphore.Release();
            });

            var msgs = Enumerable.Range(1, 10).Select(m => m.ToString())
                .SelectMany(m => new[] { m, "str_" + m }).ToList();

            publisher.Publish(msgs, "input");

            var evenMsgs = msgs.Where(m => GetRoutingKey(m).EndsWith("even")).ToSortedSet();
            var strings = msgs.Where(m => !int.TryParse(m, out _)).ToSortedSet();

            var flag = await semaphore.WaitAsync(evenMsgs.Count + strings.Count, TimeSpan.FromSeconds(5));
            Assert.True(flag);

            Assert.Equal(evenMsgs, evenList);
            Assert.Equal(strings, stringList);
        }
    }
}
