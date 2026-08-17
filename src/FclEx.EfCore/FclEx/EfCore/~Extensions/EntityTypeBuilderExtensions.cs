namespace FclEx.EfCore;

/// <summary>
/// Provides relational mapping extensions for <see cref="EntityTypeBuilder{TEntity}"/>.
/// </summary>
public static class EntityTypeBuilderExtensions
{
    /// <summary>
    /// Excludes an entity's table from migrations while preserving its current table name and schema by default.
    /// </summary>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="builder">The entity type builder.</param>
    /// <param name="tableName">An optional replacement table name; otherwise, the current mapped table name is retained.</param>
    /// <param name="schema">An optional replacement schema; otherwise, the current mapped schema is retained.</param>
    /// <returns>The same builder so additional configuration can be chained.</returns>
    public static EntityTypeBuilder<TEntity> ExcludeFromMigrations<TEntity>(this EntityTypeBuilder<TEntity> builder, string? tableName = null, string? schema = null) where TEntity : class
    {
        tableName ??= builder.Metadata.GetTableName() ?? typeof(TEntity).Name;
        schema ??= builder.Metadata.GetSchema();
        return builder.ToTable(tableName, schema, t => t.ExcludeFromMigrations());
    }
}
