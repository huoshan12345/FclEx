// ReSharper disable UseAwaitUsing
namespace FclEx.Dapper;

public partial class TruncateTests
{
    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task TruncateAsync_UsesMappedTableSchemaAndAdapterOverrides(DbDriver driver, string? schema)
    {
        SkipMySql(driver);
        var targetSchema = schema;
        using var session = await CreateSessionAsync(driver, schema, typeof(EntityWithAutoKey), typeof(HasTableAttributeEntity));
        var connection = session.Connection;
        schema = session.Schema;
        var options = new CommandOptions
        {
            EntityMappingSource = new MappingSource(Session.GetTableName(typeof(EntityWithAutoKey)), schema),
            SqlAdapter = DapperHelper.GetSqlAdapter(connection),
            TimeoutSeconds = 10,
        };
        await VerifyTruncationAsync(connection, driver, schema,
            token => connection.TruncateAsync<MappedRow>(commandOptions: options, cancellationToken: token));
        var restart = driver != DbDriver.Oracle;
        var cascade = driver is DbDriver.Npgsql or DbDriver.Oracle;
        await VerifyTruncationAsync(connection, driver, schema,
            token => connection.TruncateAsync<MappedRow>(restart, cascade, commandOptions: options, cancellationToken: token),
            restart, cascade);
        var wrongSchema = options with { EntityMappingSource = new MappingSource(Session.GetTableName(typeof(EntityWithAutoKey)), "missing_" + Guid.NewGuid().ToString("N")) };
        // Null retains the mapped schema, so only explicit-schema cases can override it.
        if (schema is not null)
            await VerifyTruncationAsync(connection, driver, schema,
                token => connection.TruncateAsync<MappedRow>(schema, wrongSchema, token));
        // TableExists intentionally excludes SQL Server temporary tables; keep its catalog check
        // against the ordinary attribute-mapped table, and truncate the isolated copy below.
        Assert.True(await connection.TableExistsAsync<HasTableAttributeEntity>(targetSchema, cancellationToken: CancellationToken));
        try
        {
            await DeleteAllAsync<HasTableAttributeEntity>(connection, schema);
            await connection.InsertAsync(new HasTableAttributeEntity(), schema, commandOptions: Options, cancellationToken: CancellationToken);
            Assert.Equal(1, await CountAsync<HasTableAttributeEntity>(connection, schema));
            await connection.TruncateAsync<HasTableAttributeEntity>(schema, commandOptions: Options, cancellationToken: CancellationToken);
            Assert.Equal(0, await CountAsync<HasTableAttributeEntity>(connection, schema));
        }
        finally
        {
            await DeleteAllAsync<HasTableAttributeEntity>(connection, schema);
        }
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task TruncateAsync_MissingLiteralTableNameDoesNotRemoveExistingRows(DbDriver driver, string? schema)
    {
        SkipMySql(driver);
        using var session = await CreateSessionAsync(driver, schema);
        var connection = session.Connection;
        schema = session.Schema;
        try
        {
            await SeedAsync(connection, schema);
            using var closed = Fixture.CreateDbConnection(driver, schema == "pg_temp" ? null : schema);
            await Assert.ThrowsAnyAsync<DbException>(() => closed.TruncateAsync("missing.'\";--", schema,
                cancellationToken: CancellationToken));
            Assert.Equal(ConnectionState.Closed, closed.State);
            Assert.Equal(2, await CountAsync<EntityWithAutoKey>(connection, schema));
        }
        finally
        {
            await DeleteAllAsync<EntityWithAutoKey>(connection, schema);
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task TruncateAsync_RejectsInvalidIdentifiersBeforeOpeningConnection(string invalid)
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await Assert.ThrowsAsync<ArgumentException>(() => connection.TruncateAsync(invalid));
        await Assert.ThrowsAsync<ArgumentException>(() => connection.TruncateAsync(invalid, true, false));
        await Assert.ThrowsAsync<ArgumentException>(() => connection.TruncateAsync("table", invalid));
        await Assert.ThrowsAsync<ArgumentException>(() => connection.TruncateAsync<EntityWithAutoKey>(invalid));
        Assert.Equal(ConnectionState.Closed, connection.State);
    }

    [Fact]
    public async Task TruncateAsync_ValidatesNullArgumentsAndCommandOptions()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await Assert.ThrowsAsync<ArgumentNullException>(() => ((DbConnection)null!).TruncateAsync("table"));
        await Assert.ThrowsAsync<ArgumentNullException>(() => ((DbConnection)null!).TruncateAsync<EntityWithAutoKey>());
        await Assert.ThrowsAsync<ArgumentNullException>(() => ((DbConnection)null!).TruncateAsync<EntityWithAutoKey>(true, false));
        await Assert.ThrowsAsync<ArgumentNullException>(() => connection.TruncateAsync(null!));
        await Assert.ThrowsAsync<ArgumentNullException>(() => connection.TruncateAsync(null!, true, false));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => connection.TruncateAsync("table",
            commandOptions: new() { TimeoutSeconds = -1 }));
        using var other = new SqliteConnection("Data Source=:memory:");
        await other.OpenAsync(CancellationToken);
        using var transaction = other.BeginTransaction();
        await Assert.ThrowsAsync<ArgumentException>(() => connection.TruncateAsync("table",
            commandOptions: new() { Transaction = transaction }));
        Assert.Equal(ConnectionState.Closed, connection.State);
    }

    private sealed class MappedRow
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int Value { get; set; }
    }

    private sealed class MappingSource(string tableName, string? schema) : IEntityMappingSource
    {
        private readonly EntityMapping _mapping = new(typeof(MappedRow), tableName,
            DapperHelper.GetEntityMapping(typeof(MappedRow)).Properties, schema);

        public EntityMapping GetMapping(Type entityType)
            => entityType == _mapping.EntityType ? _mapping : throw new ArgumentException("Unexpected entity type.", nameof(entityType));
    }
}
