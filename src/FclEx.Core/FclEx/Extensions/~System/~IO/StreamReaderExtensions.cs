namespace FclEx.Extensions;

public static class StreamReaderExtensions
{
#if !NET7_0_OR_GREATER
    private static readonly MethodInfo? _methodReadToEndAsync
        = typeof(StreamWriter).GetMethod(nameof(StreamReader.ReadToEndAsync), 0, [typeof(CancellationToken)]);

    /// <summary>
    /// Provides a cross-platform extension method for <see cref="StreamReader.ReadToEndAsync()"/> with <see cref="CancellationToken"/>. <br/>
    /// On .NET Standard 2.0, this overload is not defined, so reflection is used to call it if available. <br/>
    /// Falls back to <see cref="StreamReader.ReadToEndAsync()"/> when the cancellation-aware overload cannot be resolved.
    /// </summary>
    /// <param name="reader">The <see cref="StreamReader"/> to read from.</param>
    /// <param name="cancellationToken">A cancellation token to observe during the read operation.</param>
    /// <returns>A task representing the asynchronous read operation, returning the remaining text from the reader.</returns>
    public static Task<string> ReadToEndAsync(this StreamReader reader, CancellationToken cancellationToken)
    {
        if (_methodReadToEndAsync is { } method)
        {
            return method.Invoke<Task<string>>(reader, [cancellationToken])!;
        }
        else
        {
            return reader.ReadToEndAsync().WaitAsync(cancellationToken);
        }
    }

    private static readonly MethodInfo? _methodReadLineAsync
        = typeof(StreamWriter).GetMethod(nameof(StreamReader.ReadLineAsync), 0, [typeof(CancellationToken)]);

    /// <summary>
    /// Provides a cross-platform extension method for <see cref="StreamReader.ReadToEndAsync()"/> with <see cref="CancellationToken"/>. <br/>
    /// On .NET Standard 2.0, this overload is not defined, so reflection is used to call it if available. <br/>
    /// Falls back to <see cref="StreamReader.ReadLineAsync()"/> when the cancellation-aware overload cannot be resolved.
    /// </summary>
    /// <param name="reader">The <see cref="StreamReader"/> to read from.</param>
    /// <param name="cancellationToken">A cancellation token to observe during the read operation.</param>
    /// <returns>
    /// A value task representing the asynchronous read operation, returning the next line of characters from the input stream
    /// or <see langword="null"/> if the end of the input stream is reached.
    /// </returns>
    public static ValueTask<string?> ReadLineAsync(this StreamReader reader, CancellationToken cancellationToken)
    {
        if (_methodReadLineAsync is { } method)
        {
            return method.Invoke<ValueTask<string?>>(reader, [cancellationToken])!;
        }
        else
        {
            return reader.ReadLineAsync().WaitAsync(cancellationToken).ToValueTask();
        }
    }
#endif
}
