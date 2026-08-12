namespace FclEx.EfCore.Extensions;

public partial class DbContextExtensionsTests(EfCoreFixture fixture) : EfCoreTests(fixture)
{
    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task ReturnsExistingEntity_WhenEntityExists(DbDriver dbDriver)
    {
        var name = Guid.NewGuid().ToString();
        {
            await using var context = Fixture.CreateDbContext(dbDriver);
            var existingEntity = new EntityHasStates { Name = name };
            context.Add(existingEntity);
            await context.SaveChangesAsync();
        }

        {
            await using var context = Fixture.CreateDbContext(dbDriver);
            var result = await context.GetOrAddAsync(m => m.Name == name, () => new EntityHasStates { Name = "New" });

            Assert.NotNull(result);
            Assert.Equal(name, result.Name);
        }
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task AddsAndReturnsNewEntity_WhenEntityDoesNotExist(DbDriver dbDriver)
    {
        var name = Guid.NewGuid().ToString();
        {
            await using var context = Fixture.CreateDbContext(dbDriver);
            await context.GetOrAddAsync(m => m.Name == name, () => new EntityHasStates { Name = name });

        }

        {
            await using var context = Fixture.CreateDbContext(dbDriver);
            var result = await context.EntityHasStates.FirstOrDefaultAsync(m => m.Name == name);

            Assert.NotNull(result);
            Assert.Equal(name, result.Name);
        }
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task GetOrAddAsync_ObservesCancellationToken(DbDriver dbDriver)
    {
        await using var context = Fixture.CreateDbContext(dbDriver);
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => context.GetOrAddAsync(
            entity => entity.Name == "cancelled",
            () => new EntityHasStates(),
            cancellation.Token));
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task InsertAsync_ObservesCancellationToken(DbDriver dbDriver)
    {
        await using var context = Fixture.CreateDbContext(dbDriver);
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            context.InsertAsync(new EntityHasStates(), cancellation.Token));
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task SaveAsync_ObservesCancellationToken(DbDriver dbDriver)
    {
        await using var context = Fixture.CreateDbContext(dbDriver);
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            context.SaveAsync(new EntityHasStates(), cancellation.Token));
    }

    private static void AssertDateTime(DbDriver dbDriver, DateTimeOffset expected, DateTimeOffset actual)
    {
        switch (dbDriver)
        {
            case DbDriver.MySql:
                // MySql.EntityFrameworkCore's bug, it stores DateTime with 6 digits of milliseconds, but when it retrieves, it strips milliseconds part.
                Assert.EqualToSeconds(expected, actual);
                break;
            case DbDriver.SqlServer:
                Assert.Equal(expected, actual);
                break;
            default:
                Assert.EqualToMilliseconds(expected, actual);
                break;
        }
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task SaveAsync_ShouldAddEntity_WhenIdIsDefault(DbDriver dbDriver)
    {
        var entity = new EntityHasStates();
        {
            Assert.Equal(default, entity.CreatedAt);
            Assert.Equal(default, entity.UpdatedAt);

            await using var context = Fixture.CreateDbContext(dbDriver);
            var savedEntity = await context.SaveAsync(entity);

            Assert.NotNull(savedEntity);
            Assert.Equal(entity.Id, savedEntity.Id);
            Assert.NotEqual(default, savedEntity.CreatedAt);
            Assert.NotEqual(default, savedEntity.UpdatedAt);
            Assert.Equal(entity.CreatedAt, savedEntity.CreatedAt);
            Assert.Equal(entity.UpdatedAt, savedEntity.UpdatedAt);
        }

        {
            await using var context = Fixture.CreateDbContext(dbDriver);
            var updatedEntity = await context.EntityHasStates.GetAsync(entity.Id);
            Assert.NotNull(updatedEntity);
            Assert.Equal(entity.Name, updatedEntity.Name);
            AssertDateTime(dbDriver, entity.CreatedAt, updatedEntity.CreatedAt);
            AssertDateTime(dbDriver, entity.UpdatedAt, updatedEntity.UpdatedAt);
        }
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task SaveAsync_ShouldModifyEntity_WhenIdIsNotDefault(DbDriver dbDriver)
    {
        var entity = await CreateEntityHasStatesAsync(dbDriver);
        var createdAt = entity.CreatedAt;
        var newName = Guid.NewGuid().ToString();
        {
            await using var context = Fixture.CreateDbContext(dbDriver);
            entity.Name = newName;

            await Task.Delay(TimeSpan.FromMilliseconds(100)); // Ensure that the updated time is different
            await context.SaveAsync(entity);
            Assert.Equal(createdAt, entity.CreatedAt);
            Assert.NotEqualToMilliseconds(createdAt, entity.UpdatedAt);
        }

        {
            await using var context = Fixture.CreateDbContext(dbDriver);
            var updatedEntity = await context.EntityHasStates.GetAsync(entity.Id);
            Assert.NotNull(updatedEntity);
            Assert.Equal(newName, updatedEntity.Name);
            AssertDateTime(dbDriver, entity.CreatedAt, updatedEntity.CreatedAt);
            AssertDateTime(dbDriver, entity.UpdatedAt, updatedEntity.UpdatedAt);
        }
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task SaveAsync_ShouldExcludeSpecifiedProperties_OnUpdate(DbDriver dbDriver)
    {
        var newName = Guid.NewGuid().ToString();
        var entity = await CreateEntityHasStatesAsync(dbDriver);
        var name = entity.Name;
        {
            await using var context = Fixture.CreateDbContext(dbDriver);
            entity.Name = newName;

            await context.SaveAsync(entity, nameof(entity.Name));
        }

        {
            await using var context = Fixture.CreateDbContext(dbDriver);
            var updatedEntity = await context.EntityHasStates.GetAsync(entity.Id);
            Assert.NotNull(updatedEntity);
            Assert.Equal(newName, entity.Name);
            Assert.Equal(name, updatedEntity.Name);
        }
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task SaveAsync_ShouldExcludeSpecifiedNavigation_OnUpdate(DbDriver dbDriver)
    {
        var entity = await CreateEntityWithNavigationAsync(dbDriver);
        Assert.NotEqual(default, entity.NavigationId);
        Assert.NotNull(entity.Navigation);
        Assert.NotEqual(default, entity.Navigation.Id);

        var name = entity.Name;
        var navigationName = entity.Navigation.Name;
        var newName = Guid.NewGuid().ToString();
        var newNavigationName = Guid.NewGuid().ToString();

        {
            // create new db context to ensure that there is no tracking.
            await using var context = Fixture.CreateDbContext(dbDriver);
            context.Add(entity);
            await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        }

        {
            // create new db context to ensure that there is no tracking.
            await using var context = Fixture.CreateDbContext(dbDriver);
            entity.Name = newName;
            entity.Navigation.Name = newNavigationName;
            await context.SaveAsync(entity, nameof(entity.Navigation));
        }

        {
            await using var context = Fixture.CreateDbContext(dbDriver);
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
    [MemberData(nameof(DbDriverCases))]
    public async Task SaveAsync_ShouldHandleNonExistingProperties(DbDriver dbDriver)
    {
        var entity = await CreateEntityHasStatesAsync(dbDriver);
        await using var context = Fixture.CreateDbContext(dbDriver);

        var result = await context.SaveAsync(entity, "NonExistingProperty");

        Assert.NotNull(result);
        Assert.Equal(entity.Id, result.Id);
    }
}
