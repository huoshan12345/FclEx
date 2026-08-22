namespace FclEx.EfCore.Extensions.DbContextExtensions;

public partial class ApplyChangesSqliteTests
{
    public enum NestedGraphTrackingMode
    {
        All,
        None,
        Mixed,
        DetachedCopiesWithTrackedCounterparts,
    }

    public static TheoryData<bool, NestedGraphTrackingMode, bool> NestedGraphTrackingCases
    {
        get
        {
            var data = new TheoryData<bool, NestedGraphTrackingMode, bool>();
            foreach (var rootTracked in new[] { false, true })
            {
                foreach (var trackingMode in Enum.GetValues<NestedGraphTrackingMode>())
                {
                    data.Add(rootTracked, trackingMode, false);
                    data.Add(rootTracked, trackingMode, true);
                }
            }
            return data;
        }
    }

    [Theory]
    [MemberData(nameof(NestedGraphTrackingCases))]
    public async Task ApplyChanges_ShouldSynchronizeTwoOneToManyLevelsAcrossTrackingCombinations(
        bool rootTracked,
        NestedGraphTrackingMode trackingMode,
        bool useEntityOverload)
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        await SeedNestedGraphAsync(database);

        await using (var context = database.CreateContext())
        {
            if (rootTracked)
            {
                _ = await context.GraphRoots.SingleAsync(entity => entity.Id == "root");
            }
            else
            {
                _ = await context.GraphRoots.AsNoTracking().SingleAsync(entity => entity.Id == "root");
            }

            var existingBranches = await LoadGraphBranchesAsync(context, trackingMode);
            var existingLeaves = await LoadGraphLeavesAsync(context, trackingMode);
            Assert.Equal(rootTracked ? 1 : 0, context.ChangeTracker.Entries<GraphRoot>().Count());
            Assert.Equal(ExpectedTrackedGraphBranchCount(trackingMode), context.ChangeTracker.Entries<GraphBranch>().Count());
            Assert.Equal(ExpectedTrackedGraphLeafCount(trackingMode), context.ChangeTracker.Entries<GraphLeaf>().Count());

            var branchChanges = ApplyGraphBranchChanges(context, existingBranches, useEntityOverload);
            var leafChanges = ApplyGraphLeafChanges(context, existingLeaves, useEntityOverload);

            var insertedBranch = Assert.Single(branchChanges.Inserted);
            var updatedBranch = Assert.Single(branchChanges.Updated).New;
            var deletedBranch = Assert.Single(branchChanges.Deleted);
            Assert.Equal("branch-insert", insertedBranch.Id);
            Assert.Equal("branch-update", updatedBranch.Id);
            Assert.Equal("branch-delete", deletedBranch.Id);
            Assert.Equal(EntityState.Added, context.Entry(insertedBranch).State);
            Assert.Equal(EntityState.Modified, context.Entry(updatedBranch).State);
            Assert.Equal(EntityState.Deleted, context.Entry(deletedBranch).State);

            Assert.Equal(2, leafChanges.Inserted.Count);
            var updatedLeaf = Assert.Single(leafChanges.Updated).New;
            var deletedLeaf = Assert.Single(leafChanges.Deleted);
            Assert.Equal("leaf-update", updatedLeaf.Id);
            Assert.Equal("leaf-delete", deletedLeaf.Id);
            Assert.All(leafChanges.Inserted, entity => Assert.Equal(EntityState.Added, context.Entry(entity).State));
            Assert.Equal(EntityState.Modified, context.Entry(updatedLeaf).State);
            Assert.Equal(EntityState.Deleted, context.Entry(deletedLeaf).State);
            Assert.All(context.ChangeTracker.Entries<GraphRoot>(), entry => Assert.Equal(EntityState.Unchanged, entry.State));

            var cascadeEntry = context.ChangeTracker.Entries<GraphLeaf>()
                .SingleOrDefault(entry => entry.Entity.Id == "leaf-cascade");
            if (cascadeEntry is not null)
            {
                Assert.Equal(EntityState.Deleted, cascadeEntry.State);
            }

            await context.SaveChangesAsync();
        }

        await AssertPersistedNestedGraphAsync(database);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ApplyChanges_ShouldMoveTrackedLeafUsingDetachedExistingCopy(bool useEntityOverload)
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        await SeedNestedGraphAsync(database);

        await using (var context = database.CreateContext())
        {
            var branches = await context.GraphBranches.OrderBy(entity => entity.Id).ToListAsync();
            var trackedLeaf = await context.GraphLeaves.SingleAsync(entity => entity.Id == "leaf-update");
            var detachedLeaf = await context.GraphLeaves
                .AsNoTracking()
                .SingleAsync(entity => entity.Id == "leaf-update");

            EntityChanges<GraphLeaf> changes;
            if (useEntityOverload)
            {
                changes = context.ApplyChanges(
                    [
                        new GraphLeaf
                        {
                            Id = "leaf-update",
                            BranchId = "branch-delete",
                            Name = "Moved",
                        },
                    ],
                    [detachedLeaf],
                    entity => entity.Id);
            }
            else
            {
                changes = context.ApplyChanges(
                    [new GraphLeafMoveDto("leaf-update", "branch-delete", "Moved")],
                    dto => dto.Id,
                    [detachedLeaf],
                    entity => entity.Id,
                    dto => new GraphLeaf { Id = dto.Id, BranchId = dto.BranchId, Name = dto.Name },
                    (dto, entity) =>
                    {
                        entity.BranchId = dto.BranchId;
                        entity.Name = dto.Name;
                        return entity;
                    });
            }

            var updated = Assert.Single(changes.Updated).New;
            Assert.Equal("branch-delete", updated.BranchId);
            Assert.Equal("Moved", updated.Name);
            Assert.Equal(EntityState.Modified, context.Entry(updated).State);
            Assert.All(branches, branch => Assert.Equal(EntityState.Unchanged, context.Entry(branch).State));

            if (useEntityOverload)
            {
                Assert.NotSame(trackedLeaf, updated);
                Assert.Equal(EntityState.Detached, context.Entry(trackedLeaf).State);
            }
            else
            {
                Assert.Same(trackedLeaf, updated);
            }

            await context.SaveChangesAsync();
        }

        await using var verificationContext = database.CreateContext();
        var persisted = await verificationContext.GraphLeaves
            .AsNoTracking()
            .SingleAsync(entity => entity.Id == "leaf-update");
        Assert.Equal("branch-delete", persisted.BranchId);
        Assert.Equal("Moved", persisted.Name);
    }

    private static EntityChanges<GraphBranch> ApplyGraphBranchChanges(
        ApplyChangesDbContext context,
        IReadOnlyCollection<GraphBranch> existingBranches,
        bool useEntityOverload)
    {
        if (useEntityOverload)
        {
            return context.ApplyChanges(
                [
                    new GraphBranch { Id = "branch-update", RootId = "root", Name = "After" },
                    new GraphBranch { Id = "branch-insert", RootId = "root", Name = "Insert" },
                ],
                existingBranches,
                entity => entity.Id,
                allowDeletion: true);
        }

        return context.ApplyChanges(
            [new GraphBranchDto("branch-update", "After"), new GraphBranchDto("branch-insert", "Insert")],
            dto => dto.Id,
            existingBranches,
            entity => entity.Id,
            dto => new GraphBranch { Id = dto.Id, RootId = "root", Name = dto.Name },
            (dto, entity) =>
            {
                entity.Name = dto.Name;
                return entity;
            },
            allowDeletion: true);
    }

    private static EntityChanges<GraphLeaf> ApplyGraphLeafChanges(
        ApplyChangesDbContext context,
        IReadOnlyCollection<GraphLeaf> existingLeaves,
        bool useEntityOverload)
    {
        if (useEntityOverload)
        {
            return context.ApplyChanges(
                [
                    new GraphLeaf { Id = "leaf-update", BranchId = "branch-update", Name = "After" },
                    new GraphLeaf { Id = "leaf-insert", BranchId = "branch-update", Name = "Insert" },
                    new GraphLeaf { Id = "leaf-new-branch", BranchId = "branch-insert", Name = "New Branch" },
                ],
                existingLeaves,
                entity => entity.Id,
                allowDeletion: true);
        }

        return context.ApplyChanges(
            [
                new GraphLeafMoveDto("leaf-update", "branch-update", "After"),
                new GraphLeafMoveDto("leaf-insert", "branch-update", "Insert"),
                new GraphLeafMoveDto("leaf-new-branch", "branch-insert", "New Branch"),
            ],
            dto => dto.Id,
            existingLeaves,
            entity => entity.Id,
            dto => new GraphLeaf { Id = dto.Id, BranchId = dto.BranchId, Name = dto.Name },
            (dto, entity) =>
            {
                entity.BranchId = dto.BranchId;
                entity.Name = dto.Name;
                return entity;
            },
            allowDeletion: true);
    }

    private static async Task SeedNestedGraphAsync(SqliteTestDatabase database)
    {
        await using var context = database.CreateContext();
        context.Add(new GraphRoot { Id = "root", Name = "Root" });
        context.AddRange(
            new GraphBranch { Id = "branch-update", RootId = "root", Name = "Before" },
            new GraphBranch { Id = "branch-delete", RootId = "root", Name = "Delete" });
        context.AddRange(
            new GraphLeaf { Id = "leaf-update", BranchId = "branch-update", Name = "Before" },
            new GraphLeaf { Id = "leaf-delete", BranchId = "branch-update", Name = "Delete" },
            new GraphLeaf { Id = "leaf-cascade", BranchId = "branch-delete", Name = "Cascade" });
        await context.SaveChangesAsync();
    }

    private static async Task<List<GraphBranch>> LoadGraphBranchesAsync(
        ApplyChangesDbContext context,
        NestedGraphTrackingMode trackingMode)
    {
        IQueryable<GraphBranch> Branches() => context.GraphBranches
            .Where(entity => entity.RootId == "root")
            .OrderBy(entity => entity.Id);

        switch (trackingMode)
        {
            case NestedGraphTrackingMode.All:
                return await Branches().ToListAsync();
            case NestedGraphTrackingMode.None:
                return await Branches().AsNoTracking().ToListAsync();
            case NestedGraphTrackingMode.Mixed:
            {
                var update = await Branches().SingleAsync(entity => entity.Id == "branch-update");
                var delete = await Branches().AsNoTracking().SingleAsync(entity => entity.Id == "branch-delete");
                return [update, delete];
            }
            case NestedGraphTrackingMode.DetachedCopiesWithTrackedCounterparts:
                _ = await Branches().ToListAsync();
                return await Branches().AsNoTracking().ToListAsync();
            default:
                throw new ArgumentOutOfRangeException(nameof(trackingMode), trackingMode, null);
        }
    }

    private static async Task<List<GraphLeaf>> LoadGraphLeavesAsync(
        ApplyChangesDbContext context,
        NestedGraphTrackingMode trackingMode)
    {
        IQueryable<GraphLeaf> Leaves() => context.GraphLeaves.OrderBy(entity => entity.Id);
        IQueryable<GraphLeaf> RetainedBranchLeaves() => Leaves().Where(entity => entity.BranchId == "branch-update");

        switch (trackingMode)
        {
            case NestedGraphTrackingMode.All:
            {
                var all = await Leaves().ToListAsync();
                return all.Where(entity => entity.BranchId == "branch-update").ToList();
            }
            case NestedGraphTrackingMode.None:
                return await RetainedBranchLeaves().AsNoTracking().ToListAsync();
            case NestedGraphTrackingMode.Mixed:
            {
                var update = await Leaves().SingleAsync(entity => entity.Id == "leaf-update");
                var delete = await Leaves().AsNoTracking().SingleAsync(entity => entity.Id == "leaf-delete");
                _ = await Leaves().SingleAsync(entity => entity.Id == "leaf-cascade");
                return [update, delete];
            }
            case NestedGraphTrackingMode.DetachedCopiesWithTrackedCounterparts:
                _ = await Leaves().ToListAsync();
                return await RetainedBranchLeaves().AsNoTracking().ToListAsync();
            default:
                throw new ArgumentOutOfRangeException(nameof(trackingMode), trackingMode, null);
        }
    }

    private static int ExpectedTrackedGraphBranchCount(NestedGraphTrackingMode trackingMode)
    {
        return trackingMode switch
        {
            NestedGraphTrackingMode.All or NestedGraphTrackingMode.DetachedCopiesWithTrackedCounterparts => 2,
            NestedGraphTrackingMode.None => 0,
            NestedGraphTrackingMode.Mixed => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(trackingMode), trackingMode, null),
        };
    }

    private static int ExpectedTrackedGraphLeafCount(NestedGraphTrackingMode trackingMode)
    {
        return trackingMode switch
        {
            NestedGraphTrackingMode.All or NestedGraphTrackingMode.DetachedCopiesWithTrackedCounterparts => 3,
            NestedGraphTrackingMode.None => 0,
            NestedGraphTrackingMode.Mixed => 2,
            _ => throw new ArgumentOutOfRangeException(nameof(trackingMode), trackingMode, null),
        };
    }

    private static async Task AssertPersistedNestedGraphAsync(SqliteTestDatabase database)
    {
        await using var context = database.CreateContext();
        var root = await context.GraphRoots.AsNoTracking().SingleAsync();
        var branches = await context.GraphBranches
            .AsNoTracking()
            .OrderBy(entity => entity.Id)
            .Select(entity => new { entity.Id, entity.Name })
            .ToListAsync();
        var leaves = await context.GraphLeaves
            .AsNoTracking()
            .OrderBy(entity => entity.Id)
            .Select(entity => new { entity.Id, entity.BranchId, entity.Name })
            .ToListAsync();

        Assert.Equal("Root", root.Name);
        Assert.Equal(2, branches.Count);
        Assert.Equal("Insert", branches.Single(entity => entity.Id == "branch-insert").Name);
        Assert.Equal("After", branches.Single(entity => entity.Id == "branch-update").Name);
        Assert.Equal(3, leaves.Count);
        Assert.Equal("Insert", leaves.Single(entity => entity.Id == "leaf-insert").Name);
        Assert.Equal("branch-insert", leaves.Single(entity => entity.Id == "leaf-new-branch").BranchId);
        Assert.Equal("After", leaves.Single(entity => entity.Id == "leaf-update").Name);
        Assert.DoesNotContain(leaves, entity => entity.Id is "leaf-delete" or "leaf-cascade");
    }
}
