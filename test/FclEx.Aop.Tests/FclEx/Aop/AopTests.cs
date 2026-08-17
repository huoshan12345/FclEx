namespace FclEx.Aop;

[EnableParallelization]
[CollectionDefinition(nameof(AopTestsCollection))]
public class AopTestsCollection : ICollectionFixture<AopTestsFixture>;

[EnableParallelization]
[Collection(nameof(AopTestsCollection))]
public class AopTests
{
    public static IServiceProvider Services => AopTestsFixture.Services;
    public static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
    protected static ITestOutputHelper? Output => TestContext.Current.TestOutputHelper;
}
