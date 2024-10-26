namespace FclEx.Abp.Aop;

public class ReturnValueCacheTests : AbpAopTests<AbpTestModule>
{
    public static readonly TimeSpan CacheMaxTime = TimeSpan.FromMilliseconds(50);
    public static readonly TimeSpan SleepTime = TimeSpan.FromMilliseconds(100);

    public static IEnumerable<object[]> Numbers { get; } = new[] { -1, 0, 1, 10 }
        .Select(m => new object[] { m }).ToArray();

    public ReturnValueCacheTests(ITestOutputHelper output)
        : base(output, o => o.AddTransient<IService, Service>())
    {
    }

    public class Model
    {
        public string Id { get; }
        public Model(string id) { Id = id; }
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
            Thread.Sleep(SleepTime);
            return new Model(id.ToString());
        }

        public Model Get(int id)
        {
            Thread.Sleep(SleepTime);
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

    [Theory]
    [MemberData(nameof(Numbers))]
    public void TestSingle(int no)
    {
        var service = ServiceProvider.GetRequiredService<IService>();
        var itemFromStatic = service.GetStatic(no);
        var itemFromInstance = service.Get(no);

        for (var i = 0; i < 2; i++)
        {
            var (_, tempItem, ex, t) = Operate.Execute(() => service.Get(no));
            Assert.Null(ex);
            Assert.NotNull(tempItem);
            Assert.Equal(itemFromInstance.Id, tempItem.Id);
            Assert.True(t < CacheMaxTime, t.ToString());
        }
        for (var i = 0; i < 2; i++)
        {
            var (_, tempItem, ex, t) = Operate.Execute(() => service.GetStatic(no));
            Assert.Null(ex);
            Assert.NotNull(tempItem);
            Assert.Equal(itemFromStatic.Id, tempItem.Id);
            Assert.True(t < CacheMaxTime, t.ToString());
        }
    }

    [Theory]
    [MemberData(nameof(Numbers))]
    public void TestMultiple(int no)
    {
        var service = ServiceProvider.GetRequiredService<IService>();
        var itemFromStatic = service.GetStatic(no);
        for (var i = 0; i < 2; i++)
        {
            var tempService = ServiceProvider.GetRequiredService<IService>();
            var (_, fromStatic, _, timeFromStatic) = Operate.Execute(() => tempService.GetStatic(no));
            var (_, fromInstance, _, timeFromInstance) = Operate.Execute(() => tempService.Get(no));

            Assert.NotNull(fromStatic);
            Assert.Equal(itemFromStatic.Id, fromStatic.Id);

            Assert.NotNull(fromInstance);
            Assert.Equal($"{tempService.Id}_{no}", fromInstance.Id);

            Assert.True(timeFromStatic < CacheMaxTime, timeFromStatic.ToString());
            Assert.True(timeFromInstance > SleepTime, timeFromInstance.ToString());
        }
    }
}