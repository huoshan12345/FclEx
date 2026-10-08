// ReSharper disable UseAwaitUsing
namespace FclEx.Dapper;

public partial class TruncateTests(DapperTestsFixture fixture) : DapperTests(fixture)
{
    private Task<TruncateTestSession> CreateSessionAsync(DbDriver driver, string? schema, params Type[] entityTypes)
        => Fixture.TruncateTables.CreateSessionAsync(driver, schema,
            entityTypes.Length == 0 ? [typeof(EntityWithAutoKey)] : entityTypes, cancellationToken: CancellationToken);

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
            token => connection.TruncateAsync<EntityWithAutoKey>(schema, commandOptions: session.CommandOptions, cancellationToken: token));
        await VerifyTruncationAsync(session,
            token => connection.TruncateAsync(session.GetTableName(typeof(EntityWithAutoKey)), schema, commandOptions: session.CommandOptions, cancellationToken: token));
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task TruncateAsync_PreservesClosedConnectionState(DbDriver driver, string? schema)
    {
        Assert.SkipMySql(driver);
        using var session = Fixture.TruncateTables.CreateClosedConnectionSession(driver, schema);
        await session.Connection.OpenAsync(CancellationToken);
        await using var cleanup = CleanupRows(session);
        using var closed = Fixture.CreateDbConnection(driver, schema);
        var restart = driver != DbDriver.Oracle;
        var table = session.GetTableName(typeof(EntityWithAutoKey));
        var options = session.CommandOptions;
        Func<CancellationToken, Task>[] truncations =
        [
            token => closed.TruncateAsync<EntityWithAutoKey>(schema, options, token),
            token => closed.TruncateAsync(table, schema, options, token),
            token => closed.TruncateAsync<EntityWithAutoKey>(restart, false, schema, options, token),
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
            token => connection.TruncateAsync<EntityWithAutoKey>(restartIdentity, cascade, schema, commandOptions: session.CommandOptions, cancellationToken: token),
            restartIdentity);
        await VerifyTruncationAsync(session,
            token => connection.TruncateAsync(session.GetTableName(typeof(EntityWithAutoKey)), restartIdentity, cascade, schema, commandOptions: session.CommandOptions, cancellationToken: token),
            restartIdentity);
    }

    [Theory]
    [MemberData(nameof(TruncateTestCases.UnsupportedOptionCases), MemberType = typeof(TruncateTestCases))]
    public async Task TruncateAsync_UnsupportedOptionsThrowBeforeOpeningConnection(
        DbDriver driver, bool restartIdentity, bool cascade)
    {
        Assert.SkipMySql(driver);
        using var connection = Fixture.CreateDbConnection(driver, null);
        await Assert.ThrowsAsync<NotSupportedException>(() => connection.TruncateAsync<EntityWithAutoKey>(restartIdentity, cascade));
        await Assert.ThrowsAsync<NotSupportedException>(() => connection.TruncateAsync(nameof(EntityWithAutoKey), restartIdentity, cascade));
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
            await DeleteAllAsync<EntityWithAutoKey>(session);
        });

    private static async Task<EntityWithAutoKey> SeedAsync(TruncateTestSession session)
    {
        await DeleteAllAsync<EntityWithAutoKey>(session);
        var first = new EntityWithAutoKey { Name = Guid.NewGuid().ToString(), Value = 1 };
        var second = new EntityWithAutoKey { Name = Guid.NewGuid().ToString(), Value = -1 };
        first.Id = await session.Connection.InsertAsync<EntityWithAutoKey, int>(first, session.Schema,
            commandOptions: session.CommandOptions, cancellationToken: CancellationToken);
        second.Id = await session.Connection.InsertAsync<EntityWithAutoKey, int>(second, session.Schema,
            commandOptions: session.CommandOptions, cancellationToken: CancellationToken);
        Assert.True(second.Id > first.Id);
        Assert.Equal(2, await CountAsync<EntityWithAutoKey>(session));
        return second;
    }

    private static async Task<EntityWithAutoKey> VerifyRowsRemovedAsync(
        TruncateTestSession session, Func<CancellationToken, Task> truncate)
    {
        var entity = await SeedAsync(session);
        var initialState = session.Connection.State;
        await truncate(CancellationToken);
        Assert.Equal(initialState, session.Connection.State);
        Assert.Equal(0, await CountAsync<EntityWithAutoKey>(session));
        return entity;
    }

    private static async Task VerifyTruncationAsync(
        TruncateTestSession session, Func<CancellationToken, Task> truncate, bool? restartIdentity = null)
    {
        var entity = await VerifyRowsRemovedAsync(session, truncate);
        var nextId = await session.Connection.InsertAsync<EntityWithAutoKey, int>(
            new() { Name = Guid.NewGuid().ToString(), Value = 1 }, session.Schema,
            commandOptions: session.CommandOptions, cancellationToken: CancellationToken);
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

        using var session = await CreateSessionAsync(driver, schema, typeof(EntityWithGuidKey), typeof(EntityWithoutKey));
        var connection = session.Connection;
        schema = session.Schema;
        try
        {
            await DeleteAllAsync<EntityWithGuidKey>(session);
            await DeleteAllAsync<EntityWithoutKey>(session);
            await connection.InsertAsync(new EntityWithGuidKey { Id = Guid.NewGuid(), Value = 1, Order = 2 }, schema,
                commandOptions: session.CommandOptions, cancellationToken: CancellationToken);
            await connection.InsertAsync(new EntityWithoutKey { Name = Guid.NewGuid().ToString(), Value = 1 }, schema,
                returnGeneratedKey: false, commandOptions: session.CommandOptions, cancellationToken: CancellationToken);
            Assert.Equal(1, await CountAsync<EntityWithGuidKey>(session));
            Assert.Equal(1, await CountAsync<EntityWithoutKey>(session));
            await connection.TruncateAsync<EntityWithGuidKey>(schema, commandOptions: session.CommandOptions, cancellationToken: CancellationToken);
            await connection.TruncateAsync<EntityWithoutKey>(schema, commandOptions: session.CommandOptions, cancellationToken: CancellationToken);
            Assert.Equal(0, await CountAsync<EntityWithGuidKey>(session));
            Assert.Equal(0, await CountAsync<EntityWithoutKey>(session));
        }
        finally
        {
            await DeleteAllAsync<EntityWithGuidKey>(session);
            await DeleteAllAsync<EntityWithoutKey>(session);
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
            using var closed = Fixture.CreateDbConnection(driver, schema == "pg_temp" ? null : schema);
            var token = new CancellationToken(true);
            var restart = driver != DbDriver.Oracle;
            // Temporary tables belong to this connection; verify cancellation where the seeded rows exist.
            var connection = session.Connection;
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => connection.TruncateAsync<EntityWithAutoKey>(schema, commandOptions: session.CommandOptions, cancellationToken: token));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => connection.TruncateAsync<EntityWithAutoKey>(restart, false, schema, commandOptions: session.CommandOptions, cancellationToken: token));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => connection.TruncateAsync(session.GetTableName(typeof(EntityWithAutoKey)), schema, commandOptions: session.CommandOptions, cancellationToken: token));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => connection.TruncateAsync(session.GetTableName(typeof(EntityWithAutoKey)), restart, false, schema, commandOptions: session.CommandOptions, cancellationToken: token));
            Assert.Equal(ConnectionState.Open, connection.State);
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => closed.TruncateAsync<EntityWithAutoKey>(schema, commandOptions: session.CommandOptions, cancellationToken: token));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => closed.TruncateAsync<EntityWithAutoKey>(restart, false, schema, commandOptions: session.CommandOptions, cancellationToken: token));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => closed.TruncateAsync(session.GetTableName(typeof(EntityWithAutoKey)), schema, commandOptions: session.CommandOptions, cancellationToken: token));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => closed.TruncateAsync(session.GetTableName(typeof(EntityWithAutoKey)), restart, false, schema, commandOptions: session.CommandOptions, cancellationToken: token));
            Assert.Equal(ConnectionState.Closed, closed.State);
            Assert.Equal(2, await CountAsync<EntityWithAutoKey>(session));
        }
        finally
        {
            await DeleteAllAsync<EntityWithAutoKey>(session);
        }
    }

    public static TheoryData<DbDriver, string?> CascadeCases => TruncateTestCases.GetCascadeCases(Schemas);

    [Theory]
    [MemberData(nameof(CascadeCases))]
    public async Task TruncateAsync_CascadeRemovesReferencingRows(DbDriver driver, string? schema)
    {
        Assert.SkipMySql(driver);

        using var session = await CreateSessionAsync(driver, schema, typeof(EntityHasStates), typeof(EntityWithNavigation));
        var connection = session.Connection;
        schema = session.Schema;
        try
        {
            await DeleteAllAsync<EntityWithNavigation>(session);
            await DeleteAllAsync<EntityHasStates>(session);
            // The session mapping preserves the identity convention used by EF for these types.
            var parentId = await connection.InsertAsync(new EntityHasStates { Name = Guid.NewGuid().ToString() }, schema,
                commandOptions: session.CommandOptions, cancellationToken: CancellationToken);
            await connection.InsertAsync(new EntityWithNavigation { Name = Guid.NewGuid().ToString(), NavigationId = parentId }, schema,
                commandOptions: session.CommandOptions, cancellationToken: CancellationToken);
            Assert.Equal(1, await CountAsync<EntityHasStates>(session));
            Assert.Equal(1, await CountAsync<EntityWithNavigation>(session));
            await connection.TruncateAsync<EntityHasStates>(driver == DbDriver.Npgsql, true, schema,
                session.CommandOptions, CancellationToken);
            Assert.Equal(0, await CountAsync<EntityHasStates>(session));
            Assert.Equal(0, await CountAsync<EntityWithNavigation>(session));
        }
        finally
        {
            await DeleteAllAsync<EntityWithNavigation>(session);
            await DeleteAllAsync<EntityHasStates>(session);
        }
    }
}
