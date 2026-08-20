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
public class AbpTestsCollection : ICollectionFixture<AbpTestsFixture>;

[Collection(nameof(AbpTestsCollection))]
public class AbpTests(AbpTestsFixture fixture) : AbpTests<AbpTestsModule, AbpTestsFixture>(fixture);