using Microsoft.AspNetCore.Builder;

namespace FclEx.AspNetCore;

public static class ApplicationBuilderExtensions
{
    private const string Template = $$"""Request {{{nameof(HttpRequest.Protocol)}}} {{{nameof(HttpRequest.Method)}}} {{{nameof(HttpRequest.Path)}}} - {{{nameof(HttpResponse.StatusCode)}}} finished in {Time:f3}""";
    private const string SecondsTemplate = Template + " seconds.";
    private const string MillisecondsTemplate = Template + " ms.";

    public static IApplicationBuilder UseHttpRequestLogging(this IApplicationBuilder app, bool withJwtInfo)
    {
        return app.Use(async (context, next) =>
        {
            var start = DateTime.UtcNow;

            var logger = context.RequestServices.CreateLogger(typeof(ApplicationBuilderExtensions));
            var request = context.Request;

            using var logs = new LoggerProperties(logger)
                .Push(LogPropertyNames.StartTime, start)
                .Push(nameof(HttpContext.TraceIdentifier), context.TraceIdentifier)
                .Push(request);

            if (withJwtInfo)
            {
                var tokenInfo = request.GetJwtInfo();
                logs.Push(nameof(JwtInfo), tokenInfo, true);
            }

            await next().NoCapture();

            var end = DateTime.UtcNow;
            var duration = end - start;
            var status = context.Response.StatusCode;

            // We have to use another LoggerProperties here because the last one is before an async operation.
            using var x = new LoggerProperties(logger)
                .Push(nameof(HttpResponse.StatusCode), status)
                .Push(LogPropertyNames.DurationMilliseconds, duration.TotalMilliseconds)
                .Push(LogPropertyNames.EndTime, end);

            var (template, time) = duration.TotalSeconds > 1
                ? (SecondsTemplate, duration.TotalSeconds)
                : (MillisecondsTemplate, duration.TotalMilliseconds);

#pragma warning disable CA2254
            logger.LogInformation(template, request.Protocol, request.Method, request.Path, status, time);
#pragma warning restore CA2254
        });
    }

    public static IApplicationBuilder EnableBuffering(this IApplicationBuilder app, Func<HttpContext, bool> predicate)
    {
        return app.UseWhen(predicate, m => m.UseMiddleware<EnableBufferingMiddleware>());
    }
}