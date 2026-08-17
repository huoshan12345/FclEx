namespace FclEx.Extensions;

/// <summary>
/// Provides extension methods for working with byte arrays, <see cref="Span{T}"/>, 
/// and <see cref="ReadOnlySpan{T}"/> to enhance functionality and ease of use.
/// </summary>
public static partial class BytesExtensions
{
    [MethodImpl(AggressiveInlining)]
    public static MemoryStream ToStream(this byte[] bytes) => new(bytes);

    [MethodImpl(AggressiveInlining)]
    public static string GetString(this byte[] bytes, Encoding? encoding = null)
        => bytes.AsReadOnlySpan().GetString(encoding);

    [MethodImpl(AggressiveInlining)]
    public static string GetString(this ArraySegment<byte> bytes, Encoding? encoding = null)
        => bytes.AsReadOnlySpan().GetString(encoding);

    [MethodImpl(AggressiveInlining)]
    public static string ToBase64(this byte[] bytes) => Convert.ToBase64String(bytes);

    public static string ToHex(this byte[] bytes, bool upperCase = false)
    {
        using var builder = new ValueStringBuilder();
        var format = upperCase ? "X2" : "x2";
        foreach (var @byte in bytes)
        {
            builder.Append(@byte.ToString(format));
        }
        return builder.ToString();
    }

    /// <summary>
    /// Uses the interop marshaler to read a structure from <paramref name="bytes"/> at <paramref name="offset"/>, then
    /// advances the offset by its unmanaged size.
    /// </summary>
    /// <remarks>
    /// The structure must use sequential or explicit layout. Managed array fields are supported only when represented
    /// inline with <see cref="UnmanagedType.ByValArray"/>; strings are supported only with
    /// <see cref="UnmanagedType.ByValTStr"/>. Pointer-based managed fields are rejected so input bytes are never
    /// dereferenced as external addresses. No byte-order conversion is performed.
    /// </remarks>
    public static T MarshalReadAs<T>(this byte[] bytes, ref int offset)
    {
        Check.NotNull(bytes);
        Check.NotLessThan(offset, 0);
        typeof(T).EnsureMarshalable();

        var length = Marshal.SizeOf<T>();
        Check.NotLessThan(bytes.Length, checked(length + offset));

        var result = Marshal.ReadAs<T>(bytes.AsSpan(offset, length));
        offset += length;
        return result;
    }

    [MethodImpl(AggressiveInlining)]
    public static T MarshalReadAs<T>(this byte[] bytes)
    {
        var i = 0;
        return bytes.MarshalReadAs<T>(ref i);
    }

    /// <summary>
    /// Uses the interop marshaler to read <paramref name="count"/> consecutive structures and advances
    /// <paramref name="offset"/> past their unmanaged representations.
    /// </summary>
    public static T[] MarshalReadAsArray<T>(this byte[] bytes, int count, ref int offset)
    {
        Check.NotNull(bytes);
        Check.NotLessThan(offset, 0);
        Check.NotLessThan(count, 0);
        typeof(T).EnsureMarshalable();

        var length = Marshal.SizeOf<T>();
        var totalLength = checked(length * count);
        Check.NotLessThan(bytes.Length, checked(totalLength + offset));

        var result = Marshal.ReadAsArray<T>(bytes.AsSpan(offset, totalLength), count);
        offset += totalLength;
        return result;
    }

    [MethodImpl(AggressiveInlining)]
    public static T[] MarshalReadAsArray<T>(this byte[] bytes)
    {
        Check.NotNull(bytes);
        typeof(T).EnsureMarshalable();

        var length = Marshal.SizeOf<T>();
        if (bytes.Length % length != 0)
            throw new ArgumentException("The byte array length must be an exact multiple of the structure size.", nameof(bytes));

        var i = 0;
        return bytes.MarshalReadAsArray<T>(bytes.Length / length, ref i);
    }

    [MethodImpl(AggressiveInlining)]
    public static void WriteTo(this byte[] bytes, Stream stream) => stream.Write(bytes, 0, bytes.Length);

    [MethodImpl(AggressiveInlining)]
    public static Task WriteToAsync(this byte[] bytes, Stream stream) => stream.WriteAsync(bytes, 0, bytes.Length);

    [MethodImpl(AggressiveInlining)]
    public static int IndexOf(this byte[] bytes, byte[] subBytes)
    {
        return bytes.AsReadOnlySpan().IndexOf(subBytes);
    }

    public static int IndexOf(this ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> subBytes)
    {
        if (subBytes.Length == 0)
            return -1;

        if (subBytes.Length > bytes.Length)
            return -1;

        // KMP Algorithm
        var i = 0;
        var j = 0;
        var next = GetNextArray(subBytes);
        while (i < bytes.Length && j < subBytes.Length)
        {
            if (j == -1 || bytes[i] == subBytes[j])
            {
                i++;
                j++;
            }
            else
            {
                j = next[j];

            }
        }
        return j == subBytes.Length ? i - j : -1;

        static int[] GetNextArray(ReadOnlySpan<byte> subBytes)
        {
            var next = new int[subBytes.Length];
            next[0] = -1;
            var j = 0;
            var k = -1;

            while (j < subBytes.Length - 1)
            {
                if (k == -1 || subBytes[j] == subBytes[k])
                {
                    if (subBytes[++j] == subBytes[++k])
                        next[j] = next[k];
                    else
                        next[j] = k;
                }
                else
                    k = next[k];
            }
            return next;
        }
    }

    [MethodImpl(AggressiveInlining)]
    public static int ComputeHashCode(this byte[] bytes)
    {
        return bytes.AsReadOnlySpan().ComputeHashCode();
    }
}
