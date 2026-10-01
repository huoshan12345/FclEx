using System.Data.Common;

namespace FclEx.Dapper;

public class CancellationTests
{
    [Theory]
    [InlineData(CrudOperation.InsertWithKey)]
    [InlineData(CrudOperation.InsertWithLongKey)]
    [InlineData(CrudOperation.InsertWithExplicitGeneratedKeys)]
    [InlineData(CrudOperation.BulkInsert)]
    [InlineData(CrudOperation.Get)]
    [InlineData(CrudOperation.Delete)]
    public async Task CrudAsync_PreCanceledToken_CancelsWithoutChangingRows(CrudOperation operation)
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await connection.ExecuteAsync(
            "CREATE TABLE cancellable_rows (id INTEGER PRIMARY KEY, name TEXT NOT NULL);" +
            "INSERT INTO cancellable_rows (id, name) VALUES (1, 'one');");
        var commandOptions = new CommandOptions { TimeoutSeconds = 17 };
        var cancellationToken = new CancellationToken(true);
        var entity = new CancellableRow { Id = 2, Name = "two" };

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operation switch
        {
            CrudOperation.InsertWithKey => connection.InsertAsync<CancellableRow, int>(
                entity, commandOptions: commandOptions, cancellationToken: cancellationToken),
            CrudOperation.InsertWithLongKey => connection.InsertAsync(
                entity, commandOptions: commandOptions, cancellationToken: cancellationToken),
            CrudOperation.InsertWithExplicitGeneratedKeys => connection.InsertWithExplicitGeneratedKeysAsync(
                entity, commandOptions: commandOptions, cancellationToken: cancellationToken),
            CrudOperation.BulkInsert => connection.BulkInsertAsync(
                [entity], includeAutoKey: true, commandOptions: commandOptions, cancellationToken: cancellationToken),
            CrudOperation.Get => connection.GetAsync<CancellableRow>(
                1, commandOptions: commandOptions, cancellationToken: cancellationToken),
            CrudOperation.Delete => connection.DeleteAsync<CancellableRow>(
                1, commandOptions: commandOptions, cancellationToken: cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(operation)),
        });

        Assert.Equal(1, await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM cancellable_rows"));
        // Reusing options with the default token must not carry cancellation from the previous call.
        var existing = await connection.GetAsync<CancellableRow>(1, commandOptions: commandOptions);
        Assert.Equal("one", existing?.Name);
    }

    [Theory]
    [InlineData(CrudOperation.InsertWithKey)]
    [InlineData(CrudOperation.InsertWithLongKey)]
    [InlineData(CrudOperation.InsertWithExplicitGeneratedKeys)]
    [InlineData(CrudOperation.BulkInsert)]
    [InlineData(CrudOperation.Get)]
    [InlineData(CrudOperation.Delete)]
    public async Task TransactionCrudAsync_PreCanceledToken_IsForwarded(CrudOperation operation)
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await connection.ExecuteAsync(
            "CREATE TABLE cancellable_rows (id INTEGER PRIMARY KEY, name TEXT NOT NULL);" +
            "INSERT INTO cancellable_rows (id, name) VALUES (1, 'one');");
        using var transaction = connection.BeginTransaction();
        var cancellationToken = new CancellationToken(true);
        var commandOptions = new CommandOptions { TimeoutSeconds = 17 };
        var entity = new CancellableRow { Id = 2, Name = "two" };

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operation switch
        {
            CrudOperation.InsertWithKey => transaction.InsertAsync<CancellableRow, int>(
                entity, commandOptions: commandOptions, cancellationToken: cancellationToken),
            CrudOperation.InsertWithLongKey => transaction.InsertAsync(
                entity, commandOptions: commandOptions, cancellationToken: cancellationToken),
            CrudOperation.InsertWithExplicitGeneratedKeys => transaction.InsertWithExplicitGeneratedKeysAsync(
                entity, commandOptions: commandOptions, cancellationToken: cancellationToken),
            CrudOperation.BulkInsert => transaction.BulkInsertAsync(
                [entity], includeAutoKey: true, commandOptions: commandOptions, cancellationToken: cancellationToken),
            CrudOperation.Get => transaction.GetAsync<CancellableRow>(
                1, commandOptions: commandOptions, cancellationToken: cancellationToken),
            CrudOperation.Delete => transaction.DeleteAsync<CancellableRow>(
                1, commandOptions: commandOptions, cancellationToken: cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(operation)),
        });

        var existing = await transaction.GetAsync<CancellableRow>(1, commandOptions: commandOptions);
        Assert.Equal("one", existing?.Name);
        Assert.Null(await transaction.GetAsync<CancellableRow>(2, commandOptions: commandOptions));
        Assert.Null(commandOptions.Transaction);
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_CancelledInCallback_RollsBackInsteadOfCommitting()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await connection.ExecuteAsync(
            "CREATE TABLE cancellable_rows (id INTEGER PRIMARY KEY, name TEXT NOT NULL);");
        using var cancellationSource = new CancellationTokenSource();
        var receivedToken = default(CancellationToken);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => connection.ExecuteInTransactionAsync(
            async (transaction, cancellationToken) =>
            {
                receivedToken = cancellationToken;
                await transaction.Connection!.ExecuteAsync(new CommandDefinition(
                    "INSERT INTO cancellable_rows (id, name) VALUES (1, 'one')",
                    transaction: transaction,
                    cancellationToken: cancellationToken));
                cancellationSource.Cancel();
            },
            cancellationToken: cancellationSource.Token));

        Assert.Equal(cancellationSource.Token, receivedToken);
        Assert.Equal(0, await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM cancellable_rows"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task BulkInsertAsync_CancelBeforeSecondBatch_StopsFurtherInserts(bool throughTransaction)
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await connection.ExecuteAsync(
            "CREATE TABLE cancellable_rows (id INTEGER PRIMARY KEY, name TEXT NOT NULL);");
        using var transaction = throughTransaction ? connection.BeginTransaction() : null;
        using var cancellationSource = new CancellationTokenSource();
        var options = new CommandOptions { SqlAdapter = new CancelAtSecondBatchAdapter(cancellationSource) };
        var entities = Enumerable.Range(0, 501)
            .Select(index => new CancellableRow { Name = $"row-{index}" })
            .ToArray();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => throughTransaction
            ? transaction!.BulkInsertAsync(
                entities, commandOptions: options, cancellationToken: cancellationSource.Token)
            : connection.BulkInsertAsync(
                entities, commandOptions: options, cancellationToken: cancellationSource.Token));

        // The first batch completes before cancellation; the second must insert no rows.
        Assert.Equal(500, await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM cancellable_rows", transaction: transaction));
        Assert.Equal(0, await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM cancellable_rows WHERE name = 'row-500'", transaction: transaction));
        Assert.Equal(ConnectionState.Open, connection.State);
    }

    private sealed class CancelAtSecondBatchAdapter(CancellationTokenSource cancellationSource) : SqliteAdapter
    {
        public override DbParameter CreateParameter(string name, object? value, string? storeTypeName = null)
        {
            if (value is "row-500")
            {
                cancellationSource.Cancel();
            }

            return base.CreateParameter(name, value, storeTypeName);
        }
    }

    public enum CrudOperation
    {
        InsertWithKey,
        InsertWithLongKey,
        InsertWithExplicitGeneratedKeys,
        BulkInsert,
        Get,
        Delete,
    }

    [Table("cancellable_rows")]
    private sealed class CancellableRow
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; } = "";
    }
}
