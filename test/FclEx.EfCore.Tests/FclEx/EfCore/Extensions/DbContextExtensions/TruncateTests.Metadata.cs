using Microsoft.EntityFrameworkCore.Metadata;

namespace FclEx.EfCore.Extensions.DbContextExtensions;

public partial class TruncateTests
{
    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task TruncateAsync_MetadataOverloadsUseNamedSharedTypeTableAndBypassQueryFilter(DbDriver driver, string? schema)
    {
        Assert.SkipMySql(driver);
        using var session = await Fixture.TruncateSessions.CreateSessionAsync(driver, schema,
            [typeof(TruncateRow), typeof(TruncateOtherRow)], cancellationToken: CancellationToken);
        await using var context = session.CreateDbContext(Fixture);
        await VerifyNamedSharedTypeAsync(context, false);
    }

    internal static async Task VerifyNamedSharedTypeAsync(TestDbContext context, bool useDbSet)
    {
        await using var namedContext = CreateNamedSharedTypeContext(context);
        await using var cleanup = CleanupRows(context);
        try
        {
            var first = new TruncateOtherRow { Value = 1 };
            context.TruncateOtherRow.Add(first);
            await context.SaveChangesAsync(CancellationToken);
            var second = namedContext.Model.FindEntityType("Second")!;
            var set = namedContext.Set<Dictionary<string, object>>("Second");
            for (var explicitOptions = 0; explicitOptions < 2; explicitOptions++)
            {
                var restart = context.DbDriver != DbDriver.Oracle;
                var cascade = context.DbDriver is DbDriver.Npgsql or DbDriver.Oracle;
                await VerifyRowsRemovedAsync(context, async token =>
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
                });
                Assert.Equal(1, await context.TruncateOtherRow.CountAsync(CancellationToken));
            }
        }
        finally
        {
            await context.TruncateOtherRow.ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task TruncateAsync_ClrTypeOverloadsRejectNamedSharedTypes()
    {
        await using var reference = new TestDbContext(DbDriver.SqlServer, Fixture.ResolveTarget(DbDriver.SqlServer).BuildConnectionString());
        await using var namedContext = CreateNamedSharedTypeContext(reference);
        await Assert.ThrowsAsync<InvalidOperationException>(() => namedContext.TruncateAsync<TruncateRow>());
        await Assert.ThrowsAsync<InvalidOperationException>(() => namedContext.TruncateAsync<Dictionary<string, object>>());
        await Assert.ThrowsAsync<InvalidOperationException>(() => namedContext.TruncateAsync<TruncateRow>(true, false));
        await Assert.ThrowsAsync<InvalidOperationException>(() => namedContext.TruncateAsync(typeof(Dictionary<string, object>), true, false));
    }

    private static DbContext CreateNamedSharedTypeContext(TestDbContext context)
        => CreateModelContext(context, builder =>
        {
            foreach (var (name, clrType) in new[]
            {
                ("First", typeof(TruncateOtherRow)), ("Second", typeof(TruncateRow)),
            })
            {
                var table = context.Model.FindEntityType(clrType)!;
                builder.SharedTypeEntity<Dictionary<string, object>>(name, entity =>
                {
                    entity.IndexerProperty<int>("Id");
                    entity.IndexerProperty<int>("Value");
                    entity.HasKey("Id");
                    entity.ToTable(table.GetTableName()!, table.GetSchema());
                    if (name == "Second")
                        entity.HasQueryFilter(row => (int)row["Value"] > 0);
                });
            }
        });

    [Fact]
    public async Task TruncateAsync_MetadataOverloadsValidateNullArguments()
    {
        await using var context = new TestDbContext(DbDriver.SqlServer, Fixture.ResolveTarget(DbDriver.SqlServer).BuildConnectionString());
        var entityType = context.Model.FindEntityType(typeof(TruncateRow))!;
        await Assert.ThrowsAsync<ArgumentNullException>(() => context.TruncateAsync((IEntityType)null!));
        await Assert.ThrowsAsync<ArgumentNullException>(() => context.TruncateAsync((IEntityType)null!, true, false));
        await Assert.ThrowsAsync<ArgumentNullException>(() => ((DbContext)null!).TruncateAsync(entityType));
        await Assert.ThrowsAsync<ArgumentNullException>(() => ((DbContext)null!).TruncateAsync(entityType, true, false));
    }

    [Fact]
    public async Task TruncateAsync_MetadataOverloadsRejectMetadataFromAnotherModel()
    {
        await using var context = new TestDbContext(DbDriver.SqlServer, Fixture.ResolveTarget(DbDriver.SqlServer).BuildConnectionString());
        await using var other = new TestDbContext(DbDriver.SqlServer,
            Fixture.ResolveTarget(DbDriver.SqlServer, Schemas.Single(schema => schema is not null)).BuildConnectionString(),
            Schemas.Single(schema => schema is not null));
        var entityType = other.Model.FindEntityType(typeof(TruncateRow))!;
        var nativeException = await Assert.ThrowsAsync<ArgumentException>(() => context.TruncateAsync(entityType));
        var optionsException = await Assert.ThrowsAsync<ArgumentException>(() => context.TruncateAsync(entityType, true, false));
        Assert.Equal("entityType", nativeException.ParamName);
        Assert.Equal("entityType", optionsException.ParamName);
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task TruncateAsync_MetadataOverloadsAcceptMetadataFromSharedModel(DbDriver driver, string? schema)
    {
        Assert.SkipMySql(driver);
        using var session = await Fixture.TruncateSessions.CreateSessionAsync(driver, schema, [typeof(TruncateRow)], cancellationToken: CancellationToken);
        await using var context = session.CreateDbContext(Fixture);
        await using var cleanup = CleanupRows(context);
        await using var other = context.CreateSharedModelContext();
        Assert.Same(context.Model, other.Model);
        var entityType = other.Model.FindEntityType(typeof(TruncateRow))!;
        await VerifyRowsRemovedAsync(context, token => context.TruncateAsync(entityType, token));
        var restart = driver != DbDriver.Oracle;
        await VerifyRowsRemovedAsync(context, token => context.TruncateAsync(entityType, restart, false, token));
    }
}
