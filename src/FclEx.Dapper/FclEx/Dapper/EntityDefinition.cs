namespace FclEx.Dapper;

public class EntityDefinition<T>
{
    public static EntityDefinition Definition { get; } = DapperHelper.GetEntityDefinition(typeof(T));
}

public class EntityDefinition
{
    public EntityDefinition(Type entityType)
    {
        EntityType = entityType;
    }

    public string? Alias { get; internal init; }
    public string TableName => Alias ?? EntityType.Name;
    public Type EntityType { get; }
    public IReadOnlyList<FieldDefinition> Fields { get; internal set; } = [];
    public IReadOnlyList<FieldDefinition> InsertFields { get; internal set; } = [];
    public IReadOnlyList<FieldDefinition> Keys { get; internal set; } = [];
    public IReadOnlyList<FieldDefinition> AutoKeys { get; internal set; } = [];

    public static EntityDefinition GetDefinition(Type type)
    {
        var tableAttr = type.GetCustomAttribute<TableAttribute>(false);
        var def = new EntityDefinition(type)
        {
            Alias = tableAttr?.Name,
        };

        var q = from property in type.GetProperties()
                let columnAttr = property.GetCustomAttribute<ColumnAttribute>(false)
                let keyAttr = property.GetCustomAttribute<KeyAttribute>(false)
                let getAttr = property.GetCustomAttribute<DatabaseGeneratedAttribute>(false)
                select new FieldDefinition(property)
                {
                    Alias = columnAttr?.Name,
                    IsKey = keyAttr != null,
                    IsGenerated = getAttr?.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity,
                    DbType = columnAttr?.TypeName,
                };

        var fields = q.ToList();
        def.Fields = fields;
        def.InsertFields = fields.Where(m => m.IsGenerated == false).ToArray();
        def.AutoKeys = fields.Where(m => m.IsAutoKey).ToArray();
        def.Keys = fields.Where(m => m.IsKey).ToArray();

        return def;
    }
}

public static class EntityDefinitionExtensions
{
    public static IEnumerable<FieldDefinition> InsertFields(this EntityDefinition def, bool includeAutoKey)
    {
        return includeAutoKey ? def.InsertFields.Concat(def.AutoKeys) : def.InsertFields;
    }

    public static bool HasAutoKey(this EntityDefinition def)
    {
        return def.AutoKeys.Count > 0;
    }
}