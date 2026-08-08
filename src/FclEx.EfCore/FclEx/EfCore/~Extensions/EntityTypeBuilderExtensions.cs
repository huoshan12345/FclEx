namespace FclEx.EfCore;

public static class EntityTypeBuilderExtensions
{
    public static EntityTypeBuilder<TEntity> ExcludeFromMigrations<TEntity>(this EntityTypeBuilder<TEntity> builder, string? tableName = null, string? schema = null) where TEntity : class
    {
        tableName ??= builder.Metadata.GetTableName() ?? typeof(TEntity).Name;
        schema ??= builder.Metadata.GetSchema();
        return builder.ToTable(tableName, schema, t => t.ExcludeFromMigrations());
    }
}