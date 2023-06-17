using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Xunit.Abstractions;

public class TestOutputWriter : TextWriter
{
    public TestOutputWriter(ITestOutputHelper output)
    {
        Output = output;
    }

    public ITestOutputHelper Output { get; }

    public override Encoding Encoding { get; } = Encoding.UTF8;

    public override void Write(char value) => WriteLine(value);
    public override void Write(string? value) => WriteLine(value);
    public override void WriteLine() => WriteLine("");
    public override void WriteLine(string? value) => Output.WriteLine(value ?? "");

    public override Task WriteAsync(char value)
    {
        Write(value);
        return Task.CompletedTask;
    }

    public override Task WriteAsync(string? value)
    {
        Write(value);
        return Task.CompletedTask;
    }

    public override Task WriteLineAsync(string? value)
    {
        WriteLine(value);
        return Task.CompletedTask;
    }
}

public static class TestOutputHelperExtensions
{
    public static IDisposable SetConsole(this ITestOutputHelper output)
    {
        return new TestOutputWriter(output).SetConsole();
    }
}