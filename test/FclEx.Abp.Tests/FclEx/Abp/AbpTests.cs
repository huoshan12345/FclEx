using Volo.Abp.Modularity;

namespace FclEx.Abp;

public class AbpTests<TModule, TFixture>(TFixture fixture)
    where TFixture : AbpTestsFixture<TModule>
    where TModule : AbpModule
{
    protected static ITestOutputHelper? Output => TestContext.Current.TestOutputHelper;
    protected static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
    protected TFixture Fixture => fixture;
    protected IServiceProvider Services => Fixture.Services;
}

[CollectionDefinition(nameof(AbpTestsCollection))]
public class AbpTestsCollection : ICollectionFixture<AbpTestsFixture<AbpTestsModule>>;

[EnableParallelization]
[Collection(nameof(AbpTestsCollection))]
public class AbpTests(AbpTestsFixture<AbpTestsModule> fixture)
    : AbpTests<AbpTestsModule, AbpTestsFixture<AbpTestsModule>>(fixture)
{
    [ModuleInitializer]
    public static void Initialize()
    {
        ThreadPool.SetMinThreads(100, 100);
#pragma warning disable SYSLIB0014
        ServicePointManager.DefaultConnectionLimit = short.MaxValue;
#pragma warning restore SYSLIB0014
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }
}