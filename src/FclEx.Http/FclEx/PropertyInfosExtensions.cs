namespace FclEx;

public static class PropertyInfosExtensions
{
    private static readonly PropertyInfo _httpResponseResponseString =
        typeof(HttpResponse).GetRequiredProperty(nameof(HttpResponse.ResponseString));
    private static readonly PropertyInfo _httpResponseStatusCode =
        typeof(HttpResponse).GetRequiredProperty(nameof(HttpResponse.StatusCode));

    extension(PropertyInfos)
    {
        public static PropertyInfo HttpResponse_ResponseString => _httpResponseResponseString;
        public static PropertyInfo HttpResponse_StatusCode => _httpResponseStatusCode;
    }
}
