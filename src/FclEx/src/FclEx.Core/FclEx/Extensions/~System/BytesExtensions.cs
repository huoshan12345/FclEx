namespace FclEx.Extensions;

public static partial class BytesExtensions
{
    public static MemoryStream ToStream(this byte[] bytes) => new(bytes);

    public static string GetString(this byte[] bytes, Encoding? encoding = null)
        => bytes.AsReadOnlySpan().GetString(encoding);

    public static string GetString(this ArraySegment<byte> bytes, Encoding? encoding = null)
        => bytes.AsReadOnlySpan().GetString(encoding);

    public static string ToBase64(this byte[] bytes) => Convert.ToBase64String(bytes);

    public static string ToHex(this byte[] bytes, bool upperCase = false)
    {
        using var builder = new ValueStringBuilder();
        var format = upperCase ? "X2" : "x2";
        foreach (var @byte in bytes)
            builder.Append(@byte.ToString(format));
        return builder.ToString();
    }

    public static byte[] ToBytes(this bool num) => BitConverter.GetBytes(num);

    public static byte[] ToBytes(this char num) => BitConverter.GetBytes(num);

    public static byte[] ToBytes(this short num) => BitConverter.GetBytes(num);

    public static byte[] ToBytes(this int num) => BitConverter.GetBytes(num);

    public static byte[] ToBytes(this long num) => BitConverter.GetBytes(num);

    public static byte[] ToBytes(this ushort num) => BitConverter.GetBytes(num);

    public static byte[] ToBytes(this uint num) => BitConverter.GetBytes(num);

    public static byte[] ToBytes(this ulong num) => BitConverter.GetBytes(num);

    public static byte[] ToBytes(this float num) => BitConverter.GetBytes(num);

    public static byte[] ToBytes(this double num) => BitConverter.GetBytes(num);

    private static byte[] ToBytes(IEnumerable<bool> bits, int count)
    {
        var numBytes = count / 8;
        if (count % 8 != 0) numBytes++;

        var bytes = new byte[numBytes];
        int byteIndex = 0, bitIndex = 0;

        foreach (var bit in bits)
        {
            if (bit) bytes[byteIndex] |= (byte)(1 << bitIndex);
            ++bitIndex;
            if (bitIndex == 8)
            {
                bitIndex = 0;
                ++byteIndex;
            }

        }
        return bytes;
    }

    public static byte[] ToBytes(this bool[] bits) => ToBytes(bits, bits.Length);

    public static byte[] ToBytes(this List<bool> bits) => ToBytes(bits, bits.Count);

    public static short ToInt16(this byte[] bytes, int offset = 0) => BitConverter.ToInt16(bytes, offset);
    public static short ReadInt16(this byte[] bytes, ref int offset) => ToStructure<short>(bytes, ref offset);

    public static ushort ToUInt16(this byte[] bytes, int offset = 0) => BitConverter.ToUInt16(bytes, offset);
    public static ushort ReadUInt16(this byte[] bytes, ref int offset) => ToStructure<ushort>(bytes, ref offset);

    public static int ToInt32(this byte[] bytes, int offset = 0) => BitConverter.ToInt32(bytes, offset);
    public static int ReadInt32(this byte[] bytes, ref int offset) => ToStructure<int>(bytes, ref offset);

    public static uint ToUInt32(this byte[] bytes, int offset = 0) => BitConverter.ToUInt32(bytes, offset);
    public static uint ReadUInt32(this byte[] bytes, ref int offset) => ToStructure<uint>(bytes, ref offset);

    public static long ToInt64(this byte[] bytes, int offset = 0) => BitConverter.ToInt64(bytes, offset);
    public static long ReadInt64(this byte[] bytes, ref int offset) => ToStructure<long>(bytes, ref offset);

    public static ulong ToUInt64(this byte[] bytes, int offset = 0) => BitConverter.ToUInt64(bytes, offset);
    public static ulong ReadUInt64(this byte[] bytes, ref int offset) => ToStructure<ulong>(bytes, ref offset);

    public static float ToFloat(this byte[] bytes, int offset = 0) => BitConverter.ToSingle(bytes, offset);
    public static float ReadFloat(this byte[] bytes, ref int offset) => ToStructure<float>(bytes, ref offset);

    public static double ToDouble(this byte[] bytes, int offset = 0) => BitConverter.ToDouble(bytes, offset);
    public static double ReadDouble(this byte[] bytes, ref int offset) => ToStructure<double>(bytes, ref offset);

    public static int IndexOf(this byte[] buffer, int offset, params byte[] subBytes)
    {
        if (subBytes.Length > buffer.Length) return -1;

        var i = offset; // 主串的位置
        var j = 0; // 模式串的位置
        var next = GetNextArray(subBytes);
        while (i < buffer.Length && j < subBytes.Length)
        {
            if (j == -1 || buffer[i] == subBytes[j])
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

    private static int[] GetNextArray(byte[] subBytes)
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

    public static T ToStructure<T>(this byte[] bytes, ref int offset) where T : struct
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
        return obj;
    }

    public static T ToStructure<T>(this byte[] bytes) where T : struct
    {
        var i = 0;
        return ToStructure<T>(bytes, ref i);
    }

    public static T[] ToStructures<T>(this byte[] bytes, ref int offset, int count) where T : struct
    {
        Check.NotNull(bytes);
        Check.NotLessThan(offset, 0);
        Check.NotLessThan(count, 1);

        var length = Marshal.SizeOf<T>();
        var totalBytes = length * count;
        Check.NotLessThan(bytes.Length, totalBytes + offset);

        var result = new T[count];
        using var disposable = MarshalHelper.AllocHGlobal(length);
        var ptr = disposable.Value;
        for (var i = 0; i < count; i++)
        {
            Marshal.Copy(bytes, offset, ptr, length);
            var obj = Marshal.PtrToStructure<T>(ptr);
            offset += length;
            result[i] = obj;
        }

        return result;
    }

    public static T[] ToStructures<T>(this byte[] bytes) where T : struct
    {
        var length = Marshal.SizeOf<T>();
        var i = 0;
        return ToStructures<T>(bytes, ref i, bytes.Length / length);
    }

    public static byte[] ToBytes<T>(this T obj) where T : struct
    {
        var length = Marshal.SizeOf<T>();
        var bufByte = new byte[length];
        using var disposable = MarshalHelper.AllocHGlobal(length);
        var ptr = disposable.Value;
        Marshal.StructureToPtr(obj, ptr, true);
        Marshal.Copy(ptr, bufByte, 0, length);
        return bufByte;
    }

    public static byte[] ToBytes<T>(this IReadOnlyList<T> list) where T : struct
    {
        Check.NotNull(list);
        Check.NotEmpty(list);

        var length = Marshal.SizeOf<T>();
        var totalBytes = length * list.Count;
        var bufByte = new byte[totalBytes];
        using var disposable = MarshalHelper.AllocHGlobal(length);
        var ptr = disposable.Value;
        for (var i = 0; i < list.Count; i++)
        {
            Marshal.StructureToPtr(list[i], ptr, true);
            Marshal.Copy(ptr, bufByte, i * length, length);
        }

        return bufByte;
    }

    public static void WriteTo(this byte[] bytes, Stream stream) => stream.Write(bytes, 0, bytes.Length);

    public static Task WriteToAsync(this byte[] bytes, Stream stream) => stream.WriteAsync(bytes, 0, bytes.Length);
}