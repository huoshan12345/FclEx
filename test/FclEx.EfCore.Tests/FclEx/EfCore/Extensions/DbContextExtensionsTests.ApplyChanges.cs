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
    public async Task ApplyChanges_ShouldPersistReplacementReturnedByUpdateEntity(DbDriver dbDriver)
    {
        var existing = await CreateEntityHasStatesAsync(dbDriver);

        await using (var context = Fixture.CreateDbContext(dbDriver))
        {
            var tracked = await context.EntityHasStates.SingleAsync(e => e.Id == existing.Id);
            var dtos = new[]
            {
                new EntityHasStates { Id = existing.Id, Name = "Replacement" }
            };

            var result = context.ApplyChanges(
                dtos,
                dto => dto.Id,
                [tracked],
                entity => entity.Id,
                dto => dto,
                (dto, _) => new EntityHasStates
                {
                    Id = dto.Id,
                    Name = dto.Name,
                });

            Assert.Single(result.Updated);
            Assert.Equal("Replacement", result.Updated[0].New.Name);
            Assert.NotSame(tracked, result.Updated[0].New);
            Assert.Same(tracked, result.Updated[0].Existing);
            Assert.Equal("Replacement", tracked.Name);
            Assert.Equal(EntityState.Modified, context.Entry(tracked).State);

            await context.SaveChangesAsync();
        }

        await using var verificationContext = Fixture.CreateDbContext(dbDriver);
        var persisted = await verificationContext.EntityHasStates
            .AsNoTracking()
            .SingleAsync(e => e.Id == existing.Id);

        Assert.Equal("Replacement", persisted.Name);
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
        var deletedAt = DateTimeOffset.UtcNow.AddDays(-1);
        await using var context = Fixture.CreateDbContext(dbDriver);
        var existing = new[]
        {
            new EntityHasStates { Name = "Alice", IsDeleted = true, DeletedAt = deletedAt }
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
        Assert.Equal(default, result.Updated[0].New.DeletedAt);

        var count = await context.SaveChangesAsync();
        Assert.Equal(1, count);

        await using var verificationContext = Fixture.CreateDbContext(dbDriver);
        var restored = await verificationContext.EntityHasStates
            .AsNoTracking()
            .SingleAsync(e => e.Id == existing[0].Id);
        Assert.False(restored.IsDeleted);
        Assert.Equal(default, restored.DeletedAt);
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task ApplyChanges_ShouldRestoreNoTrackingSoftDeletedEntity(DbDriver dbDriver)
    {
        var deletedAt = DateTimeOffset.UtcNow.AddDays(-1);
        long id;
        await using (var setupContext = Fixture.CreateDbContext(dbDriver))
        {
            var entity = new EntityHasStates
            {
                Name = "No tracking",
                IsDeleted = true,
                DeletedAt = deletedAt,
            };
            await setupContext.InsertAsync(entity);
            id = entity.Id;
        }

        await using (var context = Fixture.CreateDbContext(dbDriver))
        {
            var existing = await context.EntityHasStates
                .AsNoTracking()
                .SingleAsync(entity => entity.Id == id);
            context.ApplyChanges(
                [new EntityHasStates { Id = id, Name = "Restored" }],
                dto => dto.Id,
                [existing],
                entity => entity.Id,
                dto => dto,
                (dto, entity) => new EntityHasStates
                {
                    Id = entity.Id,
                    Name = dto.Name,
                    IsDeleted = entity.IsDeleted,
                    DeletedAt = entity.DeletedAt,
                });

            await context.SaveChangesAsync();
        }

        await using var verificationContext = Fixture.CreateDbContext(dbDriver);
        var restored = await verificationContext.EntityHasStates
            .AsNoTracking()
            .SingleAsync(entity => entity.Id == id);
        Assert.Equal("Restored", restored.Name);
        Assert.False(restored.IsDeleted);
        Assert.Equal(default, restored.DeletedAt);
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task ApplyChanges_ShouldRejectDuplicateDtoKeysBeforeTrackingChanges(DbDriver dbDriver)
    {
        await using var context = Fixture.CreateDbContext(dbDriver);
        var existing = new EntityHasStates { Id = 42, Name = "Existing" };
        var dtos = new[]
        {
            new EntityHasStates { Id = 42, Name = "First" },
            new EntityHasStates { Id = 42, Name = "Second" },
        };

        var exception = Assert.Throws<ArgumentException>(() => context.ApplyChanges(
            dtos,
            dto => dto.Id,
            [existing],
            entity => entity.Id,
            dto => dto,
            (dto, entity) => dto));

        Assert.Equal("dtos", exception.ParamName);
        Assert.Empty(context.ChangeTracker.Entries());
        Assert.Equal("Existing", existing.Name);
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task ApplyChanges_ShouldAllowMultipleDtosWithDefaultKeys(DbDriver dbDriver)
    {
        await using var context = Fixture.CreateDbContext(dbDriver);
        var dtos = new[]
        {
            new EntityHasStates { Name = "First" },
            new EntityHasStates { Name = "Second" },
        };

        var changes = context.ApplyChanges(
            dtos,
            dto => dto.Id,
            Array.Empty<EntityHasStates>(),
            entity => entity.Id,
            dto => new EntityHasStates { Name = dto.Name });

        Assert.Equal(2, changes.Inserted.Count);
        Assert.Equal(2, context.ChangeTracker.Entries<EntityHasStates>().Count());
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task ApplyChanges_ShouldRejectDuplicateExistingEntityKeys(DbDriver dbDriver)
    {
        await using var context = Fixture.CreateDbContext(dbDriver);
        var existing = new[]
        {
            new EntityHasStates { Id = 42, Name = "First" },
            new EntityHasStates { Id = 42, Name = "Second" },
        };

        var exception = Assert.Throws<ArgumentException>(() => context.ApplyChanges(
            Array.Empty<EntityHasStates>(),
            dto => dto.Id,
            existing,
            entity => entity.Id,
            dto => dto));

        Assert.Equal("existingEntities", exception.ParamName);
        Assert.Empty(context.ChangeTracker.Entries());
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
