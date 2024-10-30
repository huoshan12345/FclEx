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

    /// <summary>
    /// 14 primitive types.
    /// </summary>
    public static readonly ReadOnlySet<Type> BlittableTypes =
    [
        ..PrimitiveTypes,
        typeof(decimal),
        typeof(TimeSpan),
        typeof(Guid),
        typeof(DateOnly),
        typeof(TimeOnly),
    ];

    public static readonly ReadOnlySet<Type> CommonValueTypes =
    [
        ..BlittableTypes,
        typeof(DateTime), // non-blittable
        typeof(DateTimeOffset), // non-blittable
        typeof(ValueTuple<int>), // non-blittable
        typeof(ValueTuple<int, long, DateTimeOffset, DateTime>), // non-blittable
    ];
}