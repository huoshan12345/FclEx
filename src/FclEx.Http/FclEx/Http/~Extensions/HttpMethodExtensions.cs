namespace FclEx.Http;

public static class HttpMethodExtensions
{
    public static bool IsGet(this HttpMethod method)
    {
        return string.Equals(method.Method, HttpMethod.Get.Method, StringComparison.OrdinalIgnoreCase);
    }
}