using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FclEx.EfCore;

public static class ModelBuilderExtensions
{
    public static EntityTypeBuilder<TEntity> ExcludeFromMigrations<TEntity>(this ModelBuilder modelBuilder, string? tableName = null) where TEntity : class
    {
        return modelBuilder.Entity<TEntity>().ExcludeFromMigrations(tableName);
    }
}