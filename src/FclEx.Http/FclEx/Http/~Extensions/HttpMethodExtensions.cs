namespace FclEx.Http;

/// <summary>
/// Extensions for comparing HTTP methods.
/// </summary>
public static class HttpMethodExtensions
{
    /// <summary>
    /// Returns whether the method name equals GET, ignoring case.
    /// </summary>
    public static bool IsGet(this HttpMethod method)
    {
        return string.Equals(method.Method, HttpMethod.Get.Method, StringComparison.OrdinalIgnoreCase);
    }
}
