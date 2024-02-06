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

    public static short NextShort(this Random random, short max = short.MaxValue)
    {
        Check.NotNull(random);
        Check.NotNegative(max);
        return (short)(random.NextDouble() * max);
    }

    public static bool NextBoolean(this Random random, double truePercentage) => random.NextDouble() < (truePercentage / 100.0);
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

    public static object? Next(this Random random, Type type)
    {
        if (type == typeof(Guid))
            return Guid.NewGuid();

        return Type.GetTypeCode(type) switch
        {
            TypeCode.Boolean => random.NextBoolean(),
            TypeCode.Char => random.NextChar(),
            TypeCode.SByte => random.NextSByte(),
            TypeCode.Byte => random.NextByte(),
            TypeCode.Int16 => random.NextInt16(),
            TypeCode.UInt16 => random.NextUInt16(),
            TypeCode.Int32 => random.Next(),
            TypeCode.UInt32 => random.NextUInt32(),
            TypeCode.Int64 => random.NextInt64(),
            TypeCode.UInt64 => random.NextUInt64(),
            TypeCode.Single => random.NextSingle(),
            TypeCode.Double => random.NextDouble(),
            TypeCode.Decimal => random.NextDecimal(),
            TypeCode.DateTime => random.NextDateTime(maxValue: new DateTime(2100, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
            TypeCode.String => random.NextString(10),
            TypeCode.Empty => null,
            TypeCode.Object => new object(),
            TypeCode.DBNull => DBNull.Value,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type.Name, null)
        };
    }
}