namespace FclEx.Xunit;

// ReSharper disable once PartialTypeWithSinglePart
public static partial class Extensions
{
    public static ILoggerFactory AddXunit(this ILoggerFactory factory, Func<ITestOutputHelper> outputResolver, bool consoleFallback = true)
    {
        factory.AddProvider(new XunitLoggerProvider(outputResolver, consoleFallback));
        return factory;
    }

    public static ILoggerFactory AddXunit(this ILoggerFactory factory, ITestOutputHelper output, bool consoleFallback = true)
    {
        return factory.AddXunit(() => output, consoleFallback);
    }

    public static ILoggingBuilder AddXunit(this ILoggingBuilder builder, Func<ITestOutputHelper> outputResolver, bool consoleFallback = true)
    {
        builder.Services.AddSingleton<ILoggerProvider>(new XunitLoggerProvider(outputResolver, consoleFallback));
        return builder;
    }

    public static ILoggingBuilder AddXunit(this ILoggingBuilder builder, ITestOutputHelper output, bool consoleFallback = true)
    {
        return builder.AddXunit(() => output, consoleFallback);
    }

    private static ITestOutputHelper? GetOutput()
    {
        return TestContext.Current.TestOutputHelper;
    }

    public static ILoggerFactory AddXunit(this ILoggerFactory factory, bool consoleFallback = true)
    {
        factory.AddProvider(new XunitLoggerProvider(GetOutput, consoleFallback));
        return factory;
    }

    public static ILoggingBuilder AddXunit(this ILoggingBuilder builder, bool consoleFallback = true)
    {
        builder.Services.AddSingleton<ILoggerProvider>(new XunitLoggerProvider(GetOutput, consoleFallback));
        return builder;
    }
}