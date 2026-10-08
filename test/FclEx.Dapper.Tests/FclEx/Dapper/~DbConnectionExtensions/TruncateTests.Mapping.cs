// ReSharper disable UseAwaitUsing
namespace FclEx.Dapper;

public partial class TruncateTests
{
    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task TruncateAsync_UsesMappedTableAndSchemaOverrides(DbDriver driver, string? schema)
    {
        Assert.SkipMySql(driver);

        using var session = await CreateSessionAsync(driver, schema, typeof(EntityWithAutoKey), typeof(HasTableAttributeEntity));
        await using var cleanup = CleanupRows(session);
        var connection = session.Connection;
        schema = session.Schema;
        var options = new CommandOptions
        {
            EntityMappingSource = new MappingSource(session.GetTableName(typeof(EntityWithAutoKey)), schema),
            TimeoutSeconds = 10,
        };
        await VerifyRowsRemovedAsync(session,
            token => connection.TruncateAsync<MappedRow>(commandOptions: options, cancellationToken: token));
        var restart = driver != DbDriver.Oracle;
        var cascade = driver is DbDriver.Npgsql or DbDriver.Oracle;
        await VerifyRowsRemovedAsync(session,
            token => connection.TruncateAsync<MappedRow>(restart, cascade, commandOptions: options, cancellationToken: token));
        var wrongSchema = options with { EntityMappingSource = new MappingSource(session.GetTableName(typeof(EntityWithAutoKey)), "missing_" + Guid.NewGuid().ToString("N")) };
        // Null retains the mapped schema, so only explicit-schema cases can override it.
        if (schema is not null)
            await VerifyRowsRemovedAsync(session,
                token => connection.TruncateAsync<MappedRow>(schema, wrongSchema, token));
        try
        {
            await DeleteAllAsync<HasTableAttributeEntity>(session);
            await connection.InsertAsync(new HasTableAttributeEntity(), schema, commandOptions: session.CommandOptions, cancellationToken: CancellationToken);
            Assert.Equal(1, await CountAsync<HasTableAttributeEntity>(session));
            await connection.TruncateAsync<HasTableAttributeEntity>(schema, commandOptions: session.CommandOptions, cancellationToken: CancellationToken);
            Assert.Equal(0, await CountAsync<HasTableAttributeEntity>(session));
        }
        finally
        {
            await DeleteAllAsync<HasTableAttributeEntity>(session);
        }
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task TruncateAsync_MissingLiteralTableNameDoesNotRemoveExistingRows(DbDriver driver, string? schema)
    {
        Assert.SkipMySql(driver);

        using var session = await CreateSessionAsync(driver, schema);
        schema = session.Schema;
        try
        {
            await SeedAsync(session);
            await Assert.ThrowsAnyAsync<DbException>(() => session.Connection.TruncateAsync("missing.'\";--", schema,
                cancellationToken: CancellationToken));
            using var closed = Fixture.CreateDbConnection(driver, schema == "pg_temp" ? null : schema);
            await Assert.ThrowsAnyAsync<DbException>(() => closed.TruncateAsync("missing.'\";--", schema,
                cancellationToken: CancellationToken));
            Assert.Equal(ConnectionState.Closed, closed.State);
            Assert.Equal(2, await CountAsync<EntityWithAutoKey>(session));
        }
        finally
        {
            await DeleteAllAsync<EntityWithAutoKey>(session);
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
