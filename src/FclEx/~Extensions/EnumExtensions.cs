using System;
using System.Runtime.CompilerServices;

namespace FclEx
{
    public static class EnumExtensions
    {
        // size-specific version
        public static TInt AsInt<TEnum, TInt>(this TEnum enumValue)
            where TEnum : unmanaged, Enum
            where TInt : unmanaged
        {
            if (Unsafe.SizeOf<TEnum>() == Unsafe.SizeOf<TInt>()) throw new Exception("type mismatch");
            var value = Unsafe.As<TEnum, TInt>(ref enumValue);
            return value;
        }

        public static int ToInt(this Enum enumValue)
        {
            return Convert.ToInt32(enumValue);
        }

        public static string ToIntStr(this Enum enumValue)
        {
            return enumValue.ToInt().ToString();
        }

        public static int ToInt<TEnum>(this TEnum enumValue)
            where TEnum : unmanaged, Enum
        {
            int value;
            if (Unsafe.SizeOf<TEnum>() == Unsafe.SizeOf<byte>()) value = Unsafe.As<TEnum, byte>(ref enumValue);
            else if (Unsafe.SizeOf<TEnum>() == Unsafe.SizeOf<short>()) value = Unsafe.As<TEnum, short>(ref enumValue);
            else if (Unsafe.SizeOf<TEnum>() == Unsafe.SizeOf<int>()) value = Unsafe.As<TEnum, int>(ref enumValue);
            else throw new InvalidCastException($"Cannot cast {enumValue.GetType().Name} to int");
            return value;
        }

        public static long ToLong<TEnum>(this TEnum enumValue)
            where TEnum : unmanaged, Enum
        {
            long value;
            if (Unsafe.SizeOf<TEnum>() == Unsafe.SizeOf<byte>()) value = Unsafe.As<TEnum, byte>(ref enumValue);
            else if (Unsafe.SizeOf<TEnum>() == Unsafe.SizeOf<short>()) value = Unsafe.As<TEnum, short>(ref enumValue);
            else if (Unsafe.SizeOf<TEnum>() == Unsafe.SizeOf<int>()) value = Unsafe.As<TEnum, int>(ref enumValue);
            else if (Unsafe.SizeOf<TEnum>() == Unsafe.SizeOf<long>()) value = Unsafe.As<TEnum, long>(ref enumValue);
            else throw new InvalidCastException($"Cannot cast {enumValue.GetType().Name} to long");
            return value;
        }

        public static T ToEnum<T>(this string value, T defaultValue)
            where T : struct, Enum
        {
            return ToEnum(value, s => defaultValue);
        }

        public static T ToEnum<T>(this string value, Func<string, T> defaultValueFunc)
            where T : struct, Enum
        {
            return Enum.TryParse<T>(value, true, out var result) ? result : defaultValueFunc(value);
        }
    }
}
