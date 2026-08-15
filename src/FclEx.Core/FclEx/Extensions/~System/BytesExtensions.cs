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
    /// Reads an unmanaged value from <paramref name="bytes"/> at <paramref name="offset"/> using the managed layout of
    /// <typeparamref name="T"/>, then advances the offset by the value's size.
    /// </summary>
    /// <remarks>
    /// Use <see cref="StructLayoutAttribute"/> and fixed buffers when the bytes originate from a native structure.
    /// No byte-order conversion or interop marshaling is performed.
    /// </remarks>
    public static T ReadStruct<T>(this byte[] bytes, ref int offset) where T : unmanaged
    {
        Check.NotNull(bytes);
        Check.NotLessThan(offset, 0);

        var length = Unsafe.SizeOf<T>();
        Check.NotLessThan(bytes.Length, length + offset);

        var result = MemoryMarshal.Read<T>(bytes.AsSpan(offset, length));
        offset += length;
        return result;
    }

    [MethodImpl(AggressiveInlining)]
    public static T ReadStruct<T>(this byte[] bytes) where T : unmanaged
    {
        var i = 0;
        return bytes.ReadStruct<T>(ref i);
    }

    /// <summary>
    /// Reads <paramref name="count"/> consecutive unmanaged values and advances <paramref name="offset"/> past them.
    /// </summary>
    public static T[] ReadStructArray<T>(this byte[] bytes, int count, ref int offset) where T : unmanaged
    {
        Check.NotNull(bytes);
        Check.NotLessThan(offset, 0);
        Check.NotLessThan(count, 0);

        var length = Unsafe.SizeOf<T>();
        var totalLength = checked(length * count);
        Check.NotLessThan(bytes.Length, checked(totalLength + offset));

        var result = new T[count];
        for (var i = 0; i < count; i++)
        {
            result[i] = MemoryMarshal.Read<T>(bytes.AsSpan(offset, length));
            offset += length;
        }
        return result;
    }

    [MethodImpl(AggressiveInlining)]
    public static T[] ReadStructArray<T>(this byte[] bytes) where T : unmanaged
    {
        Check.NotNull(bytes);

        var length = Unsafe.SizeOf<T>();
        if (bytes.Length % length != 0)
            throw new ArgumentException("The byte array length must be an exact multiple of the structure size.", nameof(bytes));

        var i = 0;
        return bytes.ReadStructArray<T>(bytes.Length / length, ref i);
    }

    public static byte[] MarshalArrayToBytes<T>(this IReadOnlyList<T> list)
    {
        Check.NotNull(list);

        if (list.IsEmpty())
            return [];

        var length = Marshal.SizeOf<T>();
        var totalBytes = length * list.Count;
        var bufByte = new byte[totalBytes];
        using var disposable = MarshalHelper.AllocHGlobal(length);
        var ptr = disposable.Value;
        for (var i = 0; i < list.Count; i++)
        {
            var item = list[i];
            Check.NotNull(item, nameof(list) + $"[{i}]");

            var structureInitialized = false;
            try
            {
                Marshal.StructureToPtr(item, ptr, false);
                structureInitialized = true;
                Marshal.Copy(ptr, bufByte, i * length, length);
            }
            finally
            {
                if (structureInitialized)
                    Marshal.DestroyStructure<T>(ptr);
            }
        }

        return bufByte;
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
