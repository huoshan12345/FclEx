namespace FclEx.Dapper.SqlAdapters;

/// <summary>
/// Resolves mapped table and column identifiers and quotes them with the selected SQL dialect.
/// </summary>
public static class SqlAdapterExtensions
{
    /// <summary>
    /// Gets the quoted table name, including an effective schema when supported by the adapter.
    /// </summary>
    /// <param name="sqlAdapter">The SQL dialect adapter.</param>
    /// <param name="schema">
    /// An optional schema overriding the mapping schema when non-null. Both schemas are ignored when the adapter
    /// does not support schemas. A non-null schema is quoted as supplied, including an empty string.
    /// </param>
    /// <param name="entityType">The mapped CLR entity type.</param>
    /// <param name="mappingSource">The mapping source, or null to use <see cref="DapperHelper.DefaultEntityMappingSource"/>.</param>
    /// <returns>The quoted table identifier, qualified by the quoted effective schema when it is non-null.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="entityType"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The mapping source returns no mapping or a mapping for another entity type.</exception>
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
    /// <param name="propertyOrColumnName">A CLR property name or database column name, matched without regard to case.</param>
    /// <param name="mappingSource">The mapping source, or null to use <see cref="DapperHelper.DefaultEntityMappingSource"/>.</param>
    /// <returns>The quoted database column name.</returns>
    /// <exception cref="ArgumentException">No mapped property or column has the supplied name.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="entityType"/> or <paramref name="propertyOrColumnName"/> is null.</exception>
    /// <exception cref="InvalidOperationException">The mapping source returns no mapping or a mapping for another entity type.</exception>
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

    /// <summary>
    /// Resolves the member selected by an expression to its mapped database column and quotes that column.
    /// </summary>
    /// <typeparam name="T">The mapped entity type.</typeparam>
    /// <param name="sqlAdapter">The SQL dialect adapter used to quote the column identifier.</param>
    /// <param name="selector">An expression selecting a mapped property of the entity, such as <c>entity => entity.Id</c>.</param>
    /// <param name="mappingSource">The mapping source, or null to use <see cref="DapperHelper.DefaultEntityMappingSource"/>.</param>
    /// <returns>The quoted database column name corresponding to the selected property.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="selector"/> is null.</exception>
    /// <exception cref="ArgumentException">
    /// The expression does not select a member belonging to the entity type, or its member name has no property or
    /// column in the entity mapping.
    /// </exception>
    /// <exception cref="InvalidOperationException">The mapping source returns no mapping or a mapping for another entity type.</exception>
    public static string GetQuotedColumnName<T>(
        this ISqlAdapter sqlAdapter,
        Expression<Func<T, object?>> selector,
        IEntityMappingSource? mappingSource = null)
    {
        var member = Expression.GetMember(selector);
        return sqlAdapter.GetQuotedColumnName(typeof(T), member.Name, mappingSource);
    }
}
