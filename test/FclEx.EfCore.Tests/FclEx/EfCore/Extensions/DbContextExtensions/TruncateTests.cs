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
        using var session = await Fixture.TruncateSessions.CreateSessionAsync(driver, schema, [typeof(TruncateRow)], cancellationToken: CancellationToken);
        await using var context = session.CreateDbContext(Fixture);
        await using var cleanup = CleanupRows(context);
        await VerifyTruncationAsync(context, token => context.TruncateAsync<TruncateRow>(token));
        await VerifyTruncationAsync(context, token => context.TruncateAsync(typeof(TruncateRow), token));
        var entityType = context.Model.FindEntityType(typeof(TruncateRow))!;
        await VerifyTruncationAsync(context, token => context.TruncateAsync(entityType, token));
        await VerifyTruncationAsync(context, token => context.TruncateRow.TruncateAsync(token));
    }

    [Theory]
    [MemberData(nameof(OptionCases))]
    public async Task TruncateAsync_ExplicitOptionsHonorDatabaseCapabilities(
        DbDriver driver, string? schema, bool restartIdentity, bool cascade)
    {
        Assert.SkipMySql(driver);
        using var session = await Fixture.TruncateSessions.CreateSessionAsync(driver, schema, [typeof(TruncateRow)], cancellationToken: CancellationToken);
        await using var context = session.CreateDbContext(Fixture);
        await using var cleanup = CleanupRows(context);
        await VerifyTruncationAsync(context,
            token => context.TruncateAsync<TruncateRow>(restartIdentity, cascade, token), restartIdentity);
        await VerifyTruncationAsync(context,
            token => context.TruncateAsync(typeof(TruncateRow), restartIdentity, cascade, token), restartIdentity);
        var entityType = context.Model.FindEntityType(typeof(TruncateRow))!;
        await VerifyTruncationAsync(context,
            token => context.TruncateAsync(entityType, restartIdentity, cascade, token), restartIdentity);
        await VerifyTruncationAsync(context,
            token => context.TruncateRow.TruncateAsync(restartIdentity, cascade, token), restartIdentity);
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task TruncateAsync_UsesConfiguredSchemaOnOrdinaryTable(DbDriver driver, string? schema)
    {
        Assert.SkipMySql(driver);
        using var session = await Fixture.TruncateSessions.CreateSessionAsync(driver, schema,
            [typeof(TruncateRow)], cancellationToken: CancellationToken);
        await using var context = session.CreateDbContext(Fixture);
        await using var cleanup = CleanupRows(context);
        var entityType = context.Model.FindEntityType(typeof(TruncateRow))!;
        Assert.Equal(schema, entityType.GetSchema());
        await VerifyRowsRemovedAsync(context, token => context.TruncateAsync<TruncateRow>(token));
        await VerifyRowsRemovedAsync(context, token => context.TruncateAsync(typeof(TruncateRow), token));
        await VerifyRowsRemovedAsync(context, token => context.TruncateAsync(entityType, token));
        await VerifyRowsRemovedAsync(context, token => context.TruncateRow.TruncateAsync(token));
        var restart = driver != DbDriver.Oracle;
        await VerifyRowsRemovedAsync(context, token => context.TruncateAsync<TruncateRow>(restart, false, token));
        await VerifyRowsRemovedAsync(context, token => context.TruncateAsync(typeof(TruncateRow), restart, false, token));
        await VerifyRowsRemovedAsync(context, token => context.TruncateAsync(entityType, restart, false, token));
        await VerifyRowsRemovedAsync(context, token => context.TruncateRow.TruncateAsync(restart, false, token));
    }

    // Cleanup belongs to the test; seeding clears between verification calls.
    internal static IAsyncDisposable CleanupRows(TestDbContext context)
        => AsyncDisposable.Create(async () =>
        {
            context.ChangeTracker.Clear();
            await context.TruncateRow.ExecuteDeleteAsync();
        });

    internal static async Task<TruncateRow> SeedAsync(TestDbContext context)
    {
        // A prior verification can reset identity values while leaving entities tracked.
        context.ChangeTracker.Clear();
        await context.TruncateRow.ExecuteDeleteAsync(CancellationToken);
        var first = new TruncateRow { Value = 1 };
        var second = new TruncateRow { Value = -1 };
        context.TruncateRow.AddRange(first, second);
        await context.SaveChangesAsync(CancellationToken);
        Assert.Equal(2, await context.TruncateRow.CountAsync(CancellationToken));
        Assert.True(second.Id > first.Id);
        return second;
    }

    internal static async Task<(TruncateRow Entity, int PreviousId)> VerifyRowsRemovedAsync(
        TestDbContext context, Func<CancellationToken, Task> truncate)
    {
        var tracked = await SeedAsync(context);
        var previousId = tracked.Id;
        await truncate(CancellationToken);
        Assert.Equal(0, await context.TruncateRow.CountAsync(CancellationToken));
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
        var next = new TruncateRow { Value = 1 };
        context.TruncateRow.Add(next);
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
        await using var context = new TestDbContext(driver, Fixture.ResolveTarget(driver).BuildConnectionString());
        var entityType = context.Model.FindEntityType(typeof(TruncateRow))!;
        await Assert.ThrowsAsync<NotSupportedException>(() => context.TruncateAsync<TruncateRow>(restartIdentity, cascade));
        await Assert.ThrowsAsync<NotSupportedException>(() => context.TruncateAsync(typeof(TruncateRow), restartIdentity, cascade));
        await Assert.ThrowsAsync<NotSupportedException>(() => context.TruncateAsync(entityType, restartIdentity, cascade));
        await Assert.ThrowsAsync<NotSupportedException>(() => context.TruncateRow.TruncateAsync(restartIdentity, cascade));
        Assert.Equal(ConnectionState.Closed, context.Database.GetDbConnection().State);
    }

    public static TheoryData<DbDriver, string?> CascadeCases => TruncateTestCases.GetCascadeCases(Schemas);

    [Theory]
    [MemberData(nameof(CascadeCases))]
    public async Task TruncateAsync_CascadeRemovesReferencingRows(DbDriver driver, string? schema)
    {
        Assert.SkipMySql(driver);
        using var session = await Fixture.TruncateSessions.CreateSessionAsync(driver, schema,
            [typeof(TruncateParent), typeof(TruncateChild)], cancellationToken: CancellationToken);
        await using var context = session.CreateDbContext(Fixture);
        try
        {
            await context.TruncateChild.ExecuteDeleteAsync(CancellationToken);
            await context.TruncateParent.ExecuteDeleteAsync(CancellationToken);
            var parent = new TruncateParent();
            context.TruncateParent.Add(parent);
            await context.SaveChangesAsync(CancellationToken);
            context.TruncateChild.Add(new TruncateChild
            {
                ParentId = parent.Id,
            });
            await context.SaveChangesAsync(CancellationToken);
            Assert.Equal(1, await context.TruncateParent.CountAsync(CancellationToken));
            Assert.Equal(1, await context.TruncateChild.CountAsync(CancellationToken));
            await context.TruncateAsync<TruncateParent>(driver == DbDriver.Npgsql, true, CancellationToken);
            Assert.Equal(0, await context.TruncateParent.CountAsync(CancellationToken));
            Assert.Equal(0, await context.TruncateChild.CountAsync(CancellationToken));
        }
        finally
        {
            await context.TruncateChild.ExecuteDeleteAsync();
            await context.TruncateParent.ExecuteDeleteAsync();
        }
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task TruncateAsync_HandlesTablesWithoutIdentity(DbDriver driver, string? schema)
    {
        Assert.SkipMySql(driver);
        using var session = await Fixture.TruncateSessions.CreateSessionAsync(driver, schema, [typeof(TruncateManualRow)], cancellationToken: CancellationToken);
        await using var context = session.CreateDbContext(Fixture);
        try
        {
            await context.TruncateManualRow.ExecuteDeleteAsync(CancellationToken);
            context.TruncateManualRow.AddRange(
                new TruncateManualRow { Id = 1 },
                new TruncateManualRow { Id = 2 });
            await context.SaveChangesAsync(CancellationToken);
            Assert.Equal(2, await context.TruncateManualRow.CountAsync(CancellationToken));
            await context.TruncateAsync<TruncateManualRow>(CancellationToken);
            Assert.Equal(0, await context.TruncateManualRow.CountAsync(CancellationToken));
        }
        finally
        {
            await context.TruncateManualRow.ExecuteDeleteAsync();
        }
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task TruncateAsync_HandlesKeylessTables(DbDriver driver, string? schema)
    {
        Assert.SkipMySql(driver);
        using var session = await Fixture.TruncateSessions.CreateSessionAsync(driver, schema, [typeof(TruncateKeylessRow)], cancellationToken: CancellationToken);
        await using var context = session.CreateDbContext(Fixture);
        var entityType = context.Model.FindEntityType(typeof(TruncateKeylessRow))!;
        var sqlHelper = context.GetService<ISqlGenerationHelper>();
        var table = sqlHelper.DelimitIdentifier(entityType.GetTableName()!, entityType.GetSchema());
        var value = sqlHelper.DelimitIdentifier(nameof(TruncateKeylessRow.Value));
        var insertSql = $"INSERT INTO {table} ({value}) VALUES ({{0}})";
        try
        {
            await context.TruncateKeylessRow.ExecuteDeleteAsync(CancellationToken);
            await context.Database.ExecuteSqlRawAsync(insertSql, new object[] { 1 }, CancellationToken);
            Assert.Equal(1, await context.TruncateKeylessRow.CountAsync(CancellationToken));
            await context.TruncateAsync<TruncateKeylessRow>(CancellationToken);
            Assert.Equal(0, await context.TruncateKeylessRow.CountAsync(CancellationToken));
        }
        finally
        {
            await context.TruncateKeylessRow.ExecuteDeleteAsync();
        }
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task TruncateAsync_AllEntryPointsObserveCancellationWithoutRemovingRows(DbDriver driver, string? schema)
    {
        Assert.SkipMySql(driver);
        using var session = await Fixture.TruncateSessions.CreateSessionAsync(driver, schema, [typeof(TruncateRow)], cancellationToken: CancellationToken);
        await using var context = session.CreateDbContext(Fixture);
        await using var cleanup = CleanupRows(context);
        var tracked = await SeedAsync(context);
        var entityType = context.Model.FindEntityType(typeof(TruncateRow))!;
        var restart = driver != DbDriver.Oracle;
        var token = new CancellationToken(true);
        Func<Task>[] truncations =
        [
            () => context.TruncateAsync<TruncateRow>(token),
            () => context.TruncateAsync<TruncateRow>(restart, false, token),
            () => context.TruncateAsync(typeof(TruncateRow), token),
            () => context.TruncateAsync(typeof(TruncateRow), restart, false, token),
            () => context.TruncateAsync(entityType, token),
            () => context.TruncateAsync(entityType, restart, false, token),
            () => context.TruncateRow.TruncateAsync(token),
            () => context.TruncateRow.TruncateAsync(restart, false, token),
        ];
        foreach (var truncate in truncations)
            await Assert.ThrowsAnyAsync<OperationCanceledException>(truncate);
        Assert.Equal(2, await context.TruncateRow.CountAsync(CancellationToken));
        Assert.Equal(EntityState.Unchanged, context.Entry(tracked).State);
    }

    [Fact]
    public async Task TruncateAsync_ValidatesNullArguments()
    {
        await using var context = Fixture.CreateDbContext(DbDriver.SqlServer);
        await Assert.ThrowsAsync<ArgumentNullException>(() => ((DbContext)null!).TruncateAsync<TruncateRow>());
        await Assert.ThrowsAsync<ArgumentNullException>(() => context.TruncateAsync((Type)null!));
        await Assert.ThrowsAsync<ArgumentNullException>(() => ((DbContext)null!).TruncateAsync<TruncateRow>(true, false));
        await Assert.ThrowsAsync<ArgumentNullException>(() => ((DbContext)null!).TruncateAsync(typeof(TruncateRow), true, false));
        await Assert.ThrowsAsync<ArgumentNullException>(() => context.TruncateAsync((Type)null!, true, false));
    }
}
