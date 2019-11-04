using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Dawn;
using FclEx.Helpers;
using FclEx.Utils;

namespace FclEx
{
    public static class ByteExtensions
    {
        public static MemoryStream ToStream(this byte[] bytes) => new MemoryStream(bytes);

        public static string GetString(this byte[] bytes, Encoding encoding) => encoding.GetString(bytes);

        public static string GetString(this byte[] bytes) => bytes.GetString(Encoding.UTF8);

        public static string ToBase64String(this byte[] bytes) => Convert.ToBase64String(bytes);

        public static string ToHexString(this byte[] bytes, bool upperCase = false)
        {
            var builder = new StringBuilder(bytes.Length);
            if (upperCase)
            {
                foreach (var @byte in bytes)
                    builder.Append(@byte.ToString("X2"));
            }
            else
            {
                foreach (var @byte in bytes)
                    builder.Append(@byte.ToString("x2"));

            }
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

        public static short ToInt16(this byte[] bytes, int startIndex = 0) => BitConverter.ToInt16(bytes, startIndex);
        public static short ReadInt16(this byte[] bytes, ref int startIndex) => ToUnmanagedStruct<short>(bytes, ref startIndex);

        public static ushort ToUInt16(this byte[] bytes, int startIndex = 0) => BitConverter.ToUInt16(bytes, startIndex);
        public static ushort ReadUInt16(this byte[] bytes, ref int startIndex) => ToUnmanagedStruct<ushort>(bytes, ref startIndex);

        public static int ToInt32(this byte[] bytes, int startIndex = 0) => BitConverter.ToInt32(bytes, startIndex);
        public static int ReadInt32(this byte[] bytes, ref int startIndex) => ToUnmanagedStruct<int>(bytes, ref startIndex);

        public static uint ToUInt32(this byte[] bytes, int startIndex = 0) => BitConverter.ToUInt32(bytes, startIndex);
        public static uint ReadUInt32(this byte[] bytes, ref int startIndex) => ToUnmanagedStruct<uint>(bytes, ref startIndex);

        public static long ToInt64(this byte[] bytes, int startIndex = 0) => BitConverter.ToInt64(bytes, startIndex);
        public static long ReadInt64(this byte[] bytes, ref int startIndex) => ToUnmanagedStruct<long>(bytes, ref startIndex);

        public static ulong ToUInt64(this byte[] bytes, int startIndex = 0) => BitConverter.ToUInt64(bytes, startIndex);
        public static ulong ReadUInt64(this byte[] bytes, ref int startIndex) => ToUnmanagedStruct<ulong>(bytes, ref startIndex);

        public static float ToFloat(this byte[] bytes, int startIndex = 0) => BitConverter.ToSingle(bytes, startIndex);
        public static float ReadFloat(this byte[] bytes, ref int startIndex) => ToUnmanagedStruct<float>(bytes, ref startIndex);

        public static double ToDouble(this byte[] bytes, int startIndex = 0) => BitConverter.ToDouble(bytes, startIndex);
        public static double ReadDouble(this byte[] bytes, ref int startIndex) => ToUnmanagedStruct<double>(bytes, ref startIndex);

        public static int IndexOf(this byte[] buffer, int startIndex, params byte[] subBytes)
        {
            if (subBytes.Length > buffer.Length) return -1;

            var i = startIndex; // 主串的位置
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

        public static T ToUnmanagedStruct<T>(this byte[] bytes, ref int startIndex)
            where T : struct
        {
            Guard.Argument(bytes, nameof(bytes)).NotNull();
            Guard.Argument(startIndex, nameof(startIndex)).Min(0);

            var length = Marshal.SizeOf<T>();
            Guard.Argument(bytes.Length, nameof(bytes.Length)).Min(length + startIndex);

            using (var ptr = MarshalHelper.AllocHGlobal(length))
            {
                var p = ptr.Ptr;
                Marshal.Copy(bytes, startIndex, p, length);
                var obj = Marshal.PtrToStructure<T>(p);
                startIndex += length;
                return obj;
            }
        }

        public static T ToUnmanagedStruct<T>(this byte[] bytes)
            where T : struct
        {
            var i = 0;
            return ToUnmanagedStruct<T>(bytes, ref i);
        }

        public static T[] ToUnmanagedStructs<T>(this byte[] bytes, ref int startIndex, int count)
            where T : struct
        {
            Guard.Argument(bytes, nameof(bytes)).NotNull();
            Guard.Argument(startIndex, nameof(startIndex)).Min(0);
            Guard.Argument(count, nameof(count)).Min(1);

            var length = Marshal.SizeOf<T>();
            var totalBytes = length * count;
            Guard.Argument(bytes.Length, nameof(bytes.Length)).Min(totalBytes + startIndex);

            var result = new T[count];
            using (var ptr = MarshalHelper.AllocHGlobal(length))
            {
                var p = ptr.Ptr;
                for (var i = 0; i < count; i++)
                {
                    Marshal.Copy(bytes, startIndex, p, length);
                    var obj = Marshal.PtrToStructure<T>(p);
                    startIndex += length;
                    result[i] = obj;
                }
            }
            return result;
        }

        public static T[] ToUnmanagedStructs<T>(this byte[] bytes)
            where T : struct
        {
            var length = Marshal.SizeOf<T>();
            var i = 0;
            return ToUnmanagedStructs<T>(bytes, ref i, bytes.Length / length);
        }

        public static byte[] ToUnmanagedBytes<T>(this T obj) where T : struct
        {
            var length = Marshal.SizeOf<T>();
            var bufByte = new byte[length];
            var ptr = Marshal.AllocHGlobal(length);
            Marshal.StructureToPtr(obj, ptr, true);
            Marshal.Copy(ptr, bufByte, 0, length);
            Marshal.FreeHGlobal(ptr);
            return bufByte;
        }

        public static byte[] ToUnmanagedBytes<T>(this IList<T> list) where T : struct
        {
            Guard.Argument(list, nameof(list)).NotNull().NotEmpty();

            var length = Marshal.SizeOf<T>();
            var totalBytes = length * list.Count;
            var bufByte = new byte[totalBytes];
            using (var ptr = MarshalHelper.AllocHGlobal(length))
            {
                var p = ptr.Ptr;
                for (var i = 0; i < list.Count; i++)
                {
                    Marshal.StructureToPtr(list[i], p, true);
                    Marshal.Copy(p, bufByte, i * length, length);
                }
            }
            return bufByte;
        }

        public static void WriteTo(this byte[] bytes, Stream stream) => stream.Write(bytes, 0, bytes.Length);

        public static Task WriteToAsync(this byte[] bytes, Stream stream) => stream.WriteAsync(bytes, 0, bytes.Length);
    }
}
