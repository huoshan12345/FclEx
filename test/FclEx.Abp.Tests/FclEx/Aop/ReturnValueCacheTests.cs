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

    [Theory]
    [MemberData(nameof(Numbers))]
    public void SameInstance_Test(int no)
    {
        const string name = nameof(SameInstance_Test);
        var service = Services.GetRequiredService<IService>();
        var itemFromStatic = service.GetStatic(name, no);
        var itemFromInstance = service.Get(name, no);

        for (var i = 0; i < 2; i++)
        {
            var (_, tempItem, ex, getTime) = Operation.Execute(() => service.Get(name, no));
            Assert.Null(ex);
            Assert.NotNull(tempItem);
            Assert.Equal(itemFromInstance.Id, tempItem.Id);
            Assert.True(getTime < CacheMaxTime, () => $"Expected {nameof(getTime)} < {CacheMaxTime}, but was {getTime}");
        }
        for (var i = 0; i < 2; i++)
        {
            var (_, tempItem, ex, getStaticTime) = Operation.Execute(() => service.GetStatic(name, no));
            Assert.Null(ex);
            Assert.NotNull(tempItem);
            Assert.Equal(itemFromStatic.Id, tempItem.Id);
            Assert.True(getStaticTime < CacheMaxTime, () => $"Expected {nameof(getStaticTime)} < {CacheMaxTime}, but was {getStaticTime}");
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

    [Theory]
    [MemberData(nameof(Numbers))]
    public void DifferentInstance_Test(int no)
    {
        const string name = nameof(DifferentInstance_Test);
        var service = Services.GetRequiredService<IService>();
        var itemFromStatic = service.GetStatic(name, no);

        for (var i = 0; i < 2; i++)
        {
            var tempService = Services.GetRequiredService<IService>(); // new instance

            var (_, fromStatic, _, timeFromStatic) = Operation.Execute(() => tempService.GetStatic(name, no));
            var (_, fromInstance, _, timeFromInstance) = Operation.Execute(() => tempService.Get(name, no)); // should not be cached

            Assert.NotNull(fromStatic);
            Assert.Equal(itemFromStatic.Id, fromStatic.Id);

            Assert.NotNull(fromInstance);
            Assert.Equal($"{name}_{tempService.Id}_{no}", fromInstance.Id);

            Assert.True(timeFromStatic < CacheMaxTime, () => $"Expected {nameof(timeFromStatic)} < {CacheMaxTime}, but was {timeFromStatic}");
            Assert.True(timeFromInstance > SleepTime, () => $"Expected {nameof(timeFromInstance)} > {SleepTime}, but was {timeFromInstance}");
        }
    }
}