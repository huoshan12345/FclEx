namespace FclEx.Extensions;

public static class PropertyInfoExtensions
{
    public static MethodInfo GetRequiredGetMethod(this PropertyInfo property)
    {
        return property.GetGetMethod(true) ?? throw new MissingMethodException($"No getter in propery {property.Name}");
    }

    public static MethodInfo GetRequiredSetMethod(this PropertyInfo property)
    {
        return property.GetSetMethod(true) ?? throw new MissingMethodException($"No setter in propery {property.Name}");
    }

    public static T? GetValue<T>(this PropertyInfo property, object? obj)
    {
        return property.GetValue(obj).CastTo<T>();
    }

    public static object GetRequiredValue(this PropertyInfo property, object? obj)
    {
        return property.GetValue(obj) ?? throw new InvalidOperationException($"The value of property {property.Name} is null");
    }

    public static T GetRequiredValue<T>(this PropertyInfo property, object? obj)
    {
        return property.GetRequiredValue(obj).CastTo<T>();
    }
}