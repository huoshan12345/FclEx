namespace FclEx.Extensions;

public static class CustomAttributeProviderExtensions
{
    public static bool TryGetAttribute<T>(this ICustomAttributeProvider provider, bool inherit, [NotNullWhen(true)] out T? attribute) where T : Attribute
    {
        attribute = (T?)provider.GetCustomAttributes(typeof(T), inherit).FirstOrDefault();
        return attribute != null;
    }

    public static T[] GetCustomAttributes<T>(this ICustomAttributeProvider provider, bool inherit) where T : Attribute
    {
        return (T[])provider.GetCustomAttributes(typeof(T), inherit);
    }

    public static bool IsDefined<T>(this ICustomAttributeProvider provider, bool inherit) where T : Attribute
    {
        return provider.IsDefined(typeof(T), inherit);
    }

    public static void Write(this Utf8JsonWriter writer, object? value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}