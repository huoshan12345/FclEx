using Confluent.Kafka.Admin;

namespace FclEx.Kafka;

public sealed class KafkaTestsFixture
{
    public static ClientConfig CreateClientConfig()
    {
        return CoreTestsFixture.Config.GetSection("Kafka").Get<ClientConfig>()
            ?? throw new InvalidOperationException("Configure the Kafka section in the shared CoreTestsFixture appsettings files.");
    }

    public static async Task<KafkaTestTopic> CreateTopicAsync()
    {
        var name = "fclex-" + Guid.NewGuid().ToString("N");
        using var admin = new AdminClientBuilder(CreateClientConfig()).Build();
        await admin.CreateTopicsAsync([new TopicSpecification
        {
            Name = name,
            NumPartitions = 1,
            ReplicationFactor = 1,
        }], new CreateTopicsOptions { OperationTimeout = TimeSpan.FromSeconds(15), RequestTimeout = TimeSpan.FromSeconds(20) });
        return new KafkaTestTopic(name);
    }
}
