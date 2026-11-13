using Confluent.Kafka.Admin;

namespace FclEx.Kafka;

public sealed class KafkaTestTopic(string name) : IAsyncDisposable
{
    public string Name { get; } = name;
    public string GroupId { get; } = "fclex-" + Guid.NewGuid().ToString("N");
    public TopicPartition Partition => new(Name, new Partition(0));

    public ConsumerConfig CreateConsumerConfig()
    {
        return new ConsumerConfig(KafkaTestsFixture.CreateClientConfig())
        {
            GroupId = GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            AutoCommitIntervalMs = 100,
        };
    }

    public async Task PublishAsync(params string[] values)
    {
        using var producer = new ProducerBuilder<string, string>(new ProducerConfig(KafkaTestsFixture.CreateClientConfig())
        {
            MessageTimeoutMs = 15000,
        }).Build();
        foreach (var value in values)
            await producer.ProduceAsync(Partition, new Message<string, string> { Key = "key", Value = value });
    }

    public Offset GetCommittedOffset()
    {
        using var observer = new ConsumerBuilder<string, string>(CreateConsumerConfig()).Build();
        return Assert.Single(observer.Committed([Partition], TimeSpan.FromSeconds(10))).Offset;
    }

    public string ReplayNextMessage()
    {
        var config = CreateConsumerConfig();
        config.EnableAutoCommit = false;
        config.EnableAutoOffsetStore = false;
        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        try
        {
            consumer.Subscribe(Name);
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            return consumer.Consume(timeout.Token).Message.Value;
        }
        finally
        {
            consumer.Close();
        }
    }

    public async ValueTask DisposeAsync()
    {
        using var admin = new AdminClientBuilder(KafkaTestsFixture.CreateClientConfig()).Build();
        await admin.DeleteTopicsAsync([Name], new DeleteTopicsOptions
        {
            OperationTimeout = TimeSpan.FromSeconds(15),
            RequestTimeout = TimeSpan.FromSeconds(20),
        });
        await admin.DeleteGroupsAsync([GroupId], new DeleteGroupsOptions { RequestTimeout = TimeSpan.FromSeconds(20) });
    }
}