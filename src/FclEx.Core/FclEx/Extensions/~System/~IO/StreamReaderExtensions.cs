namespace FclEx.Extensions;

public static class StreamReaderExtensions
{
#if NETSTANDARD2_0
    private static readonly MethodInfo? _methodReadToEndAsync = typeof(StreamWriter).GetMethod(nameof(StreamReader.ReadToEndAsync), 0, [typeof(CancellationToken)]);

    public static Task<string> ReadToEndAsync(this StreamReader reader, CancellationToken cancellationToken)
    {
        if (_methodReadToEndAsync is { } method)
        {
            return method.Invoke<Task<string>>(reader, [cancellationToken])!;
        }
        else
        {
            return reader.ReadToEndAsync();
        }
    }

    private static readonly MethodInfo? _methodReadLineAsync = typeof(StreamWriter).GetMethod(nameof(StreamReader.ReadLineAsync), 0, [typeof(CancellationToken)]);

    public static ValueTask<string?> ReadLineAsync(this StreamReader reader, CancellationToken cancellationToken)
    {
        if (_methodReadLineAsync is { } method)
        {
            return method.Invoke<ValueTask<string?>>(reader, [cancellationToken])!;
        }
        else
        {
            return reader.ReadLineAsync().ToValueTask();
        }
    }
#endif
}
