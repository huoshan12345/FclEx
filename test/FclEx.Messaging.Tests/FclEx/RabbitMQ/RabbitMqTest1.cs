namespace FclEx.RabbitMQ;

[CollectionDefinition(nameof(RabbitMqTestsCollection))]
public class RabbitMqTestsCollection : ICollectionFixture<RabbitMqTestsFixture>;

[EnableParallelization]
[Collection(nameof(RabbitMqTestsCollection))]
public class RabbitMqTests(RabbitMqTestsFixture fixture)
{
    public RabbitMqTestsFixture Fixture { get; } = fixture;

    public static string GetKeyName(string testName, params object?[] args)
    {
        return CoreTestsFixture.WithAssemblyInfo($"key.{testName.ToLower()}.{args.JoinWith(".")}", typeof(RabbitMqTests).Assembly, '.');
    }

    public static string GetKeyName<T>(string testName, params object?[] args)
    {
        return GetKeyName($"{testName}.{typeof(T).ShortName()}", args);
    }

    public static string GetQueueName(string testName, params object?[] args)
    {
        return CoreTestsFixture.WithAssemblyInfo($"queue.{testName.ToLower()}.{args.JoinWith(".")}", typeof(RabbitMqTests).Assembly, '.');
    }

    public static string GetQueueName<T>(string testName, params object?[] args)
    {
        return GetQueueName($"{testName}.{typeof(T).ShortName()}", args);
    }

    public static string GetExchangeName(string testName, params object?[] args)
    {
        return CoreTestsFixture.WithAssemblyInfo($"exchange.{testName.ToLower()}.{args.JoinWith(".")}", typeof(RabbitMqTests).Assembly, '.');
    }

    public static string GetExchangeName<T>(string testName, params object?[] args)
    {
        return GetExchangeName($"{testName}.{typeof(T).ShortName()}", args);
    }
}