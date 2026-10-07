namespace FclEx.Dapper;

partial class DbConnectionExtensionsTests
{
    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task TableExistsAsync_ExistingTable_UsesMappingAndPreservesConnectionState(DbDriver dbDriver, string? schema)
    {
        using var connection = Fixture.CreateDbConnection(dbDriver, schema);
        Assert.True(await connection.TableExistsAsync<EntityWithAutoKey>(schema, cancellationToken: CancellationToken));
        var options = new CommandOptions { EntityMappingSource = new MetadataMappingSource(schema) };
        Assert.True(await connection.TableExistsAsync<MetadataMappedEntity>(commandOptions: options,
            cancellationToken: CancellationToken));
        Assert.Equal(ConnectionState.Closed, connection.State);

        await connection.OpenAsync(CancellationToken);
        Assert.True(await connection.TableExistsAsync(nameof(EntityWithAutoKey), schema, cancellationToken: CancellationToken));
        Assert.True(await connection.TableExistsAsync<HasTableAttributeEntity>(schema, cancellationToken: CancellationToken));
        Assert.Equal(ConnectionState.Open, connection.State);
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task TableExistsAsync_MissingOrLiteralIdentifier_ReturnsFalse(DbDriver dbDriver, string? schema)
    {
        using var connection = Fixture.CreateDbConnection(dbDriver, schema);
        foreach (var name in new[] { "missing_" + Guid.NewGuid().ToString("N"), "missing' OR 1=1 --", "other.EntityWithAutoKey" })
            Assert.False(await connection.TableExistsAsync(name, schema, cancellationToken: CancellationToken));
        Assert.Equal(ConnectionState.Closed, connection.State);
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task TableExistsAsync_ExplicitMissingSchema_FollowsAdapterSchemaSupport(DbDriver dbDriver, string? schema)
    {
        using var connection = Fixture.CreateDbConnection(dbDriver, schema);
        var supportsSchemas = DapperHelper.GetSqlAdapter(connection).SupportsSchemas;
        var exists = await connection.TableExistsAsync(nameof(EntityWithAutoKey), "missing_" + Guid.NewGuid().ToString("N"),
            cancellationToken: CancellationToken);

        Assert.Equal(!supportsSchemas, exists);
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task TableExistsAsync_View_ReturnsFalse(DbDriver dbDriver, string? schema)
    {
        using var connection = Fixture.CreateDbConnection(dbDriver, schema);
        var adapter = DapperHelper.GetSqlAdapter(connection);
        var view = "v_" + Guid.NewGuid().ToString("N");
        var quotedView = adapter.GetQuotedTableName(view);
        if (adapter.SupportsSchemas && schema is not null)
            quotedView = adapter.GetQuotedTableName(schema) + "." + quotedView;
        var table = DapperHelper.GetTableNameWithSchema(connection, schema, typeof(EntityWithAutoKey));
        await connection.ExecuteAsync($"CREATE VIEW {quotedView} AS SELECT {adapter.GetQuotedColumnName("Id")} FROM {table}",
            cancellationToken: CancellationToken);
        try
        {
            Assert.False(await connection.TableExistsAsync(view, schema, cancellationToken: CancellationToken));
        }
        finally
        {
            await connection.ExecuteAsync($"DROP VIEW {quotedView}", cancellationToken: CancellationToken.None);
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task TableExistsAsync_InvalidNames_ThrowsBeforeOpeningConnection(string name)
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await Assert.ThrowsAsync<ArgumentException>(() => connection.TableExistsAsync(name));
        await Assert.ThrowsAsync<ArgumentException>(() => connection.TableExistsAsync("table", name));
        Assert.Equal(ConnectionState.Closed, connection.State);
    }

    [Fact]
    public async Task TableExistsAsync_NullArgumentsOrCancellation_ThrowsBeforeOpeningConnection()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await Assert.ThrowsAsync<ArgumentNullException>(() => ((DbConnection)null!).TableExistsAsync("table"));
        await Assert.ThrowsAsync<ArgumentNullException>(() => connection.TableExistsAsync(null!));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => connection.TableExistsAsync("table",
            cancellationToken: new CancellationToken(true)));
        Assert.Equal(ConnectionState.Closed, connection.State);
    }

    [Fact]
    public async Task TableExistsAsync_SqliteTransaction_SeesUncommittedTableAndPreservesTransaction()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(CancellationToken);
        using var transaction = connection.BeginTransaction();
        await connection.ExecuteAsync("CREATE TABLE \"CaseSensitive.Name\" (Id INTEGER)", transaction: transaction,
            cancellationToken: CancellationToken);

        var options = new CommandOptions { Transaction = transaction, TimeoutSeconds = 5 };
        Assert.True(await connection.TableExistsAsync("casesensitive.name", commandOptions: options,
            cancellationToken: CancellationToken));
        Assert.Equal(ConnectionState.Open, connection.State);
        transaction.Rollback();
        Assert.False(await connection.TableExistsAsync("CaseSensitive.Name", cancellationToken: CancellationToken));
    }

    private sealed class MetadataMappedEntity
    {
        public int Id { get; set; }
    }

    private sealed class MetadataMappingSource(string? schema) : IEntityMappingSource
    {
        private readonly EntityMapping _mapping = new(typeof(MetadataMappedEntity), nameof(EntityWithAutoKey),
            DapperHelper.GetEntityMapping(typeof(MetadataMappedEntity)).Properties, schema);

        public EntityMapping GetMapping(Type entityType)
            => entityType == _mapping.EntityType ? _mapping : throw new ArgumentException("Unexpected entity type.", nameof(entityType));
    }
}
