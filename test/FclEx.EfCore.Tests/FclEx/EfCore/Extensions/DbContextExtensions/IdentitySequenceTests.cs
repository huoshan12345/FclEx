using FclEx.Dapper;
using Microsoft.EntityFrameworkCore.Metadata;

namespace FclEx.EfCore.Extensions.DbContextExtensions;

public partial class IdentitySequenceTests(EfCoreFixture fixture) : EfCoreTests(fixture)
{
    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task ReseedIdentityAsync_AllEntryPoints_AdvancePastExplicitKeysAndPreserveTracking(DbDriver driver, string? schema)
    {
        using var session = await Fixture.TruncateSessions.CreateSessionAsync(driver, schema, [typeof(TruncateRow)]);
        await using var context = session.CreateDbContext(Fixture);
        await using var cleanup = TruncateTests.CleanupRows(context);
        await context.TruncateRow.ExecuteDeleteAsync();
        var tracked = new TruncateRow { Value = 1 };
        context.Add(tracked);
        await context.SaveChangesAsync();
        var originalKey = tracked.Id;
        var maximum = checked(originalKey + 100);
        await session.Connection.InsertWithExplicitGeneratedKeysAsync(new TruncateRow { Id = maximum, Value = -1 }, schema);
        var pending = new TruncateRow { Value = 9 };
        context.Add(pending);
        var pendingKey = pending.Id;
        var entityType = context.Model.FindEntityType(typeof(TruncateRow))!;
        Func<Task<bool>>[] operations =
        [
            () => context.ReseedIdentityAsync<TruncateRow>(),
            () => context.ReseedIdentityAsync(typeof(TruncateRow)),
            () => context.ReseedIdentityAsync(entityType),
        ];
        foreach (var synchronize in operations)
            Assert.True(await synchronize());
        Assert.Equal(ConnectionState.Open, session.Connection.State);
        Assert.Equal(EntityState.Unchanged, context.Entry(tracked).State);
        Assert.Equal(originalKey, tracked.Id);
        Assert.Equal(EntityState.Added, context.Entry(pending).State);
        Assert.Equal(pendingKey, pending.Id);
        Assert.Equal(2, await context.TruncateRow.CountAsync());
        context.Entry(pending).State = EntityState.Detached;
        var next = new TruncateRow { Value = 3 };
        context.Add(next);
        await context.SaveChangesAsync();
        Assert.True(next.Id > maximum);
        if (driver is DbDriver.Npgsql or DbDriver.SqlServer or DbDriver.Sqlite)
            Assert.Equal(maximum + 1, next.Id);

        await using var closed = Fixture.CreateDbContext(driver, schema);
        Assert.True(await closed.ReseedIdentityAsync<TruncateRow>());
        Assert.Equal(ConnectionState.Closed, closed.Database.GetDbConnection().State);
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task ReseedIdentityAsync_UsedEmptyTable_AllowsGeneratedInsert(DbDriver driver, string? schema)
    {
        using var session = await Fixture.TruncateSessions.CreateSessionAsync(driver, schema, [typeof(TruncateRow)]);
        await using var context = session.CreateDbContext(Fixture);
        await using var cleanup = TruncateTests.CleanupRows(context);
        await context.TruncateRow.ExecuteDeleteAsync();
        context.Add(new TruncateRow { Value = 1 });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        await context.TruncateRow.ExecuteDeleteAsync();
        Assert.True(await context.ReseedIdentityAsync<TruncateRow>());
        var next = new TruncateRow { Value = 2 };
        context.Add(next);
        await context.SaveChangesAsync();
        Assert.True(next.Id > 0);
        if (driver is DbDriver.Npgsql or DbDriver.SqlServer or DbDriver.Sqlite)
            Assert.Equal(1, next.Id);
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task ReseedIdentityAsync_MissingTableOrNativeGenerator_ReturnsFalse(DbDriver driver, string? schema)
    {
        await using var reference = Fixture.CreateDbContext(driver, schema);
        await using var context = TruncateTests.CreateModelContext(reference, builder =>
        {
            builder.Entity<ModelIdentity>().ToTable("missing_identity_table_" + Guid.NewGuid().ToString("N"), schema);
            builder.Entity<PhysicalManualIdentity>().ToTable(nameof(TruncateManualRow), schema);
            builder.Entity<PhysicalManualIdentity>().Property(row => row.Id).ValueGeneratedOnAdd();
        });
        Assert.False(await context.ReseedIdentityAsync<ModelIdentity>());
        Assert.False(await context.ReseedIdentityAsync<PhysicalManualIdentity>());
        Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
    }

    [Theory]
    [MemberData(nameof(TruncateTests.UnsupportedMappingCases), MemberType = typeof(TruncateTests))]
    public async Task ReseedIdentityAsync_RejectsUnsupportedMappingsBeforeOpening(string shape)
    {
        await using var context = TruncateTests.CreateMappingContext(Fixture, shape);
        var entityType = context.Model.FindEntityType(typeof(TruncateTests.MappingEntity))!;
        await Assert.ThrowsAsync<NotSupportedException>(() => context.ReseedIdentityAsync<TruncateTests.MappingEntity>());
        await Assert.ThrowsAsync<NotSupportedException>(() => context.ReseedIdentityAsync(entityType));
        Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
    }

    [Fact]
    public async Task ReseedIdentityAsync_ValidatesArgumentsMappingsAndCancellationBeforeOpening()
    {
        // Validation never opens the connection, so it needs no fixture database or selected SQLite driver.
        await using var context = new TestDbContext(DbDriver.Sqlite, "Data Source=:memory:");
        var entityType = context.Model.FindEntityType(typeof(TruncateRow))!;
        await Assert.ThrowsAsync<ArgumentNullException>(() => ((DbContext)null!).ReseedIdentityAsync<TruncateRow>());
        await Assert.ThrowsAsync<ArgumentNullException>(() => context.ReseedIdentityAsync((Type)null!));
        await Assert.ThrowsAsync<ArgumentNullException>(() => context.ReseedIdentityAsync((IEntityType)null!));
        await Assert.ThrowsAsync<InvalidOperationException>(() => context.ReseedIdentityAsync<string>());
        await Assert.ThrowsAsync<NotSupportedException>(() => context.ReseedIdentityAsync<TruncateManualRow>());
        await Assert.ThrowsAsync<NotSupportedException>(() => context.ReseedIdentityAsync<TruncateKeylessRow>());
        await Assert.ThrowsAsync<NotSupportedException>(() => context.ReseedIdentityAsync<EntityWithGuidKey>());
        var canceled = new CancellationToken(true);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => context.ReseedIdentityAsync<TruncateRow>(canceled));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => context.ReseedIdentityAsync(typeof(TruncateRow), canceled));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => context.ReseedIdentityAsync(entityType, canceled));
        await using var other = Fixture.CreateDbContext(DbDriver.SqlServer);
        await Assert.ThrowsAsync<ArgumentException>(() => context.ReseedIdentityAsync(other.Model.FindEntityType(typeof(TruncateRow))!, cancellationToken: canceled));
        Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
    }

    private sealed class ModelIdentity
    {
        public int Id { get; set; }
    }

    private sealed class PhysicalManualIdentity
    {
        public int Id { get; set; }
    }
}
