namespace FclEx.Extensions;

// ReSharper disable once UnusedMember.Global
public static class StreamWriterExtensions
{
#if NETSTANDARD2_0
    private static readonly MethodInfo? _methodFlushAsync = typeof(StreamWriter).GetMethod(nameof(StreamWriter.FlushAsync), 0, [typeof(CancellationToken)]);

    /// <summary>
    /// Provides a cross-platform extension method for <see cref="StreamWriter.FlushAsync(CancellationToken)"/>. <br/>
    /// On .NET Standard 2.0, this overload does not exist, so reflection is used to call it if available. <br/>
    /// Falls back to <see cref="StreamWriter.FlushAsync()"/> when the cancellation-aware overload cannot be resolved.
    /// </summary>
    /// <param name="writer">The <see cref="StreamWriter"/> to flush.</param>
    /// <param name="cancellationToken">A cancellation token to observe during the flush operation.</param>
    /// <returns>A task that represents the asynchronous flush operation.</returns>
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
