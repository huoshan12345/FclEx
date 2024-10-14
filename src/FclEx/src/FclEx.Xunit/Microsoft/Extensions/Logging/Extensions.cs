namespace Microsoft.Extensions.Logging;

public static class Extensions
{
    public static ILoggerFactory AddXunitTest(this ILoggerFactory factory, ITestOutputHelper output, bool checkDisposed)
    {
        factory.AddProvider(new TestLoggerProvider(output, checkDisposed));
        return factory;
    }

    public static IServiceCollection AddXunitTest(this IServiceCollection services, ITestOutputHelper output, bool checkDisposed)
    {
        services.Replace<ILoggerProvider, TestLoggerProvider>(new TestLoggerProvider(output, checkDisposed));
        return services;
    }

    public static ILoggingBuilder AddXunitTest(this ILoggingBuilder builder, ITestOutputHelper output, bool checkDisposed)
    {
        builder.Services.AddXunitTest(output, checkDisposed);
        return builder;
    }
}