namespace FclEx.Extensions;

public static class CustomAttributeProviderExtensions
{
    public static bool TryGetAttribute<T>(this ICustomAttributeProvider provider, bool inherit, [NotNullWhen(true)] out T? attribute) where T : Attribute
    {
        attribute = (T?)provider.GetCustomAttributes(typeof(T), inherit).FirstOrDefault();
        return attribute != null;
    }

#if NETSTANDARD2_0
    // this method's return type in dotnet version is IEnumerable<T> instead of T[], so we can't change it.
    public static IEnumerable<T> GetCustomAttributes<T>(this ICustomAttributeProvider provider, bool inherit) where T : Attribute
    {
        return (T[])provider.GetCustomAttributes(typeof(T), inherit);
    }
#endif

    public static bool IsDefined<T>(this ICustomAttributeProvider provider, bool inherit) where T : Attribute
    {
        return provider.IsDefined(typeof(T), inherit);
    }
}