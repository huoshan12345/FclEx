// ReSharper disable UseAwaitUsing
namespace FclEx.Dapper;

[TestClass(DisableParallelization = true)]
public partial class TruncateTests(DapperTestsFixture fixture) : DapperTests(fixture)
{
    public static TheoryData<DbDriver, string?, bool, bool> OptionCases =>
        (from pair in GetDriverSchemaCases(Schemas)
         from restart in new[] { false, true }
         from cascade in new[] { false, true }
         select (pair.Driver, pair.Schema, restart, cascade)).ToTheoryData();

    private static void SkipMySql(DbDriver driver)
        => Assert.SkipWhen(driver == DbDriver.MySql, "MySql.Data asynchronous timeout cleanup can hang.");

    private static bool SupportsOptions(DbDriver driver, bool restartIdentity, bool cascade) => driver switch
    {
        DbDriver.Npgsql => true,
        DbDriver.Oracle => !restartIdentity,
        DbDriver.Sqlite => !cascade,
        _ => restartIdentity && !cascade,
    };

    [Theory(DisableParallelization = true)]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task TruncateAsync_RemovesAllRowsAndPreservesEntityValuesAndConnectionState(DbDriver driver, string? schema)
    {
        SkipMySql(driver);
        using var connection = Fixture.CreateDbConnection(driver, schema);
        await VerifyTruncationAsync(connection, driver, schema,
            token => connection.TruncateAsync<EntityWithAutoKey>(schema, cancellationToken: token));
        await connection.OpenAsync(CancellationToken);
        await VerifyTruncationAsync(connection, driver, schema,
            token => connection.TruncateAsync(nameof(EntityWithAutoKey), schema, cancellationToken: token));
    }

    [Theory(DisableParallelization = true)]
    [MemberData(nameof(OptionCases))]
    public async Task TruncateAsync_ExplicitOptionsHonorDatabaseCapabilities(
        DbDriver driver, string? schema, bool restartIdentity, bool cascade)
    {
        SkipMySql(driver);
        using var connection = Fixture.CreateDbConnection(driver, schema);
        await VerifyTruncationAsync(connection, driver, schema,
            token => connection.TruncateAsync<EntityWithAutoKey>(restartIdentity, cascade, schema, cancellationToken: token),
            restartIdentity, cascade);
        await VerifyTruncationAsync(connection, driver, schema,
            token => connection.TruncateAsync(nameof(EntityWithAutoKey), restartIdentity, cascade, schema, cancellationToken: token),
            restartIdentity, cascade);
    }

    private static string Table<T>(DbConnection connection, string? schema)
        => DapperHelper.GetTableNameWithSchema(connection, schema, typeof(T));

    private static Task<int> DeleteAllAsync<T>(DbConnection connection, string? schema)
        => connection.ExecuteAsync($"DELETE FROM {Table<T>(connection, schema)}", cancellationToken: CancellationToken);

    private static Task<int> CountAsync<T>(DbConnection connection, string? schema, DbTransaction? transaction = null)
        => connection.QuerySingleAsync<int>(new CommandDefinition($"SELECT COUNT(*) FROM {Table<T>(connection, schema)}",
            transaction: transaction, cancellationToken: CancellationToken));

    private static async Task<EntityWithAutoKey> SeedAsync(DbConnection connection, string? schema)
    {
        await DeleteAllAsync<EntityWithAutoKey>(connection, schema);
        var first = new EntityWithAutoKey { Name = Guid.NewGuid().ToString(), Value = 1 };
        var second = new EntityWithAutoKey { Name = Guid.NewGuid().ToString(), Value = -1 };
        first.Id = await connection.InsertAsync<EntityWithAutoKey, int>(first, schema, cancellationToken: CancellationToken);
        second.Id = await connection.InsertAsync<EntityWithAutoKey, int>(second, schema, cancellationToken: CancellationToken);
        Assert.True(second.Id > first.Id);
        Assert.Equal(2, await CountAsync<EntityWithAutoKey>(connection, schema));
        return second;
    }

    private static async Task VerifyTruncationAsync(
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
                new() { Name = Guid.NewGuid().ToString(), Value = 1 }, schema, cancellationToken: CancellationToken);
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

    [Theory(DisableParallelization = true)]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task TruncateAsync_HandlesTablesWithoutIdentityAndKeylessMappings(DbDriver driver, string? schema)
    {
        SkipMySql(driver);
        using var connection = Fixture.CreateDbConnection(driver, schema);
        try
        {
            await DeleteAllAsync<EntityWithGuidKey>(connection, schema);
            await DeleteAllAsync<EntityWithoutKey>(connection, schema);
            await connection.InsertAsync(new EntityWithGuidKey { Id = Guid.NewGuid(), Value = 1, Order = 2 }, schema,
                cancellationToken: CancellationToken);
            await connection.InsertAsync(new EntityWithoutKey { Name = Guid.NewGuid().ToString(), Value = 1 }, schema,
                returnGeneratedKey: false, cancellationToken: CancellationToken);
            Assert.Equal(1, await CountAsync<EntityWithGuidKey>(connection, schema));
            Assert.Equal(1, await CountAsync<EntityWithoutKey>(connection, schema));
            await connection.TruncateAsync<EntityWithGuidKey>(schema, cancellationToken: CancellationToken);
            await connection.TruncateAsync<EntityWithoutKey>(schema, cancellationToken: CancellationToken);
            Assert.Equal(0, await CountAsync<EntityWithGuidKey>(connection, schema));
            Assert.Equal(0, await CountAsync<EntityWithoutKey>(connection, schema));
        }
        finally
        {
            await DeleteAllAsync<EntityWithGuidKey>(connection, schema);
            await DeleteAllAsync<EntityWithoutKey>(connection, schema);
        }
    }

    [Theory(DisableParallelization = true)]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task TruncateAsync_ObservesCancellationWithoutRemovingRows(DbDriver driver, string? schema)
    {
        SkipMySql(driver);
        using var connection = Fixture.CreateDbConnection(driver, schema);
        try
        {
            await SeedAsync(connection, schema);
            var token = new CancellationToken(true);
            var restart = driver != DbDriver.Oracle;
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => connection.TruncateAsync<EntityWithAutoKey>(schema, cancellationToken: token));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => connection.TruncateAsync<EntityWithAutoKey>(restart, false, schema, cancellationToken: token));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => connection.TruncateAsync(nameof(EntityWithAutoKey), schema, cancellationToken: token));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => connection.TruncateAsync(nameof(EntityWithAutoKey), restart, false, schema, cancellationToken: token));
            Assert.Equal(ConnectionState.Closed, connection.State);
            Assert.Equal(2, await CountAsync<EntityWithAutoKey>(connection, schema));
        }
        finally
        {
            await DeleteAllAsync<EntityWithAutoKey>(connection, schema);
        }
    }

    [Theory(DisableParallelization = true)]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task TruncateAsync_HonorsReferencingTableConstraints(DbDriver driver, string? schema)
    {
        SkipMySql(driver);
        using var connection = Fixture.CreateDbConnection(driver, schema);
        try
        {
            await DeleteAllAsync<EntityWithNavigation>(connection, schema);
            await DeleteAllAsync<EntityHasStates>(connection, schema);
            // EF creates these identity keys by convention; the shared CLR types do not
            // declare database generation attributes, so Dapper needs an explicit mapping.
            var options = new CommandOptions { EntityMappingSource = new NavigationMappingSource() };
            var parentId = await connection.InsertAsync(new EntityHasStates { Name = Guid.NewGuid().ToString() }, schema,
                commandOptions: options, cancellationToken: CancellationToken);
            await connection.InsertAsync(new EntityWithNavigation { Name = Guid.NewGuid().ToString(), NavigationId = parentId }, schema,
                commandOptions: options, cancellationToken: CancellationToken);
            if (driver == DbDriver.Npgsql)
            {
                await connection.TruncateAsync<EntityHasStates>(true, true, schema, cancellationToken: CancellationToken);
                Assert.Equal(0, await CountAsync<EntityHasStates>(connection, schema));
                Assert.Equal(0, await CountAsync<EntityWithNavigation>(connection, schema));
            }
            else
            {
                // The provisioned foreign key has no ON DELETE CASCADE, including on Oracle.
                await Assert.ThrowsAnyAsync<DbException>(() => connection.TruncateAsync<EntityHasStates>(
                    driver != DbDriver.Oracle, driver == DbDriver.Oracle, schema, cancellationToken: CancellationToken));
                Assert.Equal(1, await CountAsync<EntityHasStates>(connection, schema));
                Assert.Equal(1, await CountAsync<EntityWithNavigation>(connection, schema));
            }
        }
        finally
        {
            await DeleteAllAsync<EntityWithNavigation>(connection, schema);
            await DeleteAllAsync<EntityHasStates>(connection, schema);
        }
    }

    private sealed class NavigationMappingSource : IEntityMappingSource
    {
        private readonly Dictionary<Type, EntityMapping> _mappings = new[] { typeof(EntityHasStates), typeof(EntityWithNavigation) }
            .ToDictionary(type => type, type =>
            {
                var mapping = DapperHelper.GetEntityMapping(type);
                var properties = mapping.Properties.Select(property => property.Property.Name == "Id"
                    ? new PropertyMapping(property.Property, property.ColumnName, true, DatabaseValueGeneration.OnInsert)
                    : property);
                return new EntityMapping(type, mapping.TableName, properties, mapping.Schema);
            });

        public EntityMapping GetMapping(Type entityType) => _mappings[entityType];
    }
}
