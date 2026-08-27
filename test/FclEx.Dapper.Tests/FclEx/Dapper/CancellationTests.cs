namespace FclEx.Dapper;

public class CancellationTests
{
    [Fact]
    public async Task CrudAsync_PreCanceledCommandOptions_CancelsWithoutChangingRows()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await connection.ExecuteAsync(
            "CREATE TABLE cancellable_rows (id INTEGER PRIMARY KEY, name TEXT NOT NULL);" +
            "INSERT INTO cancellable_rows (id, name) VALUES (1, 'one');");
        var commandOptions = new CommandOptions { CancellationToken = new(true) };

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => connection.InsertAsync<CancellableRow, object>(
            new CancellableRow { Id = 2, Name = "two" },
            returnGeneratedKey: false,
            commandOptions: commandOptions));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => connection.BulkInsertAsync(
            [new CancellableRow { Id = 3, Name = "three" }],
            includeAutoKey: true,
            commandOptions: commandOptions));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            connection.GetAsync<CancellableRow>(1, commandOptions: commandOptions));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            connection.DeleteAsync<CancellableRow>(1, commandOptions: commandOptions));

        Assert.Equal(1, await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM cancellable_rows"));
    }

    [Fact]
    public async Task TransactionCrudAsync_PreCanceledToken_IsForwarded()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await connection.ExecuteAsync(
            "CREATE TABLE cancellable_rows (id INTEGER PRIMARY KEY, name TEXT NOT NULL);" +
            "INSERT INTO cancellable_rows (id, name) VALUES (1, 'one');");
        using var transaction = connection.BeginTransaction();
        var cancellationToken = new CancellationToken(true);
        var commandOptions = new CommandOptions { CancellationToken = cancellationToken };

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            transaction.GetAsync<CancellableRow>(1, commandOptions: commandOptions));
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

    [Table("cancellable_rows")]
    private sealed class CancellableRow
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; } = "";
    }
}
