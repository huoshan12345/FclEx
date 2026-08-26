using FclEx.Dapper.SqlAdapters;
using Microsoft.Data.Sqlite;

namespace FclEx.Dapper;

public class SqliteBulkInsertTests
{
    [Fact]
    public async Task BulkInsertAsync_MoreParametersThanOneCommand_SplitsIntoBatches()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await connection.ExecuteAsync(
            "CREATE TABLE batch_rows (name TEXT NOT NULL, value INTEGER NOT NULL);");
        var entities = Enumerable.Range(1, 501)
            .Select(value => new BatchRow { Name = $"row-{value}", Value = value })
            .ToArray();

        var affectedRows = await connection.BulkInsertAsync(entities);

        Assert.Equal(entities.Length, affectedRows);
        Assert.Equal(entities.Length, await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM batch_rows"));
    }

    [Fact]
    public void GetInsertCommandText_SameInputs_ReturnsCachedCommandText()
    {
        var mapping = DapperHelper.GetEntityMapping(typeof(BatchRow));
        var adapter = new SqliteAdapter();

        var first = DbConnectionExtensions.GetInsertCommandText(
            adapter,
            null,
            mapping,
            false,
            false,
            499,
            true);
        var second = DbConnectionExtensions.GetInsertCommandText(
            adapter,
            null,
            mapping,
            false,
            false,
            499,
            true);

        Assert.Same(first, second);
    }

    [Fact]
    public void GetInsertCommandText_SchemaOverride_DoesNotEnterGlobalCache()
    {
        var mapping = CreateBatchRowMapping();
        var adapter = new NpgsqlAdapter();
        var key = new InsertSqlKey(adapter, mapping, false, false, 1);

        var sql = DbConnectionExtensions.GetInsertCommandText(
            adapter,
            "tenant_cache_test",
            mapping,
            false,
            false,
            1,
            false);

        Assert.Contains("\"tenant_cache_test\".\"batch_rows\"", sql);
        Assert.False(DbConnectionExtensions.InsertSqls.ContainsKey(key));
    }

    [Fact]
    public async Task BulkInsertAsync_AdapterOverride_ReusesCommandTextWithinCall()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await connection.ExecuteAsync(
            "CREATE TABLE batch_rows (name TEXT NOT NULL, value INTEGER NOT NULL);");
        var entities = Enumerable.Range(1, 5)
            .Select(value => new BatchRow { Name = $"row-{value}", Value = value })
            .ToArray();
        var adapter = new TwoRowSqliteAdapter();

        var affectedRows = await connection.BulkInsertAsync(
            entities,
            commandInfo: new(SqlAdapter: adapter));

        Assert.Equal(entities.Length, affectedRows);
        Assert.Equal(2, adapter.BuildCount);
        Assert.DoesNotContain(
            DbConnectionExtensions.InsertSqls.Keys,
            key => ReferenceEquals(key.SqlAdapter, adapter));
    }

    [Fact]
    public void GetParameterName_UsesBoundedPositionsInsteadOfPropertyNames()
    {
        var first = DbConnectionExtensions.GetParameterName(3, 4);
        var second = DbConnectionExtensions.GetParameterName(3, 4);

        Assert.Equal("@p3_4", first);
        Assert.Same(first, second);
    }

    [Fact]
    public async Task CrudAsync_AdapterOverride_DoesNotEnterGlobalCaches()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await connection.ExecuteAsync(
            "CREATE TABLE cache_rows (id INTEGER PRIMARY KEY, name TEXT NOT NULL);" +
            "INSERT INTO cache_rows (id, name) VALUES (1, 'one');");
        var adapter = new SqliteAdapter();
        var commandInfo = new CommandInfo(SqlAdapter: adapter);
        var mapping = DapperHelper.GetEntityMapping(typeof(CacheRow));

        var insertedId = await connection.InsertAsync<CacheRow, object>(
            new CacheRow { Id = 2, Name = "two" },
            includeAutoKey: true,
            commandInfo: commandInfo);
        var row = await connection.GetAsync<CacheRow>(1, commandInfo: commandInfo);
        var deleted = await connection.DeleteAsync<CacheRow>(1, commandInfo: commandInfo);

        Assert.Null(insertedId);
        Assert.Equal("one", row?.Name);
        Assert.Equal(1, deleted);
        Assert.DoesNotContain(
            DbConnectionExtensions.InsertSqls.Keys,
            key => ReferenceEquals(key.SqlAdapter, adapter));
        Assert.DoesNotContain(
            DbConnectionExtensions.GetSqls.Keys,
            key => ReferenceEquals(key.SqlAdapter, adapter));
        Assert.DoesNotContain(
            DbConnectionExtensions.DeleteSqls.Keys,
            key => ReferenceEquals(key.SqlAdapter, adapter));
    }

    [Fact]
    public async Task InsertAsync_OnlyGeneratedKey_UsesDefaultValues()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await connection.ExecuteAsync(
            "CREATE TABLE default_rows (id INTEGER PRIMARY KEY AUTOINCREMENT);");

        var id = await connection.InsertAsync<DefaultRow, long>(new DefaultRow());

        Assert.Equal(1, id);
        Assert.Equal(1, await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM default_rows"));
    }

    [Fact]
    public async Task InsertAsync_ProviderScalar_IsConvertedToRequestedKeyType()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await connection.ExecuteAsync(
            "CREATE TABLE int_key_rows (id INTEGER PRIMARY KEY AUTOINCREMENT);");

        var id = await connection.InsertAsync<IntKeyRow, int>(new IntKeyRow());

        Assert.Equal(1, id);
    }

    [Fact]
    public async Task TransactionInsertAsync_ReturnsRequestedKeyType()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await connection.ExecuteAsync(
            "CREATE TABLE int_key_rows (id INTEGER PRIMARY KEY AUTOINCREMENT);");
        using var transaction = connection.BeginTransaction();

        var id = await transaction.InsertAsync<IntKeyRow, int>(new IntKeyRow());
        transaction.Commit();

        Assert.Equal(1, id);
    }

    [Fact]
    public async Task BulkInsertAsync_MultipleDefaultRows_ThrowsWithoutInserting()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await connection.ExecuteAsync(
            "CREATE TABLE default_rows (id INTEGER PRIMARY KEY AUTOINCREMENT);");

        await Assert.ThrowsAsync<NotSupportedException>(() =>
            connection.BulkInsertAsync([new DefaultRow(), new DefaultRow()]));

        Assert.Equal(0, await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM default_rows"));
    }

    [Table("batch_rows")]
    private sealed class BatchRow
    {
        [Column("name")]
        public string Name { get; set; } = "";

        [Column("value")]
        public int Value { get; set; }
    }

    [Table("default_rows")]
    private sealed class DefaultRow
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
    }

    [Table("int_key_rows")]
    private sealed class IntKeyRow
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
    }

    [Table("cache_rows")]
    private sealed class CacheRow
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; } = "";
    }

    private static EntityMapping CreateBatchRowMapping()
    {
        return new(
            typeof(BatchRow),
            "batch_rows",
            [
                new(typeof(BatchRow).GetProperty(nameof(BatchRow.Name))!, "name"),
                new(typeof(BatchRow).GetProperty(nameof(BatchRow.Value))!, "value"),
            ]);
    }

    private sealed class TwoRowSqliteAdapter : SqliteAdapter
    {
        public int BuildCount { get; private set; }

        public override int GetMaxInsertBatchSize(int parameterCountPerRow)
        {
            return 2;
        }

        public override string BuildInsertCommandText(
            string quotedTableName,
            string? columnListSql,
            string? valueRowsSql,
            string? quotedGeneratedKeyColumn)
        {
            BuildCount++;
            return base.BuildInsertCommandText(
                quotedTableName,
                columnListSql,
                valueRowsSql,
                quotedGeneratedKeyColumn);
        }
    }
}
