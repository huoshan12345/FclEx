namespace FclEx.Extensions;

public static class StreamWriterExtensions
{
#if NETSTANDARD2_0
    private static readonly MethodInfo? _methodFlushAsync = typeof(StreamWriter).GetMethod(nameof(StreamWriter.FlushAsync), 0, [typeof(CancellationToken)]);

    public static Task FlushAsync(this StreamWriter writer, CancellationToken cancellationToken)
    {
        if (_methodFlushAsync is { } method)
        {
            return method.Invoke<Task>(writer, [cancellationToken])!;
        }
        else
        {
            return writer.FlushAsync();
        }
    }
#endif
}
