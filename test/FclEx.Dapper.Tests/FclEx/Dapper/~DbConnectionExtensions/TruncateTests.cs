// ReSharper disable UseAwaitUsing
namespace FclEx.Dapper;

public partial class TruncateTests(DapperTestsFixture fixture) : DapperTests(fixture)
{
    private TruncateTestSession Session { get; set; } = null!;
    private CommandOptions Options => Session.CommandOptions;

    private async Task<TruncateTestSession> CreateSessionAsync(DbDriver driver, string? schema, params Type[] entityTypes)
    {
        Session = await Fixture.TruncateTables.CreateSessionAsync(driver, schema,
            entityTypes.Length == 0 ? [typeof(EntityWithAutoKey)] : entityTypes, cancellationToken: CancellationToken);
        return Session;
    }

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
        await VerifyTruncationAsync(connection, driver, schema,
            token => connection.TruncateAsync<EntityWithAutoKey>(schema, commandOptions: Options, cancellationToken: token));
        await VerifyTruncationAsync(connection, driver, schema,
            token => connection.TruncateAsync(Session.GetTableName(typeof(EntityWithAutoKey)), schema, commandOptions: Options, cancellationToken: token));
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task TruncateAsync_PreservesClosedConnectionState(DbDriver driver, string? schema)
    {
        Assert.SkipMySql(driver);

        using var session = Fixture.TruncateTables.CreateClosedConnectionSession(driver, schema);
        Session = session;
        var connection = session.Connection;
        await VerifyTruncationAsync(connection, driver, schema,
            token => connection.TruncateAsync<EntityWithAutoKey>(schema, Options, token));
        await VerifyTruncationAsync(connection, driver, schema,
            token => connection.TruncateAsync(Session.GetTableName(typeof(EntityWithAutoKey)), schema, Options, token));

        foreach (var restart in new[] { false, true })
        {
            foreach (var cascade in new[] { false, true })
            {
                await VerifyTruncationAsync(connection, driver, schema,
                    token => connection.TruncateAsync<EntityWithAutoKey>(restart, cascade, schema, Options, token), restart, cascade);

                await VerifyTruncationAsync(connection, driver, schema,
                    token => connection.TruncateAsync(Session.GetTableName(typeof(EntityWithAutoKey)), restart, cascade, schema, Options, token), restart, cascade);
            }
        }

        if (schema is not null)
        {
            var options = Options with
            {
                EntityMappingSource = new MappingSource(Session.GetTableName(typeof(EntityWithAutoKey)), "missing_schema"),
            };

            await VerifyTruncationAsync(connection, driver, schema,
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
        await VerifyTruncationAsync(connection, driver, schema,
            token => connection.TruncateAsync<EntityWithAutoKey>(restartIdentity, cascade, schema, commandOptions: Options, cancellationToken: token),
            restartIdentity, cascade);
        await VerifyTruncationAsync(connection, driver, schema,
            token => connection.TruncateAsync(Session.GetTableName(typeof(EntityWithAutoKey)), restartIdentity, cascade, schema, commandOptions: Options, cancellationToken: token),
            restartIdentity, cascade);
    }

    private string Table<T>()
        => Session.GetQualifiedTableName(typeof(T));

    private Task<int> DeleteAllAsync<T>(DbConnection connection, string? schema)
        => connection.ExecuteAsync($"DELETE FROM {Table<T>()}", cancellationToken: CancellationToken);

    private Task<int> CountAsync<T>(DbConnection connection, string? schema, DbTransaction? transaction = null)
        => connection.QuerySingleAsync<int>(new CommandDefinition($"SELECT COUNT(*) FROM {Table<T>()}",
            transaction: transaction, cancellationToken: CancellationToken));

    private async Task<EntityWithAutoKey> SeedAsync(DbConnection connection, string? schema)
    {
        await DeleteAllAsync<EntityWithAutoKey>(connection, schema);
        var first = new EntityWithAutoKey { Name = Guid.NewGuid().ToString(), Value = 1 };
        var second = new EntityWithAutoKey { Name = Guid.NewGuid().ToString(), Value = -1 };
        first.Id = await connection.InsertAsync<EntityWithAutoKey, int>(first, schema, commandOptions: Options, cancellationToken: CancellationToken);
        second.Id = await connection.InsertAsync<EntityWithAutoKey, int>(second, schema, commandOptions: Options, cancellationToken: CancellationToken);
        Assert.True(second.Id > first.Id);
        Assert.Equal(2, await CountAsync<EntityWithAutoKey>(connection, schema));
        return second;
    }

    private async Task VerifyTruncationAsync(
        DbConnection connection, DbDriver driver, string? schema, Func<CancellationToken, Task> truncate,
        bool? restartIdentity = null, bool cascade = false)
    {
        try
        {
            var entity = await SeedAsync(connection, schema);
            var previousId = entity.Id;
            var initialState = connection.State;
            if (restartIdentity is { } restart && !SupportsOptions(driver, restart, cascade))
            {
                await Assert.ThrowsAsync<NotSupportedException>(() => truncate(CancellationToken));
                Assert.Equal(initialState, connection.State);
                Assert.Equal(2, await CountAsync<EntityWithAutoKey>(connection, schema));
                Assert.Equal(previousId, entity.Id);
                return;
            }

            await truncate(CancellationToken);
            Assert.Equal(initialState, connection.State);
            Assert.Equal(0, await CountAsync<EntityWithAutoKey>(connection, schema));
            Assert.Equal(previousId, entity.Id);
            Assert.Equal(-1, entity.Value);
            var nextId = await connection.InsertAsync<EntityWithAutoKey, int>(
                new() { Name = Guid.NewGuid().ToString(), Value = 1 }, schema, commandOptions: Options, cancellationToken: CancellationToken);
            if (restartIdentity ?? driver is not (DbDriver.Npgsql or DbDriver.Oracle))
                Assert.Equal(1, nextId);
            else
                Assert.True(nextId > previousId, $"Expected identity to continue after {previousId}, got {nextId}.");
        }
        finally
        {
            await DeleteAllAsync<EntityWithAutoKey>(connection, schema);
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
            await DeleteAllAsync<EntityWithGuidKey>(connection, schema);
            await DeleteAllAsync<EntityWithoutKey>(connection, schema);
            await connection.InsertAsync(new EntityWithGuidKey { Id = Guid.NewGuid(), Value = 1, Order = 2 }, schema,
                commandOptions: Options, cancellationToken: CancellationToken);
            await connection.InsertAsync(new EntityWithoutKey { Name = Guid.NewGuid().ToString(), Value = 1 }, schema,
                returnGeneratedKey: false, commandOptions: Options, cancellationToken: CancellationToken);
            Assert.Equal(1, await CountAsync<EntityWithGuidKey>(connection, schema));
            Assert.Equal(1, await CountAsync<EntityWithoutKey>(connection, schema));
            await connection.TruncateAsync<EntityWithGuidKey>(schema, commandOptions: Options, cancellationToken: CancellationToken);
            await connection.TruncateAsync<EntityWithoutKey>(schema, commandOptions: Options, cancellationToken: CancellationToken);
            Assert.Equal(0, await CountAsync<EntityWithGuidKey>(connection, schema));
            Assert.Equal(0, await CountAsync<EntityWithoutKey>(connection, schema));
        }
        finally
        {
            await DeleteAllAsync<EntityWithGuidKey>(connection, schema);
            await DeleteAllAsync<EntityWithoutKey>(connection, schema);
        }
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task TruncateAsync_ObservesCancellationWithoutRemovingRows(DbDriver driver, string? schema)
    {
        Assert.SkipMySql(driver);

        using var session = await CreateSessionAsync(driver, schema);
        var connection = session.Connection;
        schema = session.Schema;
        try
        {
            await SeedAsync(connection, schema);
            using var closed = Fixture.CreateDbConnection(driver, schema == "pg_temp" ? null : schema);
            var token = new CancellationToken(true);
            var restart = driver != DbDriver.Oracle;
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => closed.TruncateAsync<EntityWithAutoKey>(schema, commandOptions: Options, cancellationToken: token));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => closed.TruncateAsync<EntityWithAutoKey>(restart, false, schema, commandOptions: Options, cancellationToken: token));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => closed.TruncateAsync(Session.GetTableName(typeof(EntityWithAutoKey)), schema, commandOptions: Options, cancellationToken: token));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => closed.TruncateAsync(Session.GetTableName(typeof(EntityWithAutoKey)), restart, false, schema, commandOptions: Options, cancellationToken: token));
            Assert.Equal(ConnectionState.Closed, closed.State);
            Assert.Equal(2, await CountAsync<EntityWithAutoKey>(connection, schema));
        }
        finally
        {
            await DeleteAllAsync<EntityWithAutoKey>(connection, schema);
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
            await DeleteAllAsync<EntityWithNavigation>(connection, schema);
            await DeleteAllAsync<EntityHasStates>(connection, schema);
            // The session mapping preserves the identity convention used by EF for these types.
            var options = Options;
            var parentId = await connection.InsertAsync(new EntityHasStates { Name = Guid.NewGuid().ToString() }, schema,
                commandOptions: options, cancellationToken: CancellationToken);
            await connection.InsertAsync(new EntityWithNavigation { Name = Guid.NewGuid().ToString(), NavigationId = parentId }, schema,
                commandOptions: options, cancellationToken: CancellationToken);
            Assert.Equal(1, await CountAsync<EntityHasStates>(connection, schema));
            Assert.Equal(1, await CountAsync<EntityWithNavigation>(connection, schema));
            await connection.TruncateAsync<EntityHasStates>(driver == DbDriver.Npgsql, true, schema,
                Options, CancellationToken);
            Assert.Equal(0, await CountAsync<EntityHasStates>(connection, schema));
            Assert.Equal(0, await CountAsync<EntityWithNavigation>(connection, schema));
        }
        finally
        {
            await DeleteAllAsync<EntityWithNavigation>(connection, schema);
            await DeleteAllAsync<EntityHasStates>(connection, schema);
        }
    }

}
