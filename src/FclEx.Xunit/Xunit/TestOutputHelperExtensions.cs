namespace Xunit;

public static class TestOutputHelperExtensions
{
    public static IDisposable SetConsole(this ITestOutputHelper output)
    {
        return new TestOutputWriter(output).SetConsole();
    }

    public static void WriteLine(this ITestOutputHelper output)
    {
        output.WriteLine("");
    }

    public static void Write<T>(this ITestOutputHelper output, T? value)
    {
        if (value?.ToString() is { } str)
        {
            output.Write(str);
        }
    }

    public static void WriteLine<T>(this ITestOutputHelper output, T? value)
    {
        output.WriteLine(value?.ToString() ?? "");
    }
}