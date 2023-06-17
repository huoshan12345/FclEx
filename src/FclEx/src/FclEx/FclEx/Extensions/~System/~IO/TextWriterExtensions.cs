using System.IO;

namespace FclEx.Extensions;

public static class TextWriterExtensions
{
    public static IDisposable SetConsole(this TextWriter tw)
    {
        var output = Console.Out;
        Console.SetOut(tw);
        return Disposable.Create(() => Console.SetOut(output));
    }
}