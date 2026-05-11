namespace FclEx.RabbitMQ;

[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
public class RouterTests(RabbitMQFixture fixture) : RabbitMQTests(fixture)
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

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Route_Test(bool sameAsInputExchange)
    {
        if (Skip)
            return;

        var inputExchange = new ExchangeSettings
        {
            Name = Fixture.WithAssemblyInfo("test.router.input", '.'),
            Type = "topic",
            IsDelayed = true,
        };
        var connection = RmqConnection;
        await using var publisher = await TestPublisher.CreateAsync(new PublisherSettings(connection, inputExchange));

        var routerSettings = new RouterSettings(connection, inputExchange, new QueueSettings
        {
            Name = Fixture.WithAssemblyInfo("test.router", '.'),
            BindKeys = ["input.#"],
        }, new ExchangeSettings
        {
            Name = sameAsInputExchange ? inputExchange.Name : inputExchange.Name + ".output",
            Type = "topic",
            IsDelayed = true,
        });

        using var semaphore = new SemaphoreSlim(0);
        await using var router = await TestRouter.CreateAsync(routerSettings, GetRoutingKey);

        var evenList = new SortedSet<string>();
        await using var evenConsumer = await TestConsumer.CreateAsync(new ConsumerSettings(connection,
          routerSettings.TargetExchange, new QueueSettings
          {
              Name = Fixture.WithAssemblyInfo("test.router.even", '.'),
              BindKeys = ["output.*.even"],
          }), m =>
          {
              evenList.Add(m);
              semaphore.Release();
          });

        var stringList = new SortedSet<string>();

        await using var stringConsumer = await TestConsumer.CreateAsync(new ConsumerSettings(connection,
         routerSettings.TargetExchange, new QueueSettings
         {
             Name = Fixture.WithAssemblyInfo("test.router.string", '.'),
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