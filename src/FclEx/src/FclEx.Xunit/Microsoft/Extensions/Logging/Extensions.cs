using FclEx;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Logging;

public static class Extensions
{
    public static ILoggerFactory AddXunitTest(this ILoggerFactory factory, ITestOutputHelper output, bool needToCheckDisposed)
    {
        factory.AddProvider(new TestLoggerProvider(output, needToCheckDisposed));
        return factory;
    }

    public static IServiceCollection AddXunitTest(this IServiceCollection services, ITestOutputHelper output, bool needToCheckDisposed)
    {
        services.Replace<ILoggerProvider, TestLoggerProvider>(new TestLoggerProvider(output, needToCheckDisposed));
        return services;
    }

    public static ILoggingBuilder AddXunitTest(this ILoggingBuilder builder, ITestOutputHelper output, bool needToCheckDisposed)
    {
        builder.Services.AddXunitTest(output, needToCheckDisposed);
        return builder;
    }
}