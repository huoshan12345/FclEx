using FclEx.EfCore.Extensions.DbContextExtensions;

namespace FclEx.EfCore.Extensions;

public class DbSetExtensionsTests(EfCoreFixture fixture) : EfCoreTests(fixture)
{
    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task TruncateAsync_UsesNamedSharedTypeTableAndBypassesQueryFilter(DbDriver driver, string? schema)
    {
        Assert.SkipMySql(driver);
        using var session = await Fixture.TruncateTables.CreateSessionAsync(driver, schema,
            [typeof(EntityWithAutoKey), typeof(EntityWithIdAndIndex)], cancellationToken: CancellationToken);
        await using var context = session.CreateDbContext(Fixture, schema);
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
}
