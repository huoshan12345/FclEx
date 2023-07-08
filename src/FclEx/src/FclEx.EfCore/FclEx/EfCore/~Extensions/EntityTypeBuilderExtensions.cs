using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FclEx.EfCore;

public static class EntityTypeBuilderExtensions
{
    public static EntityTypeBuilder<TEntity> ExcludeFromMigrations<TEntity>(this EntityTypeBuilder<TEntity> builder, string? tableName = null) where TEntity : class
    {
        tableName ??= builder.Metadata.GetTableName() ?? typeof(TEntity).Name.TrimEnd("Entity");
        return builder.ToTable(tableName, t => t.ExcludeFromMigrations());
    }
}