namespace FclEx.EfCore.Extensions;

public class DbContextExtensionsTests(EfCoreFixture fixture) : EfCoreTests(fixture)
{
    [Theory]
    [MemberData(nameof(DbTestCases))]
    public async Task ReturnsExistingEntity_WhenEntityExists(DbProviderType dbProviderType)
    {
        var name = Guid.NewGuid().ToString();
        {
            await using var context = Fixture.CreateDbContext(dbProviderType);
            var existingEntity = new EntityHasStates { Name = name };
            context.Add(existingEntity);
            await context.SaveChangesAsync();
        }

        {
            await using var context = Fixture.CreateDbContext(dbProviderType);
            var result = await context.GetOrAddAsync(m => m.Name == name, () => new EntityHasStates { Name = "New" });

            Assert.NotNull(result);
            Assert.Equal(name, result.Name);
        }
    }

    [Theory]
    [MemberData(nameof(DbTestCases))]
    public async Task AddsAndReturnsNewEntity_WhenEntityDoesNotExist(DbProviderType dbProviderType)
    {
        var name = Guid.NewGuid().ToString();
        {
            await using var context = Fixture.CreateDbContext(dbProviderType);
            await context.GetOrAddAsync(m => m.Name == name, () => new EntityHasStates { Name = name });

        }

        {
            await using var context = Fixture.CreateDbContext(dbProviderType);
            var result = await context.EntityHasStates.FirstOrDefaultAsync(m => m.Name == name);

            Assert.NotNull(result);
            Assert.Equal(name, result.Name);
        }
    }


    [Theory]
    [MemberData(nameof(DbTestCases))]
    public async Task SaveAsync_ShouldAddEntity_WhenIdIsDefault(DbProviderType dbProviderType)
    {
        var entity = new EntityHasStates();
        {
            Assert.Equal(default, entity.CreatedAt);
            Assert.Equal(default, entity.UpdatedAt);

            await using var context = Fixture.CreateDbContext(dbProviderType);
            var savedEntity = await context.SaveAsync(entity);

            Assert.NotNull(savedEntity);
            Assert.Equal(entity.Id, savedEntity.Id);
            Assert.NotEqual(default, savedEntity.CreatedAt);
            Assert.NotEqual(default, savedEntity.UpdatedAt);
            Assert.Equal(entity.CreatedAt, savedEntity.CreatedAt);
            Assert.Equal(entity.UpdatedAt, savedEntity.UpdatedAt);
        }

        {
            await using var context = Fixture.CreateDbContext(dbProviderType);
            var updatedEntity = await context.EntityHasStates.GetAsync(entity.Id);
            Assert.NotNull(updatedEntity);
            Assert.Equal(entity.Name, updatedEntity.Name);
            AssertExt.EqualWithinMs(entity.CreatedAt, updatedEntity.CreatedAt);
            AssertExt.EqualWithinMs(entity.UpdatedAt, updatedEntity.UpdatedAt);
        }
    }

    [Theory]
    [MemberData(nameof(DbTestCases))]
    public async Task SaveAsync_ShouldModifyEntity_WhenIdIsNotDefault(DbProviderType dbProviderType)
    {
        var entity = await CreateEntityHasStatesAsync(dbProviderType);
        var createdAt = entity.CreatedAt;
        var newName = Guid.NewGuid().ToString();
        {
            await using var context = Fixture.CreateDbContext(dbProviderType);
            entity.Name = newName;

            await Task.Delay(TimeSpan.FromMilliseconds(100)); // Ensure that the updated time is different
            await context.SaveAsync(entity);
            Assert.Equal(createdAt, entity.CreatedAt);
            AssertExt.NotEqualWithinMs(createdAt, entity.UpdatedAt);
        }

        {
            await using var context = Fixture.CreateDbContext(dbProviderType);
            var updatedEntity = await context.EntityHasStates.GetAsync(entity.Id);
            Assert.NotNull(updatedEntity);
            Assert.Equal(newName, updatedEntity.Name);
            AssertExt.EqualWithinMs(createdAt, updatedEntity.CreatedAt);
            AssertExt.EqualWithinMs(entity.UpdatedAt, updatedEntity.UpdatedAt);
        }
    }

    [Theory]
    [MemberData(nameof(DbTestCases))]
    public async Task SaveAsync_ShouldExcludeSpecifiedProperties_OnUpdate(DbProviderType dbProviderType)
    {
        var newName = Guid.NewGuid().ToString();
        var entity = await CreateEntityHasStatesAsync(dbProviderType);
        var name = entity.Name;
        {
            await using var context = Fixture.CreateDbContext(dbProviderType);
            entity.Name = newName;

            await context.SaveAsync(entity, nameof(entity.Name));
        }

        {
            await using var context = Fixture.CreateDbContext(dbProviderType);
            var updatedEntity = await context.EntityHasStates.GetAsync(entity.Id);
            Assert.NotNull(updatedEntity);
            Assert.Equal(newName, entity.Name);
            Assert.Equal(name, updatedEntity.Name);
        }
    }

    [Theory]
    [MemberData(nameof(DbTestCases))]
    public async Task SaveAsync_ShouldExcludeSpecifiedNavigation_OnUpdate(DbProviderType dbProviderType)
    {
        var entity = await CreateEntityWithNavigationAsync(dbProviderType);
        Assert.NotEqual(default, entity.NavigationId);
        Assert.NotNull(entity.Navigation);
        Assert.NotEqual(default, entity.Navigation.Id);

        var name = entity.Name;
        var navigationName = entity.Navigation.Name;
        var newName = Guid.NewGuid().ToString();
        var newNavigationName = Guid.NewGuid().ToString();

        {
            // create new db context to ensure that there is no tracking.
            await using var context = Fixture.CreateDbContext(dbProviderType);
            context.Add(entity);
            await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        }

        {
            // create new db context to ensure that there is no tracking.
            await using var context = Fixture.CreateDbContext(dbProviderType);
            entity.Name = newName;
            entity.Navigation.Name = newNavigationName;
            await context.SaveAsync(entity, nameof(entity.Navigation));
        }

        {
            await using var context = Fixture.CreateDbContext(dbProviderType);
            var updatedEntity = await context.EntityWithNavigation
                .Include(m => m.Navigation)
                .FirstOrDefaultAsync(m => m.Id == entity.Id);

            Assert.NotNull(updatedEntity);
            Assert.NotNull(updatedEntity.Navigation);

            Assert.Equal(newName, updatedEntity.Name);
            Assert.Equal(navigationName, updatedEntity.Navigation.Name);
        }
    }

    [Theory]
    [MemberData(nameof(DbTestCases))]
    public async Task SaveAsync_ShouldHandleNonExistingProperties(DbProviderType dbProviderType)
    {
        var entity = await CreateEntityHasStatesAsync(dbProviderType);
        await using var context = Fixture.CreateDbContext(dbProviderType);

        var result = await context.SaveAsync(entity, "NonExistingProperty");

        Assert.NotNull(result);
        Assert.Equal(entity.Id, result.Id);
    }
}