namespace FclEx.Xunit;

public static partial class Extensions
{
    public static ILoggerFactory AddXunitTest(this ILoggerFactory factory, ITestOutputHelper output, bool checkDisposed)
    {
        factory.AddProvider(new TestLoggerProvider(output, checkDisposed));
        return factory;
    }

    public static IServiceCollection AddXunitTest(this IServiceCollection services, ITestOutputHelper output, bool checkDisposed)
    {
        services.AddSingleton<ILoggerProvider>(new TestLoggerProvider(output, checkDisposed));
        return services;
    }

    public static ILoggingBuilder AddXunitTest(this ILoggingBuilder builder, ITestOutputHelper output, bool checkDisposed)
    {
        builder.Services.AddXunitTest(output, checkDisposed);
        return builder;
    }

    public static TheoryData<T> ToTheoryData<T>(this IEnumerable<T> enumerable)
    {
        return new TheoryData<T>(enumerable);
    }
}