namespace FclEx.EfCore.Extensions.DbContextExtensions;

public class TestEntityTests(EfCoreFixture fixture) : EfCoreTests(fixture)
{
    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public void TestEntity_AddsEntityUntilHandleIsDisposed(DbDriver dbDriver)
    {
        using var context = Fixture.CreateDbContext(dbDriver);

        var handle = context.TestEntity<EntityWithAutoKey>();

        Assert.Equal(EntityState.Added, Assert.Single(context.ChangeTracker.Entries<EntityWithAutoKey>()).State);

        handle.Dispose();

        Assert.Empty(context.ChangeTracker.Entries<EntityWithAutoKey>());
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task TestEntities_InsertsAndRemovesEachEntityType(DbDriver dbDriver)
    {
        await using var context = Fixture.CreateDbContext(dbDriver);

        await context.TestEntities(typeof(EntityWithAutoKey), typeof(EntityWithAutoKey));

        Assert.Empty(context.ChangeTracker.Entries());
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task TestEntities_ObservesCancellationToken(DbDriver dbDriver)
    {
        await using var context = Fixture.CreateDbContext(dbDriver);
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            context.TestEntities(cancellation.Token, typeof(EntityWithAutoKey)));
    }
}
