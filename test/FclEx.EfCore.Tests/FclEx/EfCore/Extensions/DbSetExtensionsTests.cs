using FclEx.EfCore.Extensions.DbContextExtensions;

namespace FclEx.EfCore.Extensions;

[TestClass(DisableParallelization = true)]
public class DbSetExtensionsTests(EfCoreFixture fixture) : EfCoreTests(fixture)
{
    [Theory(DisableParallelization = true)]
    [MemberData(nameof(TruncateTests.OptionCases), MemberType = typeof(TruncateTests))]
    public async Task TruncateAsync_ExplicitOptionsHonorDatabaseCapabilities(
        DbDriver driver, string? schema, bool restartIdentity, bool cascade)
    {
        await using var context = Fixture.CreateDbContext(driver, schema);
        await TruncateTests.VerifyTruncationAsync(context,
            token => context.EntityWithAutoKey.TruncateAsync(restartIdentity, cascade, token), restartIdentity, cascade);
    }

    [Theory(DisableParallelization = true)]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task TruncateAsync_RemovesAllRowsAndPreservesTrackedEntities(DbDriver driver, string? schema)
    {
        await using var context = Fixture.CreateDbContext(driver, schema);
        await TruncateTests.VerifyTruncationAsync(context, token => context.EntityWithAutoKey.TruncateAsync(token));
    }

    [Theory(DisableParallelization = true)]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task TruncateAsync_PreservesNamedSharedTypeEntityIdentity(DbDriver driver, string? schema)
    {
        await using var context = Fixture.CreateDbContext(driver, schema);
        await TruncateTests.VerifyNamedSharedTypeAsync(context, true);
    }

    [Fact]
    public async Task TruncateAsync_ValidatesNullSet()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => ((DbSet<EntityWithAutoKey>)null!).TruncateAsync());
        await Assert.ThrowsAsync<ArgumentNullException>(() => ((DbSet<EntityWithAutoKey>)null!).TruncateAsync(true, false));
    }

    [Fact]
    public async Task TruncateAsync_RejectsSharedPhysicalTable()
    {
        await using var context = TruncateTests.CreateMappingContext(Fixture, "owned");
        await Assert.ThrowsAsync<NotSupportedException>(() => context.Set<TruncateTests.MappingEntity>().TruncateAsync());
        await Assert.ThrowsAsync<NotSupportedException>(() => context.Set<TruncateTests.MappingEntity>().TruncateAsync(true, false));
    }

    [Theory(DisableParallelization = true)]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task TruncateAsync_ObservesCancellationWithoutRemovingRows(DbDriver driver, string? schema)
    {
        await using var context = Fixture.CreateDbContext(driver, schema);
        try
        {
            var tracked = await TruncateTests.SeedAsync(context);
            using var source = new CancellationTokenSource();
            source.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => context.EntityWithAutoKey.TruncateAsync(source.Token));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => context.EntityWithAutoKey.TruncateAsync(driver != DbDriver.Oracle, false, source.Token));
            Assert.Equal(2, await context.EntityWithAutoKey.CountAsync(CancellationToken));
            Assert.Equal(EntityState.Unchanged, context.Entry(tracked).State);
        }
        finally
        {
            await context.EntityWithAutoKey.ExecuteDeleteAsync();
        }
    }
}
