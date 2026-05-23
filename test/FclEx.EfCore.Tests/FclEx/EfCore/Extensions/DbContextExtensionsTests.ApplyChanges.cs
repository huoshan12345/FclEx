namespace FclEx.EfCore.Extensions;

partial class DbContextExtensionsTests
{
    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task ApplyChanges_ShouldInsertNewEntities(DbDriver dbDriver)
    {
        await using var context = Fixture.CreateDbContext(dbDriver);
        var dtos = new[]
        {
            new EntityHasStates { Name = "Alice" },
            new EntityHasStates { Name = "Bob" }
        };

        var result = context.ApplyChanges(
            dtos: dtos,
            dtoKey: dto => dto.Id,
            existingEntities: [],
            entityKey: e => e.Id,
            insertEntity: dto => new EntityHasStates { Name = dto.Name });

        Assert.Equal(2, result.Inserted.Count);
        Assert.Empty(result.Updated);
        Assert.Empty(result.Deleted);

        var count = await context.SaveChangesAsync();
        Assert.Equal(2, count);
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task ApplyChanges_ShouldUpdateExistingEntities(DbDriver dbDriver)
    {
        await using var context = Fixture.CreateDbContext(dbDriver);
        var existing = new[]
        {
            new EntityHasStates { Name = "Alice" }
        };
        await context.InsertRangeAsync(existing);

        var dtos = new[]
        {
            new EntityHasStates { Id = existing[0].Id, Name = "Alice Updated" }
        };

        var result = context.ApplyChanges(
            dtos,
            dto => dto.Id,
            existing,
            e => e.Id,
            dto => dto,
            (dto, entity) =>
            {
                entity.Name = dto.Name;
                return entity;
            });

        Assert.Empty(result.Inserted);
        Assert.Single(result.Updated);
        Assert.Empty(result.Deleted);
        Assert.Equal("Alice Updated", result.Updated[0].New.Name);

        var count = await context.SaveChangesAsync();
        Assert.Equal(1, count);
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task ApplyChanges_ShouldDeleteMissingEntities_WhenAllowed(DbDriver dbDriver)
    {
        await using var context = Fixture.CreateDbContext(dbDriver);
        var existing = new[]
        {
            new EntityHasStates { Name = "Old" },
            new EntityHasStates { Name = "Remove Me" }
        };
        await context.InsertRangeAsync(existing);

        var dtos = new[]
        {
            new EntityHasStates { Id = existing[0].Id, Name = "Old" }
        };

        var result = context.ApplyChanges(
            dtos,
            dto => dto.Id,
            existing,
            e => e.Id,
            dto => dto,
            allowDeletion: true);

        Assert.Empty(result.Inserted);
        Assert.Empty(result.Updated);
        Assert.Single(result.Deleted);
        Assert.Equal(existing[1].Id, result.Deleted[0].Id);

        var count = await context.SaveChangesAsync();
        Assert.Equal(1, count);
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task ApplyChanges_ShouldNotDelete_WhenAllowDeletionIsFalse(DbDriver dbDriver)
    {
        await using var context = Fixture.CreateDbContext(dbDriver);
        var existing = new[]
        {
            new EntityHasStates { Name = "Old" },
            new EntityHasStates { Name = "Keep Me" }
        };
        await context.InsertRangeAsync(existing);

        var dtos = new[]
        {
            new EntityHasStates { Id = existing[0].Id, Name = "Old" }
        };

        var result = context.ApplyChanges(
            dtos,
            dto => dto.Id,
            existing,
            e => e.Id,
            dto => dto,
            allowDeletion: false);

        Assert.Empty(result.Inserted);
        Assert.Empty(result.Updated);
        Assert.Empty(result.Deleted);

        var count = await context.SaveChangesAsync();
        Assert.Equal(0, count);
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task ApplyChanges_ShouldRestoreSoftDeletedEntity(DbDriver dbDriver)
    {
        await using var context = Fixture.CreateDbContext(dbDriver);
        var existing = new[]
        {
            new EntityHasStates { Name = "Alice", IsDeleted = true }
        };

        await context.InsertRangeAsync(existing);

        var dtos = new[]
        {
            new EntityHasStates { Id = existing[0].Id, Name = "Alice" }
        };

        var result = context.ApplyChanges(
            dtos,
            dto => dto.Id,
            existing,
            e => e.Id,
            dto => dto,
            (dto, entity) =>
            {
                entity.Name = dto.Name;
                return entity;
            });

        Assert.Empty(result.Inserted);
        Assert.Single(result.Updated);
        Assert.False(result.Updated[0].New.IsDeleted);

        var count = await context.SaveChangesAsync();
        Assert.Equal(1, count);
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task ApplyChanges_ShouldHandleMixedOperations(DbDriver dbDriver)
    {
        await using var context = Fixture.CreateDbContext(dbDriver);
        var existing = new[]
        {
            new EntityHasStates { Name = "Keep" },
            new EntityHasStates { Name = "UpdateMe" },
            new EntityHasStates { Name = "DeleteMe" }
        };

        await context.InsertRangeAsync(existing);

        var dtos = new[]
        {
            new EntityHasStates { Id = existing[0].Id, Name = "Keep" },
            new EntityHasStates { Id = existing[1].Id, Name = "Updated" },
            new EntityHasStates { Name = "NewOne" }
        };

        var result = context.ApplyChanges(
            dtos,
            dto => dto.Id,
            existing,
            e => e.Id,
            dto => new EntityHasStates { Id = dto.Id, Name = dto.Name },
            (dto, entity) =>
            {
                if (dto.Name == entity.Name)
                    return null;

                entity.Name = dto.Name;
                return entity;
            }, true);

        Assert.Single(result.Inserted);
        Assert.Equal("NewOne", result.Inserted[0].Name);
        Assert.Single(result.Updated);
        Assert.Equal("Updated", result.Updated[0].New.Name);
        Assert.Single(result.Deleted);
        Assert.Equal("DeleteMe", result.Deleted[0].Name);

        var count = await context.SaveChangesAsync();
        Assert.Equal(3, count);
    }
}
