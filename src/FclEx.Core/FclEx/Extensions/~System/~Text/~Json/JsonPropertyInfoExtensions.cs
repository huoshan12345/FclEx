namespace FclEx.Extensions;

public static class JsonPropertyInfoExtensions
{
    public static bool IsDefined<T>(this JsonPropertyInfo propertyInfo, bool inherit) where T : Attribute
    {
        return propertyInfo.AttributeProvider?.IsDefined<T>(inherit) == true;
    }

    public static bool TryGetAttribute<T>(this JsonPropertyInfo propertyInfo, bool inherit, [NotNullWhen(true)] out T? attribute) where T : Attribute
    {
        attribute = null;
        return propertyInfo.AttributeProvider?.TryGetAttribute(inherit, out attribute) == true;
    }
}
