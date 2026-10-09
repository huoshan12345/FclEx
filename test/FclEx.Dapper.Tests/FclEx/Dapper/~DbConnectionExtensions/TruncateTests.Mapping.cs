// ReSharper disable UseAwaitUsing
namespace FclEx.Dapper;

public partial class TruncateTests
{
    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task TruncateAsync_UsesMappedTableAndSchemaOverrides(DbDriver driver, string? schema)
    {
        Assert.SkipMySql(driver);

        using var session = await CreateSessionAsync(driver, schema, typeof(TruncateRow), typeof(TruncateAttributedRow));
        await using var cleanup = CleanupRows(session);
        var connection = session.Connection;
        schema = session.Schema;
        var options = new CommandOptions
        {
            EntityMappingSource = new MappingSource(session.GetTableName(typeof(TruncateRow)), schema),
            TimeoutSeconds = 10,
        };
        await VerifyRowsRemovedAsync(session,
            token => connection.TruncateAsync<MappedRow>(commandOptions: options, cancellationToken: token));
        var restart = driver != DbDriver.Oracle;
        var cascade = driver is DbDriver.Npgsql or DbDriver.Oracle;
        await VerifyRowsRemovedAsync(session,
            token => connection.TruncateAsync<MappedRow>(restart, cascade, commandOptions: options, cancellationToken: token));
        var wrongSchema = options with { EntityMappingSource = new MappingSource(session.GetTableName(typeof(TruncateRow)), "missing_" + Guid.NewGuid().ToString("N")) };
        // Null retains the mapped schema, so only explicit-schema cases can override it.
        if (schema is not null)
            await VerifyRowsRemovedAsync(session,
                token => connection.TruncateAsync<MappedRow>(schema, wrongSchema, token));
        try
        {
            await DeleteAllAsync<TruncateAttributedRow>(session);
            await connection.InsertAsync(new TruncateAttributedRow(), schema, cancellationToken: CancellationToken);
            Assert.Equal(1, await CountAsync<TruncateAttributedRow>(session));
            await connection.TruncateAsync<TruncateAttributedRow>(schema, cancellationToken: CancellationToken);
            Assert.Equal(0, await CountAsync<TruncateAttributedRow>(session));
        }
        finally
        {
            await DeleteAllAsync<TruncateAttributedRow>(session);
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
            using var closed = Fixture.CreateDbConnection(driver, schema);
            await Assert.ThrowsAnyAsync<DbException>(() => closed.TruncateAsync("missing.'\";--", schema,
                cancellationToken: CancellationToken));
            Assert.Equal(ConnectionState.Closed, closed.State);
            Assert.Equal(2, await CountAsync<TruncateRow>(session));
        }
        finally
        {
            await DeleteAllAsync<TruncateRow>(session);
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
        await Assert.ThrowsAsync<ArgumentException>(() => connection.TruncateAsync<TruncateRow>(invalid));
        Assert.Equal(ConnectionState.Closed, connection.State);
    }

    [Fact]
    public async Task TruncateAsync_ValidatesNullArgumentsAndCommandOptions()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await Assert.ThrowsAsync<ArgumentNullException>(() => ((DbConnection)null!).TruncateAsync("table"));
        await Assert.ThrowsAsync<ArgumentNullException>(() => ((DbConnection)null!).TruncateAsync<TruncateRow>());
        await Assert.ThrowsAsync<ArgumentNullException>(() => ((DbConnection)null!).TruncateAsync<TruncateRow>(true, false));
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
