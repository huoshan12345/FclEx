using FclEx.Utils;

namespace FclEx.EfCore.Extensions.DbContextExtensions;

public partial class TruncateTests(EfCoreFixture fixture) : EfCoreTests(fixture)
{
    public static TheoryData<DbDriver, string?, bool, bool> OptionCases => TruncateTestCases.GetOptionCases(Schemas);

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task TruncateAsync_DefaultEntryPointsRemoveAllRowsAndPreserveTrackedEntities(DbDriver driver, string? schema)
    {
        Assert.SkipMySql(driver);
        using var session = await Fixture.TruncateTables.CreateSessionAsync(driver, schema, [typeof(EntityWithAutoKey)], cancellationToken: CancellationToken);
        await using var context = session.CreateDbContext(Fixture, schema);
        await using var cleanup = CleanupRows(context);
        await VerifyTruncationAsync(context, token => context.TruncateAsync<EntityWithAutoKey>(token));
        await VerifyTruncationAsync(context, token => context.TruncateAsync(typeof(EntityWithAutoKey), token));
        var entityType = context.Model.FindEntityType(typeof(EntityWithAutoKey))!;
        await VerifyTruncationAsync(context, token => context.TruncateAsync(entityType, token));
        await VerifyTruncationAsync(context, token => context.EntityWithAutoKey.TruncateAsync(token));
    }

    [Theory]
    [MemberData(nameof(OptionCases))]
    public async Task TruncateAsync_ExplicitOptionsHonorDatabaseCapabilities(
        DbDriver driver, string? schema, bool restartIdentity, bool cascade)
    {
        Assert.SkipMySql(driver);
        using var session = await Fixture.TruncateTables.CreateSessionAsync(driver, schema, [typeof(EntityWithAutoKey)], cancellationToken: CancellationToken);
        await using var context = session.CreateDbContext(Fixture, schema);
        await using var cleanup = CleanupRows(context);
        await VerifyTruncationAsync(context,
            token => context.TruncateAsync<EntityWithAutoKey>(restartIdentity, cascade, token), restartIdentity);
        await VerifyTruncationAsync(context,
            token => context.TruncateAsync(typeof(EntityWithAutoKey), restartIdentity, cascade, token), restartIdentity);
        var entityType = context.Model.FindEntityType(typeof(EntityWithAutoKey))!;
        await VerifyTruncationAsync(context,
            token => context.TruncateAsync(entityType, restartIdentity, cascade, token), restartIdentity);
        await VerifyTruncationAsync(context,
            token => context.EntityWithAutoKey.TruncateAsync(restartIdentity, cascade, token), restartIdentity);
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task TruncateAsync_UsesConfiguredSchemaOnOrdinaryTable(DbDriver driver, string? schema)
    {
        Assert.SkipMySql(driver);
        using var session = Fixture.TruncateTables.CreateClosedConnectionSession(driver, schema);
        await using var context = session.CreateDbContext(Fixture, schema);
        await using var cleanup = CleanupRows(context);
        var entityType = context.Model.FindEntityType(typeof(EntityWithAutoKey))!;
        Assert.Equal(schema, entityType.GetSchema());
        await VerifyRowsRemovedAsync(context, token => context.TruncateAsync<EntityWithAutoKey>(token));
        await VerifyRowsRemovedAsync(context, token => context.TruncateAsync(typeof(EntityWithAutoKey), token));
        await VerifyRowsRemovedAsync(context, token => context.TruncateAsync(entityType, token));
        await VerifyRowsRemovedAsync(context, token => context.EntityWithAutoKey.TruncateAsync(token));
        var restart = driver != DbDriver.Oracle;
        await VerifyRowsRemovedAsync(context, token => context.TruncateAsync<EntityWithAutoKey>(restart, false, token));
        await VerifyRowsRemovedAsync(context, token => context.TruncateAsync(typeof(EntityWithAutoKey), restart, false, token));
        await VerifyRowsRemovedAsync(context, token => context.TruncateAsync(entityType, restart, false, token));
        await VerifyRowsRemovedAsync(context, token => context.EntityWithAutoKey.TruncateAsync(restart, false, token));
    }

    // Cleanup belongs to the test; seeding clears between verification calls.
    internal static IAsyncDisposable CleanupRows(TestDbContext context)
        => AsyncDisposable.Create(async () =>
        {
            context.ChangeTracker.Clear();
            await context.EntityWithAutoKey.ExecuteDeleteAsync();
        });

    internal static async Task<EntityWithAutoKey> SeedAsync(TestDbContext context)
    {
        // A prior verification can reset identity values while leaving entities tracked.
        context.ChangeTracker.Clear();
        await context.EntityWithAutoKey.ExecuteDeleteAsync(CancellationToken);
        var first = new EntityWithAutoKey { Name = Guid.NewGuid().ToString(), Value = 1 };
        var second = new EntityWithAutoKey { Name = Guid.NewGuid().ToString(), Value = -1 };
        context.EntityWithAutoKey.AddRange(first, second);
        await context.SaveChangesAsync(CancellationToken);
        Assert.Equal(2, await context.EntityWithAutoKey.CountAsync(CancellationToken));
        Assert.True(second.Id > first.Id);
        return second;
    }

    internal static async Task<(EntityWithAutoKey Entity, int PreviousId)> VerifyRowsRemovedAsync(
        TestDbContext context, Func<CancellationToken, Task> truncate)
    {
        var tracked = await SeedAsync(context);
        var previousId = tracked.Id;
        await truncate(CancellationToken);
        Assert.Equal(0, await context.EntityWithAutoKey.CountAsync(CancellationToken));
        return (tracked, previousId);
    }

    // Identity and tracking contracts are checked by the core result/option tests.
    internal static async Task VerifyTruncationAsync(
        TestDbContext context, Func<CancellationToken, Task> truncate, bool? restartIdentity = null)
    {
        var (tracked, previousId) = await VerifyRowsRemovedAsync(context, truncate);
        Assert.Equal(EntityState.Unchanged, context.Entry(tracked).State);
        Assert.Equal(previousId, tracked.Id);
        context.ChangeTracker.Clear();
        var next = new EntityWithAutoKey { Name = Guid.NewGuid().ToString(), Value = 1 };
        context.EntityWithAutoKey.Add(next);
        await context.SaveChangesAsync(CancellationToken);
        if (restartIdentity ?? context.DbDriver is not (DbDriver.Npgsql or DbDriver.Oracle))
            Assert.Equal(1, next.Id);
        else
            Assert.True(next.Id > previousId, $"Expected identity to continue after {previousId}, got {next.Id}.");
    }

    [Theory]
    [MemberData(nameof(TruncateTestCases.UnsupportedOptionCases), MemberType = typeof(TruncateTestCases))]
    public async Task TruncateAsync_UnsupportedOptionsThrowBeforeOpeningConnection(
        DbDriver driver, bool restartIdentity, bool cascade)
    {
        Assert.SkipMySql(driver);
        await using var context = Fixture.CreateDbContext(driver);
        var entityType = context.Model.FindEntityType(typeof(EntityWithAutoKey))!;
        await Assert.ThrowsAsync<NotSupportedException>(() => context.TruncateAsync<EntityWithAutoKey>(restartIdentity, cascade));
        await Assert.ThrowsAsync<NotSupportedException>(() => context.TruncateAsync(typeof(EntityWithAutoKey), restartIdentity, cascade));
        await Assert.ThrowsAsync<NotSupportedException>(() => context.TruncateAsync(entityType, restartIdentity, cascade));
        await Assert.ThrowsAsync<NotSupportedException>(() => context.EntityWithAutoKey.TruncateAsync(restartIdentity, cascade));
        Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
    }

    public static TheoryData<DbDriver, string?> CascadeCases => TruncateTestCases.GetCascadeCases(Schemas);

    [Theory]
    [MemberData(nameof(CascadeCases))]
    public async Task TruncateAsync_CascadeRemovesReferencingRows(DbDriver driver, string? schema)
    {
        Assert.SkipMySql(driver);
        using var session = await Fixture.TruncateTables.CreateSessionAsync(driver, schema,
            [typeof(EntityHasStates), typeof(EntityWithNavigation)], cancellationToken: CancellationToken);
        await using var context = session.CreateDbContext(Fixture, schema);
        try
        {
            await context.EntityWithNavigation.ExecuteDeleteAsync(CancellationToken);
            await context.EntityHasStates.ExecuteDeleteAsync(CancellationToken);
            var parent = new EntityHasStates { Name = Guid.NewGuid().ToString() };
            context.EntityWithNavigation.Add(new EntityWithNavigation
            {
                Name = Guid.NewGuid().ToString(),
                Navigation = parent,
            });
            await context.SaveChangesAsync(CancellationToken);
            Assert.Equal(1, await context.EntityHasStates.CountAsync(CancellationToken));
            Assert.Equal(1, await context.EntityWithNavigation.CountAsync(CancellationToken));
            await context.TruncateAsync<EntityHasStates>(driver == DbDriver.Npgsql, true, CancellationToken);
            Assert.Equal(0, await context.EntityHasStates.CountAsync(CancellationToken));
            Assert.Equal(0, await context.EntityWithNavigation.CountAsync(CancellationToken));
        }
        finally
        {
            await context.EntityWithNavigation.ExecuteDeleteAsync();
            await context.EntityHasStates.ExecuteDeleteAsync();
        }
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task TruncateAsync_HandlesTablesWithoutIdentity(DbDriver driver, string? schema)
    {
        Assert.SkipMySql(driver);
        using var session = await Fixture.TruncateTables.CreateSessionAsync(driver, schema, [typeof(EntityWithGuidKey)], cancellationToken: CancellationToken);
        await using var context = session.CreateDbContext(Fixture, schema);
        try
        {
            await context.EntityWithGuidKey.ExecuteDeleteAsync(CancellationToken);
            context.EntityWithGuidKey.AddRange(
                new EntityWithGuidKey { Id = Guid.NewGuid(), Value = 1, Order = 1 },
                new EntityWithGuidKey { Id = Guid.NewGuid(), Value = 2, Order = 2 });
            await context.SaveChangesAsync(CancellationToken);
            Assert.Equal(2, await context.EntityWithGuidKey.CountAsync(CancellationToken));
            await context.TruncateAsync<EntityWithGuidKey>(CancellationToken);
            Assert.Equal(0, await context.EntityWithGuidKey.CountAsync(CancellationToken));
        }
        finally
        {
            await context.EntityWithGuidKey.ExecuteDeleteAsync();
        }
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task TruncateAsync_HandlesKeylessTables(DbDriver driver, string? schema)
    {
        Assert.SkipMySql(driver);
        using var session = await Fixture.TruncateTables.CreateSessionAsync(driver, schema, [typeof(EntityWithoutKey)], cancellationToken: CancellationToken);
        await using var context = session.CreateDbContext(Fixture, schema);
        var entityType = context.Model.FindEntityType(typeof(EntityWithoutKey))!;
        var sqlHelper = context.GetService<ISqlGenerationHelper>();
        var table = sqlHelper.DelimitIdentifier(entityType.GetTableName()!, entityType.GetSchema());
        var name = sqlHelper.DelimitIdentifier(nameof(EntityWithoutKey.Name));
        var value = sqlHelper.DelimitIdentifier(nameof(EntityWithoutKey.Value));
        var insertSql = $"INSERT INTO {table} ({name}, {value}) VALUES ({{0}}, {{1}})";
        try
        {
            await context.EntityWithoutKey.ExecuteDeleteAsync(CancellationToken);
            await context.Database.ExecuteSqlRawAsync(insertSql, new object[] { Guid.NewGuid().ToString(), 1 }, CancellationToken);
            Assert.Equal(1, await context.EntityWithoutKey.CountAsync(CancellationToken));
            await context.TruncateAsync<EntityWithoutKey>(CancellationToken);
            Assert.Equal(0, await context.EntityWithoutKey.CountAsync(CancellationToken));
        }
        finally
        {
            await context.EntityWithoutKey.ExecuteDeleteAsync();
        }
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task TruncateAsync_AllEntryPointsObserveCancellationWithoutRemovingRows(DbDriver driver, string? schema)
    {
        Assert.SkipMySql(driver);
        using var session = await Fixture.TruncateTables.CreateSessionAsync(driver, schema, [typeof(EntityWithAutoKey)], cancellationToken: CancellationToken);
        await using var context = session.CreateDbContext(Fixture, schema);
        await using var cleanup = CleanupRows(context);
        var tracked = await SeedAsync(context);
        var entityType = context.Model.FindEntityType(typeof(EntityWithAutoKey))!;
        var restart = driver != DbDriver.Oracle;
        var token = new CancellationToken(true);
        Func<Task>[] truncations =
        [
            () => context.TruncateAsync<EntityWithAutoKey>(token),
            () => context.TruncateAsync<EntityWithAutoKey>(restart, false, token),
            () => context.TruncateAsync(typeof(EntityWithAutoKey), token),
            () => context.TruncateAsync(typeof(EntityWithAutoKey), restart, false, token),
            () => context.TruncateAsync(entityType, token),
            () => context.TruncateAsync(entityType, restart, false, token),
            () => context.EntityWithAutoKey.TruncateAsync(token),
            () => context.EntityWithAutoKey.TruncateAsync(restart, false, token),
        ];
        foreach (var truncate in truncations)
            await Assert.ThrowsAnyAsync<OperationCanceledException>(truncate);
        Assert.Equal(2, await context.EntityWithAutoKey.CountAsync(CancellationToken));
        Assert.Equal(EntityState.Unchanged, context.Entry(tracked).State);
    }

    [Fact]
    public async Task TruncateAsync_ValidatesNullArguments()
    {
        await using var context = Fixture.CreateDbContext(DbDriver.SqlServer);
        await Assert.ThrowsAsync<ArgumentNullException>(() => ((DbContext)null!).TruncateAsync<EntityWithAutoKey>());
        await Assert.ThrowsAsync<ArgumentNullException>(() => context.TruncateAsync((Type)null!));
        await Assert.ThrowsAsync<ArgumentNullException>(() => ((DbContext)null!).TruncateAsync<EntityWithAutoKey>(true, false));
        await Assert.ThrowsAsync<ArgumentNullException>(() => ((DbContext)null!).TruncateAsync(typeof(EntityWithAutoKey), true, false));
        await Assert.ThrowsAsync<ArgumentNullException>(() => context.TruncateAsync((Type)null!, true, false));
    }
}
