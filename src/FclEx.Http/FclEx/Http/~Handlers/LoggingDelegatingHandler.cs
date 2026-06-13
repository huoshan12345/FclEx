namespace FclEx.Http;

/// <summary>
/// Logs each outgoing <see cref="HttpClient"/> request after the inner handler completes or throws.
/// </summary>
/// <remarks>
/// The log scope includes the request properties, response status code when one is available, start time, end time,
/// and duration. Successful sends use the configured success level; exceptions are logged with the configured failure level.
/// </remarks>
public class LoggingDelegatingHandler : DelegatingHandler
{
    private readonly ILogger<LoggingDelegatingHandler> _logger;
    private readonly LogLevel _successLevel;
    private readonly LogLevel _failureLevel;

    /// <summary>
    /// Initializes a logging handler.
    /// </summary>
    /// <param name="logger">The logger factory used to create the handler logger.</param>
    /// <param name="successLevel">The level used when the inner handler returns a response.</param>
    /// <param name="failureLevel">The level used when the inner handler throws.</param>
    public LoggingDelegatingHandler(
        ILoggerFactory logger,
        LogLevel successLevel = LogLevel.Information,
        LogLevel failureLevel = LogLevel.Warning)
    {
        _logger = logger.CreateLogger<LoggingDelegatingHandler>();
        _successLevel = successLevel;
        _failureLevel = failureLevel;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var uri = request.RequestUri;
        var start = DateTime.UtcNow;
        var e = default(Exception);
        var statusCode = default(HttpStatusCode?);

        try
        {
            var response = await base.SendAsync(request, cancellationToken);
            statusCode = response.StatusCode;
            return response;
        }
        catch (Exception ex)
        {
            e = ex;
            throw;
        }
        finally
        {
            var end = DateTime.UtcNow;
            var duration = end - start;
            var level = e is null
                ? _successLevel
                : _failureLevel;

            using var x = new LoggerProperties(_logger)
                .Push(nameof(HttpResponseMessage.StatusCode), statusCode)
                .Push(LogPropertyNames.DurationMilliseconds, duration.TotalMilliseconds)
                .Push(LogPropertyNames.RequestEndTime, end)
                .Push(LogPropertyNames.RequestStartTime, start)
                .Push(request);

            _logger.Log(level, e, duration.TotalSeconds > 1
                ? $"Request from HttpClient finished in {duration.TotalSeconds:f3} seconds."
                : $"Request from HttpClient finished in {duration.TotalMilliseconds:f3} ms.");
        }
    }
}
