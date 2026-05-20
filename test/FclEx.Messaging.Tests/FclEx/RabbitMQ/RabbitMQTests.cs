namespace FclEx.RabbitMQ;

[CollectionDefinition(nameof(RabbitMQTestsCollection))]
public class RabbitMQTestsCollection : ICollectionFixture<RabbitMQFixture>;

[EnableParallelization]
[Collection(nameof(RabbitMQTestsCollection))]
public class RabbitMQTests(RabbitMQFixture fixture)
{
    public RabbitMQFixture Fixture { get; } = fixture;

    public static string GetKeyName(string testName, params object?[] args)
    {
        return GlobalFixture.WithAssemblyInfo($"key_{testName.ToLower()}_{args.JoinWith("_")}", typeof(RabbitMQTests).Assembly);
    }

    public static string GetKeyName<T>(string testName, params object?[] args)
    {
        return GetKeyName($"{testName}_{typeof(T).ShortName()}", args);
    }

    public static string GetQueueName(string testName, params object?[] args)
    {
        return GlobalFixture.WithAssemblyInfo($"queue_{testName.ToLower()}_{args.JoinWith("_")}", typeof(RabbitMQTests).Assembly);
    }

    public static string GetQueueName<T>(string testName, params object?[] args)
    {
        return GetQueueName($"{testName}_{typeof(T).ShortName()}", args);
    }

    public static string GetExchangeName(string testName, params object?[] args)
    {
        return GlobalFixture.WithAssemblyInfo($"exchange_{testName.ToLower()}_{args.JoinWith("_")}", typeof(RabbitMQTests).Assembly);
    }

    public static string GetExchangeName<T>(string testName, params object?[] args)
    {
        return GetExchangeName($"{testName}_{typeof(T).ShortName()}", args);
    }
}