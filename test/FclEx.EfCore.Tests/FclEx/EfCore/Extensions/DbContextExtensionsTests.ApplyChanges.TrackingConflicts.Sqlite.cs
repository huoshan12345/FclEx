namespace FclEx.EfCore.Extensions;

public partial class DbContextExtensionsApplyChangesSqliteTests
{
    [Fact]
    public async Task DtoOverload_ShouldNotOverwriteDetachedParentWhenTrackedChildIsUpdated()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        var parentId = await SeedParentWithChildrenAsync(database);

        await using (var context = database.CreateContext())
        {
            var detachedParent = await context.Parents
                .AsNoTracking()
                .SingleAsync(parent => parent.Id == parentId);
            var trackedChildren = await context.Children
                .Where(child => child.ParentId == parentId)
                .OrderBy(child => child.Code)
                .ToListAsync();

            await using (var concurrentContext = database.CreateContext())
            {
                var currentParent = await concurrentContext.Parents.SingleAsync(parent => parent.Id == parentId);
                currentParent.Name = "Current";
                await concurrentContext.SaveChangesAsync();
            }

            var changes = ApplyMixedChildChanges(
                context,
                detachedParent,
                trackedChildren,
                useEntityOverload: false);

            AssertMixedChanges(context, changes);
            await context.SaveChangesAsync();
        }

        await using var verificationContext = database.CreateContext();
        var persistedParent = await verificationContext.Parents
            .AsNoTracking()
            .SingleAsync(parent => parent.Id == parentId);
        var persistedChildren = await verificationContext.Children
            .AsNoTracking()
            .Where(child => child.ParentId == parentId)
            .OrderBy(child => child.Code)
            .Select(child => new { child.Code, child.Name })
            .ToListAsync();

        Assert.Equal("Current", persistedParent.Name);
        Assert.Equal(2, persistedChildren.Count);
        Assert.Equal("Insert", persistedChildren.Single(child => child.Code == "insert").Name);
        Assert.Equal("After", persistedChildren.Single(child => child.Code == "update").Name);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ApplyChanges_ShouldNotChangeTrackedChildOutsideExistingCollection(bool useEntityOverload)
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        var parentId = await SeedParentWithChildrenAsync(database);

        await using (var setupContext = database.CreateContext())
        {
            setupContext.Add(new Child
            {
                ParentId = parentId,
                Code = "outside",
                Name = "Outside",
            });
            await setupContext.SaveChangesAsync();
        }

        await using (var context = database.CreateContext())
        {
            var parent = await context.Parents
                .Include(entity => entity.Children)
                .SingleAsync(entity => entity.Id == parentId);
            var outside = parent.Children.Single(child => child.Code == "outside");
            var existingChildren = parent.Children
                .Where(child => child.Code != "outside")
                .ToList();

            var changes = ApplyMixedChildChanges(context, parent, existingChildren, useEntityOverload);

            AssertMixedChanges(context, changes);
            Assert.Equal(EntityState.Unchanged, context.Entry(outside).State);
            await context.SaveChangesAsync();
        }

        await using var verificationContext = database.CreateContext();
        var persistedChildren = await verificationContext.Children
            .AsNoTracking()
            .Where(child => child.ParentId == parentId)
            .OrderBy(child => child.Code)
            .Select(child => new { child.Code, child.Name })
            .ToListAsync();

        Assert.Equal(3, persistedChildren.Count);
        Assert.Equal("Outside", persistedChildren.Single(child => child.Code == "outside").Name);
        Assert.Equal("Insert", persistedChildren.Single(child => child.Code == "insert").Name);
        Assert.Equal("After", persistedChildren.Single(child => child.Code == "update").Name);
    }
}
