using Microsoft.Extensions.Configuration;

namespace FclEx.Caching.Redis;

public class RedisConfig
{
    public string Host { get; set; } = "";
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
    public int Port { get; set; } = 6379;
    public int? ConnectionTimeout { get; set; }
}

public class RedisTestsFixture : CoreTestsFixture
{
    private readonly Lazy<IServiceProvider> _services;
    public IServiceProvider Services => _services.Value;

    public RedisTestsFixture()
    {
        _services = new(CreateServices);
    }

    protected virtual RedisConfig GetRedisConfig()
    {
        return Config.GetSection("Redis").Get<RedisConfig>()!;
    }

    public IServiceProvider CreateServices()
    {
        var config = GetRedisConfig();
        var dbOptions = new RedisDBOptions
        {
            Username = config.UserName,
            Password = config.Password,
            Endpoints =
            {
                new()
                {
                    Host = config.Host,
                    Port = config.Port,
                }
            },
            Database = Environment.Version.Major,
        };

        if (config.ConnectionTimeout is { } connectionTimeout)
            dbOptions.ConnectionTimeout = connectionTimeout;

        var options = new RedisOptions { DbOptions = dbOptions }
            .ConfigureAllCollections(x => x.UseGlobalPrefix = true);

        return new ServiceCollection()
            .AddFclExCachingWithRedis(options)
            .AddTransient<IRedisService, RedisService>()
            .BuildServiceProvider();
    }
}

public class RedisModel(string id)
{
    public string Id { get; } = id;
}

public interface IRedisService
{
    int Id { get; }
    RedisModel GetStatic(string id);
    RedisModel Get(string id);
}

public class RedisService : IRedisService
{
    public const int SleepMilliseconds = 200;

    private static int _id = short.MinValue;
    public int Id { get; }

    public RedisService()
    {
        Id = Interlocked.Increment(ref _id);
    }

    public RedisModel GetStatic(string id)
    {
        Thread.Sleep(SleepMilliseconds);
        return new RedisModel(id);
    }

    public RedisModel Get(string id)
    {
        Thread.Sleep(SleepMilliseconds);
        return new RedisModel($"{Id}_{id}");
    }
}
