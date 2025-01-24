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
        if (output is not TestOutputHelper helper)
            throw new NotSupportedException(nameof(output).GetType().FullName);

        _method.Invoke(helper, [message ?? ""]);
    }

    public static void WriteLine(this ITestOutputHelper output)
    {
        output.WriteLine("");
    }

    public static void Write<T>(this ITestOutputHelper output, T? value)
    {
        output.Write(value?.ToString());
    }

    public static void WriteLine<T>(this ITestOutputHelper output, T? value)
    {
        output.WriteLine(value?.ToString() ?? "");
    }
}