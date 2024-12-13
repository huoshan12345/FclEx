namespace FclEx.EfCore;

public class EfCoreTests(EfCoreFixture fixture) : DatabaseTests, IAssemblyFixture<EfCoreFixture>
{
    public EfCoreFixture Fixture { get; } = fixture;
}