using FclEx.Caching;

namespace FclEx.Aop;

public class AopTestsFixture
{
    public static readonly IServiceProvider Services = new ServiceCollection()
        .AddTransient<IService, Service>()
        .AddUserClient<LoginAndRetryClient>()
        .AddFclExCaching()
        .AddLogging()
        .AddAop()
        .BuildDynamicProxyProvider();
}

public class Model(string id)
{
    public string Id { get; } = id;
}

public interface IService
{
    int Id { get; }

    [ReturnValueCache(IsStatic = true)]
    Model GetStatic(string name, int id);

    [ReturnValueCache]
    Model Get(string name, int id);
}

public class Service : IService
{
    private static int _id = short.MinValue;
    public int Id { get; }

    public static readonly TimeSpan CacheMaxTime = TimeSpan.FromMilliseconds(100);
    public static readonly TimeSpan SleepTime = TimeSpan.FromMilliseconds(200);

    public Service()
    {
        Id = Interlocked.Increment(ref _id);
    }

    public Model GetStatic(string name, int id)
    {
        Thread.Sleep(SleepTime);
        return new Model($"{name}_{id}");
    }

    public Model Get(string name, int id)
    {
        Thread.Sleep(SleepTime);
        return new Model($"{name}_{Id}_{id}");
    }

    public override int GetHashCode()
    {
        return Id;
    }
}

public class LoginAndRetryClient(ILoggerFactory loggerFactory) : UserClient(loggerFactory: loggerFactory)
{
    [LoginAndRetry]
    public virtual Task<OperationResult> DoAsync()
    {
        return this.IsOnline
            ? Operation.Success()
            : Operation.Error("");
    }

    protected override Task<OperationResult> LoginActionAsync(CancellationToken token)
    {
        return Operation.Success();
    }
}