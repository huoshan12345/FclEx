using FclEx.Dapper;

namespace FclEx.Databases;

public class TruncateTestSessionTests(DapperTestsFixture fixture) : DapperTests(fixture)
{
    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task Sessions_OrdinaryTablesAreCreatedOnDemandAndDroppedOnDispose(DbDriver driver, string? schema)
    {
        Assert.SkipMySql(driver);
        var session = await Fixture.TruncateTables.CreateSessionAsync(driver, schema,
            [typeof(TruncateRow)], cancellationToken: CancellationToken, ordinary: true);
        using var connection = Fixture.CreateDbConnection(driver, schema);
        var table = session.GetTableName(typeof(TruncateRow));
        try
        {
            Assert.NotEqual(DapperHelper.GetEntityMapping(typeof(EntityWithAutoKey)).TableName, table);
            Assert.True(await connection.TableExistsAsync(table, schema, cancellationToken: CancellationToken));
        }
        finally
        {
            session.Dispose();
        }

        Assert.False(await connection.TableExistsAsync(table, schema, cancellationToken: CancellationToken));
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task Sessions_IsolateWholeTableOperations(DbDriver driver, string? schema)
    {
        Assert.SkipMySql(driver);
        using var first = await Fixture.TruncateTables.CreateSessionAsync(driver, schema, [typeof(TruncateRow)], cancellationToken: CancellationToken);
        using var second = await Fixture.TruncateTables.CreateSessionAsync(driver, schema, [typeof(TruncateRow)], cancellationToken: CancellationToken);
        var firstTable = first.GetQualifiedTableName(typeof(TruncateRow));
        var secondTable = second.GetQualifiedTableName(typeof(TruncateRow));
        try
        {
            await first.Connection.ExecuteAsync($"DELETE FROM {firstTable}", cancellationToken: CancellationToken);
            await second.Connection.ExecuteAsync($"DELETE FROM {secondTable}", cancellationToken: CancellationToken);
            await first.Connection.InsertAsync(new TruncateRow { Value = 1 }, first.Schema,
                commandOptions: first.CommandOptions, cancellationToken: CancellationToken);
            await second.Connection.InsertAsync(new TruncateRow { Value = 2 }, second.Schema,
                commandOptions: second.CommandOptions, cancellationToken: CancellationToken);
            await first.Connection.TruncateAsync<TruncateRow>(first.Schema, first.CommandOptions, CancellationToken);
            var firstCount = await first.Connection.QuerySingleAsync<int>(new CommandDefinition(
                $"SELECT COUNT(*) FROM {firstTable}", cancellationToken: CancellationToken));
            var secondCount = await second.Connection.QuerySingleAsync<int>(new CommandDefinition(
                $"SELECT COUNT(*) FROM {secondTable}", cancellationToken: CancellationToken));
            Assert.Equal(0, firstCount);
            Assert.Equal(1, secondCount);
        }
        finally
        {
            await first.Connection.ExecuteAsync($"DELETE FROM {firstTable}", cancellationToken: CancellationToken);
            await second.Connection.ExecuteAsync($"DELETE FROM {secondTable}", cancellationToken: CancellationToken);
        }
    }
}
