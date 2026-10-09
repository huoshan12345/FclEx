// ReSharper disable UseAwaitUsing
namespace FclEx.Dapper;

public partial class TruncateTests(DapperTestsFixture fixture) : DapperTests(fixture)
{
    private Task<TruncateTestSession> CreateSessionAsync(DbDriver driver, string? schema, params Type[] entityTypes)
        => Fixture.TruncateSessions.CreateSessionAsync(driver, schema,
            entityTypes.Length == 0 ? [typeof(TruncateRow)] : entityTypes, cancellationToken: CancellationToken);

    public static TheoryData<DbDriver, string?, bool, bool> OptionCases => TruncateTestCases.GetOptionCases(Schemas);

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task TruncateAsync_RemovesAllRowsAndPreservesConnectionState(DbDriver driver, string? schema)
    {
        Assert.SkipMySql(driver);
        using var session = await CreateSessionAsync(driver, schema);
        await using var cleanup = CleanupRows(session);
        var connection = session.Connection;
        schema = session.Schema;
        await VerifyTruncationAsync(session,
            token => connection.TruncateAsync<TruncateRow>(schema, cancellationToken: token));
        await VerifyTruncationAsync(session,
            token => connection.TruncateAsync(session.GetTableName(typeof(TruncateRow)), schema, cancellationToken: token));
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task TruncateAsync_PreservesClosedConnectionState(DbDriver driver, string? schema)
    {
        Assert.SkipMySql(driver);
        using var session = await Fixture.TruncateSessions.CreateSessionAsync(driver, schema,
            [typeof(TruncateRow)], cancellationToken: CancellationToken);
        await using var cleanup = CleanupRows(session);
        using var closed = Fixture.CreateDbConnection(driver, schema);
        var restart = driver != DbDriver.Oracle;
        var table = session.GetTableName(typeof(TruncateRow));
        var options = new CommandOptions();
        Func<CancellationToken, Task>[] truncations =
        [
            token => closed.TruncateAsync<TruncateRow>(schema, options, token),
            token => closed.TruncateAsync(table, schema, options, token),
            token => closed.TruncateAsync<TruncateRow>(restart, false, schema, options, token),
            token => closed.TruncateAsync(table, restart, false, schema, options, token),
        ];
        foreach (var truncate in truncations)
            await VerifyRowsRemovedAsync(session, async token =>
            {
                Assert.Equal(ConnectionState.Closed, closed.State);
                await truncate(token);
                Assert.Equal(ConnectionState.Closed, closed.State);
            });

        // Verify explicit schema precedence against an ordinary table, including SQL Server.
        if (schema is not null)
        {
            var wrongSchema = options with { EntityMappingSource = new MappingSource(table, "missing_schema") };
            await VerifyRowsRemovedAsync(session, async token =>
            {
                await closed.TruncateAsync<MappedRow>(schema, wrongSchema, token);
                Assert.Equal(ConnectionState.Closed, closed.State);
            });
        }
    }

    [Theory]
    [MemberData(nameof(OptionCases))]
    public async Task TruncateAsync_ExplicitOptionsHonorDatabaseCapabilities(
        DbDriver driver, string? schema, bool restartIdentity, bool cascade)
    {
        Assert.SkipMySql(driver);
        using var session = await CreateSessionAsync(driver, schema);
        await using var cleanup = CleanupRows(session);
        var connection = session.Connection;
        schema = session.Schema;
        await VerifyTruncationAsync(session,
            token => connection.TruncateAsync<TruncateRow>(restartIdentity, cascade, schema, cancellationToken: token),
            restartIdentity);
        await VerifyTruncationAsync(session,
            token => connection.TruncateAsync(session.GetTableName(typeof(TruncateRow)), restartIdentity, cascade, schema, cancellationToken: token),
            restartIdentity);
    }

    [Theory]
    [MemberData(nameof(TruncateTestCases.UnsupportedOptionCases), MemberType = typeof(TruncateTestCases))]
    public async Task TruncateAsync_UnsupportedOptionsThrowBeforeOpeningConnection(
        DbDriver driver, bool restartIdentity, bool cascade)
    {
        Assert.SkipMySql(driver);
        using var connection = Fixture.CreateDbConnection(driver, null);
        await Assert.ThrowsAsync<NotSupportedException>(() => connection.TruncateAsync<TruncateRow>(restartIdentity, cascade));
        await Assert.ThrowsAsync<NotSupportedException>(() => connection.TruncateAsync(nameof(TruncateRow), restartIdentity, cascade));
        Assert.Equal(ConnectionState.Closed, connection.State);
    }

    private static Task<int> DeleteAllAsync<T>(TruncateTestSession session)
        => session.Connection.ExecuteAsync($"DELETE FROM {session.GetQualifiedTableName(typeof(T))}", cancellationToken: CancellationToken);

    private static Task<int> CountAsync<T>(TruncateTestSession session, DbTransaction? transaction = null)
        => session.Connection.QuerySingleAsync<int>(new CommandDefinition($"SELECT COUNT(*) FROM {session.GetQualifiedTableName(typeof(T))}",
            transaction: transaction, cancellationToken: CancellationToken));

    // Cleanup belongs to the test. SeedAsync clears between calls; no second DELETE is needed after each verification.
    private static IAsyncDisposable CleanupRows(TruncateTestSession session)
        => AsyncDisposable.Create(async () =>
        {
            await DeleteAllAsync<TruncateRow>(session);
        });

    private static async Task<TruncateRow> SeedAsync(TruncateTestSession session)
    {
        await DeleteAllAsync<TruncateRow>(session);
        var first = new TruncateRow { Value = 1 };
        var second = new TruncateRow { Value = -1 };
        first.Id = await session.Connection.InsertAsync<TruncateRow, int>(first, session.Schema,
            cancellationToken: CancellationToken);
        second.Id = await session.Connection.InsertAsync<TruncateRow, int>(second, session.Schema,
            cancellationToken: CancellationToken);
        Assert.True(second.Id > first.Id);
        Assert.Equal(2, await CountAsync<TruncateRow>(session));
        return second;
    }

    private static async Task<TruncateRow> VerifyRowsRemovedAsync(
        TruncateTestSession session, Func<CancellationToken, Task> truncate)
    {
        var entity = await SeedAsync(session);
        var initialState = session.Connection.State;
        await truncate(CancellationToken);
        Assert.Equal(initialState, session.Connection.State);
        Assert.Equal(0, await CountAsync<TruncateRow>(session));
        return entity;
    }

    private static async Task VerifyTruncationAsync(
        TruncateTestSession session, Func<CancellationToken, Task> truncate, bool? restartIdentity = null)
    {
        var entity = await VerifyRowsRemovedAsync(session, truncate);
        var nextId = await session.Connection.InsertAsync<TruncateRow, int>(
            new() { Value = 1 }, session.Schema,
            cancellationToken: CancellationToken);
        if (restartIdentity ?? session.Driver is not (DbDriver.Npgsql or DbDriver.Oracle))
            Assert.Equal(1, nextId);
        else
            Assert.True(nextId > entity.Id, $"Expected identity to continue after {entity.Id}, got {nextId}.");
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task TruncateAsync_HandlesTablesWithoutIdentityAndKeylessMappings(DbDriver driver, string? schema)
    {
        Assert.SkipMySql(driver);

        using var session = await CreateSessionAsync(driver, schema, typeof(TruncateManualRow), typeof(TruncateKeylessRow));
        var connection = session.Connection;
        schema = session.Schema;
        try
        {
            await DeleteAllAsync<TruncateManualRow>(session);
            await DeleteAllAsync<TruncateKeylessRow>(session);
            await connection.InsertAsync(new TruncateManualRow { Id = 1 }, schema,
                cancellationToken: CancellationToken);
            await connection.InsertAsync(new TruncateKeylessRow { Value = 1 }, schema,
                returnGeneratedKey: false, cancellationToken: CancellationToken);
            Assert.Equal(1, await CountAsync<TruncateManualRow>(session));
            Assert.Equal(1, await CountAsync<TruncateKeylessRow>(session));
            await connection.TruncateAsync<TruncateManualRow>(schema, cancellationToken: CancellationToken);
            await connection.TruncateAsync<TruncateKeylessRow>(schema, cancellationToken: CancellationToken);
            Assert.Equal(0, await CountAsync<TruncateManualRow>(session));
            Assert.Equal(0, await CountAsync<TruncateKeylessRow>(session));
        }
        finally
        {
            await DeleteAllAsync<TruncateManualRow>(session);
            await DeleteAllAsync<TruncateKeylessRow>(session);
        }
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task TruncateAsync_ObservesCancellationWithoutRemovingRows(DbDriver driver, string? schema)
    {
        Assert.SkipMySql(driver);

        using var session = await CreateSessionAsync(driver, schema);
        schema = session.Schema;
        try
        {
            await SeedAsync(session);
            using var closed = Fixture.CreateDbConnection(driver, schema);
            var token = new CancellationToken(true);
            var restart = driver != DbDriver.Oracle;
            var connection = session.Connection;
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => connection.TruncateAsync<TruncateRow>(schema, cancellationToken: token));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => connection.TruncateAsync<TruncateRow>(restart, false, schema, cancellationToken: token));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => connection.TruncateAsync(session.GetTableName(typeof(TruncateRow)), schema, cancellationToken: token));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => connection.TruncateAsync(session.GetTableName(typeof(TruncateRow)), restart, false, schema, cancellationToken: token));
            Assert.Equal(ConnectionState.Open, connection.State);
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => closed.TruncateAsync<TruncateRow>(schema, cancellationToken: token));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => closed.TruncateAsync<TruncateRow>(restart, false, schema, cancellationToken: token));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => closed.TruncateAsync(session.GetTableName(typeof(TruncateRow)), schema, cancellationToken: token));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => closed.TruncateAsync(session.GetTableName(typeof(TruncateRow)), restart, false, schema, cancellationToken: token));
            Assert.Equal(ConnectionState.Closed, closed.State);
            Assert.Equal(2, await CountAsync<TruncateRow>(session));
        }
        finally
        {
            await DeleteAllAsync<TruncateRow>(session);
        }
    }

    public static TheoryData<DbDriver, string?> CascadeCases => TruncateTestCases.GetCascadeCases(Schemas);

    [Theory]
    [MemberData(nameof(CascadeCases))]
    public async Task TruncateAsync_CascadeRemovesReferencingRows(DbDriver driver, string? schema)
    {
        Assert.SkipMySql(driver);

        using var session = await CreateSessionAsync(driver, schema, typeof(TruncateParent), typeof(TruncateChild));
        var connection = session.Connection;
        schema = session.Schema;
        try
        {
            await DeleteAllAsync<TruncateChild>(session);
            await DeleteAllAsync<TruncateParent>(session);
            var parentId = await connection.InsertAsync<TruncateParent, int>(new TruncateParent(), schema,
                cancellationToken: CancellationToken);
            await connection.InsertAsync(new TruncateChild { ParentId = parentId }, schema,
                cancellationToken: CancellationToken);
            Assert.Equal(1, await CountAsync<TruncateParent>(session));
            Assert.Equal(1, await CountAsync<TruncateChild>(session));
            await connection.TruncateAsync<TruncateParent>(driver == DbDriver.Npgsql, true, schema,
                cancellationToken: CancellationToken);
            Assert.Equal(0, await CountAsync<TruncateParent>(session));
            Assert.Equal(0, await CountAsync<TruncateChild>(session));
        }
        finally
        {
            await DeleteAllAsync<TruncateChild>(session);
            await DeleteAllAsync<TruncateParent>(session);
        }
    }
}
