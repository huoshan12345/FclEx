namespace FclEx.Abp.RedisCache.Collections;

public enum RedisCollectionType
{
    List = 0,
    Set = 1,
    SortedSet = 2,
    Hash = 3,
}

public interface IRedisCol
{
    string Name { get; }
    string Key { get; }
    RedisCollectionType CollectionType { get; }
}

public interface IRedisCollection<T> : IRedisCol;