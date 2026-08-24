namespace FclEx.Dapper;

/// <summary>
/// Builds entity mappings from the supported DataAnnotations mapping attributes and persistent-property conventions.
/// </summary>
/// <remarks>
/// The source honors <see cref="TableAttribute"/>, <see cref="ColumnAttribute"/>, <see cref="KeyAttribute"/>,
/// <see cref="NotMappedAttribute"/>, and every <see cref="DatabaseGeneratedOption"/> value. By convention,
/// unannotated properties must be public instance scalar properties with a getter and setter. A non-scalar property
/// is included only when it explicitly declares a mapping attribute.
/// </remarks>
public sealed class DataAnnotationsEntityMappingSource : IEntityMappingSource
{
    private readonly ConditionalWeakTable<Type, EntityMapping> _mappings = new();

    /// <summary>
    /// Gets the shared default DataAnnotations mapping source.
    /// </summary>
    public static DataAnnotationsEntityMappingSource Instance { get; } = new();

    /// <inheritdoc />
    public EntityMapping GetMapping(Type entityType)
    {
        if (entityType is null)
            throw new ArgumentNullException(nameof(entityType));

        return _mappings.GetValue(entityType, CreateMapping);
    }

    private static EntityMapping CreateMapping(Type entityType)
    {
        var tableAttribute = entityType.GetCustomAttribute<TableAttribute>(false);
        var properties = entityType
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(IsPersistentProperty)
            .Select(CreatePropertyMapping)
            .ToArray();

        return new EntityMapping(
            entityType,
            tableAttribute?.Name ?? entityType.Name,
            properties,
            tableAttribute?.Schema);
    }

    private static bool IsPersistentProperty(PropertyInfo property)
    {
        if (property.GetCustomAttribute<NotMappedAttribute>(false) is not null
            || property.GetIndexParameters().Length != 0
            || property.GetMethod is null
            || property.SetMethod is null
            || property.GetMethod.IsStatic
            || property.SetMethod.IsStatic)
        {
            return false;
        }

        var hasExplicitMapping = property.GetCustomAttribute<ColumnAttribute>(false) is not null
                                 || property.GetCustomAttribute<KeyAttribute>(false) is not null
                                 || property.GetCustomAttribute<DatabaseGeneratedAttribute>(false) is not null;
        return hasExplicitMapping || IsScalarType(property.PropertyType);
    }

    private static bool IsScalarType(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return type.IsValueType
               || type.IsEnum
               || type == typeof(string)
               || type == typeof(byte[])
               || type == typeof(char[]);
    }

    private static PropertyMapping CreatePropertyMapping(PropertyInfo property)
    {
        var columnAttribute = property.GetCustomAttribute<ColumnAttribute>(false);
        var generatedAttribute = property.GetCustomAttribute<DatabaseGeneratedAttribute>(false);
        var valueGeneration = generatedAttribute?.DatabaseGeneratedOption switch
        {
            DatabaseGeneratedOption.Identity => DatabaseValueGeneration.OnInsert,
            DatabaseGeneratedOption.Computed => DatabaseValueGeneration.OnInsertOrUpdate,
            _ => DatabaseValueGeneration.None,
        };

        return new PropertyMapping(
            property,
            columnAttribute?.Name,
            property.GetCustomAttribute<KeyAttribute>(false) is not null,
            valueGeneration,
            columnAttribute?.TypeName);
    }
}
