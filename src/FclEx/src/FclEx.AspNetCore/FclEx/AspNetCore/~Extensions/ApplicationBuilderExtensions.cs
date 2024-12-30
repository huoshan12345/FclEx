using FclEx.Logging;
using Microsoft.AspNetCore.Builder;

namespace FclEx.AspNetCore;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseHttpLog(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            var start = DateTime.UtcNow;
            var request = context.Request;
            var logger = context.RequestServices.GetRequiredService<ILoggerFactory>()
                .CreateLogger(typeof(ApplicationBuilderExtensions));

            using var logs = new LoggerProperties(logger)
                .Push(LogPropertyNames.RequestStartTime, start)
                .Push(LogPropertyNames.TraceId, context.TraceIdentifier)
                .Push(request);

            await next().ConfigureAwait(false);

            var end = DateTime.UtcNow;
            var duration = end - start;
            
            using var x = new LoggerProperties(logger)
                .Push(nameof(HttpResponse.StatusCode), context.Response?.StatusCode)
                .Push(LogPropertyNames.DurationMilliseconds, duration.TotalMilliseconds)
                .Push(LogPropertyNames.RequestEndTime, end);

            logger.LogInformation(duration.TotalSeconds > 1
                ? $"Request finished in {duration.TotalSeconds:f3} seconds."
                : $"Request finished in {duration.TotalMilliseconds:f3} ms.");

        });
    }
}