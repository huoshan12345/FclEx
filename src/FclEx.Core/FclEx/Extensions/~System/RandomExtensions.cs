namespace FclEx.Extensions;

/// <summary>
/// Provides extensions for generating pseudo-random values from <see cref="Random" />.
/// </summary>
/// <remarks>
/// These APIs use <see cref="Random" /> and are not suitable for cryptographic keys, passwords, tokens, or other
/// security-sensitive values. Range-based methods follow the <see cref="Random" /> convention: the lower bound is
/// inclusive and the upper bound is exclusive. <see cref="Next{T}(Random)" /> creates test data and does not
/// guarantee that generated objects satisfy their domain invariants.
/// </remarks>
public static class RandomExtensions
{
    private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    private const int DefaultMaxObjectGraphDepth = 10;

    extension(Random)
    {
#if !NET5_0_OR_GREATER
        public static Random Shared => ThreadSafeRandom.Instance;
#endif
    }

    public static char NextChar(this Random random)
    {
        Check.NotNull(random);
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
    {
        Check.NotNull(random);
        CheckProbability(trueProbability);
        return random.NextDouble() >= 1.0D - trueProbability;
    }

    [MethodImpl(AggressiveInlining)]
    public static bool NextBooleanPercent(this Random random, double truePercentage = 50)
    {
        Check.NotNull(random);
        CheckPercentage(truePercentage);
        return random.NextBoolean(truePercentage / 100D);
    }

    [MethodImpl(AggressiveInlining)]
    public static sbyte NextSByte(this Random random) => random.NextUnmanaged<sbyte>();

    [MethodImpl(AggressiveInlining)]
    public static sbyte NextSByte(this Random random, sbyte min, sbyte max)
    {
        Check.NotNull(random);
        return (sbyte)random.Next(min, max);
    }

    [MethodImpl(AggressiveInlining)]
    public static byte NextByte(this Random random) => random.NextUnmanaged<byte>();

    [MethodImpl(AggressiveInlining)]
    public static byte NextByte(this Random random, byte min, byte max)
    {
        Check.NotNull(random);
        return (byte)random.Next(min, max);
    }

    [MethodImpl(AggressiveInlining)]
    public static short NextInt16(this Random random) => random.NextUnmanaged<short>();

    [MethodImpl(AggressiveInlining)]
    public static short NextInt16(this Random random, short min, short max)
    {
        Check.NotNull(random);
        return (short)random.Next(min, max);
    }

    [MethodImpl(AggressiveInlining)]
    public static ushort NextUInt16(this Random random) => random.NextUnmanaged<ushort>();

    [MethodImpl(AggressiveInlining)]
    public static ushort NextUInt16(this Random random, ushort min, ushort max)
    {
        Check.NotNull(random);
        return (ushort)random.Next(min, max);
    }

    [MethodImpl(AggressiveInlining)]
    public static uint NextUInt32(this Random random) => random.NextUnmanaged<uint>();

    [MethodImpl(AggressiveInlining)]
    public static uint NextUInt32(this Random random, uint min, uint max)
        => (uint)random.NextUInt64(min, max);

    [MethodImpl(AggressiveInlining)]
    public static long NextInt64(this Random random) => random.NextInt64(0, long.MaxValue);

    [MethodImpl(AggressiveInlining)]
    public static ulong NextUInt64(this Random random) => random.NextUnmanaged<ulong>();

    [MethodImpl(AggressiveInlining)]
    public static ulong NextUInt64(this Random random, ulong min, ulong max)
    {
        Check.NotNull(random);
        CheckRange(min, max);

        if (min == max)
            return min;

        var range = max - min;
        var limit = ulong.MaxValue - ulong.MaxValue % range;
        ulong r;

        do
        {
            r = random.NextUnmanaged<ulong>();
        } while (r >= limit);

        return r % range + min;
    }

    [MethodImpl(AggressiveInlining)]
    public static double NextDouble(this Random random, double min, double max)
    {
        Check.NotNull(random);
        CheckRange(min, max);

        if (min == max)
            return min;

        var r = random.NextDouble();
        return max * r + min * (1 - r);
    }

#if !NET6_0_OR_GREATER
    [MethodImpl(AggressiveInlining)]
    public static float NextSingle(this Random random)
    {
        Check.NotNull(random);
        const int precision = 1 << 24;
        return random.Next(precision) * (1f / precision);
    }
#endif

    [MethodImpl(AggressiveInlining)]
    public static decimal NextDecimal(this Random random, decimal min = 0, decimal max = decimal.MaxValue)
    {
        Check.NotNull(random);
        CheckRange(min, max);

        if (min == max)
            return min;

        // min + (max - min) * r => max * r + min * (1 - r)
        // because max - min may be larger than Type.MaxValue.
        var r = (decimal)random.NextDouble();
        return max * r + min * (1 - r);
    }

    public static DateTime NextDateTime(this Random random, DateTime? minValue = null, DateTime? maxValue = null)
    {
        Check.NotNull(random);
        var min = minValue ?? DateTime.UnixEpoch;
        var max = maxValue ?? DateTime.MaxValue;
        return random.NextDateTimeOffset(min, max).DateTime;
    }

    public static DateTimeOffset NextDateTimeOffset(this Random random, DateTimeOffset? minValue = null, DateTimeOffset? maxValue = null)
    {
        Check.NotNull(random);
        var min = minValue ?? DateTimeOffset.UnixEpoch;
        var max = maxValue ?? DateTimeOffset.MaxValue;
        CheckRange(min, max);

        if (min == max)
            return min;

        var ticks = random.NextInt64(min.UtcTicks, max.UtcTicks);
        var utcValue = new DateTimeOffset(ticks, TimeSpan.Zero);
        return utcValue.ToOffset(min.Offset);
    }

#if NET6_0_OR_GREATER
    public static DateOnly NextDateOnly(this Random random, DateOnly? minValue = null, DateOnly? maxValue = null)
    {
        Check.NotNull(random);
        var min = minValue ?? DateOnly.MinValue;
        var max = maxValue ?? DateOnly.MaxValue;
        CheckRange(min, max);

        if (min == max)
            return min;

        var number = random.Next(min.DayNumber, max.DayNumber);
        return DateOnly.FromDayNumber(number);
    }

    public static TimeOnly NextTimeOnly(this Random random, TimeOnly? minValue = null, TimeOnly? maxValue = null)
    {
        Check.NotNull(random);
        var min = minValue ?? TimeOnly.MinValue;
        var max = maxValue ?? TimeOnly.MaxValue;
        CheckRange(min, max);

        if (min == max)
            return min;

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
        Check.NotNull(random);
        typeof(T).EnsureMarshalable();
        var size = Marshal.SizeOf<T>();
        var bytes = new byte[size];
        random.NextBytes(bytes);
        using var memory = MarshalHelper.AllocHGlobal(size);
        Marshal.Copy(bytes, 0, memory.Value, size);
        return Marshal.PtrToStructure<T>(memory.Value)!;
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
        Check.NotNull(random);
#if !NET5_0_OR_GREATER
        var bytes = new byte[sizeof(T)];
        random.NextBytes(bytes);
        var result = MemoryMarshal.Read<T>(bytes);
#else
        Unsafe.SkipInit(out T result);
        var value = MemoryMarshal.CreateSpan(ref result, 1);
        random.NextBytes(MemoryMarshal.AsBytes(value));
#endif
        return result;
    }

#if !NET5_0_OR_GREATER
    public static long NextInt64(this Random random, long min, long max)
    {
        Check.NotNull(random);
        CheckRange(min, max);

        if (min == max)
            return min;

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

    /// <summary>
    /// Creates an arbitrary value of <typeparamref name="T"/> for test-data scenarios.
    /// </summary>
    /// <remarks>
    /// This method recursively constructs object graphs by invoking constructors, including non-public constructors,
    /// and then assigning instance fields, including non-public fields. Constructors and field assignment can have
    /// arbitrary side effects. The generated value is not guaranteed to satisfy the type's invariants. Interfaces,
    /// abstract types, unsupported runtime types, readonly members, and constructors that reject generated arguments
    /// can fail at runtime. Recursive reference chains are truncated after ten occurrences of the same type on one path.
    /// Use an explicit factory when valid domain objects are required.
    /// </remarks>
    [MethodImpl(AggressiveInlining)]
    public static T Next<T>(this Random random)
    {
        Check.NotNull(random);
        return (T)random.Next(typeof(T), null, null);
    }

    /// <summary>
    /// Creates an arbitrary value of <paramref name="type"/> for test-data scenarios.
    /// </summary>
    /// <remarks>
    /// This overload has the same constructor, side-effect, invariant, and recursion limitations as
    /// <see cref="Next{T}(Random)"/>.
    /// </remarks>
    [MethodImpl(AggressiveInlining)]
    public static object Next(this Random random, Type type)
    {
        Check.NotNull(random);
        Check.NotNull(type);
        return random.Next(type, null, null);
    }

    private static object Next(this Random random, Type type, ICustomAttributeProvider? provider, Dictionary<Type, int>? depth)
    {
        if (Nullable.GetUnderlyingType(type) is { } nullable)
            type = nullable;

        if (type.IsEnum)
        {
            var values = Enum.GetValues(type);
            return values.GetValue(random.Next(values.Length))!;
        }

        if (type.GetElementType() is { } elementType)
        {
            int? length = null;
            if (provider != null && provider.TryGetAttribute<MarshalAsAttribute>(false, out var attribute))
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
                var element = random.Next(elementType, null, depth);
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
        {
            var bytes = new byte[16];
            random.NextBytes(bytes);
            return new Guid(bytes);
        }

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

        depth = NextDepth(depth, type);
        var instance = random.CreateInstance(type, depth);
        var fields = type.GetAllInstanceFields();
        foreach (var field in fields)
        {
            // to avoid too much circular references.
            if (HasReachedMaxDepth(field.FieldType, depth))
                continue;

            var value = random.Next(field.FieldType, field, depth);
            field.SetValue(instance, value);
        }
        return instance;
    }

    private static object CreateInstance(this Random random, Type type, Dictionary<Type, int> depth)
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
            var args = new List<object?>();

            try
            {
                foreach (var para in paras)
                {
                    var arg = HasReachedMaxDepth(para.ParameterType, depth)
                        ? GetDefaultValue(para.ParameterType)
                        : random.Next(para.ParameterType, para, depth);
                    args.Add(arg);
                }

                return ctor.Invoke(args.AsReadOnlySpan().ToArray());
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

        // the upper bound is exclusive, so no need to minus 1.
        var i = random.Next(0, list.Count);
        return list[i];
    }

    public static T Sample<T>(this Random random, IEnumerable<T> source)
    {
        Check.NotNull(random);
        Check.NotNull(source);

        using var enumerator = source.GetEnumerator();

        if (enumerator.MoveNext() == false)
            throw new ArgumentException("The source sequence must not be empty.", nameof(source));

        var selected = enumerator.Current;
        var count = 1;

        while (enumerator.MoveNext())
        {
            count++;

            if (random.Next(count) == 0)
            {
                selected = enumerator.Current;
            }
        }

        return selected;
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

    private static void CheckRange<T>(T min, T max) where T : IComparable<T>
    {
        if (Comparer<T>.Default.Compare(min, max) > 0)
            throw new ArgumentOutOfRangeException(nameof(max), max, "The maximum value must be greater than or equal to the minimum value.");
    }

    private static void CheckProbability(double probability)
    {
        if (double.IsNaN(probability) || probability is < 0D or > 1D)
            throw new ArgumentOutOfRangeException(nameof(probability), probability, "The probability must be between 0 and 1.");
    }

    private static void CheckPercentage(double percentage)
    {
        if (double.IsNaN(percentage) || percentage is < 0D or > 100D)
            throw new ArgumentOutOfRangeException(nameof(percentage), percentage, "The percentage must be between 0 and 100.");
    }

    private static Dictionary<Type, int> NextDepth(Dictionary<Type, int>? depth, Type type)
    {
        var result = depth is null
            ? []
            : new Dictionary<Type, int>(depth);
        result[type] = result.Get(type) + 1;
        return result;
    }

    private static bool HasReachedMaxDepth(Type type, Dictionary<Type, int> depth)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
        return underlyingType.IsValueType == false
            && depth.Get(underlyingType) >= DefaultMaxObjectGraphDepth;
    }

    private static object? GetDefaultValue(Type type)
    {
        return type.IsValueType
            ? Activator.CreateInstance(type)
            : null;
    }
}
