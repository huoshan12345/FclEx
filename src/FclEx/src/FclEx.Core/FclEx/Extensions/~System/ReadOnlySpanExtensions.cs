namespace FclEx.Extensions;

public static class ReadOnlySpanExtensions
{
    public static bool StartsWith<T>(this ReadOnlySpan<T> span, T value) where T : IEquatable<T>
    {
        var valueSpan = Span.Create(ref value);
        return span.StartsWith(valueSpan);
    }

    public static string GetString(this ReadOnlySpan<byte> span, Encoding? encoding = null)
    {
        return (encoding ?? Encoding.UTF8).GetString(span);
    }

#if NET6_0_OR_GREATER
    public static string ToBase64(this ReadOnlySpan<byte> span) => Convert.ToBase64String(span);
#endif

    public static unsafe T ToStructure<T>(this ReadOnlySpan<byte> span) where T : struct
    {
        var size = Marshal.SizeOf<T>();
        Check.NotLessThan(span.Length, size);

        using var disposable = MarshalHelper.AllocHGlobal(size);
        var ptr = disposable.Value;

        var buffer = new Span<byte>(ptr.ToPointer(), size);
        span.CopyTo(buffer);
        var obj = ptr.ToStructure<T>();
        return obj;
    }

    public static unsafe T[] ToStructures<T>(this ReadOnlySpan<byte> span) where T : struct
    {
        var size = Marshal.SizeOf<T>();
        Check.NotLessThan(span.Length, size);

        var count = span.Length / size; // count should be >= 1
        var total = size * count;

        using var disposable = MarshalHelper.AllocHGlobal(total);
        var ptr = disposable.Value;

        var result = new T[count];
        for (var i = 0; i < count; i++)
        {
            var buffer = new Span<byte>(ptr.ToPointer(), size);
            span.Slice(i * size, size).CopyTo(buffer);
            var obj = ptr.ToStructure<T>();
            result[i] = obj;
        }
        return result;
    }
}