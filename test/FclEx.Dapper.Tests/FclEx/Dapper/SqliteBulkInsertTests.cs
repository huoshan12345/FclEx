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

        var first = DbConnectionExtensions.GetInsertCommandText(
            SqliteAdapter.Instance,
            null,
            mapping,
            false,
            false,
            499);
        var second = DbConnectionExtensions.GetInsertCommandText(
            SqliteAdapter.Instance,
            null,
            mapping,
            false,
            false,
            499);

        Assert.Same(first, second);
    }

    [Fact]
    public async Task InsertAsync_OnlyGeneratedKey_UsesDefaultValues()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await connection.ExecuteAsync(
            "CREATE TABLE default_rows (id INTEGER PRIMARY KEY AUTOINCREMENT);");

        var id = (long?)await connection.InsertAsync(new DefaultRow());

        Assert.NotNull(id);
        Assert.Equal(1, await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM default_rows"));
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
}
