namespace FclEx.EfCore.Extensions.DbContextExtensions;

public partial class TruncateTests(EfCoreFixture fixture) : EfCoreTests(fixture)
{
    public static TheoryData<DbDriver, string?, bool, bool> OptionCases
    {
        get
        {
            var cases = new TheoryData<DbDriver, string?, bool, bool>();
            foreach (var (driver, schema) in GetDriverSchemaCases(Schemas))
            foreach (var restart in new[] { false, true })
            foreach (var cascade in new[] { false, true })
                cases.Add(driver, schema, restart, cascade);
            return cases;
        }
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task TruncateAsync_RemovesAllRowsAndPreservesTrackedEntities(DbDriver driver, string? schema)
    {
        Assert.SkipWhen(driver == DbDriver.MySql, "MySql.Data asynchronous timeout cleanup can hang.");
        using var session = await Fixture.TruncateTables.CreateSessionAsync(driver, schema, [typeof(EntityWithAutoKey)], CancellationToken);
        await using var context = session.CreateDbContext(Fixture, schema);
        await VerifyTruncationAsync(context, token => context.TruncateAsync<EntityWithAutoKey>(token));
        await VerifyTruncationAsync(context, token => context.TruncateAsync(typeof(EntityWithAutoKey), token));
    }

    [Theory]
    [MemberData(nameof(OptionCases))]
    public async Task TruncateAsync_ExplicitOptionsHonorDatabaseCapabilities(
        DbDriver driver, string? schema, bool restartIdentity, bool cascade)
    {
        Assert.SkipWhen(driver == DbDriver.MySql, "MySql.Data asynchronous timeout cleanup can hang.");
        using var session = await Fixture.TruncateTables.CreateSessionAsync(driver, schema, [typeof(EntityWithAutoKey)], CancellationToken);
        await using var context = session.CreateDbContext(Fixture, schema);
        await VerifyTruncationAsync(context,
            token => context.TruncateAsync<EntityWithAutoKey>(restartIdentity, cascade, token), restartIdentity, cascade);
        await VerifyTruncationAsync(context,
            token => context.TruncateAsync(typeof(EntityWithAutoKey), restartIdentity, cascade, token), restartIdentity, cascade);
    }

    internal static bool SupportsOptions(DbDriver driver, bool restartIdentity, bool cascade) => driver switch
    {
        DbDriver.Npgsql => true,
        DbDriver.Oracle => !restartIdentity,
        DbDriver.Sqlite => !cascade,
        _ => restartIdentity && !cascade,
    };

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task TruncateAsync_UsesConfiguredSchemaOnOrdinaryTable(DbDriver driver, string? schema)
    {
        Assert.SkipWhen(driver == DbDriver.MySql, "MySql.Data asynchronous timeout cleanup can hang.");
        using var session = Fixture.TruncateTables.CreateClosedConnectionSession(driver, schema);
        await using var context = session.CreateDbContext(Fixture, schema);
        var entityType = context.Model.FindEntityType(typeof(EntityWithAutoKey))!;
        Assert.Equal(schema, entityType.GetSchema());
        await VerifyTruncationAsync(context, token => context.TruncateAsync<EntityWithAutoKey>(token));
        await VerifyTruncationAsync(context, token => context.TruncateAsync(typeof(EntityWithAutoKey), token));
        await VerifyTruncationAsync(context, token => context.TruncateAsync(entityType, token));
        await VerifyTruncationAsync(context, token => context.EntityWithAutoKey.TruncateAsync(token));
        var restart = driver != DbDriver.Oracle;
        await VerifyTruncationAsync(context, token => context.TruncateAsync<EntityWithAutoKey>(restart, false, token), restart);
        await VerifyTruncationAsync(context, token => context.TruncateAsync(typeof(EntityWithAutoKey), restart, false, token), restart);
        await VerifyTruncationAsync(context, token => context.TruncateAsync(entityType, restart, false, token), restart);
        await VerifyTruncationAsync(context, token => context.EntityWithAutoKey.TruncateAsync(restart, false, token), restart);
    }

    internal static async Task<EntityWithAutoKey> SeedAsync(TestDbContext context)
    {
        await context.EntityWithAutoKey.ExecuteDeleteAsync(CancellationToken);
        var first = new EntityWithAutoKey { Name = Guid.NewGuid().ToString(), Value = 1 };
        var second = new EntityWithAutoKey { Name = Guid.NewGuid().ToString(), Value = -1 };
        context.EntityWithAutoKey.AddRange(first, second);
        await context.SaveChangesAsync(CancellationToken);
        Assert.Equal(2, await context.EntityWithAutoKey.CountAsync(CancellationToken));
        Assert.True(second.Id > first.Id);
        return second;
    }

    // All overloads share this result-based contract against an isolated EntityWithAutoKey table.
    internal static async Task VerifyTruncationAsync(
        TestDbContext context, Func<CancellationToken, Task> truncate,
        bool? restartIdentity = null, bool cascade = false)
    {
        try
        {
            var tracked = await SeedAsync(context);
            var previousId = tracked.Id;
            if (restartIdentity is { } option && !SupportsOptions(context.DbDriver, option, cascade))
            {
                await Assert.ThrowsAsync<NotSupportedException>(() => truncate(CancellationToken));
                Assert.Equal(2, await context.EntityWithAutoKey.CountAsync(CancellationToken));
                Assert.Equal(EntityState.Unchanged, context.Entry(tracked).State);
                return;
            }

            await truncate(CancellationToken);
            Assert.Equal(0, await context.EntityWithAutoKey.CountAsync(CancellationToken));
            Assert.Equal(EntityState.Unchanged, context.Entry(tracked).State);
            Assert.Equal(previousId, tracked.Id);
            // Clear tracking only after checking the contract, before the database can reuse old keys.
            context.ChangeTracker.Clear();
            var next = new EntityWithAutoKey { Name = Guid.NewGuid().ToString(), Value = 1 };
            context.EntityWithAutoKey.Add(next);
            await context.SaveChangesAsync(CancellationToken);
            var resets = restartIdentity ?? context.DbDriver is not (DbDriver.Npgsql or DbDriver.Oracle);
            if (resets)
                Assert.Equal(1, next.Id);
            else
                Assert.True(next.Id > previousId, $"Expected identity to continue after {previousId}, got {next.Id}.");
        }
        finally
        {
            context.ChangeTracker.Clear();
            await context.EntityWithAutoKey.ExecuteDeleteAsync();
        }
    }

    public static TheoryData<DbDriver, string?> CascadeCases
    {
        get
        {
            var cases = GetDriverSchemaCases(Schemas)
                .Where(pair => pair.Driver is DbDriver.Npgsql or DbDriver.Oracle)
                .Select(pair => (pair.Driver, pair.Schema)).ToTheoryData();
            // xUnit 4's deferred zero-row theory reports a failed summary even with SkipTestWithoutData.
            // An explicitly skipped row preserves filtered runs when no cascade driver is selected.
            if (cases.Count == 0)
                cases.Add(new TheoryDataRow<DbDriver, string?>(SelectedDrivers.FirstOrDefault(), null)
                {
                    Skip = "No selected driver supports TRUNCATE CASCADE.",
                });
            return cases;
        }
    }

    [Theory]
    [MemberData(nameof(CascadeCases))]
    public async Task TruncateAsync_CascadeRemovesReferencingRows(DbDriver driver, string? schema)
    {
        Assert.SkipWhen(driver == DbDriver.MySql, "MySql.Data asynchronous timeout cleanup can hang.");
        using var session = await Fixture.TruncateTables.CreateSessionAsync(driver, schema,
            [typeof(EntityHasStates), typeof(EntityWithNavigation)], CancellationToken);
        await using var context = session.CreateDbContext(Fixture, schema);
        try
        {
            await context.EntityWithNavigation.ExecuteDeleteAsync(CancellationToken);
            await context.EntityHasStates.ExecuteDeleteAsync(CancellationToken);
            var parent = new EntityHasStates { Name = Guid.NewGuid().ToString() };
            context.EntityWithNavigation.Add(new EntityWithNavigation
            {
                Name = Guid.NewGuid().ToString(), Navigation = parent,
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
        Assert.SkipWhen(driver == DbDriver.MySql, "MySql.Data asynchronous timeout cleanup can hang.");
        using var session = await Fixture.TruncateTables.CreateSessionAsync(driver, schema, [typeof(EntityWithGuidKey)], CancellationToken);
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
        Assert.SkipWhen(driver == DbDriver.MySql, "MySql.Data asynchronous timeout cleanup can hang.");
        using var session = await Fixture.TruncateTables.CreateSessionAsync(driver, schema, [typeof(EntityWithoutKey)], CancellationToken);
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
    public async Task TruncateAsync_ObservesCancellationWithoutRemovingRows(DbDriver driver, string? schema)
    {
        Assert.SkipWhen(driver == DbDriver.MySql, "MySql.Data asynchronous timeout cleanup can hang.");
        using var session = await Fixture.TruncateTables.CreateSessionAsync(driver, schema, [typeof(EntityWithAutoKey)], CancellationToken);
        await using var context = session.CreateDbContext(Fixture, schema);
        try
        {
            var tracked = await SeedAsync(context);
            using var source = new CancellationTokenSource();
            source.Cancel();
            var restart = driver != DbDriver.Oracle;
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => context.TruncateAsync<EntityWithAutoKey>(source.Token));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => context.TruncateAsync<EntityWithAutoKey>(restart, false, source.Token));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => context.TruncateAsync(typeof(EntityWithAutoKey), restart, false, source.Token));
            Assert.Equal(2, await context.EntityWithAutoKey.CountAsync(CancellationToken));
            Assert.Equal(EntityState.Unchanged, context.Entry(tracked).State);
        }
        finally
        {
            await context.EntityWithAutoKey.ExecuteDeleteAsync();
        }
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
