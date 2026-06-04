namespace FclEx.RabbitMQ;

[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
public class RouterTests(RabbitMqTestsFixture fixture) : RabbitMqTests(fixture)
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
            return num % 2 == 0 ? "even" : "odd";
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
                var hash = str.GetHashCode();
                return prefix + GetNumType(hash);
            }
        }
    }

    [RetryTheory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Route_Test(bool sameAsInputExchange)
    {
        var inputExchange = new RabbitMqExchangeOptions
        {
            Name = GetExchangeName(nameof(Route_Test) + "_input", sameAsInputExchange),
        };
        var connection = RabbitMqTestsFixture.ConnectionSettings;
        await using var publisher = await TestPublisher.CreateAsync(new RabbitMqPublisherOptions(connection, inputExchange));

        var routerSettings = new RabbitMqRouterOptions(connection, inputExchange, new RabbitMqQueueOptions
        {
            Name = GetQueueName(nameof(Route_Test), sameAsInputExchange),
            BindKeys = ["input.#"],
        }, new RabbitMqExchangeOptions
        {
            Name = sameAsInputExchange ? inputExchange.Name : GetExchangeName(nameof(Route_Test) + "_route", sameAsInputExchange),
        });

        using var semaphore = new SemaphoreSlim(0);
        await using var router = await TestRouter.CreateAsync(routerSettings, GetRoutingKey);

        var evenList = new SortedSet<string>();
        await using var evenConsumer = await TestConsumer.CreateAsync(new RabbitMqConsumerOptions(connection,
          routerSettings.TargetExchange, new RabbitMqQueueOptions
          {
              Name = GetQueueName(nameof(Route_Test) + "_even", sameAsInputExchange),
              BindKeys = ["output.*.even"],
          }), m =>
          {
              evenList.Add(m);
              semaphore.Release();
          });

        var stringList = new SortedSet<string>();

        await using var stringConsumer = await TestConsumer.CreateAsync(new RabbitMqConsumerOptions(connection,
         routerSettings.TargetExchange, new RabbitMqQueueOptions
         {
             Name = GetQueueName(nameof(Route_Test) + "_string", sameAsInputExchange),
             BindKeys = ["output.string.*"],
         }), m =>
         {
             stringList.Add(m);
             semaphore.Release();
         });

        var messages = Enumerable.Range(1, 10).Select(m => m.ToString())
            .SelectMany(m => new[] { m, "str_" + m }).ToList();

        await publisher.PublishAsync<string>(messages, "input");

        var evenMessages = messages.Where(m => GetRoutingKey(m).EndsWith("even")).ToSortedSet();
        var strings = messages.Where(m => !int.TryParse(m, out _)).ToSortedSet();

        var flag = await semaphore.WaitAsync(evenMessages.Count + strings.Count, TimeSpan.FromSeconds(2));
        Assert.True(flag);

        Assert.Equal(evenMessages, evenList);
        Assert.Equal(strings, stringList);
    }
}