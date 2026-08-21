using Microsoft.Data.Sqlite;

namespace FclEx.EfCore.Extensions;

public partial class DbContextExtensionsApplyChangesSqliteTests
{
    [Fact]
    public async Task EmptySets_ShouldProduceNoChanges()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        await using var context = database.CreateContext();

        var changes = context.ApplyChanges(
            Array.Empty<ItemDto>(),
            dto => dto.Code,
            [],
            entity => entity.Code,
            dto => new Item { Code = dto.Code, Name = dto.Name });

        Assert.Empty(changes.Inserted);
        Assert.Empty(changes.Updated);
        Assert.Empty(changes.Deleted);
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [Fact]
    public async Task DtoOverload_ShouldGenerateKeyWhenInsertedEntityHasDefaultKey()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        await using (var context = database.CreateContext())
        {
            var changes = context.ApplyChanges(
                [new ItemDto("new", "New")],
                dto => dto.Code,
                [],
                entity => entity.Code,
                dto => new Item { Code = dto.Code, Name = dto.Name });

            var inserted = Assert.Single(changes.Inserted);
            Assert.Equal(EntityState.Added, context.Entry(inserted).State);

            await context.SaveChangesAsync();
            Assert.True(inserted.Id > 0);
        }

        await using var verificationContext = database.CreateContext();
        var persisted = await verificationContext.Items.AsNoTracking().SingleAsync();
        Assert.Equal("new", persisted.Code);
        Assert.Equal("New", persisted.Name);
    }

    [Fact]
    public async Task DtoOverload_ShouldPreserveExplicitGeneratedKeyOnInsert()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        await using (var context = database.CreateContext())
        {
            var changes = context.ApplyChanges(
                [new ItemDto("new", "New")],
                dto => dto.Code,
                [],
                entity => entity.Code,
                dto => new Item { Id = 123, Code = dto.Code, Name = dto.Name });

            var inserted = Assert.Single(changes.Inserted);
            Assert.Equal(123, inserted.Id);
            Assert.Equal(EntityState.Added, context.Entry(inserted).State);

            await context.SaveChangesAsync();
            Assert.Equal(123, inserted.Id);
        }

        await using var verificationContext = database.CreateContext();
        var persisted = await verificationContext.Items.AsNoTracking().SingleAsync();
        Assert.Equal(123, persisted.Id);
        Assert.Equal("new", persisted.Code);
        Assert.Equal("New", persisted.Name);
    }

    [Fact]
    public async Task DtoOverload_ShouldPersistInPlaceUpdateOfTrackedEntity()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        await using (var context = database.CreateContext())
        {
            var existing = new Item { Code = "one", Name = "Before" };
            context.Add(existing);
            await context.SaveChangesAsync();

            var changes = context.ApplyChanges(
                [new ItemDto("one", "After")],
                dto => dto.Code,
                [existing],
                entity => entity.Code,
                dto => new Item { Code = dto.Code, Name = dto.Name },
                (dto, entity) =>
                {
                    entity.Name = dto.Name;
                    return entity;
                });

            var update = Assert.Single(changes.Updated);
            Assert.Same(existing, update.New);
            Assert.Same(existing, update.Existing);
            Assert.Equal(EntityState.Modified, context.Entry(existing).State);
            await context.SaveChangesAsync();
        }

        await using var verificationContext = database.CreateContext();
        Assert.Equal("After", await verificationContext.Items.Select(entity => entity.Name).SingleAsync());
    }

    [Fact]
    public async Task DtoOverload_ShouldPersistInPlaceUpdateOfUntrackedEntity()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        long id;
        await using (var setupContext = database.CreateContext())
        {
            var item = new Item { Code = "one", Name = "Before" };
            setupContext.Add(item);
            await setupContext.SaveChangesAsync();
            id = item.Id;
        }

        await using (var context = database.CreateContext())
        {
            var existing = await context.Items.AsNoTracking().SingleAsync(entity => entity.Id == id);
            context.ApplyChanges(
                [new ItemDto("one", "After")],
                dto => dto.Code,
                [existing],
                entity => entity.Code,
                dto => new Item { Code = dto.Code, Name = dto.Name },
                (dto, entity) =>
                {
                    entity.Name = dto.Name;
                    return entity;
                });

            Assert.Equal(EntityState.Modified, context.Entry(existing).State);
            await context.SaveChangesAsync();
        }

        await using var verificationContext = database.CreateContext();
        Assert.Equal("After", await verificationContext.Items.Select(entity => entity.Name).SingleAsync());
    }

    [Fact]
    public async Task DtoOverload_ShouldPersistReplacementReturnedForTrackedEntity()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        await using (var context = database.CreateContext())
        {
            var existing = new Item { Code = "one", Name = "Before" };
            context.Add(existing);
            await context.SaveChangesAsync();

            var changes = context.ApplyChanges(
                [new ItemDto("one", "After")],
                dto => dto.Code,
                [existing],
                entity => entity.Code,
                dto => new Item { Code = dto.Code, Name = dto.Name },
                (dto, _) => new Item { Code = dto.Code, Name = dto.Name });

            var update = Assert.Single(changes.Updated);
            Assert.NotSame(existing, update.New);
            Assert.Equal(existing.Id, update.New.Id);
            await context.SaveChangesAsync();
        }

        await using var verificationContext = database.CreateContext();
        Assert.Equal("After", await verificationContext.Items.Select(entity => entity.Name).SingleAsync());
    }

    [Fact]
    public async Task DtoOverload_ShouldExcludeNamedScalarPropertyFromUpdate()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        await using (var context = database.CreateContext())
        {
            var existing = new Item { Code = "one", Name = "Before", ProtectedValue = "Keep" };
            context.Add(existing);
            await context.SaveChangesAsync();

            context.ApplyChanges(
                [new ItemDto("one", "After", "Replace")],
                dto => dto.Code,
                [existing],
                entity => entity.Code,
                dto => new Item { Code = dto.Code, Name = dto.Name, ProtectedValue = dto.ProtectedValue },
                (dto, entity) =>
                {
                    entity.Name = dto.Name;
                    entity.ProtectedValue = dto.ProtectedValue;
                    return entity;
                },
                excludeOnUpdate: [nameof(Item.ProtectedValue)]);

            Assert.False(context.Entry(existing).Property(entity => entity.ProtectedValue).IsModified);
            await context.SaveChangesAsync();
        }

        await using var verificationContext = database.CreateContext();
        var persisted = await verificationContext.Items.AsNoTracking().SingleAsync();
        Assert.Equal("After", persisted.Name);
        Assert.Equal("Keep", persisted.ProtectedValue);
    }

    [Fact]
    public async Task DtoOverload_ShouldKeepMissingEntityWhenDeletionIsDisabled()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        await using (var context = database.CreateContext())
        {
            var existing = new Item { Code = "missing", Name = "Keep" };
            context.Add(existing);
            await context.SaveChangesAsync();

            var changes = context.ApplyChanges(
                Array.Empty<ItemDto>(),
                dto => dto.Code,
                [existing],
                entity => entity.Code,
                dto => new Item { Code = dto.Code, Name = dto.Name });

            Assert.Empty(changes.Deleted);
            Assert.Equal(EntityState.Unchanged, context.Entry(existing).State);
            await context.SaveChangesAsync();
        }

        await using var verificationContext = database.CreateContext();
        Assert.Equal(1, await verificationContext.Items.CountAsync());
    }

    [Fact]
    public async Task DtoOverload_ShouldDeleteOnlyMissingEntityWhenDeletionIsEnabled()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        await using (var context = database.CreateContext())
        {
            var retained = new Item { Code = "retained", Name = "Retained" };
            var missing = new Item { Code = "missing", Name = "Missing" };
            context.AddRange(retained, missing);
            await context.SaveChangesAsync();

            var changes = context.ApplyChanges(
                [new ItemDto("retained", "Retained")],
                dto => dto.Code,
                [retained, missing],
                entity => entity.Code,
                dto => new Item { Code = dto.Code, Name = dto.Name },
                (_, entity) => entity,
                allowDeletion: true);

            Assert.Equal(missing, Assert.Single(changes.Deleted));
            Assert.DoesNotContain(retained, changes.Deleted);
            await context.SaveChangesAsync();
        }

        await using var verificationContext = database.CreateContext();
        Assert.Equal(["retained"], await verificationContext.Items.Select(entity => entity.Code).ToListAsync());
    }

    [Fact]
    public async Task DtoOverload_ShouldKeepMatchedEntityWhenUpdateReturnsNullAndDeletionIsEnabled()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        await using (var context = database.CreateContext())
        {
            var existing = new Item { Code = "one", Name = "Unchanged" };
            context.Add(existing);
            await context.SaveChangesAsync();

            var changes = context.ApplyChanges(
                [new ItemDto("one", "Unchanged")],
                dto => dto.Code,
                [existing],
                entity => entity.Code,
                dto => new Item { Code = dto.Code, Name = dto.Name },
                (_, _) => null,
                allowDeletion: true);

            Assert.Empty(changes.Updated);
            Assert.Empty(changes.Deleted);
            Assert.Equal(EntityState.Unchanged, context.Entry(existing).State);
            await context.SaveChangesAsync();
        }

        await using var verificationContext = database.CreateContext();
        Assert.Equal(1, await verificationContext.Items.CountAsync());
    }

    [Fact]
    public async Task EntityOverload_DefaultMappings_ShouldPersistMixedChanges()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        await using (var context = database.CreateContext())
        {
            var update = new Item { Code = "update", Name = "Before" };
            var delete = new Item { Code = "delete", Name = "Delete" };
            context.AddRange(update, delete);
            await context.SaveChangesAsync();

            var changes = context.ApplyChanges(
                [
                    new Item { Id = update.Id, Code = "update", Name = "After" },
                    new Item { Code = "insert", Name = "Insert" },
                ],
                [update, delete],
                entity => entity.Code,
                allowDeletion: true);

            Assert.Single(changes.Inserted);
            Assert.Single(changes.Updated);
            Assert.Equal(delete, Assert.Single(changes.Deleted));
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

    [Fact]
    public async Task DtoOverload_ShouldRestoreMatchedSoftDeletedEntityWithoutUpdateDelegate()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        await using (var context = database.CreateContext())
        {
            var existing = new Item { Code = "one", Name = "Deleted", IsDeleted = true };
            context.Add(existing);
            await context.SaveChangesAsync();

            var changes = context.ApplyChanges(
                [new ItemDto("one", "Ignored")],
                dto => dto.Code,
                [existing],
                entity => entity.Code,
                dto => new Item { Code = dto.Code, Name = dto.Name },
                allowDeletion: true);

            Assert.False(existing.IsDeleted);
            Assert.Single(changes.Updated);
            Assert.Empty(changes.Deleted);
            await context.SaveChangesAsync();
        }

        await using var verificationContext = database.CreateContext();
        Assert.False(await verificationContext.Items.Select(entity => entity.IsDeleted).SingleAsync());
    }

    [Fact]
    public async Task DtoOverload_ShouldSynchronizeRequiredChildren()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        await using (var context = database.CreateContext())
        {
            var parent = new Parent { Name = "Parent" };
            var update = new Child { Code = "update", Name = "Before", Parent = parent };
            var delete = new Child { Code = "delete", Name = "Delete", Parent = parent };
            context.AddRange(update, delete);
            await context.SaveChangesAsync();

            var changes = context.ApplyChanges(
                [
                    new ChildDto("update", "After"),
                    new ChildDto("insert", "Insert"),
                ],
                dto => dto.Code,
                [update, delete],
                entity => entity.Code,
                dto => new Child { ParentId = parent.Id, Code = dto.Code, Name = dto.Name },
                (dto, entity) =>
                {
                    entity.Name = dto.Name;
                    return entity;
                },
                allowDeletion: true);

            Assert.Single(changes.Inserted);
            Assert.Single(changes.Updated);
            Assert.Single(changes.Deleted);
            await context.SaveChangesAsync();
        }

        await using var verificationContext = database.CreateContext();
        var children = await verificationContext.Children
            .AsNoTracking()
            .OrderBy(entity => entity.Code)
            .Select(entity => new { entity.Code, entity.Name })
            .ToListAsync();
        Assert.Equal(2, children.Count);
        Assert.Equal("Insert", children.Single(entity => entity.Code == "insert").Name);
        Assert.Equal("After", children.Single(entity => entity.Code == "update").Name);
    }

    [Fact]
    public async Task DtoOverload_ShouldApplyLastDuplicateDtoToMatchedEntity()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        await using (var context = database.CreateContext())
        {
            var existing = new Item { Code = "one", Name = "Before" };
            context.Add(existing);
            await context.SaveChangesAsync();

            var changes = context.ApplyChanges(
                [new ItemDto("one", "First"), new ItemDto("one", "Second")],
                dto => dto.Code,
                [existing],
                entity => entity.Code,
                dto => new Item { Code = dto.Code, Name = dto.Name },
                (dto, entity) =>
                {
                    entity.Name = dto.Name;
                    return entity;
                },
                allowDeletion: true);

            Assert.Equal(2, changes.Updated.Count);
            Assert.Empty(changes.Deleted);
            await context.SaveChangesAsync();
        }

        await using var verificationContext = database.CreateContext();
        Assert.Equal("Second", await verificationContext.Items.Select(entity => entity.Name).SingleAsync());
    }

    [Fact]
    public async Task DtoOverload_ShouldPreserveExplicitNonGeneratedPrimaryKeyOnInsert()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        await using (var context = database.CreateContext())
        {
            var changes = context.ApplyChanges(
                [new ManualKeyDto("manual-id", "Manual")],
                dto => dto.Id,
                [],
                entity => entity.Id,
                dto => new ManualKeyItem { Id = dto.Id, Name = dto.Name });

            Assert.Equal("manual-id", Assert.Single(changes.Inserted).Id);
            await context.SaveChangesAsync();
        }

        await using var verificationContext = database.CreateContext();
        var persisted = await verificationContext.ManualKeyItems.AsNoTracking().SingleAsync();
        Assert.Equal("manual-id", persisted.Id);
        Assert.Equal("Manual", persisted.Name);
    }

    [Fact]
    public async Task DtoOverload_ShouldUpdateEntityWithShadowPrimaryKeyInPlace()
    {
        await using var database = await SqliteTestDatabase.CreateAsync();
        await using (var context = database.CreateContext())
        {
            var existing = new ShadowKeyItem { Code = "one", Name = "Before" };
            context.Add(existing);
            await context.SaveChangesAsync();

            var changes = context.ApplyChanges(
                [new ItemDto("one", "After")],
                dto => dto.Code,
                [existing],
                entity => entity.Code,
                dto => new ShadowKeyItem { Code = dto.Code, Name = dto.Name },
                (dto, entity) =>
                {
                    entity.Name = dto.Name;
                    return entity;
                });

            Assert.Single(changes.Updated);
            await context.SaveChangesAsync();
        }

        await using var verificationContext = database.CreateContext();
        Assert.Equal("After", await verificationContext.ShadowKeyItems.Select(entity => entity.Name).SingleAsync());
    }

    private sealed record ItemDto(string Code, string Name, string ProtectedValue = "");

    private sealed record ChildDto(string Code, string Name);

    private sealed record ManualKeyDto(string Id, string Name);

    private sealed class Item : ISoftDeletable
    {
        public long Id { get; set; }

        public string Code { get; set; } = "";

        public string Name { get; set; } = "";

        public string ProtectedValue { get; set; } = "";

        public bool IsDeleted { get; set; }
    }

    private sealed class Parent
    {
        public long Id { get; set; }

        public string Name { get; set; } = "";

        public List<Child> Children { get; set; } = [];
    }

    private sealed class Child
    {
        public long Id { get; set; }

        public long ParentId { get; set; }

        public Parent Parent { get; set; } = null!;

        public string Code { get; set; } = "";

        public string Name { get; set; } = "";
    }

    private sealed class ManualKeyItem
    {
        public string Id { get; set; } = "";

        public string Name { get; set; } = "";
    }

    private sealed class ShadowKeyItem
    {
        public string Code { get; set; } = "";

        public string Name { get; set; } = "";
    }

    private sealed class ApplyChangesDbContext(DbContextOptions<ApplyChangesDbContext> options) : DbContext(options)
    {
        public DbSet<Item> Items => Set<Item>();

        public DbSet<Parent> Parents => Set<Parent>();

        public DbSet<Child> Children => Set<Child>();

        public DbSet<ManualKeyItem> ManualKeyItems => Set<ManualKeyItem>();

        public DbSet<ShadowKeyItem> ShadowKeyItems => Set<ShadowKeyItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Item>(builder =>
            {
                builder.HasKey(entity => entity.Id);
                builder.Property(entity => entity.Id).ValueGeneratedOnAdd();
                builder.Property(entity => entity.Code).IsRequired();
                builder.Property(entity => entity.Name).IsRequired();
                builder.Property(entity => entity.ProtectedValue).IsRequired();
            });

            modelBuilder.Entity<Parent>(builder =>
            {
                builder.HasKey(entity => entity.Id);
                builder.Property(entity => entity.Id).ValueGeneratedOnAdd();
                builder.Property(entity => entity.Name).IsRequired();
            });

            modelBuilder.Entity<Child>(builder =>
            {
                builder.HasKey(entity => entity.Id);
                builder.Property(entity => entity.Id).ValueGeneratedOnAdd();
                builder.Property(entity => entity.Code).IsRequired();
                builder.Property(entity => entity.Name).IsRequired();
                builder.HasOne(entity => entity.Parent)
                    .WithMany(entity => entity.Children)
                    .HasForeignKey(entity => entity.ParentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ManualKeyItem>(builder =>
            {
                builder.HasKey(entity => entity.Id);
                builder.Property(entity => entity.Id).ValueGeneratedNever();
                builder.Property(entity => entity.Name).IsRequired();
            });

            modelBuilder.Entity<ShadowKeyItem>(builder =>
            {
                builder.Property<long>("Id").ValueGeneratedOnAdd();
                builder.HasKey("Id");
                builder.Property(entity => entity.Code).IsRequired();
                builder.Property(entity => entity.Name).IsRequired();
            });
        }
    }

    private sealed class SqliteTestDatabase : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<ApplyChangesDbContext> _options;

        private SqliteTestDatabase(SqliteConnection connection)
        {
            _connection = connection;
            _options = new DbContextOptionsBuilder<ApplyChangesDbContext>()
                .UseSqlite(connection)
                .Options;
        }

        public static async Task<SqliteTestDatabase> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var database = new SqliteTestDatabase(connection);
            await using var context = database.CreateContext();
            await context.Database.EnsureCreatedAsync();
            return database;
        }

        public ApplyChangesDbContext CreateContext() => new(_options);

        public ValueTask DisposeAsync() => _connection.DisposeAsync();
    }
}
