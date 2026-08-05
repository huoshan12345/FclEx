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

    [ConfigureSoftDeleteIndexes(false)]
    [Index(nameof(Name), IsUnique = true)]
    public class EntityWithSoftDeleteIndexesDisabled : ISoftDeletable
    {
        public int Id { get; set; }
        public bool IsDeleted { get; set; }
        public required string Name { get; set; }
    }

    [ConfigureSoftDeleteIndexes]
    [Index(nameof(Name), IsUnique = true)]
    public class EntityWithSoftDeleteIndexesEnabled : ISoftDeletable
    {
        public int Id { get; set; }
        public bool IsDeleted { get; set; }
        public required string Name { get; set; }
    }

    public class TestDbContext : DbContext
    {
        public const string SoftDeleteIndexDatabaseName = "UX_EntityWithSoftDelete_Name";
        public const string SoftDeleteIndexFilter = "\"Name\" IS NOT NULL";
        public const string SoftDeleteIndexAnnotation = "Test:Annotation";

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
            modelBuilder.Entity<EntityWithSoftDeleteIndexesDisabled>();
            modelBuilder.Entity<EntityWithSoftDeleteIndexesEnabled>();

            modelBuilder.Entity<EntityWithSoftDelete>()
                .HasIndex(e => e.Name)
                .IsUnique()
                .IsDescending()
                .HasDatabaseName(SoftDeleteIndexDatabaseName)
                .HasFilter(SoftDeleteIndexFilter)
                .HasAnnotation(SoftDeleteIndexAnnotation, true);
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
    public async Task ConfigureSoftDeleteIndexes_ShouldPreserveIndexMetadata()
    {
        await using var context = new TestDbContext();
        var entityType = context.Model.FindEntityType(typeof(EntityWithSoftDelete));
        Assert.NotNull(entityType);

        var index = Assert.Single(entityType.GetIndexes());

        Assert.Equal(TestDbContext.SoftDeleteIndexDatabaseName, index.GetDatabaseName());
        Assert.Equal(TestDbContext.SoftDeleteIndexFilter, index.GetFilter());
        Assert.Equal(true, index[TestDbContext.SoftDeleteIndexAnnotation]);
        Assert.Equal(
            [nameof(EntityWithSoftDelete.Name), nameof(EntityWithSoftDelete.IsDeleted)],
            index.Properties.Select(property => property.Name));
        Assert.Equal([true, false], index.IsDescending);
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

    [Fact]
    public async Task ConfigureSoftDeleteIndexes_WithDisabledAttribute_ShouldNotModifyIndexes()
    {
        await using var context = new TestDbContext();
        var entityType = context.Model.FindEntityType(typeof(EntityWithSoftDeleteIndexesDisabled));
        Assert.NotNull(entityType);

        var index = Assert.Single(entityType.GetIndexes());

        Assert.True(index.IsUnique);
        Assert.Collection(
            index.Properties,
            property => Assert.Equal(nameof(EntityWithSoftDeleteIndexesDisabled.Name), property.Name));
    }

    [Fact]
    public async Task ConfigureSoftDeleteIndexes_WithEnabledAttribute_ShouldModifyIndexes()
    {
        await using var context = new TestDbContext();
        var entityType = context.Model.FindEntityType(typeof(EntityWithSoftDeleteIndexesEnabled));
        Assert.NotNull(entityType);

        var index = Assert.Single(entityType.GetIndexes());

        Assert.True(index.IsUnique);
        Assert.Equal(
            [nameof(EntityWithSoftDeleteIndexesEnabled.Name), nameof(ISoftDeletable.IsDeleted)],
            index.Properties.Select(property => property.Name));
    }
}
