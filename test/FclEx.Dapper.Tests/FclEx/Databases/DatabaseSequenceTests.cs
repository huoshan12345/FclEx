using FclEx.Dapper;

namespace FclEx.Databases;

public class DatabaseSequenceTests(DapperTestsFixture fixture) : DapperTests(fixture)
{
    [Fact]
    public async Task SynchronizeIdentitySequenceAsync_MissingPostgreSqlTable_ReturnsZero()
    {
        Assert.SkipUnlessIncluded(DbDriver.Npgsql);
        using var connection = Fixture.CreateDbConnection(DbDriver.Npgsql, null);

        var result = await SynchronizeIdentitySequenceAsync<MissingSequenceEntity>(connection, DbDriver.Npgsql, null);

        Assert.Equal(0, result);
        Assert.Equal(ConnectionState.Closed, connection.State);
    }

    private sealed class MissingSequenceEntity
    {
        public int Id { get; set; }
    }
}
