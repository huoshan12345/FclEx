namespace FclEx.Http;

public static class HttpStatusCodeExtensions
{
    public static bool IsInfo(this HttpStatusCode code) => (int)code >= 100 && (int)code <= 199;
    public static bool IsSuccess(this HttpStatusCode code) => (int)code >= 200 && (int)code <= 299;
    public static bool IsRedirection(this HttpStatusCode code) => (int)code >= 300 && (int)code <= 399;
    public static bool IsClientError(this HttpStatusCode code) => (int)code >= 400 && (int)code <= 499;
    public static bool IsServerError(this HttpStatusCode code) => (int)code >= 500 && (int)code <= 599;

    public static HttpStatusCodeType GetCodeType(this HttpStatusCode code)
    {
        var digit = ((int)code) / 100;
        if (digit >= 0 && digit <= 5) return (HttpStatusCodeType)digit;
        else return HttpStatusCodeType.Unknown;
    }
}