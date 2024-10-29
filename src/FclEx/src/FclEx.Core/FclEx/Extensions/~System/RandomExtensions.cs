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

    public static bool NextBoolean(this Random random, double trueProbability) => random.NextDouble() >= 1.0D - trueProbability;
    public static bool NextBoolean(this Random random) => random.Next(1, 2) == 1;
    public static sbyte NextSByte(this Random random) => (sbyte)random.Next(sbyte.MinValue, sbyte.MaxValue);
    public static byte NextByte(this Random random) => (byte)random.Next(byte.MinValue, byte.MaxValue);
    public static short NextInt16(this Random random) => (short)random.Next(short.MinValue, short.MaxValue);
    public static ushort NextUInt16(this Random random) => (ushort)random.Next(ushort.MinValue, ushort.MaxValue);
    public static uint NextUInt32(this Random random) => (uint)random.NextInt64(uint.MinValue, uint.MaxValue);
    public static ulong NextUInt64(this Random random) => (ulong)random.NextInt64(long.MinValue, long.MaxValue);
    public static decimal NextDecimal(this Random random) => (decimal)random.NextDouble();
    public static DateTime NextDateTime(this Random random, DateTime? minValue = null, DateTime? maxValue = null)
    {
        var min = minValue ?? DateTimeExtensions.UnixEpoch;
        var max = maxValue ?? DateTime.MaxValue;
        var ticks = random.NextInt64(min.Ticks, max.Ticks);
        return new DateTime(ticks);
    }

#if NET6_0_OR_GREATER
    /// <summary>
    /// Generates a random value of blittable type.
    /// </summary>
    /// <param name="random">The source of random numbers.</param>
    /// <typeparam name="T">The blittable type.</typeparam>
    /// <returns>The randomly generated value.</returns>
    //public static T Next<T>(this Random random) where T : struct
    //{
    //    var size = Marshal.SizeOf<T>();
    //    var bytes = new byte[size];
    //    random.NextBytes(bytes);
    //    var result = bytes.ToStructure<T>();
    //    return result;
    //}

    [SkipLocalsInit]
    public static T Next<T>(this Random random) where T : struct
    {
        Unsafe.SkipInit(out T result);
        random.NextBytes(Span.AsBytes(ref result));
        return result;
    }
#endif

#if NETSTANDARD2_0

    /// <summary>
    /// Generates a random value of blittable type.
    /// </summary>
    /// <param name="random">The source of random numbers.</param>
    /// <typeparam name="T">The blittable type.</typeparam>
    /// <returns>The randomly generated value.</returns>
    [SkipLocalsInit]
    public static unsafe T Next<T>(this Random random) where T : struct
    {
        Unsafe.SkipInit(out T result);
        var bytes = new byte[sizeof(T)];
        Unsafe.As<byte, T>(ref bytes[0]) = result;
        random.NextBytes(bytes);
        return result;
    }

    public static long NextInt64(this Random random, long min, long max)
    {
        var rand = random.Next<long>();
        return min + rand % (max + 1 - min);
    }
#endif
}