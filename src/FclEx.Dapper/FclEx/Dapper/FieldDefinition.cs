namespace FclEx.Dapper;

public class FieldDefinition(PropertyInfo propertyInfo)
{
    public string? Alias { get; internal init; }
    public string FieldName => Alias ?? PropertyInfo.Name;
    public PropertyInfo PropertyInfo { get; } = propertyInfo;
    public bool IsKey { get; internal init; }
    public bool IsGenerated { get; internal init; }
    public bool IsAutoKey => IsKey && IsGenerated;
    public string? DbType { get; internal init; }
}