namespace FclEx.Dapper;

public class FieldDefinition
{
    public FieldDefinition(PropertyInfo propertyInfo)
    {
        PropertyInfo = propertyInfo;
    }

    public string? Alias { get; internal init; }
    public string FieldName => Alias ?? PropertyInfo.Name;
    public PropertyInfo PropertyInfo { get; }
    public bool IsKey { get; internal init; }
    public bool IsAutoKey { get; internal init; }
    public string? DbType { get; internal init; }
}