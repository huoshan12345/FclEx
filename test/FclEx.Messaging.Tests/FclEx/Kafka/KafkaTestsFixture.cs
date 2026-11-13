using Confluent.Kafka.Admin;

namespace FclEx.Kafka;

public sealed class KafkaTestsFixture
{
    public static ClientConfig CreateClientConfig()
    {
        return CoreTestsFixture.Config.GetSection("Kafka").Get<ClientConfig>()
            ?? throw new InvalidOperationException("Configure Kafka in appsettings.local.json or FCLEX_TEST_Kafka__ environment variables.");
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