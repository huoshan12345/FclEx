namespace FclEx.Extensions;

/// <summary>
/// Provides extension methods for working with byte arrays, <see cref="Span{T}"/>, 
/// and <see cref="ReadOnlySpan{T}"/> to enhance functionality and ease of use.
/// </summary>
public static partial class BytesExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MemoryStream ToStream(this byte[] bytes) => new(bytes);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetString(this byte[] bytes, Encoding? encoding = null)
        => bytes.AsReadOnlySpan().GetString(encoding);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetString(this ArraySegment<byte> bytes, Encoding? encoding = null)
        => bytes.AsReadOnlySpan().GetString(encoding);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    public static T ToBlittable<T>(this byte[] bytes, ref int offset)
    {
        Check.NotNull(bytes);
        Check.NotLessThan(offset, 0);

        var length = Marshal.SizeOf<T>();
        Check.NotLessThan(bytes.Length, length + offset);

        using var disposable = MarshalHelper.AllocHGlobal(length);
        var ptr = disposable.Value;
        Marshal.Copy(bytes, offset, ptr, length);
        var obj = Marshal.PtrToStructure<T>(ptr);
        offset += length;
        return obj!;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ToBlittable<T>(this byte[] bytes)
    {
        var i = 0;
        return ToBlittable<T>(bytes, ref i);
    }

    public static T[] ToBlittableArray<T>(this byte[] bytes, int count, ref int offset)
    {
        Check.NotNull(bytes);
        Check.NotLessThan(offset, 0);
        Check.NotLessThan(count, 1);

        var length = Marshal.SizeOf<T>();
        Check.NotLessThan(bytes.Length, length * count + offset);

        var result = new T[count];
        using var disposable = MarshalHelper.AllocHGlobal(length);
        var ptr = disposable.Value;
        for (var i = 0; i < count; i++)
        {
            Marshal.Copy(bytes, offset, ptr, length);
            var obj = Marshal.PtrToStructure<T>(ptr);
            offset += length;
            result[i] = obj!;
        }
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] ToBlittableArray<T>(this byte[] bytes)
    {
        var length = Marshal.SizeOf<T>();
        var i = 0;
        return ToBlittableArray<T>(bytes, bytes.Length / length, ref i);
    }

    public static byte[] BlittableToBytes<T>(this T obj)
    {
        Check.NotNull(obj);

        var length = Marshal.SizeOf<T>();
        var bufByte = new byte[length];
        using var disposable = MarshalHelper.AllocHGlobal(length);
        var ptr = disposable.Value;
        Marshal.StructureToPtr(obj, ptr, true);
        Marshal.Copy(ptr, bufByte, 0, length);
        return bufByte;
    }

    public static byte[] BlittableArrayToBytes<T>(this IReadOnlyList<T> list)
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

            Marshal.StructureToPtr(item, ptr, true);
            Marshal.Copy(ptr, bufByte, i * length, length);
        }

        return bufByte;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteTo(this byte[] bytes, Stream stream) => stream.Write(bytes, 0, bytes.Length);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task WriteToAsync(this byte[] bytes, Stream stream) => stream.WriteAsync(bytes, 0, bytes.Length);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        var i = 0; // 主串的位置
        var j = 0; // 模式串的位置
        var next = GetNextArray(subBytes);
        while (i < bytes.Length && j < subBytes.Length)
        {
            if (j == -1 || bytes[i] == subBytes[j])
            {
                i++; // 当j为-1时，要移动的是i，当然j也要归0
                j++;
            }
            else
            {
                // i不需要回溯了
                // i = i - j + 1;
                j = next[j]; // j回到指定位置

            }
        }
        return j == subBytes.Length ? i - j : -1;
    }

    private static int[] GetNextArray(ReadOnlySpan<byte> subBytes)
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
                    next[j] = next[k]; // 当两个字符相等时要跳过
                else
                    next[j] = k;
            }
            else
                k = next[k];
        }
        return next;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ComputeHashCode(this byte[] bytes)
    {
        return bytes.AsReadOnlySpan().ComputeHashCode();
    }
}