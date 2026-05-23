namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

public class ConventionModelBuilderExtensionsTests
{
    [Index(nameof(Name), IsUnique = true)]
    public class EntityWithoutSoftDelete
    {
        public int Id { get; set; }
        public required string Name { get; set; }
    }

    public class EntityWithoutUniqueIndex : ISoftDeletable
    {
        public int Id { get; set; }
        public bool IsDeleted { get; set; }
    }

    [Index(nameof(Name), IsUnique = true)]
    public class EntityWithSoftDelete : ISoftDeletable
    {
        public int Id { get; set; }
        public bool IsDeleted { get; set; }
        public required string Name { get; set; }
    }

    [Index(nameof(Name), IsUnique = true)]
    public class EntityWithDeletedAt : IHasDeletedAt
    {
        public int Id { get; set; }
        public DateTimeOffset DeletedAt { get; set; }
        public required string Name { get; set; }
    }

    [Index(nameof(Name), IsUnique = true)]
    public class EntityWithBoth : ISoftDeletable, IHasDeletedAt
    {
        public int Id { get; set; }
        public bool IsDeleted { get; set; }
        public DateTimeOffset DeletedAt { get; set; }
        public required string Name { get; set; }
    }

    public class TestDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={Guid.NewGuid()};Mode=Memory;Cache=Shared");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EntityWithoutUniqueIndex>();
            modelBuilder.Entity<EntityWithoutSoftDelete>();
            modelBuilder.Entity<EntityWithSoftDelete>();
            modelBuilder.Entity<EntityWithDeletedAt>();
            modelBuilder.Entity<EntityWithBoth>();
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Conventions.Add(m => ConfigureSoftDeleteIndexesConvention.Instance);
        }
    }

    [Fact]
    public async Task ConfigureSoftDeleteIndexes_NoSoftDeleteOrDeletedAt_ShouldNotAddOrRemoveIndexes()
    {
        await using var context = new TestDbContext();
        var entityType = context.Model.FindEntityType(typeof(EntityWithoutSoftDelete));
        Assert.NotNull(entityType);

        var indexes = entityType.GetIndexes().ToList();
        Assert.Single(indexes);

        var index = indexes.First();
        Assert.True(index.IsUnique);
        Assert.Single(index.Properties);
        Assert.Contains(index.Properties, p => p.Name == nameof(EntityWithoutSoftDelete.Name));
    }

    [Fact]
    public async Task ConfigureSoftDeleteIndexes_WithoutUniqueIndex_ShouldNotAddOrRemoveIndexes()
    {
        await using var context = new TestDbContext();
        var entityType = context.Model.FindEntityType(typeof(EntityWithoutUniqueIndex));
        Assert.NotNull(entityType);
        Assert.Empty(entityType.GetIndexes());
    }

    [Fact]
    public async Task ConfigureSoftDeleteIndexes_WithSoftDelete_ShouldAddIsDeletedToUniqueIndexes()
    {
        await using var context = new TestDbContext();
        var entityType = context.Model.FindEntityType(typeof(EntityWithSoftDelete));
        Assert.NotNull(entityType);

        var indexes = entityType.GetIndexes().ToList();
        Assert.Single(indexes);

        var index = indexes.First();
        Assert.True(index.IsUnique);
        Assert.Equal(2, index.Properties.Count);
        Assert.Contains(index.Properties, p => p.Name == nameof(ISoftDeletable.IsDeleted));
        Assert.Contains(index.Properties, p => p.Name == nameof(EntityWithoutSoftDelete.Name));
    }

    [Fact]
    public async Task ConfigureSoftDeleteIndexes_WithDeletedAt_ShouldAddDeletedAtToUniqueIndexes()
    {
        await using var context = new TestDbContext();
        var entityType = context.Model.FindEntityType(typeof(EntityWithDeletedAt));
        Assert.NotNull(entityType);

        var indexes = entityType.GetIndexes().ToList();
        Assert.Single(indexes);

        var index = indexes.First();
        Assert.True(index.IsUnique);
        Assert.Equal(2, index.Properties.Count);
        Assert.Contains(index.Properties, p => p.Name == nameof(IHasDeletedAt.DeletedAt));
        Assert.Contains(index.Properties, p => p.Name == nameof(EntityWithoutSoftDelete.Name));
    }

    [Fact]
    public async Task ConfigureSoftDeleteIndexes_WithBothInterfaces_ShouldAddBothPropertiesToUniqueIndexes()
    {
        await using var context = new TestDbContext();
        var entityType = context.Model.FindEntityType(typeof(EntityWithBoth));
        Assert.NotNull(entityType);

        var indexes = entityType.GetIndexes().ToList();
        Assert.Single(indexes);

        var index = indexes.First();
        Assert.True(index.IsUnique);
        Assert.Equal(3, index.Properties.Count);
        Assert.Contains(index.Properties, p => p.Name == nameof(ISoftDeletable.IsDeleted));
        Assert.Contains(index.Properties, p => p.Name == nameof(IHasDeletedAt.DeletedAt));
        Assert.Contains(index.Properties, p => p.Name == nameof(EntityWithoutSoftDelete.Name));
    }
}