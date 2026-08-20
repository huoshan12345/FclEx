using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.Sqlite;

namespace FclEx.EfCore.Extensions;

public class DatabaseFacadeExtensionsTests(EfCoreFixture fixture) : EfCoreTests(fixture)
{
    private enum ScalarStatus
    {
        Active = 1,
    }

    [Fact]
    public async Task ExecuteScalarRawAsync_UsesCurrentSqlServerTransaction()
    {
        Assert.SkipUnlessIncluded(DbDriver.SqlServer);

        await using var context = Fixture.CreateDbContext(DbDriver.SqlServer);
        await using var transaction = await context.Database.BeginTransactionAsync();

        var result = await context.Database.ExecuteScalarRawAsync<int>("SELECT 1");

        Assert.Equal(1, result);
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task ExecuteScalarRawAsync_ReturnsValue(DbDriver dbDriver)
    {
        await using var context = Fixture.CreateDbContext(dbDriver);
        var result = await context.Database.ExecuteScalarRawAsync<int>("SELECT 1");

        Assert.Equal(1, result);
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task ExecuteScalarRawAsync_ReturnsNull_WhenSqlReturnsNull(DbDriver dbDriver)
    {
        await using var context = Fixture.CreateDbContext(dbDriver);
        var result = await context.Database.ExecuteScalarRawAsync<int?>("SELECT NULL");

        Assert.Null(result);
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    // ReSharper disable once InconsistentNaming
    public async Task ExecuteScalarRawAsync_ReturnsDefault_WhenDBNull(DbDriver dbDriver)
    {
        await using var context = Fixture.CreateDbContext(dbDriver);
        var result = await context.Database.ExecuteScalarRawAsync<string>("SELECT NULL");

        Assert.Null(result);
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task ExecuteScalarRawAsync_SupportsParameters(DbDriver dbDriver)
    {
        var parameter = CreateParameter(dbDriver, "@p0", 5);
        await using var context = Fixture.CreateDbContext(dbDriver);
        var result = await context.Database.ExecuteScalarRawAsync<int>("SELECT @p0 + 1", [parameter]);
        Assert.Equal(6, result);
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task ExecuteScalarRawAsync_ConvertsResultType(DbDriver dbDriver)
    {
        await using var context = Fixture.CreateDbContext(dbDriver);
        var result = await context.Database.ExecuteScalarRawAsync<long>("SELECT 1");

        Assert.Equal(1L, result);
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task ExecuteScalarRawAsync_Works_WhenConnectionAlreadyOpen(DbDriver dbDriver)
    {
        await using var context = Fixture.CreateDbContext(dbDriver);
        await context.Database.OpenConnectionAsync();

        var result = await context.Database.ExecuteScalarRawAsync<int>("SELECT 1");

        Assert.Equal(1, result);
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task ExecuteScalarRawAsync_Works_WithEmptyParameters(DbDriver dbDriver)
    {
        await using var context = Fixture.CreateDbContext(dbDriver);
        var result = await context.Database.ExecuteScalarRawAsync<int>("SELECT 1", []);

        Assert.Equal(1, result);
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task ExecuteScalarRawAsync_NumericWidening(DbDriver dbDriver)
    {
        await using var context = Fixture.CreateDbContext(dbDriver);
        var result = await context.Database.ExecuteScalarRawAsync<long>("SELECT COUNT(*)");

        Assert.Equal(1L, result);
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task ExecuteScalarRawAsync_NullableNumericWidening(DbDriver dbDriver)
    {
        await using var context = Fixture.CreateDbContext(dbDriver);
        var result = await context.Database.ExecuteScalarRawAsync<long?>("SELECT 1");

        Assert.Equal(1L, result);
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task ExecuteScalarRawAsync_ConvertsNullableEnum(DbDriver dbDriver)
    {
        await using var context = Fixture.CreateDbContext(dbDriver);
        var result = await context.Database.ExecuteScalarRawAsync<ScalarStatus?>("SELECT 1");

        Assert.Equal(ScalarStatus.Active, result);
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task ExecuteScalarRawAsync_Guid(DbDriver dbDriver)
    {
        var expected = Guid.NewGuid();
        var parameter = CreateParameter(dbDriver, "@p0", expected);

        await using var context = Fixture.CreateDbContext(dbDriver);
        var result = await context.Database.ExecuteScalarRawAsync<Guid>("SELECT @p0", [parameter]);

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task ExecuteScalarRawAsync_RetriesEntireOperationAndAppliesCommandTimeout()
    {
        await using var connection = new RetryingDbConnection(new SqliteConnection("Data Source=:memory:"));
        var options = new DbContextOptionsBuilder()
            .UseSqlite(connection)
            .ReplaceService<IExecutionStrategyFactory, RetryingExecutionStrategyFactory>()
            .Options;
        await using var context = new DbContext(options);
        context.Database.SetCommandTimeout(37);
        var parameter = new SqliteParameter("@p0", 42);

        var result = await context.Database.ExecuteScalarRawAsync<int>("SELECT @p0", [parameter]);

        Assert.Equal(42, result);
        Assert.Equal(2, connection.OpenCount);
        Assert.Equal(2, connection.CreateCommandCount);
        Assert.Equal(2, connection.ExecuteCount);
        Assert.Equal([37, 37], connection.CommandTimeouts);
        Assert.Equal(ConnectionState.Closed, connection.State);
    }

    private sealed class RetryableException : Exception;

    private sealed class RetryingExecutionStrategyFactory(ExecutionStrategyDependencies dependencies)
        : IExecutionStrategyFactory
    {
        public IExecutionStrategy Create()
        {
            return new RetryingExecutionStrategy(dependencies);
        }
    }

    private sealed class RetryingExecutionStrategy(ExecutionStrategyDependencies dependencies)
        : ExecutionStrategy(dependencies, 2, TimeSpan.Zero)
    {
        protected override bool ShouldRetryOn(Exception exception)
        {
            return exception is RetryableException;
        }
    }

    private sealed class RetryingDbConnection(DbConnection inner) : DbConnection
    {
        private DbConnection Inner => inner;

        public int OpenCount { get; private set; }
        public int CreateCommandCount { get; private set; }
        public int ExecuteCount { get; private set; }
        public List<int> CommandTimeouts { get; } = [];

        [AllowNull]
        public override string ConnectionString
        {
            get => inner.ConnectionString;
            set => inner.ConnectionString = value;
        }

        public override string Database => inner.Database;
        public override string DataSource => inner.DataSource;
        public override string ServerVersion => inner.ServerVersion;
        public override ConnectionState State => inner.State;

        public override void ChangeDatabase(string databaseName)
        {
            inner.ChangeDatabase(databaseName);
        }

        public override void Close()
        {
            inner.Close();
        }

        public override Task CloseAsync()
        {
            return inner.CloseAsync();
        }

        public override void Open()
        {
            OpenCount++;
            inner.Open();
        }

        public override async Task OpenAsync(CancellationToken cancellationToken)
        {
            OpenCount++;
            await inner.OpenAsync(cancellationToken);
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return inner.BeginTransaction(isolationLevel);
        }

        protected override DbCommand CreateDbCommand()
        {
            CreateCommandCount++;
            return new RetryingDbCommand(this, inner.CreateCommand());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                inner.Dispose();

            base.Dispose(disposing);
        }

        private sealed class RetryingDbCommand(RetryingDbConnection owner, DbCommand innerCommand) : DbCommand
        {
            [AllowNull]
            public override string CommandText
            {
                get => innerCommand.CommandText;
                set => innerCommand.CommandText = value;
            }

            public override int CommandTimeout
            {
                get => innerCommand.CommandTimeout;
                set => innerCommand.CommandTimeout = value;
            }

            public override CommandType CommandType
            {
                get => innerCommand.CommandType;
                set => innerCommand.CommandType = value;
            }

            public override bool DesignTimeVisible
            {
                get => innerCommand.DesignTimeVisible;
                set => innerCommand.DesignTimeVisible = value;
            }

            public override UpdateRowSource UpdatedRowSource
            {
                get => innerCommand.UpdatedRowSource;
                set => innerCommand.UpdatedRowSource = value;
            }

            protected override DbConnection? DbConnection
            {
                get => owner;
                set => innerCommand.Connection = value is null ? null : owner.Inner;
            }

            protected override DbParameterCollection DbParameterCollection => innerCommand.Parameters;

            protected override DbTransaction? DbTransaction
            {
                get => innerCommand.Transaction;
                set => innerCommand.Transaction = value;
            }

            public override void Cancel()
            {
                innerCommand.Cancel();
            }

            public override int ExecuteNonQuery()
            {
                return innerCommand.ExecuteNonQuery();
            }

            public override object? ExecuteScalar()
            {
                return innerCommand.ExecuteScalar();
            }

            public override void Prepare()
            {
                innerCommand.Prepare();
            }

            protected override DbParameter CreateDbParameter()
            {
                return innerCommand.CreateParameter();
            }

            protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
            {
                return innerCommand.ExecuteReader(behavior);
            }

            public override async Task<object?> ExecuteScalarAsync(CancellationToken cancellationToken)
            {
                owner.ExecuteCount++;
                owner.CommandTimeouts.Add(CommandTimeout);
                if (owner.ExecuteCount == 1)
                    throw new RetryableException();

                return await innerCommand.ExecuteScalarAsync(cancellationToken);
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                    innerCommand.Dispose();

                base.Dispose(disposing);
            }

        }
    }
}
