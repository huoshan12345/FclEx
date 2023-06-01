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
}