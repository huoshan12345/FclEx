using Microsoft.EntityFrameworkCore.Infrastructure;

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

    [Index(nameof(Name))]
    public class EntityWithNonUniqueIndex : ISoftDeletable
    {
        public int Id { get; set; }
        public bool IsDeleted { get; set; }
        public required string Name { get; set; }
    }

    [Index(nameof(Name), nameof(IsDeleted), IsUnique = true)]
    public class EntityWithExistingSoftDeleteProperty : ISoftDeletable
    {
        public int Id { get; set; }
        public bool IsDeleted { get; set; }
        public required string Name { get; set; }
    }

    [Index(nameof(Name), IsUnique = true)]
    [Index(nameof(Code), IsUnique = true)]
    public class EntityWithMultipleUniqueIndexes : ISoftDeletable
    {
        public int Id { get; set; }
        public bool IsDeleted { get; set; }
        public required string Name { get; set; }
        public required string Code { get; set; }
    }

    [Index(nameof(Name), IsUnique = true)]
    [Index(nameof(Name), nameof(IsDeleted), IsUnique = true)]
    public class EntityWithConvergingUniqueIndexes : ISoftDeletable
    {
        public int Id { get; set; }
        public bool IsDeleted { get; set; }
        public required string Name { get; set; }
    }

    [Index(nameof(Name), IsUnique = true)]
    [Index(nameof(Name), nameof(IsDeleted))]
    public class EntityWithExistingNonUniqueReplacement : ISoftDeletable
    {
        public int Id { get; set; }
        public bool IsDeleted { get; set; }
        public required string Name { get; set; }
    }

    public class EntityWithMixedSortOrder : ISoftDeletable
    {
        public int Id { get; set; }
        public bool IsDeleted { get; set; }
        public required string Name { get; set; }
        public required string Code { get; set; }
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
        public const string SoftDeleteIndexModelName = "SoftDeleteModelIndex";
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
            modelBuilder.Entity<EntityWithNonUniqueIndex>();
            modelBuilder.Entity<EntityWithExistingSoftDeleteProperty>();
            modelBuilder.Entity<EntityWithMultipleUniqueIndexes>();
            modelBuilder.Entity<EntityWithConvergingUniqueIndexes>();
            modelBuilder.Entity<EntityWithExistingNonUniqueReplacement>();

            modelBuilder.Entity<EntityWithSoftDelete>()
                .HasIndex(e => e.Name, SoftDeleteIndexModelName)
                .IsUnique()
                .IsDescending()
                .HasDatabaseName(SoftDeleteIndexDatabaseName)
                .HasFilter(SoftDeleteIndexFilter)
                .HasAnnotation(SoftDeleteIndexAnnotation, true);

            modelBuilder.Entity<EntityWithMixedSortOrder>()
                .HasIndex(e => new { e.Name, e.Code })
                .IsUnique()
                .IsDescending(true, false);
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
        var model = context.GetService<IDesignTimeModel>().Model;
        var entityType = model.FindEntityType(typeof(EntityWithSoftDelete));
        Assert.NotNull(entityType);

        var index = Assert.Single(entityType.GetIndexes());

        Assert.Equal(TestDbContext.SoftDeleteIndexModelName, index.Name);
        Assert.Equal(TestDbContext.SoftDeleteIndexDatabaseName, index.GetDatabaseName());
        Assert.Equal(TestDbContext.SoftDeleteIndexFilter, index.GetFilter());
        Assert.Equal(true, index[TestDbContext.SoftDeleteIndexAnnotation]);
        Assert.Equal(
            [nameof(EntityWithSoftDelete.Name), nameof(EntityWithSoftDelete.IsDeleted)],
            index.Properties.Select(property => property.Name));
        Assert.Equal([true, false], index.IsDescending);

        var conventionIndex = Assert.IsAssignableFrom<IConventionIndex>(index);
        Assert.Equal(ConfigurationSource.Explicit, conventionIndex.GetConfigurationSource());
        Assert.Equal(ConfigurationSource.Explicit, conventionIndex.GetIsUniqueConfigurationSource());
        Assert.Equal(ConfigurationSource.Explicit, conventionIndex.GetIsDescendingConfigurationSource());
        Assert.Equal(
            ConfigurationSource.Explicit,
            conventionIndex.GetAnnotation(TestDbContext.SoftDeleteIndexAnnotation).GetConfigurationSource());
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
        var model = context.GetService<IDesignTimeModel>().Model;
        var entityType = model.FindEntityType(typeof(EntityWithSoftDeleteIndexesEnabled));
        Assert.NotNull(entityType);

        var index = Assert.Single(entityType.GetIndexes());

        Assert.True(index.IsUnique);
        Assert.Equal(
            [nameof(EntityWithSoftDeleteIndexesEnabled.Name), nameof(ISoftDeletable.IsDeleted)],
            index.Properties.Select(property => property.Name));

        var conventionIndex = Assert.IsAssignableFrom<IConventionIndex>(index);
        Assert.Equal(ConfigurationSource.DataAnnotation, conventionIndex.GetConfigurationSource());
        Assert.Equal(ConfigurationSource.DataAnnotation, conventionIndex.GetIsUniqueConfigurationSource());
    }

    [Fact]
    public async Task ConfigureSoftDeleteIndexes_WithNonUniqueIndex_ShouldNotModifyIndex()
    {
        await using var context = new TestDbContext();
        var entityType = context.Model.FindEntityType(typeof(EntityWithNonUniqueIndex));
        Assert.NotNull(entityType);

        var index = Assert.Single(entityType.GetIndexes());

        Assert.False(index.IsUnique);
        Assert.Collection(
            index.Properties,
            property => Assert.Equal(nameof(EntityWithNonUniqueIndex.Name), property.Name));
    }

    [Fact]
    public async Task ConfigureSoftDeleteIndexes_WhenIndexAlreadyContainsProperty_ShouldNotDuplicateProperty()
    {
        await using var context = new TestDbContext();
        var entityType = context.Model.FindEntityType(typeof(EntityWithExistingSoftDeleteProperty));
        Assert.NotNull(entityType);

        var index = Assert.Single(entityType.GetIndexes());

        Assert.True(index.IsUnique);
        Assert.Equal(
            [nameof(EntityWithExistingSoftDeleteProperty.Name), nameof(ISoftDeletable.IsDeleted)],
            index.Properties.Select(property => property.Name));
    }

    [Fact]
    public async Task ConfigureSoftDeleteIndexes_WithMultipleUniqueIndexes_ShouldUpdateEveryIndex()
    {
        await using var context = new TestDbContext();
        var entityType = context.Model.FindEntityType(typeof(EntityWithMultipleUniqueIndexes));
        Assert.NotNull(entityType);

        var indexes = entityType.GetIndexes()
            .OrderBy(index => index.Properties[0].Name)
            .ToArray();

        Assert.Collection(
            indexes,
            index => Assert.Equal(
                [nameof(EntityWithMultipleUniqueIndexes.Code), nameof(ISoftDeletable.IsDeleted)],
                index.Properties.Select(property => property.Name)),
            index => Assert.Equal(
                [nameof(EntityWithMultipleUniqueIndexes.Name), nameof(ISoftDeletable.IsDeleted)],
                index.Properties.Select(property => property.Name)));
    }

    [Fact]
    public async Task ConfigureSoftDeleteIndexes_WhenIndexesConverge_ShouldKeepSingleIndex()
    {
        await using var context = new TestDbContext();
        var entityType = context.Model.FindEntityType(typeof(EntityWithConvergingUniqueIndexes));
        Assert.NotNull(entityType);

        var index = Assert.Single(entityType.GetIndexes());

        Assert.True(index.IsUnique);
        Assert.Equal(
            [nameof(EntityWithConvergingUniqueIndexes.Name), nameof(ISoftDeletable.IsDeleted)],
            index.Properties.Select(property => property.Name));
    }

    [Fact]
    public async Task ConfigureSoftDeleteIndexes_WhenNonUniqueReplacementExists_ShouldPreserveUniqueness()
    {
        await using var context = new TestDbContext();
        var entityType = context.Model.FindEntityType(typeof(EntityWithExistingNonUniqueReplacement));
        Assert.NotNull(entityType);

        var index = Assert.Single(entityType.GetIndexes());

        Assert.True(index.IsUnique);
        Assert.Equal(
            [nameof(EntityWithExistingNonUniqueReplacement.Name), nameof(ISoftDeletable.IsDeleted)],
            index.Properties.Select(property => property.Name));
    }

    [Fact]
    public async Task ConfigureSoftDeleteIndexes_WithMixedSortOrder_ShouldPreserveExistingOrder()
    {
        await using var context = new TestDbContext();
        var model = context.GetService<IDesignTimeModel>().Model;
        var entityType = model.FindEntityType(typeof(EntityWithMixedSortOrder));
        Assert.NotNull(entityType);

        var index = Assert.Single(entityType.GetIndexes());

        Assert.Equal(
            [nameof(EntityWithMixedSortOrder.Name), nameof(EntityWithMixedSortOrder.Code), nameof(ISoftDeletable.IsDeleted)],
            index.Properties.Select(property => property.Name));
        Assert.Equal([true, false, false], index.IsDescending);
    }
}
