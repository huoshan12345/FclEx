using System.Diagnostics.CodeAnalysis;

namespace FclEx.Dapper;

public class DbCommandExtensionsTests
{
    [Fact]
    public async Task ExecuteNonQueryAsync_NonDbCommand_DoesNotBlockCallingThread()
    {
        using var command = new BlockingCommand();
        var invocation = Task.Factory.StartNew(
            () => command.ExecuteNonQueryAsync(),
            CancellationToken.None,
            TaskCreationOptions.DenyChildAttach,
            TaskScheduler.Default);

        try
        {
            Assert.True(command.Started.Wait(TimeSpan.FromSeconds(5)));
            Assert.Same(invocation, await Task.WhenAny(invocation, Task.Delay(TimeSpan.FromSeconds(5))));

            var execution = await invocation;
            Assert.False(execution.IsCompleted);
            command.Release.Set();
            Assert.Equal(7, await execution);
        }
        finally
        {
            command.Release.Set();
        }
    }

    [Fact]
    public async Task ExecuteNonQueryAsync_NonDbCommand_CancellationAfterStartDoesNotDetachExecution()
    {
        using var command = new BlockingCommand();
        using var cancellationSource = new CancellationTokenSource();
        var execution = command.ExecuteNonQueryAsync(cancellationSource.Token);

        try
        {
            Assert.True(command.Started.Wait(TimeSpan.FromSeconds(5)));
            cancellationSource.Cancel();
            await Task.Delay(50);
            Assert.False(execution.IsCompleted);

            command.Release.Set();
            Assert.Equal(7, await execution);
        }
        finally
        {
            command.Release.Set();
        }
    }

    private sealed class BlockingCommand : IDbCommand
    {
        public ManualResetEventSlim Started { get; } = new();
        public ManualResetEventSlim Release { get; } = new();
        [AllowNull]
        public string CommandText { get; set; } = "";
        public int CommandTimeout { get; set; }
        public CommandType CommandType { get; set; }
        public IDbConnection? Connection { get; set; }
        public IDataParameterCollection Parameters => throw new NotSupportedException();
        public IDbTransaction? Transaction { get; set; }
        public UpdateRowSource UpdatedRowSource { get; set; }

        public void Cancel() { }

        public IDbDataParameter CreateParameter()
        {
            throw new NotSupportedException();
        }

        public void Dispose()
        {
            Started.Dispose();
            Release.Dispose();
        }

        public int ExecuteNonQuery()
        {
            Started.Set();
            Release.Wait();
            return 7;
        }

        public IDataReader ExecuteReader()
        {
            throw new NotSupportedException();
        }

        public IDataReader ExecuteReader(CommandBehavior behavior)
        {
            throw new NotSupportedException();
        }

        public object? ExecuteScalar()
        {
            throw new NotSupportedException();
        }

        public void Prepare() { }
    }
}
