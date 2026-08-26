using System.Data.Common;

namespace FclEx.Dapper;

public class ConnectionOwnershipTests
{
    [Fact]
    public async Task CrudAsync_RestoresInitialConnectionState()
    {
        using var database = await SqliteMigrationTestDatabase.CreateAsync();
        using var connection = database.CreateConnection();
        var first = new SqliteMigrationTestEntity { Name = "first", Value = 1 };

        var firstId = await connection.InsertAsync<SqliteMigrationTestEntity, long>(first);
        Assert.Equal(ConnectionState.Closed, connection.State);

        var persisted = await connection.GetAsync<SqliteMigrationTestEntity>(firstId);
        Assert.Equal(ConnectionState.Closed, connection.State);
        Assert.Equal(first.Name, persisted?.Name);

        await connection.BulkInsertAsync(
            [
                new SqliteMigrationTestEntity { Name = "second", Value = 2 },
                new SqliteMigrationTestEntity { Name = "third", Value = 3 },
            ]);
        Assert.Equal(ConnectionState.Closed, connection.State);

        await connection.DeleteAsync<SqliteMigrationTestEntity>(firstId);
        Assert.Equal(ConnectionState.Closed, connection.State);

        await connection.OpenAsync();
        await connection.InsertAsync<SqliteMigrationTestEntity, long>(new SqliteMigrationTestEntity { Name = "open", Value = 4 });
        Assert.Equal(ConnectionState.Open, connection.State);
        await connection.BulkInsertAsync(
            [new SqliteMigrationTestEntity { Name = "open-bulk", Value = 5 }]);
        Assert.Equal(ConnectionState.Open, connection.State);
    }

    [Fact]
    public async Task InsertAsync_ExceptionOrCancellation_ClosesConnectionOpenedByOperation()
    {
        using var database = await SqliteMigrationTestDatabase.CreateAsync();
        using var connection = database.CreateConnection();

        await Assert.ThrowsAnyAsync<DbException>(() =>
            connection.InsertAsync<MissingTableEntity, object>(new MissingTableEntity { Value = 1 }));
        Assert.Equal(ConnectionState.Closed, connection.State);

        var commandOptions = new CommandOptions { CancellationToken = new(true) };
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => connection.InsertAsync<SqliteMigrationTestEntity, long>(
            new SqliteMigrationTestEntity { Name = "cancelled", Value = 2 },
            commandOptions: commandOptions));
        Assert.Equal(ConnectionState.Closed, connection.State);
    }

    [Fact]
    public async Task DoTransactionAsync_RestoresInitialConnectionStateOnEveryExitPath()
    {
        using var database = await SqliteMigrationTestDatabase.CreateAsync();
        using var connection = database.CreateConnection();

        await connection.DoTransactionAsync(_ => Task.CompletedTask);
        Assert.Equal(ConnectionState.Closed, connection.State);

        await Assert.ThrowsAsync<InvalidOperationException>(() => connection.DoTransactionAsync(_ =>
            Task.FromException(new InvalidOperationException("expected"))));
        Assert.Equal(ConnectionState.Closed, connection.State);

        using var cancellationSource = new CancellationTokenSource();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => connection.DoTransactionAsync(
            (_, _) =>
            {
                cancellationSource.Cancel();
                return Task.CompletedTask;
            },
            cancellationToken: cancellationSource.Token));
        Assert.Equal(ConnectionState.Closed, connection.State);

        await connection.OpenAsync();
        await connection.DoTransactionAsync(_ => Task.CompletedTask);
        Assert.Equal(ConnectionState.Open, connection.State);
    }

    [Table("missing_connection_ownership_table")]
    private sealed class MissingTableEntity
    {
        public int Value { get; set; }
    }
}
