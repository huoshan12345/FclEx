namespace FclEx.Extensions;

public static class FieldInfoExtensions
{
    public static T? GetValue<T>(this FieldInfo field, object? obj)
    {
        return field.GetValue(obj).CastTo<T>();
    }

    public static object GetRequiredValue(this FieldInfo field, object? obj)
    {
        return field.GetValue(obj) ?? throw new InvalidOperationException($"The value of field {field.Name} is null");
    }

    public static T GetRequiredValue<T>(this FieldInfo field, object? obj)
    {
        return field.GetRequiredValue(obj).CastTo<T>();
    }

    private static readonly Regex _backingFieldName = new(@"^<(\w+)>k__BackingField$", RegexOptions.Compiled);
    public static bool TryGetCorrespondingProperty(this FieldInfo field, [NotNullWhen(true)] out PropertyInfo? property)
    {
        property = null;

        var type = field.DeclaringType;
        if (type is null)
            return false;

        return _backingFieldName.TryMatch(field.Name, 1, out var propertyName)
               && type.TryGetProperty(propertyName, out property);
    }
}