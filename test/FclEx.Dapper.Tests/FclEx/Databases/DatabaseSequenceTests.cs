using FclEx.Dapper;

namespace FclEx.Databases;

public class DatabaseSequenceTests(DapperTestsFixture fixture) : DapperTests(fixture)
{
    [Fact]
    public async Task ReseedIdentityAsync_MissingPostgreSqlTable_ReturnsFalse()
    {
        Assert.SkipUnlessIncluded(DbDriver.Npgsql);
        using var connection = Fixture.CreateDbConnection(DbDriver.Npgsql, null);

        var result = await connection.ReseedIdentityAsync<MissingSequenceEntity>(null);

        Assert.False(result);
        Assert.Equal(ConnectionState.Closed, connection.State);
    }

    private sealed class MissingSequenceEntity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
    }
}
