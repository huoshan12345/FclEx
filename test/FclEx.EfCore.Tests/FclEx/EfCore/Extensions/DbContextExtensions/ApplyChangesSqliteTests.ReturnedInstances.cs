namespace FclEx.EfCore.Extensions.DbContextExtensions;

public partial class ApplyChangesSqliteTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ApplyChanges_ShouldReturnContextEntryEntitiesForMixedChanges(bool useEntityOverload)
    {
        await using var database = await SqliteTestDatabase.CreateAsync();

        await using (var context = database.CreateContext())
        {
            var trackedUpdate = new Item { Code = "update", Name = "Before" };
            var trackedDelete = new Item { Code = "delete", Name = "Delete" };
            context.AddRange(trackedUpdate, trackedDelete);
            await context.SaveChangesAsync();

            var detachedExisting = await context.Items
                .AsNoTracking()
                .OrderBy(entity => entity.Code)
                .ToListAsync();
            var detachedUpdate = detachedExisting.Single(entity => entity.Code == "update");

            EntityChanges<Item> changes;
            Item expectedInserted;
            Item expectedUpdated;
            if (useEntityOverload)
            {
                expectedUpdated = new Item { Code = "update", Name = "After" };
                expectedInserted = new Item { Code = "insert", Name = "Insert" };
                changes = context.ApplyChanges(
                    [expectedUpdated, expectedInserted],
                    detachedExisting,
                    entity => entity.Code,
                    allowDeletion: true);
            }
            else
            {
                expectedInserted = new Item { Code = "insert", Name = "Insert" };
                expectedUpdated = trackedUpdate;
                changes = context.ApplyChanges(
                    [new ItemDto("update", "After"), new ItemDto("insert", "Insert")],
                    dto => dto.Code,
                    detachedExisting,
                    entity => entity.Code,
                    _ => expectedInserted,
                    (dto, entity) =>
                    {
                        entity.Name = dto.Name;
                        return entity;
                    },
                    allowDeletion: true);
            }

            var update = Assert.Single(changes.Updated);
            Assert.Same(expectedInserted, Assert.Single(changes.Inserted));
            Assert.Same(expectedUpdated, update.New);
            Assert.Same(detachedUpdate, update.Existing);
            Assert.Same(trackedDelete, Assert.Single(changes.Deleted));
            Assert.Equal(EntityState.Added, context.Entry(expectedInserted).State);
            Assert.Equal(EntityState.Modified, context.Entry(expectedUpdated).State);
            Assert.Equal(EntityState.Deleted, context.Entry(trackedDelete).State);

            await context.SaveChangesAsync();
        }

        await using var verificationContext = database.CreateContext();
        var persisted = await verificationContext.Items
            .AsNoTracking()
            .OrderBy(entity => entity.Code)
            .Select(entity => new { entity.Code, entity.Name })
            .ToListAsync();
        Assert.Equal(2, persisted.Count);
        Assert.Equal("Insert", persisted.Single(entity => entity.Code == "insert").Name);
        Assert.Equal("After", persisted.Single(entity => entity.Code == "update").Name);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ApplyChanges_ShouldReturnAlreadyTrackedPendingInsert(bool useEntityOverload)
    {
        await using var database = await SqliteTestDatabase.CreateAsync();

        await using (var context = database.CreateContext())
        {
            var tracked = new ManualKeyItem { Id = "manual-id", Name = "Before" };
            var incoming = new ManualKeyItem { Id = "manual-id", Name = "After" };
            context.Add(tracked);

            var changes = useEntityOverload
                ? context.ApplyChanges(
                    [incoming],
                    [],
                    entity => entity.Id)
                : context.ApplyChanges(
                    [new ManualKeyDto("manual-id", "After")],
                    dto => dto.Id,
                    [],
                    entity => entity.Id,
                    _ => incoming);

            var inserted = Assert.Single(changes.Inserted);
            Assert.Same(tracked, inserted);
            Assert.NotSame(incoming, inserted);
            Assert.Equal("After", inserted.Name);
            Assert.Equal(EntityState.Added, context.Entry(inserted).State);
            Assert.Equal(EntityState.Detached, context.Entry(incoming).State);

            await context.SaveChangesAsync();
        }

        await using var verificationContext = database.CreateContext();
        var persisted = await verificationContext.ManualKeyItems.AsNoTracking().SingleAsync();
        Assert.Equal("manual-id", persisted.Id);
        Assert.Equal("After", persisted.Name);
    }
}
