namespace FclEx.Extensions;

public static class CustomAttributeProviderExtensions
{
    public static bool TryGetAttribute<T>(this ICustomAttributeProvider provider, bool inherit, [NotNullWhen(true)] out T? attribute) where T : Attribute
    {
        attribute = (T?)provider.GetCustomAttributes(typeof(T), inherit).FirstOrDefault();
        return attribute != null;
    }
}