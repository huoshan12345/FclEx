namespace FclEx.TestModels;

public static class Types
{
    /// <summary>
    /// 10 built-in integer types.
    /// </summary>
    public static readonly ReadOnlySet<Type> IntegerTypes =
    [
        typeof(sbyte),
        typeof(byte),
        typeof(short),
        typeof(ushort),
        typeof(int),
        typeof(uint),
        typeof(long),
        typeof(ulong),
        typeof(nint),
        typeof(nuint),
    ];

    /// <summary>
    /// 2 built-in floating types.
    /// </summary>
    public static readonly ReadOnlySet<Type> FloatingTypes =
    [
        typeof(float),
        typeof(double),
    ];

    /// <summary>
    /// 14 primitive types.
    /// </summary>
    public static readonly ReadOnlySet<Type> PrimitiveTypes =
    [
        ..IntegerTypes,
        ..FloatingTypes,
        typeof(bool),
        typeof(char),
    ];

    public static readonly ReadOnlySet<Type> CommonValueTypes =
    [
        ..PrimitiveTypes,
        typeof(decimal),
        typeof(DateTime), // non-blittable
        typeof(TimeSpan),
        typeof(Guid),
        typeof(DateTimeOffset), // non-blittable
        typeof(DateOnly),
        typeof(TimeOnly),
        typeof(ValueTuple<int>),
        typeof(ValueTuple<int, long, DateTimeOffset, DateTime>), // non-blittable
    ];
}