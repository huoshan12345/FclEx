namespace FclEx.Caching.Redis._Unreachable;

public class RedisUnreachableTestsFixture : RedisTestsFixture
{
    protected override RedisConfig GetRedisConfig()
    {
        var config = base.GetRedisConfig();
        config.Host = "127.0.0.2";
        config.ConnectionTimeout = 10;
        return config;
    }
}