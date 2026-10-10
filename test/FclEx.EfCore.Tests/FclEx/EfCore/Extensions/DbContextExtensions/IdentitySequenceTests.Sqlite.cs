using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FclEx.EfCore.Extensions.DbContextExtensions;

public partial class IdentitySequenceTests
{
    [Fact]
    public async Task SynchronizeIdentitySequenceAsync_FluentMappingInt64KeyAndFilter_UsesAllPhysicalRows()
    {
        await using var reference = new TestDbContext(DbDriver.Sqlite, "Data Source=:memory:");
        await reference.Database.OpenConnectionAsync();
        const string tableName = "identity ' \" {table}";
        const string columnName = "key ' \" {column}";
        await using var context = TruncateTests.CreateModelContext(reference, builder =>
        {
            var entity = builder.Entity<LongIdentity>();
            entity.ToTable(tableName);
            entity.HasKey(row => row.Number);
            entity.Property(row => row.Number).HasColumnName(columnName).ValueGeneratedOnAdd();
            entity.HasQueryFilter(row => row.Number < int.MaxValue);
        });
        var sqlHelper = context.GetService<ISqlGenerationHelper>();
        var table = sqlHelper.DelimitIdentifier(tableName);
        var column = sqlHelper.DelimitIdentifier(columnName);
        const long maximum = (long)int.MaxValue + 100;
        // The setup DDL contains literal braces too, so escape EF's SQL-formatting placeholders.
        await context.Database.ExecuteSqlRawAsync($"CREATE TABLE {table} ({column} INTEGER PRIMARY KEY AUTOINCREMENT)".Replace("{", "{{").Replace("}", "}}"));
        await context.Database.ExecuteSqlRawAsync($"INSERT INTO {table} VALUES ({maximum})".Replace("{", "{{").Replace("}", "}}"));
        Assert.Empty(await context.Set<LongIdentity>().ToListAsync());
        Assert.True(await context.SynchronizeIdentitySequenceAsync<LongIdentity>());
        var next = new LongIdentity();
        context.Add(next);
        await context.SaveChangesAsync();
        Assert.Equal(maximum + 1, next.Number);
    }

    [Fact]
    public async Task SynchronizeIdentitySequenceAsync_ShadowKeyNamedSharedType_UsesMetadata()
    {
        await using var reference = new TestDbContext(DbDriver.Sqlite, "Data Source=:memory:");
        await reference.Database.OpenConnectionAsync();
        await using var context = TruncateTests.CreateModelContext(reference, builder =>
        {
            builder.SharedTypeEntity<Dictionary<string, object>>("NamedIdentity", entity =>
            {
                entity.ToTable("ShadowIdentity");
                entity.IndexerProperty<long>("Number").HasColumnName("physical_key").ValueGeneratedOnAdd();
                entity.HasKey("Number");
            });
        });
        await context.Database.ExecuteSqlRawAsync("""
            CREATE TABLE ShadowIdentity (physical_key INTEGER PRIMARY KEY AUTOINCREMENT);
            INSERT INTO ShadowIdentity VALUES (500);
            DELETE FROM ShadowIdentity;
            INSERT INTO ShadowIdentity VALUES (10);
            """);
        var entityType = context.Model.FindEntityType("NamedIdentity")!;
        Assert.True(await context.SynchronizeIdentitySequenceAsync(entityType));
        await Assert.ThrowsAsync<InvalidOperationException>(() => context.SynchronizeIdentitySequenceAsync<Dictionary<string, object>>());
        var row = new Dictionary<string, object>();
        context.Set<Dictionary<string, object>>("NamedIdentity").Add(row);
        await context.SaveChangesAsync();
        Assert.Equal(11L, row["Number"]);
    }

    [Fact]
    public async Task SynchronizeIdentitySequenceAsync_TemporaryTableAndCurrentTransaction_PreserveMainAndRollback()
    {
        await using var context = new TestDbContext(DbDriver.Sqlite, "Data Source=:memory:");
        await context.Database.OpenConnectionAsync();
        await context.Database.ExecuteSqlRawAsync("""
            CREATE TABLE main.TruncateRow (Id INTEGER PRIMARY KEY AUTOINCREMENT, Value INTEGER NOT NULL);
            CREATE TEMP TABLE TruncateRow (Id INTEGER PRIMARY KEY AUTOINCREMENT, Value INTEGER NOT NULL);
            INSERT INTO main.TruncateRow VALUES (100, 1);
            INSERT INTO temp.TruncateRow VALUES (200, 1);
            DELETE FROM temp.TruncateRow;
            INSERT INTO temp.TruncateRow VALUES (10, 1);
            """);
        await using (var transaction = await context.Database.BeginTransactionAsync())
        {
            Assert.True(await context.SynchronizeIdentitySequenceAsync<TruncateRow>());
            Assert.Same(transaction, context.Database.CurrentTransaction);
            Assert.Equal(10, await context.Database.SqlQueryRaw<int>("SELECT seq AS Value FROM temp.sqlite_sequence WHERE name = 'TruncateRow'").SingleAsync());
            await transaction.RollbackAsync();
        }
        Assert.Equal(200, await context.Database.SqlQueryRaw<int>("SELECT seq AS Value FROM temp.sqlite_sequence WHERE name = 'TruncateRow'").SingleAsync());
        Assert.Equal(100, await context.Database.SqlQueryRaw<int>("SELECT seq AS Value FROM main.sqlite_sequence WHERE name = 'TruncateRow'").SingleAsync());
        Assert.True(await context.SynchronizeIdentitySequenceAsync<TruncateRow>());
        context.Add(new TruncateRow { Value = 2 });
        await context.SaveChangesAsync();
        Assert.Equal(11, await context.TruncateRow.MaxAsync(row => row.Id));
    }

    [Fact]
    public async Task SynchronizeIdentitySequenceAsync_CommandsUseEfInterceptorsTimeoutAndLocalTransaction()
    {
        var interceptor = new IdentityCommandInterceptor();
        var options = new DbContextOptionsBuilder<TestDbContext>().AddInterceptors(interceptor).Options;
        await using var context = new TestDbContext(DbDriver.Sqlite, "Data Source=:memory:", options: options);
        await context.Database.OpenConnectionAsync();
        await context.Database.ExecuteSqlRawAsync("CREATE TABLE TruncateRow (Id INTEGER PRIMARY KEY AUTOINCREMENT, Value INTEGER NOT NULL)");
        context.Database.SetCommandTimeout(37);
        interceptor.Commands.Clear();
        Assert.True(await context.SynchronizeIdentitySequenceAsync<TruncateRow>());
        Assert.NotEmpty(interceptor.Commands);
        Assert.All(interceptor.Commands, command =>
        {
            Assert.Equal(37, command.Timeout);
            Assert.True(command.HasTransaction);
        });
        Assert.Contains(interceptor.Commands, command => command.Sql.StartsWith("INSERT INTO", StringComparison.Ordinal));
        Assert.Null(context.Database.CurrentTransaction);
        Assert.Equal(ConnectionState.Open, context.Database.GetDbConnection().State);
    }

    [Fact]
    public async Task SynchronizeIdentitySequenceAsync_MidOperationFailure_RollsBackLocalTransactionAndKeepsConnectionUsable()
    {
        var interceptor = new IdentityCommandInterceptor();
        var options = new DbContextOptionsBuilder<TestDbContext>().AddInterceptors(interceptor).Options;
        await using var context = new TestDbContext(DbDriver.Sqlite, "Data Source=:memory:", options: options);
        await context.Database.OpenConnectionAsync();
        await context.Database.ExecuteSqlRawAsync("""
            CREATE TABLE TruncateRow (Id INTEGER PRIMARY KEY AUTOINCREMENT, Value INTEGER NOT NULL);
            INSERT INTO TruncateRow VALUES (100, 1);
            DELETE FROM TruncateRow;
            INSERT INTO TruncateRow VALUES (10, 1);
            """);
        interceptor.FailInsert = true;
        await Assert.ThrowsAsync<InvalidOperationException>(() => context.SynchronizeIdentitySequenceAsync<TruncateRow>());
        interceptor.FailInsert = false;
        Assert.Equal(100, await context.Database.SqlQueryRaw<int>("SELECT seq AS Value FROM sqlite_sequence WHERE name = 'TruncateRow'").SingleAsync());
        Assert.Null(context.Database.CurrentTransaction);
        Assert.True(await context.SynchronizeIdentitySequenceAsync<TruncateRow>());
    }

    private sealed class LongIdentity
    {
        public long Number { get; set; }
    }

    private sealed class IdentityCommandInterceptor : DbCommandInterceptor
    {
        public List<(string Sql, int Timeout, bool HasTransaction)> Commands { get; } = [];
        public bool FailInsert { get; set; }

        public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(DbCommand command, CommandEventData eventData,
            InterceptionResult<object> result, CancellationToken cancellationToken = default)
        {
            Commands.Add((command.CommandText, command.CommandTimeout, command.Transaction is not null));
            return ValueTask.FromResult(result);
        }

        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command, CommandEventData eventData,
            InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            Commands.Add((command.CommandText, command.CommandTimeout, command.Transaction is not null));
            if (FailInsert && command.CommandText.StartsWith("INSERT INTO", StringComparison.Ordinal))
                throw new InvalidOperationException("Simulated failure between sequence deletion and replacement.");
            return ValueTask.FromResult(result);
        }
    }
}
