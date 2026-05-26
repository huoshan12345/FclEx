namespace FclEx.Caching.Redis;

public class RedisOptions
{
    public string? SerializerName { get; set; } = "json";
    public RedisDBOptions DbOptions { get; set; } = new();
    public List<RedisCollectionConfigurator> CollectionConfigurators { get; } = [];

    public RedisOptions ConfigureCollection(string name, Action<RedisCollectionOptions> action)
    {
        CollectionConfigurators.Add(new RedisCollectionConfigurator(name, action));
        return this;
    }

    public RedisOptions ConfigureAllCollections(Action<RedisCollectionOptions> action)
    {
        CollectionConfigurators.Add(new RedisCollectionConfigurator(action));
        return this;
    }
}