namespace FclEx.EfCore.Extensions;

public class ChangeTrackerExtensionsTests(EfCoreFixture fixture) : EfCoreTests(fixture)
{
    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task SetsTimestamps_ForAddedEntities(DbDriver dbDriver)
    {
        await using var context = Fixture.CreateDbContext(dbDriver);
        var entity = new EntityHasStates();
        context.Add(entity);

        context.ChangeTracker.ApplyEntityStateRules();

        Assert.NotEqual(default, entity.CreatedAt);
        Assert.NotEqual(default, entity.UpdatedAt);
        Assert.Equal(entity.CreatedAt, entity.UpdatedAt);
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task UpdatesTimestamp_ForModifiedEntities(DbDriver dbDriver)
    {
        await using var context = Fixture.CreateDbContext(dbDriver);
        var entity = new EntityHasStates { CreatedAt = DateTimeOffset.UtcNow.AddDays(-1) };
        context.Add(entity);
        await context.SaveChangesAsync();

        context.Update(entity);

        context.ChangeTracker.ApplyEntityStateRules();

        Assert.NotEqual(default, entity.UpdatedAt);
        Assert.Equal(default, entity.DeletedAt);
        Assert.False(entity.IsDeleted);
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task HandlesSoftDelete_ForDeletedEntities(DbDriver dbDriver)
    {
        await using var context = Fixture.CreateDbContext(dbDriver);
        var entity = new EntityHasStates { CreatedAt = DateTimeOffset.UtcNow };
        context.Add(entity);
        await context.SaveChangesAsync();

        context.Remove(entity);

        context.ChangeTracker.ApplyEntityStateRules();

        Assert.True(entity.IsDeleted);
        Assert.NotEqual(default, entity.DeletedAt);
        Assert.Contains(context.ChangeTracker.Entries<EntityHasStates>(), e => e.State == EntityState.Modified);
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task DoesNotModifyUnchangedEntities(DbDriver dbDriver)
    {
        await using var context = Fixture.CreateDbContext(dbDriver);
        var entity = new EntityHasStates { CreatedAt = DateTimeOffset.UtcNow };
        context.Add(entity);
        await context.SaveChangesAsync();
        var createdAt = entity.CreatedAt;
        var updatedAt = entity.UpdatedAt;

        context.ChangeTracker.ApplyEntityStateRules();

        Assert.Equal(createdAt, entity.CreatedAt);
        Assert.Equal(updatedAt, entity.UpdatedAt);
        Assert.Equal(default, entity.DeletedAt);
        Assert.False(entity.IsDeleted);
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task DoesNotModifyDetachedEntities(DbDriver dbDriver)
    {
        await using var context = Fixture.CreateDbContext(dbDriver);
        var entity = new EntityHasStates();
        context.Add(entity);
        await context.SaveChangesAsync();
        context.Entry(entity).State = EntityState.Detached;
        var updatedAt = entity.UpdatedAt;

        context.ChangeTracker.ApplyEntityStateRules();

        Assert.Equal(updatedAt, entity.UpdatedAt);
        Assert.Empty(context.ChangeTracker.Entries<EntityHasStates>());
    }
}
