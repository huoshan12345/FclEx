namespace FclEx.Dapper;

/// <summary>
/// Describes how one CLR property maps to a database column.
/// </summary>
public sealed class PropertyMapping
{
    /// <summary>
    /// Creates a property mapping.
    /// </summary>
    /// <param name="property">The mapped CLR property.</param>
    /// <param name="columnName">The database column name. The CLR property name is used when omitted.</param>
    /// <param name="isKey">Whether the property is part of the entity key.</param>
    /// <param name="valueGeneration">When the database generates the property value.</param>
    /// <param name="storeTypeName">
    /// An optional provider-specific database type name, such as <c>jsonb</c> or <c>nvarchar(max)</c>.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="property"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="columnName"/> is empty or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="valueGeneration"/> is invalid.</exception>
    public PropertyMapping(
        PropertyInfo property,
        string? columnName = null,
        bool isKey = false,
        DatabaseValueGeneration valueGeneration = DatabaseValueGeneration.None,
        string? storeTypeName = null)
    {
        Property = property ?? throw new ArgumentNullException(nameof(property));
        ColumnName = columnName ?? property.Name;
        if (string.IsNullOrWhiteSpace(ColumnName))
            throw new ArgumentException("A column name cannot be empty or whitespace.", nameof(columnName));
        if (valueGeneration is not DatabaseValueGeneration.None
            and not DatabaseValueGeneration.OnInsert
            and not DatabaseValueGeneration.OnInsertOrUpdate)
        {
            throw new ArgumentOutOfRangeException(nameof(valueGeneration), valueGeneration, null);
        }

        IsKey = isKey;
        ValueGeneration = valueGeneration;
        StoreTypeName = storeTypeName;
    }

    /// <summary>
    /// Gets the mapped CLR property.
    /// </summary>
    public PropertyInfo Property { get; }

    /// <summary>
    /// Gets the database column name.
    /// </summary>
    public string ColumnName { get; }

    /// <summary>
    /// Gets whether the property is part of the entity key.
    /// </summary>
    public bool IsKey { get; }

    /// <summary>
    /// Gets when the database generates the property value.
    /// </summary>
    public DatabaseValueGeneration ValueGeneration { get; }

    /// <summary>
    /// Gets an optional provider-specific database type name.
    /// </summary>
    public string? StoreTypeName { get; }

    /// <summary>
    /// Gets whether ordinary insert operations should include this property.
    /// </summary>
    public bool IsInsertable => ValueGeneration == DatabaseValueGeneration.None;

    /// <summary>
    /// Gets whether ordinary update operations should include this property.
    /// </summary>
    public bool IsUpdatable => ValueGeneration == DatabaseValueGeneration.None;

    /// <summary>
    /// Gets whether this is a database-generated key that may be included only during explicit-key insertion.
    /// </summary>
    public bool IsGeneratedKey => IsKey && ValueGeneration == DatabaseValueGeneration.OnInsert;
}
