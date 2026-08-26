using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace FclEx.Dapper;

public class CommandOptionsTests
{
    [Fact]
    public void ValidateFor_NegativeTimeout_Throws()
    {
        using var connection = new SqliteConnection();
        var options = new CommandOptions { TimeoutSeconds = -1 };

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => options.ValidateFor(connection));

        Assert.Equal(nameof(CommandOptions.TimeoutSeconds), exception.ParamName);
    }

    [Fact]
    public void ValidateFor_TransactionWithoutConnection_Throws()
    {
        using var connection = new SqliteConnection();
        using var transaction = new DetachedTransaction();
        var options = new CommandOptions { Transaction = transaction };

        Assert.Throws<InvalidOperationException>(() => options.ValidateFor(connection));
    }

    [Fact]
    public async Task ValidateFor_TransactionFromAnotherConnection_Throws()
    {
        using var transactionConnection = new SqliteConnection("Data Source=:memory:");
        await transactionConnection.OpenAsync();
        using var otherConnection = new SqliteConnection("Data Source=:memory:");
        await otherConnection.OpenAsync();
        using var transaction = transactionConnection.BeginTransaction();
        var options = new CommandOptions { Transaction = transaction };

        var exception = Assert.Throws<ArgumentException>(() => options.ValidateFor(otherConnection));

        Assert.Equal(nameof(CommandOptions.Transaction), exception.ParamName);
    }

    [Fact]
    public async Task ValidateFor_TransactionFromSameConnection_Succeeds()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();
        var options = new CommandOptions { Transaction = transaction };

        options.ValidateFor(connection);
    }

    [Fact]
    public async Task CrudAsync_InvalidTimeout_UsesCommandOptionsValidation()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        var options = new CommandOptions { TimeoutSeconds = -1 };

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            connection.InsertAsync<OptionsEntity, long>(new OptionsEntity(), commandOptions: options));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            connection.BulkInsertAsync([new OptionsEntity()], commandOptions: options));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            connection.GetAsync<OptionsEntity>(1, commandOptions: options));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            connection.DeleteAsync<OptionsEntity>(1, commandOptions: options));
    }

    private sealed class DetachedTransaction : DbTransaction
    {
        public override IsolationLevel IsolationLevel => IsolationLevel.Unspecified;

        protected override DbConnection DbConnection => null!;

        public override void Commit()
        {
            throw new NotSupportedException();
        }

        public override void Rollback()
        {
            throw new NotSupportedException();
        }
    }

    private sealed class OptionsEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
    }
}
