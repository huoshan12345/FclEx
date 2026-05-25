namespace FclEx.Redis;

public enum RedisCollectionType
{
    List = 0,
    Set = 1,
    SortedSet = 2,
    Hash = 3,
}

public interface IRedisCollection
{
    string Name { get; }
    string Key { get; }
    RedisCollectionType CollectionType { get; }
    void Configure(Action<RedisCollectionOptions> action);
}

public interface IRedisCollection<T> : IRedisCollection;