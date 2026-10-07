// ReSharper disable UseAwaitUsing
namespace FclEx.Dapper;

public partial class TruncateTests(DapperTestsFixture fixture) : DapperTests(fixture)
{
    private Task<TruncateTestSession> CreateSessionAsync(DbDriver driver, string? schema, params Type[] entityTypes)
        => Fixture.TruncateTables.CreateSessionAsync(driver, schema,
            entityTypes.Length == 0 ? [typeof(EntityWithAutoKey)] : entityTypes, cancellationToken: CancellationToken);

    public static TheoryData<DbDriver, string?, bool, bool> OptionCases =>
        (from pair in GetDriverSchemaCases(Schemas)
         from restart in new[] { false, true }
         from cascade in new[] { false, true }
         select (pair.Driver, pair.Schema, restart, cascade)).ToTheoryData();


    private static bool SupportsOptions(DbDriver driver, bool restartIdentity, bool cascade) => driver switch
    {
        DbDriver.Npgsql => true,
        DbDriver.Oracle => !restartIdentity,
        DbDriver.Sqlite => !cascade,
        _ => restartIdentity && !cascade,
    };

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task TruncateAsync_RemovesAllRowsAndPreservesEntityValuesAndConnectionState(DbDriver driver, string? schema)
    {
        Assert.SkipMySql(driver);

        using var session = await CreateSessionAsync(driver, schema);
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
        var connection = session.Connection;
        await VerifyTruncationAsync(session,
            token => connection.TruncateAsync<EntityWithAutoKey>(schema, session.CommandOptions, token));
        await VerifyTruncationAsync(session,
            token => connection.TruncateAsync(session.GetTableName(typeof(EntityWithAutoKey)), schema, session.CommandOptions, token));

        foreach (var restart in new[] { false, true })
        {
            foreach (var cascade in new[] { false, true })
            {
                await VerifyTruncationAsync(session,
                    token => connection.TruncateAsync<EntityWithAutoKey>(restart, cascade, schema, session.CommandOptions, token), restart, cascade);

                await VerifyTruncationAsync(session,
                    token => connection.TruncateAsync(session.GetTableName(typeof(EntityWithAutoKey)), restart, cascade, schema, session.CommandOptions, token), restart, cascade);
            }
        }

        if (schema is not null)
        {
            var options = session.CommandOptions with
            {
                EntityMappingSource = new MappingSource(session.GetTableName(typeof(EntityWithAutoKey)), "missing_schema"),
            };

            await VerifyTruncationAsync(session,
                token => connection.TruncateAsync<MappedRow>(schema, options, token));
        }

        Assert.Equal(ConnectionState.Closed, connection.State);
    }

    [Theory]
    [MemberData(nameof(OptionCases))]
    public async Task TruncateAsync_ExplicitOptionsHonorDatabaseCapabilities(
        DbDriver driver, string? schema, bool restartIdentity, bool cascade)
    {
        Assert.SkipMySql(driver);

        using var session = await CreateSessionAsync(driver, schema);
        var connection = session.Connection;
        schema = session.Schema;
        await VerifyTruncationAsync(session,
            token => connection.TruncateAsync<EntityWithAutoKey>(restartIdentity, cascade, schema, commandOptions: session.CommandOptions, cancellationToken: token),
            restartIdentity, cascade);
        await VerifyTruncationAsync(session,
            token => connection.TruncateAsync(session.GetTableName(typeof(EntityWithAutoKey)), restartIdentity, cascade, schema, commandOptions: session.CommandOptions, cancellationToken: token),
            restartIdentity, cascade);
    }

    private static Task<int> DeleteAllAsync<T>(TruncateTestSession session)
        => session.Connection.ExecuteAsync($"DELETE FROM {session.GetQualifiedTableName(typeof(T))}", cancellationToken: CancellationToken);

    private static Task<int> CountAsync<T>(TruncateTestSession session, DbTransaction? transaction = null)
        => session.Connection.QuerySingleAsync<int>(new CommandDefinition($"SELECT COUNT(*) FROM {session.GetQualifiedTableName(typeof(T))}",
            transaction: transaction, cancellationToken: CancellationToken));

    private static async Task<EntityWithAutoKey> SeedAsync(TruncateTestSession session)
    {
        var connection = session.Connection;
        var schema = session.Schema;
        await DeleteAllAsync<EntityWithAutoKey>(session);
        var first = new EntityWithAutoKey { Name = Guid.NewGuid().ToString(), Value = 1 };
        var second = new EntityWithAutoKey { Name = Guid.NewGuid().ToString(), Value = -1 };
        first.Id = await connection.InsertAsync<EntityWithAutoKey, int>(first, schema, commandOptions: session.CommandOptions, cancellationToken: CancellationToken);
        second.Id = await connection.InsertAsync<EntityWithAutoKey, int>(second, schema, commandOptions: session.CommandOptions, cancellationToken: CancellationToken);
        Assert.True(second.Id > first.Id);
        Assert.Equal(2, await CountAsync<EntityWithAutoKey>(session));
        return second;
    }

    private static async Task VerifyTruncationAsync(
        TruncateTestSession session, Func<CancellationToken, Task> truncate,
        bool? restartIdentity = null, bool cascade = false)
    {
        var connection = session.Connection;
        var driver = session.Driver;
        var schema = session.Schema;
        try
        {
            var entity = await SeedAsync(session);
            var previousId = entity.Id;
            var initialState = connection.State;
            if (restartIdentity is { } restart && !SupportsOptions(driver, restart, cascade))
            {
                await Assert.ThrowsAsync<NotSupportedException>(() => truncate(CancellationToken));
                Assert.Equal(initialState, connection.State);
                Assert.Equal(2, await CountAsync<EntityWithAutoKey>(session));
                Assert.Equal(previousId, entity.Id);
                return;
            }

            await truncate(CancellationToken);
            Assert.Equal(initialState, connection.State);
            Assert.Equal(0, await CountAsync<EntityWithAutoKey>(session));
            Assert.Equal(previousId, entity.Id);
            Assert.Equal(-1, entity.Value);
            var nextId = await connection.InsertAsync<EntityWithAutoKey, int>(
                new() { Name = Guid.NewGuid().ToString(), Value = 1 }, schema, commandOptions: session.CommandOptions, cancellationToken: CancellationToken);
            if (restartIdentity ?? driver is not (DbDriver.Npgsql or DbDriver.Oracle))
                Assert.Equal(1, nextId);
            else
                Assert.True(nextId > previousId, $"Expected identity to continue after {previousId}, got {nextId}.");
        }
        finally
        {
            await DeleteAllAsync<EntityWithAutoKey>(session);
        }
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

    public static TheoryData<DbDriver, string?> CascadeCases
    {
        get
        {
            var cases = GetDriverSchemaCases(Schemas)
                .Where(pair => pair.Driver is DbDriver.Npgsql or DbDriver.Oracle)
                .Select(pair => (pair.Driver, pair.Schema)).ToTheoryData();
            // xUnit 4's deferred zero-row theory reports a failed summary even with SkipTestWithoutData.
            // An explicitly skipped row preserves filtered runs when no cascade driver is selected.
            if (cases.Count == 0)
                cases.Add(new TheoryDataRow<DbDriver, string?>(SelectedDrivers.FirstOrDefault(), null)
                {
                    Skip = "No selected driver supports TRUNCATE CASCADE.",
                });
            return cases;
        }
    }

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
