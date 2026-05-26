namespace FclEx.Aop;

public class ReturnValueCacheTests(AbpTestsFixture fixture) : AbpTests(fixture)
{
    public static TimeSpan CacheMaxTime => Service.CacheMaxTime;
    public static TimeSpan SleepTime => Service.SleepTime;

    public static IEnumerable<object[]> Numbers { get; } = new[] { -1, 0, 1, 10 }
        .Select(m => new object[] { m })
        .ToArray();


    [Fact]
    public void Aop_Test()
    {
        var service = Services.GetRequiredService<IService>();
        Assert.IsNotType<Service>(service);
        Assert.True(service.IsProxy());
    }

    [RetryTheory]
    [MemberData(nameof(Numbers))]
    public void SameInstance_Test(int no)
    {
        var service = Services.GetRequiredService<IService>();
        var itemFromStatic = service.GetStatic(no);
        var itemFromInstance = service.Get(no);

        for (var i = 0; i < 2; i++)
        {
            var (_, tempItem, ex, t) = Operation.Execute(() => service.Get(no));
            Assert.Null(ex);
            Assert.NotNull(tempItem);
            Assert.Equal(itemFromInstance.Id, tempItem.Id);
            Assert.True(t < CacheMaxTime, t.ToString());
        }
        for (var i = 0; i < 2; i++)
        {
            var (_, tempItem, ex, t) = Operation.Execute(() => service.GetStatic(no));
            Assert.Null(ex);
            Assert.NotNull(tempItem);
            Assert.Equal(itemFromStatic.Id, tempItem.Id);
            Assert.True(t < CacheMaxTime, t.ToString());
        }
    }


    private static int _errorCount;

    [RetryFact]
    public void Error_Test()
    {
        Interlocked.Increment(ref _errorCount);
        Output?.WriteLine("Current error count: {0}", _errorCount);

        Assert.True(_errorCount >= 3); // Retry 3 times
    }

    [RetryTheory]
    [MemberData(nameof(Numbers))]
    public void DifferentInstance_Test(int no)
    {
        var service = Services.GetRequiredService<IService>();
        var itemFromStatic = service.GetStatic(no);

        for (var i = 0; i < 2; i++)
        {
            var tempService = Services.GetRequiredService<IService>(); // new instance

            var (_, fromStatic, _, timeFromStatic) = Operation.Execute(() => tempService.GetStatic(no));
            var (_, fromInstance, _, timeFromInstance) = Operation.Execute(() => tempService.Get(no)); // should not be cached

            Assert.NotNull(fromStatic);
            Assert.Equal(itemFromStatic.Id, fromStatic.Id);

            Assert.NotNull(fromInstance);
            Assert.Equal($"{tempService.Id}_{no}", fromInstance.Id);

            Assert.True(timeFromStatic < CacheMaxTime, timeFromStatic.ToString());
            Assert.True(timeFromInstance > SleepTime, timeFromInstance.ToString());
        }
    }
}