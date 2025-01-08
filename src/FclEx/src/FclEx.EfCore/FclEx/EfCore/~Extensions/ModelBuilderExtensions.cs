namespace FclEx.EfCore;

public static class ModelBuilderExtensions
{
    public static EntityTypeBuilder<TEntity> ExcludeFromMigrations<TEntity>(this ModelBuilder modelBuilder, string? tableName = null) where TEntity : class
    {
        return modelBuilder.Entity<TEntity>().ExcludeFromMigrations(tableName);
    }

    public static ModelBuilder HasQueryFilter<T>(this ModelBuilder modelBuilder, IMutableEntityType type, Expression<Func<T, bool>>? filter)
    {
        if (filter is null)
            return modelBuilder;

        if (type.ClrType.IsAssignableTo(typeof(T)) == false)
            return modelBuilder;

        var builder = modelBuilder.Entity(type.Name);
        var parameter = Expression.Parameter(type.ClrType);
        var body = ReplacingExpressionVisitor.Replace(filter.Parameters.First(), parameter, filter.Body);
        var lambda = Expression.Lambda(body, parameter);
        builder.HasQueryFilter(lambda);
        return modelBuilder;
    }
}