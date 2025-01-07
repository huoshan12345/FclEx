namespace FclEx.EfCore.Extensions;

public class ChangeTrackerExtensionsTests(EfCoreFixture fixture) : EfCoreTests(fixture)
{
    [Theory]
    [MemberData(nameof(DbTestCases))]
    public async Task SetsTimestamps_ForAddedEntities(DbProviderType dbProviderType)
    {
        await using var context = Fixture.CreateDbContext(dbProviderType);
        var entity = new EntityWithStates();
        context.Add(entity);

        context.ChangeTracker.ApplyEntityStateRules();

        Assert.NotEqual(default, entity.CreatedAt);
        Assert.NotEqual(default, entity.UpdatedAt);
        Assert.Equal(entity.CreatedAt, entity.UpdatedAt);
    }

    [Theory]
    [MemberData(nameof(DbTestCases))]
    public async Task UpdatesTimestamp_ForModifiedEntities(DbProviderType dbProviderType)
    {
        await using var context = Fixture.CreateDbContext(dbProviderType);
        var entity = new EntityWithStates { CreatedAt = DateTimeOffset.UtcNow.AddDays(-1) };
        context.Add(entity);
        await context.SaveChangesAsync();

        context.Update(entity);

        context.ChangeTracker.ApplyEntityStateRules();

        Assert.NotEqual(default, entity.UpdatedAt);
        Assert.Equal(default, entity.DeletedAt);
        Assert.False(entity.IsDeleted);
    }

    [Theory]
    [MemberData(nameof(DbTestCases))]
    public async Task HandlesSoftDelete_ForDeletedEntities(DbProviderType dbProviderType)
    {
        await using var context = Fixture.CreateDbContext(dbProviderType);
        var entity = new EntityWithStates { CreatedAt = DateTimeOffset.UtcNow };
        context.Add(entity);
        await context.SaveChangesAsync();

        context.Remove(entity);

        context.ChangeTracker.ApplyEntityStateRules();

        Assert.True(entity.IsDeleted);
        Assert.NotEqual(default, entity.DeletedAt);
        Assert.Contains(context.ChangeTracker.Entries<EntityWithStates>(), e => e.State == EntityState.Modified);
    }

    [Theory]
    [MemberData(nameof(DbTestCases))]
    public async Task DoesNotModifyUntrackedOrUnchangedEntities(DbProviderType dbProviderType)
    {
        await using var context = Fixture.CreateDbContext(dbProviderType);
        var entity = new EntityWithStates { CreatedAt = DateTimeOffset.UtcNow };
        context.Add(entity);
        await context.SaveChangesAsync();

        context.ChangeTracker.ApplyEntityStateRules();

        Assert.Equal(entity.CreatedAt, entity.CreatedAt);
        Assert.Equal(default, entity.DeletedAt);
        Assert.False(entity.IsDeleted);
    }
}