namespace FclEx.Dapper;

/// <summary>
/// Provides the immutable table, property, key, and database-generation mapping for an entity type.
/// </summary>
public sealed class EntityMapping
{
    private readonly IReadOnlyList<PropertyMapping> _explicitInsertProperties;
    private readonly IReadOnlyDictionary<string, PropertyMapping> _propertiesByIdentifier;

    /// <summary>
    /// Creates an entity mapping.
    /// </summary>
    /// <param name="entityType">The mapped CLR entity type.</param>
    /// <param name="tableName">The unquoted database table name.</param>
    /// <param name="properties">All persistent properties. Omit transient and navigation properties.</param>
    /// <param name="schema">The optional unquoted database schema name.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="entityType"/> or <paramref name="properties"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// A name is invalid, no properties are supplied, a property belongs to another type, or property and
    /// column identifiers are ambiguous under case-insensitive comparison.
    /// </exception>
    public EntityMapping(
        Type entityType,
        string tableName,
        IEnumerable<PropertyMapping> properties,
        string? schema = null)
    {
        EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentException("A table name cannot be empty or whitespace.", nameof(tableName));
        if (schema is not null && string.IsNullOrWhiteSpace(schema))
            throw new ArgumentException("A schema name cannot be empty or whitespace.", nameof(schema));
        if (properties is null)
            throw new ArgumentNullException(nameof(properties));

        TableName = tableName;
        Schema = schema;

        var propertyArray = properties.ToArray();
        if (propertyArray.Length == 0)
            throw new ArgumentException("An entity mapping must contain at least one persistent property.", nameof(properties));

        var identifiers = new Dictionary<string, PropertyMapping>(StringComparer.OrdinalIgnoreCase);
        foreach (var propertyMapping in propertyArray)
        {
            if (propertyMapping is null)
                throw new ArgumentException("A property mapping cannot be null.", nameof(properties));

            var declaringType = propertyMapping.Property.DeclaringType;
            if (declaringType is null || !declaringType.IsAssignableFrom(entityType))
            {
                throw new ArgumentException(
                    $"Property '{propertyMapping.Property.Name}' does not belong to entity type '{entityType.FullName}'.",
                    nameof(properties));
            }
            if (propertyMapping.Property.GetIndexParameters().Length != 0
                || propertyMapping.Property.GetMethod is null
                || propertyMapping.Property.SetMethod is null
                || propertyMapping.Property.GetMethod.IsStatic
                || propertyMapping.Property.SetMethod.IsStatic)
            {
                throw new ArgumentException(
                    $"Property '{propertyMapping.Property.Name}' must be a readable and writable instance property without index parameters.",
                    nameof(properties));
            }

            AddIdentifier(propertyMapping.Property.Name, propertyMapping);
            AddIdentifier(propertyMapping.ColumnName, propertyMapping);
        }

        Properties = Array.AsReadOnly(propertyArray);
        Keys = Array.AsReadOnly(propertyArray.Where(property => property.IsKey).ToArray());
        InsertProperties = Array.AsReadOnly(propertyArray.Where(property => property.IsInsertable).ToArray());
        GeneratedKeys = Array.AsReadOnly(propertyArray.Where(property => property.IsGeneratedKey).ToArray());
        _explicitInsertProperties = Array.AsReadOnly(InsertProperties.Concat(GeneratedKeys).ToArray());
        _propertiesByIdentifier = identifiers;

        void AddIdentifier(string identifier, PropertyMapping propertyMapping)
        {
            if (identifiers.TryGetValue(identifier, out var existing) && !ReferenceEquals(existing, propertyMapping))
            {
                throw new ArgumentException(
                    $"Identifier '{identifier}' is ambiguous between properties '{existing.Property.Name}' " +
                    $"and '{propertyMapping.Property.Name}' on entity type '{entityType.FullName}'.",
                    nameof(properties));
            }

            identifiers[identifier] = propertyMapping;
        }
    }

    /// <summary>
    /// Gets the mapped CLR entity type.
    /// </summary>
    public Type EntityType { get; }

    /// <summary>
    /// Gets the unquoted database table name.
    /// </summary>
    public string TableName { get; }

    /// <summary>
    /// Gets the optional unquoted database schema name.
    /// </summary>
    public string? Schema { get; }

    /// <summary>
    /// Gets all persistent properties.
    /// </summary>
    public IReadOnlyList<PropertyMapping> Properties { get; }

    /// <summary>
    /// Gets the properties that form the entity key.
    /// </summary>
    public IReadOnlyList<PropertyMapping> Keys { get; }

    /// <summary>
    /// Gets the properties included in ordinary insert operations.
    /// </summary>
    public IReadOnlyList<PropertyMapping> InsertProperties { get; }

    /// <summary>
    /// Gets database-generated key properties.
    /// </summary>
    public IReadOnlyList<PropertyMapping> GeneratedKeys { get; }

    /// <summary>
    /// Gets the properties to include in an insert operation.
    /// </summary>
    /// <param name="includeGeneratedKeys">Whether database-generated keys should be inserted explicitly.</param>
    /// <returns>The ordered insert property mappings.</returns>
    public IReadOnlyList<PropertyMapping> GetInsertProperties(bool includeGeneratedKeys)
    {
        return includeGeneratedKeys ? _explicitInsertProperties : InsertProperties;
    }

    /// <summary>
    /// Resolves a mapping by CLR property name or database column name using case-insensitive comparison.
    /// </summary>
    /// <param name="identifier">The CLR property name or database column name.</param>
    /// <returns>The matching property mapping, or <see langword="null"/> when none exists.</returns>
    public PropertyMapping? FindProperty(string identifier)
    {
        if (identifier is null)
            throw new ArgumentNullException(nameof(identifier));

        return _propertiesByIdentifier.TryGetValue(identifier, out var property) ? property : null;
    }
}
