namespace FclEx.Abp.RedisCache.Collections;

public enum RedisColType
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
    RedisColType ColType { get; }
}

public interface IRedisCol<T> : IRedisCol
{
}