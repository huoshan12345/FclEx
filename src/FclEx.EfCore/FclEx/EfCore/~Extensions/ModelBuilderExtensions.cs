namespace FclEx.EfCore;

/// <summary>
/// Provides model-wide helpers for relational mappings and query filters.
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// Configures an entity table as excluded from migrations.
    /// </summary>
    /// <typeparam name="TEntity">The entity type being configured.</typeparam>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="tableName">An optional replacement table name; otherwise, the existing mapping is retained.</param>
    /// <param name="schema">An optional replacement schema; otherwise, the existing mapping is retained.</param>
    /// <returns>The entity type builder.</returns>
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

    /// <summary>
    /// Applies a query filter expressed for a base type or interface to a compatible mutable entity type.
    /// </summary>
    /// <typeparam name="T">The type for which the filter is expressed.</typeparam>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="type">The entity type that may receive the filter.</param>
    /// <param name="filter">The filter to adapt, or <see langword="null"/> to make no change.</param>
    /// <returns>The same model builder.</returns>
    /// <remarks>A non-null filter is ignored when <paramref name="type"/>'s CLR type is not assignable to <typeparamref name="T"/>.</remarks>
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
    /// <summary>
    /// Applies a named or unnamed query filter expressed for a base type or interface to a compatible mutable entity type.
    /// </summary>
    /// <typeparam name="T">The type for which the filter is expressed.</typeparam>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="type">The entity type that may receive the filter.</param>
    /// <param name="filterKey">The EF Core filter name, or <see langword="null"/> to configure the unnamed filter.</param>
    /// <param name="filter">The filter to adapt, or <see langword="null"/> to make no change.</param>
    /// <returns>The same model builder.</returns>
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
