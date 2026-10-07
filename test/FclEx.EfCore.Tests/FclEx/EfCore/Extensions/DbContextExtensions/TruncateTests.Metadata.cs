using Microsoft.EntityFrameworkCore.Metadata;

namespace FclEx.EfCore.Extensions.DbContextExtensions;

public partial class TruncateTests
{
    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task TruncateAsync_MetadataOverloadRemovesAllRows(DbDriver driver, string? schema)
    {
        Assert.SkipWhen(driver == DbDriver.MySql, "MySql.Data asynchronous timeout cleanup can hang.");
        using var session = await Fixture.TruncateTables.CreateSessionAsync(driver, schema, [typeof(EntityWithAutoKey)], CancellationToken);
        await using var context = session.CreateDbContext(Fixture, schema);
        var entityType = context.Model.FindEntityType(typeof(EntityWithAutoKey))!;
        await VerifyTruncationAsync(context, token => context.TruncateAsync(entityType, token));
    }

    [Theory]
    [MemberData(nameof(OptionCases))]
    public async Task TruncateAsync_MetadataOptionsHonorDatabaseCapabilities(
        DbDriver driver, string? schema, bool restartIdentity, bool cascade)
    {
        Assert.SkipWhen(driver == DbDriver.MySql, "MySql.Data asynchronous timeout cleanup can hang.");
        using var session = await Fixture.TruncateTables.CreateSessionAsync(driver, schema, [typeof(EntityWithAutoKey)], CancellationToken);
        await using var context = session.CreateDbContext(Fixture, schema);
        var entityType = context.Model.FindEntityType(typeof(EntityWithAutoKey))!;
        await VerifyTruncationAsync(context,
            token => context.TruncateAsync(entityType, restartIdentity, cascade, token), restartIdentity, cascade);
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task TruncateAsync_MetadataOverloadsPreserveNamedSharedTypeIdentity(DbDriver driver, string? schema)
    {
        Assert.SkipWhen(driver == DbDriver.MySql, "MySql.Data asynchronous timeout cleanup can hang.");
        using var session = await Fixture.TruncateTables.CreateSessionAsync(driver, schema, [typeof(EntityWithAutoKey), typeof(EntityWithIdAndIndex)], CancellationToken);
        await using var context = session.CreateDbContext(Fixture, schema);
        await VerifyNamedSharedTypeAsync(context, false);
    }

    internal static async Task VerifyNamedSharedTypeAsync(TestDbContext context, bool useDbSet)
    {
        // Alternative entity names share this session's isolated TestDbContext tables.
        await using var namedContext = CreateModelContext(context, builder =>
        {
            foreach (var (name, clrType) in new[]
            {
                ("First", typeof(EntityWithIdAndIndex)), ("Second", typeof(EntityWithAutoKey)),
            })
            {
                var table = context.Model.FindEntityType(clrType)!;
                builder.SharedTypeEntity<Dictionary<string, object>>(name, entity =>
                {
                    entity.IndexerProperty<int>("Id");
                    entity.IndexerProperty<string>("Name").IsRequired(name == "First");
                    entity.IndexerProperty<int>("Value");
                    entity.HasKey("Id");
                    entity.ToTable(table.GetTableName()!, table.GetSchema());
                    if (name == "Second")
                        entity.HasQueryFilter(row => (int)row["Value"] > 0);
                });
            }
        });
        await Assert.ThrowsAsync<InvalidOperationException>(() => namedContext.TruncateAsync<EntityWithAutoKey>());
        await Assert.ThrowsAsync<InvalidOperationException>(() => namedContext.TruncateAsync<Dictionary<string, object>>());
        await Assert.ThrowsAsync<InvalidOperationException>(() => namedContext.TruncateAsync<EntityWithAutoKey>(true, false));
        await Assert.ThrowsAsync<InvalidOperationException>(() => namedContext.TruncateAsync(typeof(Dictionary<string, object>), true, false));

        var firstName = Guid.NewGuid().ToString();
        var nextName = Guid.NewGuid().ToString();
        try
        {
            var first = new EntityWithIdAndIndex { Name = firstName, Value = 1 };
            context.EntityWithIdAndIndex.Add(first);
            await context.SaveChangesAsync(CancellationToken);
            var firstId = first.Id;
            var second = namedContext.Model.FindEntityType("Second")!;
            var set = namedContext.Set<Dictionary<string, object>>("Second");
            for (var explicitOptions = 0; explicitOptions < 2; explicitOptions++)
            {
                var restart = context.DbDriver != DbDriver.Oracle;
                var cascade = context.DbDriver is DbDriver.Npgsql or DbDriver.Oracle;
                await VerifyTruncationAsync(context, async token =>
                {
                    // The filtered-out row must also be removed by truncation.
                    Assert.Equal(1, await set.CountAsync(token));
                    if (useDbSet)
                    {
                        if (explicitOptions == 0)
                            await set.TruncateAsync(token);
                        else
                            await set.TruncateAsync(restart, cascade, token);
                    }
                    else if (explicitOptions == 0)
                        await namedContext.TruncateAsync(second, token);
                    else
                        await namedContext.TruncateAsync(second, restart, cascade, token);
                }, explicitOptions == 0 ? null : restart, explicitOptions != 0 && cascade);
                Assert.Equal(1, await context.EntityWithIdAndIndex.CountAsync(row => row.Name == firstName, CancellationToken));
            }
            var next = new EntityWithIdAndIndex { Name = nextName, Value = 2 };
            context.EntityWithIdAndIndex.Add(next);
            await context.SaveChangesAsync(CancellationToken);
            Assert.True(next.Id > firstId);
        }
        finally
        {
            await context.EntityWithIdAndIndex.Where(row => row.Name == firstName || row.Name == nextName).ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task TruncateAsync_MetadataOverloadsValidateNullArguments()
    {
        await using var context = Fixture.CreateDbContext(DbDriver.SqlServer);
        var entityType = context.Model.FindEntityType(typeof(EntityWithAutoKey))!;
        await Assert.ThrowsAsync<ArgumentNullException>(() => context.TruncateAsync((IEntityType)null!));
        await Assert.ThrowsAsync<ArgumentNullException>(() => context.TruncateAsync((IEntityType)null!, true, false));
        await Assert.ThrowsAsync<ArgumentNullException>(() => ((DbContext)null!).TruncateAsync(entityType));
        await Assert.ThrowsAsync<ArgumentNullException>(() => ((DbContext)null!).TruncateAsync(entityType, true, false));
    }

    [Fact]
    public async Task TruncateAsync_MetadataOverloadsRejectMetadataFromAnotherModel()
    {
        await using var context = Fixture.CreateDbContext(DbDriver.SqlServer);
        await using var other = Fixture.CreateDbContext(DbDriver.SqlServer, Schemas.Single(schema => schema is not null));
        var entityType = other.Model.FindEntityType(typeof(EntityWithAutoKey))!;
        var nativeException = await Assert.ThrowsAsync<ArgumentException>(() => context.TruncateAsync(entityType));
        var optionsException = await Assert.ThrowsAsync<ArgumentException>(() => context.TruncateAsync(entityType, true, false));
        Assert.Equal("entityType", nativeException.ParamName);
        Assert.Equal("entityType", optionsException.ParamName);
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task TruncateAsync_MetadataOverloadsAcceptMetadataFromSharedModel(DbDriver driver, string? schema)
    {
        Assert.SkipWhen(driver == DbDriver.MySql, "MySql.Data asynchronous timeout cleanup can hang.");
        using var session = await Fixture.TruncateTables.CreateSessionAsync(driver, schema, [typeof(EntityWithAutoKey)], CancellationToken);
        await using var context = session.CreateDbContext(Fixture, schema);
        await using var other = context.CreateSharedModelContext();
        Assert.Same(context.Model, other.Model);
        var entityType = other.Model.FindEntityType(typeof(EntityWithAutoKey))!;
        await VerifyTruncationAsync(context, token => context.TruncateAsync(entityType, token));
        var restart = driver != DbDriver.Oracle;
        await VerifyTruncationAsync(context, token => context.TruncateAsync(entityType, restart, false, token), restart);
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task TruncateAsync_MetadataOverloadsObserveCancellationWithoutRemovingRows(DbDriver driver, string? schema)
    {
        Assert.SkipWhen(driver == DbDriver.MySql, "MySql.Data asynchronous timeout cleanup can hang.");
        using var session = await Fixture.TruncateTables.CreateSessionAsync(driver, schema, [typeof(EntityWithAutoKey)], CancellationToken);
        await using var context = session.CreateDbContext(Fixture, schema);
        var entityType = context.Model.FindEntityType(typeof(EntityWithAutoKey))!;
        try
        {
            var tracked = await SeedAsync(context);
            using var source = new CancellationTokenSource();
            source.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => context.TruncateAsync(entityType, source.Token));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => context.TruncateAsync(entityType, driver != DbDriver.Oracle, false, source.Token));
            Assert.Equal(2, await context.EntityWithAutoKey.CountAsync(CancellationToken));
            Assert.Equal(EntityState.Unchanged, context.Entry(tracked).State);
        }
        finally
        {
            await context.EntityWithAutoKey.ExecuteDeleteAsync();
        }
    }
}
