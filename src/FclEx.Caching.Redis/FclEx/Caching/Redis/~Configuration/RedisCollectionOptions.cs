namespace FclEx.Caching.Redis;

public class RedisCollectionOptions
{
    internal RedisCollectionOptions(string name)
    {
        Name = name;
    }

    public bool UseGlobalPrefix { get; set; } = false;
    public string Name { get; }
    public TimeSpan? DefaultExpiration { get; set; }
}