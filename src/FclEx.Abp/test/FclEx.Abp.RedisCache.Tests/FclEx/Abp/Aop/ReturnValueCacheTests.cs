using AspectCore.DynamicProxy;
using FclEx.Abp.RedisCache;
// ReSharper disable SpecifyACultureInStringConversionExplicitly

namespace FclEx.Abp.Aop;

public class ReturnValueCacheTests(ITestOutputHelper output)
    : AbpRedisTests(output, o => o.AddTransient<IService, Service>())
{
    public const int CacheMaxMilliseconds = 100;
    public const int SleepMilliseconds = 200;

    public static IEnumerable<object[]> Numbers { get; } = new[] { -1, 0, 1, 10 }
        .Select(m => new object[] { m }).ToArray();

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

        public Service()
        {
            Id = Interlocked.Increment(ref _id);
        }

        public Model GetStatic(int id)
        {
            Thread.Sleep(SleepMilliseconds);
            return new Model(id.ToString());
        }

        public Model Get(int id)
        {
            Thread.Sleep(SleepMilliseconds);
            return new Model($"{_id}_{id}");
        }
    }

    [Fact]
    public void Aop_Test()
    {
        var service = ServiceProvider.GetRequiredService<IService>();
        Assert.IsNotType<Service>(service);
        Assert.True(service.IsProxy());
    }

    [RetryTheory]
    [MemberData(nameof(Numbers))]
    public void IsStatic_SameObject_Test(int no)
    {
        var service = ServiceProvider.GetRequiredService<IService>();
        var itemFromStatic = service.GetStatic(no);

        var (_, tempItem, _, t) = Operation.Execute(() => service.GetStatic(no));
        Assert.NotNull(tempItem);
        Assert.Equal(itemFromStatic.Id, tempItem.Id);
        Assert.True(t.TotalMilliseconds < CacheMaxMilliseconds, t.TotalSeconds.ToString());
    }

    [RetryTheory]
    [MemberData(nameof(Numbers))]
    public void IsStatic_DiffObject_Test(int no)
    {
        var service = ServiceProvider.GetRequiredService<IService>();
        var itemFromStatic = service.GetStatic(no);

        var tempService = ServiceProvider.GetRequiredService<IService>();
        var (_, fromStatic, _, t) = Operation.Execute(() => tempService.GetStatic(no));
        Assert.NotNull(fromStatic);
        Assert.Equal(itemFromStatic.Id, fromStatic.Id);
        Assert.True(t.TotalMilliseconds < CacheMaxMilliseconds, t.TotalSeconds.ToString());
    }


    [RetryTheory]
    [MemberData(nameof(Numbers))]
    public void NotStatic_SameObject_Test(int no)
    {
        var service = ServiceProvider.GetRequiredService<IService>();
        var fromInstance = service.Get(no);

        var (_, tempItem, _, t) = Operation.Execute(() => service.Get(no));
        Assert.NotNull(tempItem);
        Assert.Equal(fromInstance.Id, tempItem.Id);
        Assert.True(t.TotalMilliseconds < CacheMaxMilliseconds, t.TotalSeconds.ToString());
    }

    [RetryTheory]
    [MemberData(nameof(Numbers))]
    public void NotStatic_DiffObject_Test(int no)
    {
        var service = ServiceProvider.GetRequiredService<IService>();
        var fromInstance = service.Get(no);

        var tempService = ServiceProvider.GetRequiredService<IService>();
        var (_, temp, _, t) = Operation.Execute(() => tempService.Get(no));
        Assert.NotNull(temp);
        Assert.NotEqual(fromInstance.Id, temp.Id);
        Assert.Equal($"{tempService.Id}_{no}", temp.Id);
        Assert.True(t.TotalMilliseconds > SleepMilliseconds, t.TotalSeconds.ToString());
    }
}