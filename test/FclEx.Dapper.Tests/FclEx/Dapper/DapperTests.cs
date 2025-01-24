namespace FclEx.Dapper;

public class DapperTests(DapperFixture fixture) : DatabaseTests, IAssemblyFixture<DapperFixture>
{
    public DapperFixture Fixture { get; } = fixture;
}