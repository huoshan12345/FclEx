namespace FclEx.Xunit;

// ReSharper disable once PartialTypeWithSinglePart
/// <summary>Provides logging integration with xUnit v3 test output.</summary>
public static partial class Extensions
{
    /// <summary>Adds a provider that resolves the target xUnit output helper for each message.</summary>
    /// <param name="factory">The factory to configure.</param>
    /// <param name="outputResolver">Resolves the output helper when a message is written.</param>
    /// <param name="consoleFallback">Whether to write to the console when test output is unavailable.</param>
    /// <returns>The same factory.</returns>
    /// <remarks>Each call adds a provider. Factory filters apply before messages reach it.</remarks>
    public static ILoggerFactory AddXunit(this ILoggerFactory factory, Func<ITestOutputHelper> outputResolver, bool consoleFallback = true)
    {
        factory.AddProvider(new XunitLoggerProvider(outputResolver, consoleFallback));
        return factory;
    }

    /// <summary>Adds a provider that writes to a fixed xUnit output helper.</summary>
    /// <param name="factory">The factory to configure.</param>
    /// <param name="output">The output helper associated with the target test.</param>
    /// <param name="consoleFallback">Whether to write to the console when test output is unavailable.</param>
    /// <returns>The same factory.</returns>
    /// <remarks>Each call adds a provider.</remarks>
    public static ILoggerFactory AddXunit(this ILoggerFactory factory, ITestOutputHelper output, bool consoleFallback = true)
    {
        return factory.AddXunit(() => output, consoleFallback);
    }

    /// <summary>Registers a provider that resolves the target xUnit output helper for each message.</summary>
    /// <param name="builder">The logging builder to configure.</param>
    /// <param name="outputResolver">Resolves the output helper when a message is written.</param>
    /// <param name="consoleFallback">Whether to write to the console when test output is unavailable.</param>
    /// <returns>The same builder.</returns>
    /// <remarks>Each call adds a provider, so repeated registrations can duplicate output.</remarks>
    public static ILoggingBuilder AddXunit(this ILoggingBuilder builder, Func<ITestOutputHelper> outputResolver, bool consoleFallback = true)
    {
        builder.Services.AddSingleton<ILoggerProvider>(new XunitLoggerProvider(outputResolver, consoleFallback));
        return builder;
    }

    /// <summary>Registers a provider that writes to a fixed xUnit output helper.</summary>
    /// <param name="builder">The logging builder to configure.</param>
    /// <param name="output">The output helper associated with the target test.</param>
    /// <param name="consoleFallback">Whether to write to the console when test output is unavailable.</param>
    /// <returns>The same builder.</returns>
    /// <remarks>Each call adds a provider.</remarks>
    public static ILoggingBuilder AddXunit(this ILoggingBuilder builder, ITestOutputHelper output, bool consoleFallback = true)
    {
        return builder.AddXunit(() => output, consoleFallback);
    }

    private static ITestOutputHelper? GetOutput()
    {
        return TestContext.Current.TestOutputHelper;
    }

    /// <summary>Adds a provider that resolves the current test's output helper for each message.</summary>
    /// <param name="factory">The factory to configure.</param>
    /// <param name="consoleFallback">Whether to write to the console when the current test has no available output helper.</param>
    /// <returns>The same factory.</returns>
    /// <remarks>Uses <see cref="TestContext.Current"/> at logging time. Each call adds a provider.</remarks>
    public static ILoggerFactory AddXunit(this ILoggerFactory factory, bool consoleFallback = true)
    {
        factory.AddProvider(new XunitLoggerProvider(GetOutput, consoleFallback));
        return factory;
    }

    /// <summary>Registers a provider that resolves the current test's output helper for each message.</summary>
    /// <param name="builder">The logging builder to configure.</param>
    /// <param name="consoleFallback">Whether to write to the console when the current test has no available output helper.</param>
    /// <returns>The same builder.</returns>
    /// <remarks>
    /// Uses <see cref="TestContext.Current"/> at logging time. Each call adds a provider,
    /// and messages must pass the factory's filters before reaching test output.
    /// </remarks>
    public static ILoggingBuilder AddXunit(this ILoggingBuilder builder, bool consoleFallback = true)
    {
        builder.Services.AddSingleton<ILoggerProvider>(new XunitLoggerProvider(GetOutput, consoleFallback));
        return builder;
    }
}
