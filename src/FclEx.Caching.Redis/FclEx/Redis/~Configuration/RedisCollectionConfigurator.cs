namespace FclEx.Redis;

public record RedisCollectionConfigurator(string Name, Action<RedisCollectionOptions> Action)
{
    public RedisCollectionConfigurator(Action<RedisCollectionOptions> initAction) : this("", initAction)
    {
    }
}