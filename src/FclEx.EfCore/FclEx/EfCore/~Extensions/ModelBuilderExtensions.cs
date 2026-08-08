namespace FclEx.EfCore;

public static class ModelBuilderExtensions
{
    public static EntityTypeBuilder<TEntity> ExcludeFromMigrations<TEntity>(this ModelBuilder modelBuilder, string? tableName = null, string? schema = null) where TEntity : class
    {
        return modelBuilder.Entity<TEntity>().ExcludeFromMigrations(tableName, schema);
    }

    private static LambdaExpression GetLambdaExpression<T>(Expression<Func<T, bool>> filter, Type targetType)
    {
        var parameter = Expression.Parameter(targetType);
        var body = ReplacingExpressionVisitor.Replace(filter.Parameters.First(), parameter, filter.Body);
        return Expression.Lambda(body, parameter);
    }

    public static ModelBuilder HasQueryFilter<T>(this ModelBuilder modelBuilder, IMutableEntityType type, Expression<Func<T, bool>>? filter)
    {
        if (filter is null || type.ClrType.IsAssignableTo(typeof(T)) == false)
            return modelBuilder;

        var builder = modelBuilder.Entity(type.ClrType);
        var lambda = GetLambdaExpression(filter, type.ClrType);
        builder.HasQueryFilter(lambda);
        return modelBuilder;
    }

#if NET10_0_OR_GREATER
    public static ModelBuilder HasQueryFilter<T>(this ModelBuilder modelBuilder, IMutableEntityType type, string? filterKey, Expression<Func<T, bool>>? filter)
    {
        if (filter is null || type.ClrType.IsAssignableTo(typeof(T)) == false)
            return modelBuilder;

        var builder = modelBuilder.Entity(type.ClrType);
        var lambda = GetLambdaExpression(filter, type.ClrType);

        if (filterKey is null)
        {
            builder.HasQueryFilter(lambda);
        }
        else
        {
            builder.HasQueryFilter(filterKey, lambda);
        }

        return modelBuilder;
    }    
#endif
}