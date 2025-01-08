namespace FclEx.EfCore;

public class EfCoreTests(EfCoreFixture fixture) : DatabaseTests, ICollectionFixture<EfCoreFixture>
{
    public EfCoreFixture Fixture { get; } = fixture;

    protected async Task<EntityHasStates> CreateEntityHasStatesAsync(DbProviderType dbProviderType)
    {
        var entity = new EntityHasStates { Name = Guid.NewGuid().ToString() };
        await using var context = Fixture.CreateDbContext(dbProviderType);
        context.EntityHasStates.Add(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    protected async Task<EntityWithNavigation> CreateEntityWithNavigationAsync(DbProviderType dbProviderType)
    {
        var entity = new EntityWithNavigation
        {
            Name = Guid.NewGuid().ToString(),
            Navigation = new EntityHasStates
            {
                Name = Guid.NewGuid().ToString(),
            },
        };
        await using var context = Fixture.CreateDbContext(dbProviderType);
        context.Add(entity);
        await context.SaveChangesAsync();
        return entity;
    }
}