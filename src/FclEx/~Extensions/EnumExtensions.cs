using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using FclEx.Helpers;

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

        public static int ToInt<TEnum>(this TEnum enumValue) where TEnum : Enum
        {
            // The approved types for an enum are: 
            // byte, sbyte,
            // short, ushort,
            // int, uint,
            // long, ulong. 
            int value;
            var size = Unsafe.SizeOf<TEnum>();
            if (size == Unsafe.SizeOf<byte>()) value = Unsafe.As<TEnum, byte>(ref enumValue);
            else if (size == Unsafe.SizeOf<short>()) value = Unsafe.As<TEnum, short>(ref enumValue);
            else if (size == Unsafe.SizeOf<int>()) value = Unsafe.As<TEnum, int>(ref enumValue);
            else throw new InvalidCastException($"Cannot cast {enumValue.GetType().Name} to int");
            return value;
        }

        public static long ToLong<TEnum>(this TEnum enumValue) where TEnum : Enum
        {
            long value;
            var size = Unsafe.SizeOf<TEnum>();
            if (size == Unsafe.SizeOf<byte>()) value = Unsafe.As<TEnum, byte>(ref enumValue);
            else if (size == Unsafe.SizeOf<short>()) value = Unsafe.As<TEnum, short>(ref enumValue);
            else if (size == Unsafe.SizeOf<int>()) value = Unsafe.As<TEnum, int>(ref enumValue);
            else if (size == Unsafe.SizeOf<long>()) value = Unsafe.As<TEnum, long>(ref enumValue);
            else throw new InvalidCastException($"Cannot cast {enumValue.GetType().Name} to long");
            return value;
        }

        public static T ToEnum<T>(this string? value, T defaultValue) where T : struct, Enum
        {
            return value.ToEnum(s => defaultValue);
        }

        public static T ToEnum<T>(this string? value, Func<string?, T> defaultValueFunc) where T : struct, Enum
        {
            return Enum.TryParse<T>(value, true, out var result) ? result : defaultValueFunc(value);
        }

        public static T ToEnum<T>(this string? value) where T : struct, Enum
        {
            return value.ToEnum<T>(s => throw new FormatException($"Cannot parse to type of {typeof(T).ShortName()} from this value: " + s));
        }

        public static bool IsValid<T>(this T value) where T : Enum
        {
            var validValues = EnumHelper.GetValues<T>();
            return validValues.Contains(value);
        }

        public static bool IsEachValid<T>(this IEnumerable<T> values) where T : Enum
        {
            var validValues = EnumHelper.GetValues<T>();
            return values.All(m => validValues.Contains(m));
        }

        public static TEnum IfNotValid<TEnum>(this TEnum e, TEnum defaultValue = default) where TEnum : Enum
        {
            return e.IsValid() ? e : defaultValue;
        }
    }
}
