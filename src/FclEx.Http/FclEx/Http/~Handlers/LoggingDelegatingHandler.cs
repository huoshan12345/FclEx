namespace FclEx.Http;

public class LoggingDelegatingHandler : DelegatingHandler
{
    private readonly ILogger<LoggingDelegatingHandler> _logger;
    private readonly LogLevel _successLevel;
    private readonly LogLevel _failureLevel;

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