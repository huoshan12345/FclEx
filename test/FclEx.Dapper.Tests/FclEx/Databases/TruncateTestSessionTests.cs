using FclEx.Dapper;

namespace FclEx.Databases;

public class TruncateTestSessionTests(DapperTestsFixture fixture) : DapperTests(fixture)
{
    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task Sessions_UseExistingTablesAndPreserveThemOnDispose(DbDriver driver, string? schema)
    {
        Assert.SkipMySql(driver);
        var session = await Fixture.TruncateSessions.CreateSessionAsync(driver, schema,
            [typeof(TruncateRow)], cancellationToken: CancellationToken);
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

        Assert.True(await connection.TableExistsAsync(table, schema, cancellationToken: CancellationToken));
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task Sessions_CancelWaitingAndReuseTablesWithoutRecreatingThem(DbDriver driver, string? schema)
    {
        Assert.SkipMySql(driver);
        using var first = await Fixture.TruncateSessions.CreateSessionAsync(driver, schema,
            [typeof(TruncateRow)], cancellationToken: CancellationToken);
        var table = first.GetQualifiedTableName(typeof(TruncateRow));
        try
        {
            await first.Connection.ExecuteAsync($"DELETE FROM {table}", cancellationToken: CancellationToken);
            await first.Connection.InsertAsync(new TruncateRow { Value = 1 }, first.Schema,
                cancellationToken: CancellationToken);
            using var canceled = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken);
            var waiting = Fixture.TruncateSessions.CreateSessionAsync(driver, schema,
                [typeof(TruncateOtherRow), typeof(TruncateRow)], cancellationToken: canceled.Token);
            Assert.False(waiting.IsCompleted);
            canceled.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => waiting);
            using var connection = Fixture.CreateDbConnection(driver, schema);
            Assert.Equal(1, await connection.QuerySingleAsync<int>(new CommandDefinition(
                $"SELECT COUNT(*) FROM {table}", cancellationToken: CancellationToken)));
        }
        finally
        {
            await first.Connection.ExecuteAsync($"DELETE FROM {table}", cancellationToken: CancellationToken);
        }
        first.Dispose();
        first.Dispose();
        using var other = await Fixture.TruncateSessions.CreateSessionAsync(driver, schema,
            [typeof(TruncateOtherRow)], cancellationToken: CancellationToken);
        using var reused = await Fixture.TruncateSessions.CreateSessionAsync(driver, schema,
            [typeof(TruncateRow)], cancellationToken: CancellationToken);
        Assert.Equal(table, reused.GetQualifiedTableName(typeof(TruncateRow)));
        Assert.True(await reused.Connection.TableExistsAsync(reused.GetTableName(typeof(TruncateRow)), schema,
            cancellationToken: CancellationToken));
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task Sessions_IsolateWholeTableOperations(DbDriver driver, string? schema)
    {
        Assert.SkipMySql(driver);
        using var first = await Fixture.TruncateSessions.CreateSessionAsync(driver, schema, [typeof(TruncateRow)], cancellationToken: CancellationToken);
        using var second = await Fixture.TruncateSessions.CreateSessionAsync(driver, schema, [typeof(TruncateIsolationRow)], cancellationToken: CancellationToken);
        var firstTable = first.GetQualifiedTableName(typeof(TruncateRow));
        var secondTable = second.GetQualifiedTableName(typeof(TruncateIsolationRow));
        Assert.NotEqual(firstTable, secondTable);
        try
        {
            await first.Connection.ExecuteAsync($"DELETE FROM {firstTable}", cancellationToken: CancellationToken);
            await second.Connection.ExecuteAsync($"DELETE FROM {secondTable}", cancellationToken: CancellationToken);
            await first.Connection.InsertAsync(new TruncateRow { Value = 1 }, first.Schema,
                cancellationToken: CancellationToken);
            await second.Connection.InsertAsync(new TruncateIsolationRow { Value = 2 }, second.Schema,
                cancellationToken: CancellationToken);
            await first.Connection.TruncateAsync<TruncateRow>(first.Schema, cancellationToken: CancellationToken);
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
