namespace FclEx.EfCore.Extensions;

public class EntityTypeBuilderExtensionsTests
{
    private class CustomerEntity
    {
        public int Id { get; set; }
    }

    [Fact]
    public void ExcludeFromMigrations_ShouldPreserveDefaultTableName()
    {
        var modelBuilder = new ModelBuilder(new ConventionSet());

        var builder = modelBuilder.Entity<CustomerEntity>().ExcludeFromMigrations();

        Assert.Equal(nameof(CustomerEntity), builder.Metadata.GetTableName());
        Assert.True(builder.Metadata.IsTableExcludedFromMigrations());
    }

    [Fact]
    public void ExcludeFromMigrations_ShouldPreserveExistingSchema()
    {
        var modelBuilder = new ModelBuilder(new ConventionSet());
        var entityBuilder = modelBuilder.Entity<CustomerEntity>()
            .ToTable("Customers", "tenant");

        entityBuilder.ExcludeFromMigrations();

        Assert.Equal("Customers", entityBuilder.Metadata.GetTableName());
        Assert.Equal("tenant", entityBuilder.Metadata.GetSchema());
        Assert.True(entityBuilder.Metadata.IsTableExcludedFromMigrations());
    }
}
