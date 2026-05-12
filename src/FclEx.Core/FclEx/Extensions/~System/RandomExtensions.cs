using System;
using System.Runtime.InteropServices;

namespace FclEx.Extensions;

public static class RandomExtensions
{
    private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    public static char NextChar(this Random random)
    {
        var index = random.Next(0, Chars.Length);
        return Chars[index];
    }

    public static string NextString(this Random random, int length)
    {
        Check.NotNull(random);
        var stringChars = new char[length];
        for (var i = 0; i < stringChars.Length; ++i)
        {
            stringChars[i] = Chars[random.Next(Chars.Length)];
        }
        return new string(stringChars);
    }

    [MethodImpl(AggressiveInlining)]
    public static bool NextBoolean(this Random random, double trueProbability = 0.5)
        => random.NextDouble() >= 1.0D - trueProbability;

    [MethodImpl(AggressiveInlining)]
    public static sbyte NextSByte(this Random random, sbyte min = 0, sbyte max = sbyte.MaxValue)
        => (sbyte)random.Next(min, max);

    [MethodImpl(AggressiveInlining)]
    public static byte NextByte(this Random random, byte min = 0, byte max = byte.MaxValue)
        => (byte)random.Next(min, max);

    [MethodImpl(AggressiveInlining)]
    public static short NextInt16(this Random random, short min = 0, short max = short.MaxValue)
        => (short)random.Next(min, max);

    [MethodImpl(AggressiveInlining)]
    public static ushort NextUInt16(this Random random, ushort min = 0, ushort max = ushort.MaxValue)
        => (ushort)random.Next(min, max);

    [MethodImpl(AggressiveInlining)]
    public static uint NextUInt32(this Random random, uint min = 0, uint max = uint.MaxValue)
        => (uint)random.NextUInt64(min, max);

    [MethodImpl(AggressiveInlining)]
    public static long NextInt64(this Random random) => random.NextInt64(0, long.MaxValue);

    [MethodImpl(AggressiveInlining)]
    public static ulong NextUInt64(this Random random, ulong min = 0, ulong max = ulong.MaxValue)
    {
        var r = random.NextDouble();
        return (ulong)(max * r + min * (1 - r));
    }

    [MethodImpl(AggressiveInlining)]
    public static double NextDouble(this Random random, double min, double max)
    {
        var r = random.NextDouble();
        return max * r + min * (1 - r);
    }

    [MethodImpl(AggressiveInlining)]
    public static float NextSingle(this Random random, float min = 0, float max = float.MaxValue)
    {
        var r = (float)random.NextDouble();
        return max * r + min * (1 - r);
    }

    [MethodImpl(AggressiveInlining)]
    public static decimal NextDecimal(this Random random, decimal min = 0, decimal max = decimal.MaxValue)
    {
        // min + (max - min) * r => max * r + min * (1 - r)
        // because max - min may be larger than Type.MaxValue.
        var r = (decimal)random.NextDouble();
        return max * r + min * (1 - r);
    }

    public static DateTime NextDateTime(this Random random, DateTime? minValue = null, DateTime? maxValue = null)
    {
        var min = minValue ?? DateTimeExtensions.UnixEpoch;
        var max = maxValue ?? DateTime.MaxValue;
        var ticks = random.NextInt64(min.Ticks, max.Ticks);
        return new DateTime(ticks);
    }

    public static DateTimeOffset NextDateTimeOffset(this Random random, DateTimeOffset? minValue = null, DateTimeOffset? maxValue = null)
    {
        return random.NextDateTime(minValue?.UtcDateTime, maxValue?.UtcDateTime);
    }

#if NET6_0_OR_GREATER
    public static DateOnly NextDateOnly(this Random random, DateOnly? minValue = null, DateOnly? maxValue = null)
    {
        var min = minValue ?? DateOnly.MinValue;
        var max = maxValue ?? DateOnly.MaxValue;
        var number = random.Next(min.DayNumber, max.DayNumber);
        return DateOnly.FromDayNumber(number);
    }

    public static TimeOnly NextTimeOnly(this Random random, TimeOnly? minValue = null, TimeOnly? maxValue = null)
    {
        var min = minValue ?? TimeOnly.MinValue;
        var max = maxValue ?? TimeOnly.MaxValue;
        var ticks = random.NextInt64(min.Ticks, max.Ticks);
        return new TimeOnly(ticks);
    }
#endif

    /// <summary>
    /// Generates a random value of blittable type.
    /// </summary>
    /// <param name="random">The source of random numbers.</param>
    /// <typeparam name="T">The blittable type.</typeparam>
    /// <returns>The randomly generated value.</returns>
    public static T NextMarshalable<T>(this Random random)
    {
        typeof(T).EnsureMarshalable();
        var size = Marshal.SizeOf<T>();
        var bytes = new byte[size];
        random.NextBytes(bytes);
        var result = bytes.MarshalTo<T>();
        return result;
    }

    /// <summary>
    /// Generates a random value of unmanaged type.
    /// </summary>
    /// <param name="random">The source of random numbers.</param>
    /// <typeparam name="T">The blittable type.</typeparam>
    /// <returns>The randomly generated value.</returns>
    [SkipLocalsInit]
    public static
#if !NET5_0_OR_GREATER
        unsafe
#endif
        T NextUnmanaged<T>(this Random random) where T : unmanaged
    {
#if !NET5_0_OR_GREATER
        var bytes = new byte[sizeof(T)];
        random.NextBytes(bytes);
        var result = Unsafe.As<byte, T>(ref bytes[0]) ;
#else
        Unsafe.SkipInit(out T result);
        random.NextBytes(Span.AsBytes(ref result));
#endif
        return result;
    }

#if !NET5_0_OR_GREATER
    public static long NextInt64(this Random random, long min, long max)
    {
        var range = (ulong)(max - min);
        var limit = ulong.MaxValue - ulong.MaxValue % range;
        ulong r;

        do
        {
            r = random.NextUnmanaged<ulong>();
        } while (r >= limit);

        return (long)(r % range) + min;
    }
#endif

    [MethodImpl(AggressiveInlining)]
    public static T Next<T>(this Random random)
    {
        Check.NotNull(random);
        return (T)random.Next(typeof(T), null, null);
    }

    [MethodImpl(AggressiveInlining)]
    public static object Next(this Random random, Type type)
    {
        Check.NotNull(random);
        return random.Next(type, null, null);
    }

    private static object Next(this Random random, Type type, ICustomAttributeProvider? provider, Dictionary<Type, int>? depth)
    {
        if (Nullable.GetUnderlyingType(type) is { } nullable)
            type = nullable;

        if (type.GetElementType() is { } elementType)
        {
            int? length = null;
            if (type.GetProperty("IsFixedSize", false) is { } property)
            {
                // TODO
            }
            else if (provider != null && provider.TryGetAttribute<MarshalAsAttribute>(false, out var attribute))
            {
                if (attribute.Value is UnmanagedType.ByValArray)
                {
                    length = attribute.SizeConst;
                }
            }

            length ??= random.Next(1, 5);

            var array = Array.CreateInstance(elementType, length.Value);
            for (var i = 0; i < length; i++)
            {
                var element = random.Next(elementType, provider, depth);
                array.SetValue(element, i);
            }
            return array;
        }

        var code = type.GetTypeCode();
        return code switch
        {
            TypeCode.Boolean => random.NextBoolean(),
            TypeCode.Byte => random.NextByte(),
            TypeCode.Char => random.NextChar(),
            TypeCode.DateTime => random.NextDateTime(),
            TypeCode.DBNull => DBNull.Value,
            TypeCode.Decimal => random.NextDecimal(),
            TypeCode.Double => random.NextDouble(),
            TypeCode.Empty => throw new ArgumentNullException(nameof(type)),
            TypeCode.Int16 => random.NextInt16(),
            TypeCode.Int32 => random.Next(),
            TypeCode.Int64 => random.NextInt64(long.MinValue, long.MaxValue),
            TypeCode.SByte => random.NextSByte(),
            TypeCode.Single => random.NextSingle(),
            TypeCode.String => random.NextString(random.Next(5, 20)),
            TypeCode.UInt16 => random.NextUInt16(),
            TypeCode.UInt32 => random.NextUInt32(),
            TypeCode.UInt64 => random.NextUInt64(),
            TypeCode.Object => random.NextObject(type, depth),
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, null)
        };
    }

    private static object NextObject(this Random random, Type type, Dictionary<Type, int>? depth)
    {
        var is64Bit = Environment.Is64BitProcess;
        if (type == typeof(object))
            return new object();

        if (type == typeof(Guid))
            return Guid.NewGuid();

        if (type == typeof(IntPtr))
            return is64Bit
                ? new IntPtr(random.NextInt64())
                : new IntPtr(random.Next());

        if (type == typeof(UIntPtr))
            return is64Bit
                ? new UIntPtr(random.NextUInt64())
                : new UIntPtr(random.NextUInt32());

        if (type == typeof(TimeSpan))
            return TimeSpan.FromTicks(random.NextInt64());

        if (type == typeof(DateTime))
            return random.NextDateTime();

        if (type == typeof(DateTimeOffset))
            return random.NextDateTimeOffset(); // for .net10+, there will be error if DateTimeOffset created from fields.

#if NET6_0_OR_GREATER
        if (type == typeof(DateOnly))
            return random.NextDateOnly();

        if (type == typeof(TimeOnly))
            return random.NextTimeOnly();
#endif

        depth ??= [];
        depth[type] = depth.Get(type) + 1; // only care about the depth of compound type types
        var instance = random.CreateInstance(type, depth);
        var fields = type.GetAllInstanceFields();
        foreach (var field in fields)
        {
            // to avoid too much circular references.
            if (depth.Get(field.FieldType) >= 10)
                continue;

            var value = random.Next(field.FieldType, field, depth);
            field.SetValue(instance, value);
        }
        return instance;
    }

    private static object CreateInstance(this Random random, Type type, Dictionary<Type, int>? depth)
    {
        if (type.IsValueType)
            return Activator.CreateInstance(type)!;

        var ctors = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (ctors.Length == 0)
            throw new ArgumentException($"The type '{type.LongName()}' does not have any constructors.");

        var defaultCtor = ctors.FirstOrDefault(m => m.GetParameters().Length == 0);

        if (defaultCtor is not null)
            return defaultCtor.Invoke([]);

        var exceptions = new List<Exception>();
        foreach (var ctor in ctors)
        {
            var paras = ctor.GetParameters();
            var args = new List<object>();

            try
            {
                foreach (var para in paras)
                {
                    var arg = random.Next(para.ParameterType, para, depth);
                    args.Add(arg);
                }

                return ctor.Invoke(args.AsSpan().ToArray());
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }

        var e = exceptions.Count == 1
            ? exceptions[0]
            : new AggregateException(exceptions);

        throw e;
    }

    public static T Sample<T>(this Random random, IReadOnlyList<T> list)
    {
        Check.NotNull(random);
        Check.NotEmpty(list);

        var i = random.Next(0, list.Count - 1);
        return list[i];
    }

    public static void Shuffle<T>(this Random random, IList<T> list)
    {
        Check.NotNull(random);
        Check.NotNull(list);

        for (var i = list.Count - 1; i > 0; --i)
        {
            var index = random.Next(i + 1);
            (list[i], list[index]) = (list[index], list[i]);
        }
    }


    extension(Random)
    {
#if !NET5_0_OR_GREATER
        public static Random Shared => ThreadSafeRandom.Instance;
#endif
    }
}