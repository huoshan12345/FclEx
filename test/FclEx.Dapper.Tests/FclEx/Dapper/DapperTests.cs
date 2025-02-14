namespace FclEx.Dapper;

public class DapperTests(DapperFixture fixture) : DatabaseTests, IClassFixture<DapperFixture>
{
    public DapperFixture Fixture { get; } = fixture;
}