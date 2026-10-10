namespace FclEx.Dapper;

public class IdentitySequenceTests(DapperTestsFixture fixture) : DapperTests(fixture)
{
    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task ReseedIdentityAsync_ExplicitKeys_NextGeneratedKeyExceedsMaximum(DbDriver driver, string? schema)
    {
        using var session = await Fixture.TruncateSessions.CreateSessionAsync(driver, schema, [typeof(TruncateRow)]);
        var connection = session.Connection;
        var table = session.GetQualifiedTableName(typeof(TruncateRow));
        await connection.ExecuteAsync($"DELETE FROM {table}");
        await using var cleanup = AsyncDisposable.Create(() => connection.ExecuteAsync($"DELETE FROM {table}"));

        var initial = await connection.InsertAsync<TruncateRow, int>(new() { Value = 1 }, schema);
        var maximum = checked(initial + 100);
        await connection.InsertWithExplicitGeneratedKeysAsync(new TruncateRow { Id = maximum, Value = 2 }, schema);

        Assert.True(await connection.ReseedIdentityAsync<TruncateRow>(schema));
        Assert.Equal(ConnectionState.Open, connection.State);
        var next = await connection.InsertAsync<TruncateRow, int>(new() { Value = 3 }, schema);
        Assert.True(next > maximum, $"Next key {next} must exceed {maximum}.");
        if (driver is DbDriver.Npgsql or DbDriver.SqlServer or DbDriver.Sqlite)
            Assert.Equal(maximum + 1, next);

        // A remapped CLR key name and explicit schema must use the physical identity column.
        using var closed = Fixture.CreateDbConnection(driver, schema);
        Assert.True(await closed.ReseedIdentityAsync<RenamedIdentity>(schema));
        Assert.Equal(ConnectionState.Closed, closed.State);
        Assert.True(await connection.ReseedIdentityAsync(session.GetTableName(typeof(TruncateRow)), "Id", schema));
        Assert.Equal(3, await connection.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM {table}"));
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task ReseedIdentityAsync_EmptyTable_AllowsGeneratedInsert(DbDriver driver, string? schema)
    {
        using var session = await Fixture.TruncateSessions.CreateSessionAsync(driver, schema, [typeof(TruncateRow)]);
        var connection = session.Connection;
        var table = session.GetQualifiedTableName(typeof(TruncateRow));
        await connection.ExecuteAsync($"DELETE FROM {table}");
        await using var cleanup = AsyncDisposable.Create(() => connection.ExecuteAsync($"DELETE FROM {table}"));
        await connection.InsertAsync(new TruncateRow { Value = 1 }, schema);
        await connection.ExecuteAsync($"DELETE FROM {table}");

        Assert.True(await connection.ReseedIdentityAsync<TruncateRow>(schema));
        var key = await connection.InsertAsync<TruncateRow, int>(new() { Value = 2 }, schema);
        Assert.True(key > 0);
        if (driver is DbDriver.Npgsql or DbDriver.SqlServer or DbDriver.Sqlite)
            Assert.Equal(1, key);
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task ReseedIdentityAsync_MissingTableOrIdentity_ReturnsFalse(DbDriver driver, string? schema)
    {
        using var connection = Fixture.CreateDbConnection(driver, schema);
        Assert.False(await connection.ReseedIdentityAsync("missing_identity_" + Guid.NewGuid().ToString("N"), "Id", schema));
        Assert.Equal(ConnectionState.Closed, connection.State);
        Assert.False(await connection.ReseedIdentityAsync(nameof(TruncateManualRow), "Id", schema));
        Assert.False(await connection.ReseedIdentityAsync(nameof(TruncateRow), "Value", schema));
        Assert.False(await connection.ReseedIdentityAsync(nameof(TruncateRow), "missing_identity_column", schema));
        Assert.Equal(ConnectionState.Closed, connection.State);
    }

    [Fact]
    public async Task ReseedIdentityAsync_InvalidMappingOrArguments_RejectsBeforeOpening()
    {
        using var connection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=:memory:");
        await Assert.ThrowsAsync<DataException>(() => connection.ReseedIdentityAsync<TruncateManualRow>());
        await Assert.ThrowsAsync<DataException>(() => connection.ReseedIdentityAsync<EntityWithGuidKey>());
        await Assert.ThrowsAsync<ArgumentNullException>(() => connection.ReseedIdentityAsync(null!, "Id"));
        await Assert.ThrowsAsync<ArgumentException>(() => connection.ReseedIdentityAsync(" ", "Id"));
        await Assert.ThrowsAsync<ArgumentException>(() => connection.ReseedIdentityAsync("Row", " "));
        await Assert.ThrowsAsync<ArgumentException>(() => connection.ReseedIdentityAsync("Row", "Id", " "));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => connection.ReseedIdentityAsync("Row", "Id",
            commandOptions: new() { TimeoutSeconds = -1 }));
        using var canceled = new CancellationTokenSource();
        canceled.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => connection.ReseedIdentityAsync<TruncateRow>(
            cancellationToken: canceled.Token));
        Assert.Equal(ConnectionState.Closed, connection.State);
    }

    [Fact]
    public async Task ReseedIdentityAsync_SqliteTempTable_RollbackPreservesBothSequences()
    {
        using var connection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await connection.ExecuteAsync("""
            CREATE TABLE main.Row (Id INTEGER PRIMARY KEY AUTOINCREMENT);
            CREATE TEMP TABLE Row (Id INTEGER PRIMARY KEY AUTOINCREMENT);
            INSERT INTO main.Row VALUES (100);
            INSERT INTO temp.Row VALUES (200);
            DELETE FROM temp.Row;
            INSERT INTO temp.Row VALUES (10);
            """);
        using (var transaction = connection.BeginTransaction())
        {
            Assert.True(await connection.ReseedIdentityAsync("Row", "Id", commandOptions: new() { Transaction = transaction }));
            Assert.Equal(10, await connection.ExecuteScalarAsync<int>("SELECT seq FROM temp.sqlite_sequence WHERE name = 'Row'", transaction: transaction));
            transaction.Rollback();
        }
        Assert.Equal(200, await connection.ExecuteScalarAsync<int>("SELECT seq FROM temp.sqlite_sequence WHERE name = 'Row'"));
        Assert.Equal(100, await connection.ExecuteScalarAsync<int>("SELECT seq FROM main.sqlite_sequence WHERE name = 'Row'"));
        Assert.True(await connection.ReseedIdentityAsync("Row", "Id"));
        await connection.ExecuteAsync("INSERT INTO temp.Row DEFAULT VALUES");
        Assert.Equal(11, await connection.ExecuteScalarAsync<int>("SELECT MAX(Id) FROM temp.Row"));
        Assert.Equal(100, await connection.ExecuteScalarAsync<int>("SELECT seq FROM main.sqlite_sequence WHERE name = 'Row'"));
    }

    [Fact]
    public async Task ReseedIdentityAsync_QuotedMappingAndInt64Key_UsesPhysicalNames()
    {
        using var connection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var adapter = DapperHelper.GetSqlAdapter(connection);
        var table = adapter.GetTableNameWithSchema(null, typeof(QuotedIdentity));
        var column = adapter.GetQuotedColumnName<QuotedIdentity>(row => row.Number);
        await connection.ExecuteAsync($"CREATE TABLE {table} ({column} INTEGER PRIMARY KEY AUTOINCREMENT)");
        const long maximum = (long)int.MaxValue + 100;
        await connection.InsertWithExplicitGeneratedKeysAsync(new QuotedIdentity { Number = maximum + 100 });
        await connection.ExecuteAsync($"DELETE FROM {table}");
        await connection.InsertWithExplicitGeneratedKeysAsync(new QuotedIdentity { Number = maximum });
        Assert.True(await connection.ReseedIdentityAsync<QuotedIdentity>());
        Assert.Equal(maximum + 1, await connection.InsertAsync<QuotedIdentity, long>(new()));
    }

    [Fact]
    public async Task ReseedIdentityAsync_SqliteUnusedIdentityAndOrdinaryRowId_DistinguishesGenerators()
    {
        using var connection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await connection.ExecuteAsync("""
            CREATE TABLE Fresh (Id INTEGER PRIMARY KEY AUTOINCREMENT);
            CREATE TABLE Plain (Id INTEGER PRIMARY KEY /* AUTOINCREMENT */);
            CREATE TABLE Quoted (Id INTEGER PRIMARY KEY, "AUTOINCREMENT" TEXT);
            """);
        Assert.True(await connection.ReseedIdentityAsync("Fresh", "Id"));
        Assert.False(await connection.ReseedIdentityAsync("Plain", "Id"));
        Assert.False(await connection.ReseedIdentityAsync("Quoted", "Id"));
        await connection.ExecuteAsync("INSERT INTO Fresh DEFAULT VALUES");
        Assert.Equal(1, await connection.ExecuteScalarAsync<int>("SELECT Id FROM Fresh"));
    }

    [Fact]
    public async Task ReseedIdentityAsync_TransactionFromOtherConnection_Throws()
    {
        using var connection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=:memory:");
        using var other = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=:memory:");
        await other.OpenAsync();
        using var transaction = other.BeginTransaction();
        await Assert.ThrowsAsync<ArgumentException>(() => connection.ReseedIdentityAsync("Row", "Id",
            commandOptions: new() { Transaction = transaction }));
        Assert.Equal(ConnectionState.Closed, connection.State);
    }

    [Table("identity ' \" table")]
    private sealed class QuotedIdentity
    {
        [Key, Column("key ' \" value"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Number { get; set; }
    }

    [Table(nameof(TruncateRow))]
    private sealed class RenamedIdentity
    {
        [Key, Column("Id"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int KeyNumber { get; set; }
    }
}
