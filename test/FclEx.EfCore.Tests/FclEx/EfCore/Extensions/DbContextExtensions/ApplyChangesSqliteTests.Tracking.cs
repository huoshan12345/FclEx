namespace FclEx.EfCore.Extensions.DbContextExtensions;

public partial class ApplyChangesSqliteTests
{
    public enum ChildTrackingMode
    {
        All,
        None,
        UpdateOnly,
        DeleteOnly,
    }

    public static TheoryData<bool, ChildTrackingMode> ParentAndChildTrackingCases => new()
    {
        { true, ChildTrackingMode.All },
        { true, ChildTrackingMode.None },
        { true, ChildTrackingMode.UpdateOnly },
        { true, ChildTrackingMode.DeleteOnly },
        { false, ChildTrackingMode.All },
        { false, ChildTrackingMode.None },
        { false, ChildTrackingMode.UpdateOnly },
        { false, ChildTrackingMode.DeleteOnly },
    };

    [Theory]
    [MemberData(nameof(ParentAndChildTrackingCases))]
    public async Task DtoOverload_ShouldSynchronizeChildrenAcrossTrackingCombinations(
        bool parentTracked,
        ChildTrackingMode childTrackingMode)
    {
        await RunChildSynchronizationScenarioAsync(parentTracked, childTrackingMode, useEntityOverload: false);
    }

    [Theory]
    [MemberData(nameof(ParentAndChildTrackingCases))]
    public async Task EntityOverload_ShouldSynchronizeChildrenAcrossTrackingCombinations(
        bool parentTracked,
        ChildTrackingMode childTrackingMode)
    {
        await RunChildSynchronizationScenarioAsync(parentTracked, childTrackingMode, useEntityOverload: true);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ApplyChanges_ShouldUseTrackedChildrenWhenExistingCollectionContainsDetachedCopies(bool useEntityOverload)
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        var parentId = await SeedParentWithChildrenAsync(database);

        await using (var context = database.CreateContext())
        {
            var trackedParent = await context.Parents
                .Include(parent => parent.Children)
                .SingleAsync(parent => parent.Id == parentId);
            var detachedChildren = await context.Children
                .AsNoTracking()
                .Where(child => child.ParentId == parentId)
                .OrderBy(child => child.Code)
                .ToListAsync();

            Assert.Equal(2, context.ChangeTracker.Entries<Child>().Count());
            Assert.All(detachedChildren, child =>
                Assert.DoesNotContain(context.ChangeTracker.Entries<Child>(), entry => ReferenceEquals(entry.Entity, child)));

            var changes = ApplyMixedChildChanges(
                context,
                trackedParent,
                detachedChildren,
                useEntityOverload);

            AssertMixedChanges(context, changes);
            AssertParentIsNotPendingModification(context);
            await context.SaveChangesAsync();
        }

        await AssertPersistedMixedChildChangesAsync(database, parentId);
    }

    [Fact]
    public async Task DtoOverload_ShouldRetainTrackedChildAlreadyMarkedDeletedWhenDtoContainsIt()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        var parentId = await SeedParentWithChildrenAsync(database);

        await using (var context = database.CreateContext())
        {
            var existing = await context.Children.SingleAsync(child => child.Code == "update");
            context.Remove(existing);
            Assert.Equal(EntityState.Deleted, context.Entry(existing).State);

            var changes = context.ApplyChanges(
                [new ChildDto("update", "After")],
                dto => dto.Code,
                [existing],
                child => child.Code,
                dto => new Child { ParentId = parentId, Code = dto.Code, Name = dto.Name },
                (dto, child) =>
                {
                    child.Name = dto.Name;
                    return child;
                },
                allowDeletion: true);

            Assert.Single(changes.Updated);
            Assert.Empty(changes.Deleted);
            Assert.Equal(EntityState.Modified, context.Entry(existing).State);
            await context.SaveChangesAsync();
        }

        await using var verificationContext = database.CreateContext();
        var persisted = await verificationContext.Children
            .AsNoTracking()
            .SingleAsync(child => child.Code == "update");
        Assert.Equal("After", persisted.Name);
    }

    private static async Task RunChildSynchronizationScenarioAsync(
        bool parentTracked,
        ChildTrackingMode childTrackingMode,
        bool useEntityOverload)
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        var parentId = await SeedParentWithChildrenAsync(database);

        await using (var context = database.CreateContext())
        {
            var parent = parentTracked
                ? await context.Parents.SingleAsync(entity => entity.Id == parentId)
                : await context.Parents.AsNoTracking().SingleAsync(entity => entity.Id == parentId);
            var existingChildren = await LoadChildrenAsync(context, parentId, childTrackingMode);

            Assert.Equal(parentTracked ? 1 : 0, context.ChangeTracker.Entries<Parent>().Count());
            Assert.Equal(ExpectedTrackedChildCount(childTrackingMode), context.ChangeTracker.Entries<Child>().Count());

            var changes = ApplyMixedChildChanges(context, parent, existingChildren, useEntityOverload);

            AssertMixedChanges(context, changes);
            AssertParentIsNotPendingModification(context);
            await context.SaveChangesAsync();
        }

        await AssertPersistedMixedChildChangesAsync(database, parentId);
    }

    private static EntityChanges<Child> ApplyMixedChildChanges(
        ApplyChangesDbContext context,
        Parent parent,
        IReadOnlyCollection<Child> existingChildren,
        bool useEntityOverload)
    {
        if (useEntityOverload)
        {
            return context.ApplyChanges(
                [
                    new Child
                    {
                        ParentId = parent.Id,
                        Code = "update",
                        Name = "After",
                    },
                    new Child
                    {
                        ParentId = parent.Id,
                        Code = "insert",
                        Name = "Insert",
                    },
                ],
                existingChildren,
                child => child.Code,
                allowDeletion: true);
        }

        return context.ApplyChanges(
            [
                new ChildDto("update", "After"),
                new ChildDto("insert", "Insert"),
            ],
            dto => dto.Code,
            existingChildren,
            child => child.Code,
            dto => new Child
            {
                ParentId = parent.Id,
                Code = dto.Code,
                Name = dto.Name,
            },
            (dto, child) =>
            {
                child.Name = dto.Name;
                return child;
            },
            allowDeletion: true);
    }

    private static void AssertMixedChanges(ApplyChangesDbContext context, EntityChanges<Child> changes)
    {
        var inserted = Assert.Single(changes.Inserted);
        var updated = Assert.Single(changes.Updated).New;
        var deleted = Assert.Single(changes.Deleted);

        Assert.Equal("insert", inserted.Code);
        Assert.Equal("update", updated.Code);
        Assert.Equal("delete", deleted.Code);
        Assert.Equal(EntityState.Added, context.Entry(inserted).State);
        Assert.Equal(EntityState.Modified, context.Entry(updated).State);
        Assert.Equal(EntityState.Deleted, context.Entry(deleted).State);
    }

    private static void AssertParentIsNotPendingModification(ApplyChangesDbContext context)
    {
        Assert.DoesNotContain(
            context.ChangeTracker.Entries<Parent>(),
            entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted);
    }

    private static async Task<long> SeedParentWithChildrenAsync(SqliteTestDatabase database)
    {
        await using var context = database.CreateContext();
        var parent = new Parent
        {
            Name = "Parent",
            Children =
            [
                new Child { Code = "update", Name = "Before" },
                new Child { Code = "delete", Name = "Delete" },
            ],
        };
        context.Add(parent);
        await context.SaveChangesAsync();
        return parent.Id;
    }

    private static async Task<List<Child>> LoadChildrenAsync(
        ApplyChangesDbContext context,
        long parentId,
        ChildTrackingMode trackingMode)
    {
        IQueryable<Child> Children() => context.Children
            .Where(child => child.ParentId == parentId)
            .OrderBy(child => child.Code);

        switch (trackingMode)
        {
            case ChildTrackingMode.All:
                return await Children().ToListAsync();
            case ChildTrackingMode.None:
                return await Children().AsNoTracking().ToListAsync();
            case ChildTrackingMode.UpdateOnly:
            {
                var update = await Children().SingleAsync(child => child.Code == "update");
                var delete = await Children().AsNoTracking().SingleAsync(child => child.Code == "delete");
                return [update, delete];
            }
            case ChildTrackingMode.DeleteOnly:
            {
                var update = await Children().AsNoTracking().SingleAsync(child => child.Code == "update");
                var delete = await Children().SingleAsync(child => child.Code == "delete");
                return [update, delete];
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(trackingMode), trackingMode, null);
        }
    }

    private static int ExpectedTrackedChildCount(ChildTrackingMode trackingMode)
    {
        return trackingMode switch
        {
            ChildTrackingMode.All => 2,
            ChildTrackingMode.None => 0,
            ChildTrackingMode.UpdateOnly or ChildTrackingMode.DeleteOnly => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(trackingMode), trackingMode, null),
        };
    }

    private static async Task AssertPersistedMixedChildChangesAsync(SqliteTestDatabase database, long parentId)
    {
        await using var context = database.CreateContext();
        var parent = await context.Parents
            .AsNoTracking()
            .SingleAsync(entity => entity.Id == parentId);
        var children = await context.Children
            .AsNoTracking()
            .Where(child => child.ParentId == parentId)
            .OrderBy(child => child.Code)
            .Select(child => new { child.Code, child.Name })
            .ToListAsync();

        Assert.Equal("Parent", parent.Name);
        Assert.Equal(2, children.Count);
        Assert.Equal("Insert", children.Single(child => child.Code == "insert").Name);
        Assert.Equal("After", children.Single(child => child.Code == "update").Name);
        Assert.DoesNotContain(children, child => child.Code == "delete");
    }
}
