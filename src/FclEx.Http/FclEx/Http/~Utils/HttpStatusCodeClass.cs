namespace FclEx.Http;

/// <summary>
/// Standard HTTP status-code classes derived from the first digit of a numeric status code.
/// </summary>
public enum HttpStatusCodeClass
{
    /// <summary>The status code is outside the standard 1xx through 5xx ranges.</summary>
    Unknown = 0,
    /// <summary>1xx informational response.</summary>
    Informational = 1,
    /// <summary>2xx successful response.</summary>
    Successful = 2,
    /// <summary>3xx redirection response.</summary>
    Redirection = 3,
    /// <summary>4xx client error response.</summary>
    ClientError = 4,
    /// <summary>5xx server error response.</summary>
    ServerError = 5,
}
