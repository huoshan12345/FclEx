using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace FclEx.Dapper;

public class CommandDisposalTests
{
    private static readonly CommandOptions CommandOptions = new() { SqlAdapter = new SqliteAdapter() };

    [Fact]
    public async Task ExecuteAsync_Success_DisposesCommand()
    {
        using var connection = new TrackingConnection();

        var result = await connection.ExecuteAsync(
            CommandOptions,
            _ => CreateSqlInfo(),
            _ => Task.FromResult(42));

        Assert.Equal(42, result);
        Assert.True(connection.Command.IsDisposed);
    }

    [Fact]
    public async Task ExecuteAsync_OpenFailure_DisposesCommand()
    {
        var expected = new InvalidOperationException("open failed");
        using var connection = new TrackingConnection(expected);

        var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => connection.ExecuteAsync(
            CommandOptions,
            _ => CreateSqlInfo(),
            _ => Task.FromResult(42)));

        Assert.Same(expected, actual);
        Assert.True(connection.Command.IsDisposed);
    }

    [Fact]
    public async Task ExecuteAsync_ExecutionFailure_DisposesCommand()
    {
        var expected = new InvalidOperationException("execution failed");
        using var connection = new TrackingConnection();

        var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => connection.ExecuteAsync<int>(
            CommandOptions,
            _ => CreateSqlInfo(),
            _ => Task.FromException<int>(expected)));

        Assert.Same(expected, actual);
        Assert.True(connection.Command.IsDisposed);
    }

    private static SqlInfo CreateSqlInfo()
    {
        return new SqlInfo("SELECT 1", Array.Empty<DbParameter>());
    }

    private sealed class TrackingConnection(Exception? openException = null) : DbConnection
    {
        private ConnectionState _state = ConnectionState.Closed;

        public TrackingCommand Command { get; } = new();

        [AllowNull]
        public override string ConnectionString { get; set; } = "";
        public override string Database => "test";
        public override string DataSource => "test";
        public override string ServerVersion => "1";
        public override ConnectionState State => _state;

        public override void ChangeDatabase(string databaseName) { }

        public override void Close()
        {
            _state = ConnectionState.Closed;
        }

        public override void Open()
        {
            if (openException is not null)
                throw openException;

            _state = ConnectionState.Open;
        }

        public override Task OpenAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (openException is not null)
                return Task.FromException(openException);

            _state = ConnectionState.Open;
            return Task.CompletedTask;
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            throw new NotSupportedException();
        }

        protected override DbCommand CreateDbCommand()
        {
            return Command;
        }
    }

    private sealed class TrackingCommand : DbCommand
    {
        public bool IsDisposed { get; private set; }

        [AllowNull]
        public override string CommandText { get; set; } = "";
        public override int CommandTimeout { get; set; }
        public override CommandType CommandType { get; set; }
        public override bool DesignTimeVisible { get; set; }
        public override UpdateRowSource UpdatedRowSource { get; set; }
        protected override DbConnection? DbConnection { get; set; }
        protected override DbParameterCollection DbParameterCollection => throw new NotSupportedException();
        protected override DbTransaction? DbTransaction { get; set; }

        public override void Cancel() { }

        public override int ExecuteNonQuery()
        {
            throw new NotSupportedException();
        }

        public override object? ExecuteScalar()
        {
            throw new NotSupportedException();
        }

        public override void Prepare() { }

        protected override DbParameter CreateDbParameter()
        {
            throw new NotSupportedException();
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }
    }
}
