namespace FclEx.Extensions;

public static class HttpRequestExceptionExtensions
{
    extension(HttpRequestException)
    {
        public static HttpRequestException From(string? message, Exception? inner, HttpStatusCode? statusCode)
        {
#if NET5_0_OR_GREATER
            return new HttpRequestException(message, inner, statusCode);
#else
            var ex = new HttpRequestException(message, inner);
            if (statusCode is not null && ex.Data is { IsReadOnly: false } data)
            {
                data["StatusCode"] = statusCode;
            }
            return ex;
#endif
        }

        public static HttpRequestException From(string? message, HttpStatusCode? statusCode)
        {
            return HttpRequestException.From(message, null, statusCode);
        }
    }
}
