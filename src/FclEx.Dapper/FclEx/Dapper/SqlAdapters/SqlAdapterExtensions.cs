namespace FclEx.Dapper.SqlAdapters;

public static class SqlAdapterExtensions
{
    /// <summary>
    /// Gets the quoted table name, including an effective schema when supported by the adapter.
    /// </summary>
    /// <param name="sqlAdapter">The SQL dialect adapter.</param>
    /// <param name="schema">An optional schema overriding the mapping schema when non-null.</param>
    /// <param name="entityType">The mapped CLR entity type.</param>
    /// <param name="mappingSource">An optional entity mapping source.</param>
    /// <returns>The quoted table identifier.</returns>
    public static string GetTableNameWithSchema(
        this ISqlAdapter sqlAdapter,
        string? schema,
        Type entityType,
        IEntityMappingSource? mappingSource = null)
    {
        var mapping = DapperHelper.GetEntityMapping(entityType, mappingSource);
        return DapperHelper.GetTableNameWithSchema(sqlAdapter, schema, mapping);
    }

    /// <summary>
    /// Gets a quoted column name from either its CLR property name or database column name.
    /// </summary>
    /// <param name="sqlAdapter">The SQL dialect adapter.</param>
    /// <param name="entityType">The mapped CLR entity type.</param>
    /// <param name="propertyOrColumnName">A CLR property name or database column name.</param>
    /// <param name="mappingSource">An optional entity mapping source.</param>
    /// <returns>The quoted database column name.</returns>
    /// <exception cref="ArgumentException">No mapped property or column has the supplied name.</exception>
    public static string GetQuotedColumnName(
        this ISqlAdapter sqlAdapter,
        Type entityType,
        string propertyOrColumnName,
        IEntityMappingSource? mappingSource = null)
    {
        var mapping = DapperHelper.GetEntityMapping(entityType, mappingSource);
        var property = mapping.FindProperty(propertyOrColumnName);
        return property is null
            ? throw new ArgumentException(
                $"Property or column '{propertyOrColumnName}' was not found in the mapping for '{entityType.FullName}'.",
                nameof(propertyOrColumnName))
            : sqlAdapter.GetQuotedColumnName(property.ColumnName);
    }

    public static string GetQuotedColumnName<T>(
        this ISqlAdapter sqlAdapter,
        Expression<Func<T, object?>> selector,
        IEntityMappingSource? mappingSource = null)
    {
        var member = Expression.GetMember(selector);
        return sqlAdapter.GetQuotedColumnName(typeof(T), member.Name, mappingSource);
    }
}
