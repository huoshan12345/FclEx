using FclEx.Aop;
using Volo.Abp.Modularity;

namespace FclEx.Abp;

[DependsOn(typeof(FclExAbpModule))]
public class AbpTestsModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddTransient<IService, Service>()
            .AddUserClient<LoginAndRetryClient>();
    }
}

public class Model(string id)
{
    public string Id { get; } = id;
}

public interface IService
{
    int Id { get; }

    [ReturnValueCache(IsStatic = true)]
    Model GetStatic(int id);

    [ReturnValueCache]
    Model Get(int id);
}

public class Service : IService
{
    private static int _id = short.MinValue;
    public int Id { get; }

    public static readonly TimeSpan CacheMaxTime = TimeSpan.FromMilliseconds(50);
    public static readonly TimeSpan SleepTime = TimeSpan.FromMilliseconds(100);

    public Service()
    {
        Id = Interlocked.Increment(ref _id);
    }

    public Model GetStatic(int id)
    {
        Thread.Sleep(SleepTime);
        return new Model(id.ToString());
    }

    public Model Get(int id)
    {
        Thread.Sleep(SleepTime);
        return new Model($"{Id}_{id}");
    }

    public override int GetHashCode()
    {
        return Id;
    }
}