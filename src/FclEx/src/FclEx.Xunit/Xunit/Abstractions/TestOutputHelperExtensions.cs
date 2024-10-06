namespace Xunit.Abstractions;

public static class TestOutputHelperExtensions
{
    public static IDisposable SetConsole(this ITestOutputHelper output)
    {
        return new TestOutputWriter(output).SetConsole();
    }

    private static readonly MethodInfo _method = typeof(TestOutputHelper).GetRequiredMethod("QueueTestOutput");

    public static void Write(this ITestOutputHelper output, string? message)
    {
        ArgumentNullException.ThrowIfNull(output);

        if (output is not TestOutputHelper helper)
            throw new NotSupportedException(nameof(output).GetType().FullName);

        _method.Invoke(helper, [message]);
    }
}