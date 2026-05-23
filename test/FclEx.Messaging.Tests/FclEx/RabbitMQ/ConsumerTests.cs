using FclEx.TestModels;

namespace FclEx.RabbitMQ;

[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
public class ConsumerTests(RabbitMqTestsFixture fixture) : RabbitMqTests(fixture)
{
    [Fact]
    public async Task Consume_Test()
    {
        var exchange = new RabbitMqExchangeOptions
        {
            Name = GetExchangeName(nameof(Consume_Test)),
        };

        var connection = RabbitMqTestsFixture.ConnectionSettings;
        await using var publisher = await TestPublisher.CreateAsync(new RabbitMqPublisherOptions(connection, exchange));

        var msgList = Enumerable.Range(1, 10).Select(m => (Seq: m, Msg: "msg_" + m)).ToList();
        var list = new List<string>();

        using var semaphore = new SemaphoreSlim(0);
        await using var consumer = await TestConsumer.CreateAsync(new RabbitMqConsumerOptions
        {
            Connection = connection,
            Exchange = exchange,
            RabbitMqQueue = new RabbitMqQueueOptions
            {
                Name = GetQueueName(nameof(Consume_Test)),
                BindKeys = ["#"],
            },
        }, m =>
        {
            list.Add(m);
            semaphore.Release();
        });

        await publisher.PublishAsync(msgList, m => (m.Msg, m.Seq.ToString()));

        var flag = await semaphore.WaitAsync(msgList.Count, TimeSpan.FromSeconds(2));
        Assert.True(flag);

        Assert.Equal(msgList.Select(m => m.Msg), list);
    }

    private static async Task ConsumePushBackTest<T>(string testName, T valueToPublish, TimeSpan delay = default)
    {
        var exchange = new RabbitMqExchangeOptions
        {
            Name = GetExchangeName<T>(testName),
        };

        var connection = RabbitMqTestsFixture.ConnectionSettings;
        await using var publisher = await TestPublisher.CreateAsync(new RabbitMqPublisherOptions(connection, exchange));

        var key = GetKeyName<T>(testName);
        var list = new List<T>();

        const int retryTimes = 1;
        using var semaphore = new SemaphoreSlim(0);
        await using var consumer = await TestConsumer<T>.CreateAsync(new RabbitMqConsumerOptions
        {
            Connection = connection,
            Exchange = exchange,
            RabbitMqQueue = new RabbitMqQueueOptions
            {
                Name = GetQueueName<T>(testName),
                BindKeys = [key],
            },
        }, m =>
        {
            list.Add(m);
            semaphore.Release();
            return Operation.Cancel(); // create an error
        }, retryTimes, m => delay);

        await publisher.PublishAsync(valueToPublish, key);

        var flag = await semaphore.WaitAsync(retryTimes + 1, delay + TimeSpan.FromSeconds(1));
        Assert.True(flag);

        Assert.True(list.Count == retryTimes + 1);
        foreach (var m in list)
        {
            Assert.Equal(valueToPublish, m);
        }
    }

    [RetryTheory]
    [InlineData(0)]
    [InlineData(0.3)]
    public async Task Consume_PushBack_String_Test(double delaySeconds)
    {
        await ConsumePushBackTest(nameof(Consume_PushBack_String_Test), "testValue", TimeSpan.FromSeconds(delaySeconds));
    }

    [RetryTheory]
    [InlineData(0)]
    [InlineData(0.3)]
    public async Task Consume_PushBack_Int_Test(double delaySeconds)
    {
        await ConsumePushBackTest(nameof(Consume_PushBack_Int_Test), 10, TimeSpan.FromSeconds(delaySeconds));
    }

    [RetryTheory]
    [InlineData(0)]
    [InlineData(0.3)]
    public async Task Consume_PushBack_Class_Test(double delaySeconds)
    {
        await ConsumePushBackTest(nameof(Consume_PushBack_Class_Test), new Person
        {
            Id = 10,
            Name = "Jim",
            Age = 30,
            CoinCount = 5,
        }, TimeSpan.FromSeconds(delaySeconds));
    }

    [Fact]
    public async Task Consume_MultiBind_Test()
    {
        var exchange = new RabbitMqExchangeOptions
        {
            Name = GetExchangeName(nameof(Consume_MultiBind_Test)),
        };

        var connection = RabbitMqTestsFixture.ConnectionSettings;
        await using var publisher = await TestPublisher.CreateAsync(new RabbitMqPublisherOptions(connection, exchange));

        var msgList = Enumerable.Range(1, 10).Select(m => (Seq: m, Msg: "msg_" + m)).ToList();
        var list = new List<string>();

        using var semaphore = new SemaphoreSlim(0);
        await using var consumer = await TestConsumer.CreateAsync(new RabbitMqConsumerOptions
        {
            Connection = connection,
            Exchange = exchange,
            RabbitMqQueue = new RabbitMqQueueOptions
            {
                Name = GetQueueName(nameof(Consume_MultiBind_Test)),
                BindKeys = ["output.0", "output.1"],
            }
        }, m =>
        {
            list.Add(m);
            semaphore.Release();
        });

        await publisher.PublishAsync(msgList, m => (m.Msg, "output." + m.Seq % 3));

        var expectedList = msgList.Where(m => m.Seq % 3 != 2).Select(m => m.Msg).ToList();

        var flag = await semaphore.WaitAsync(expectedList.Count, TimeSpan.FromSeconds(2));
        Assert.True(flag);

        Assert.Equal(expectedList, list);
    }
}