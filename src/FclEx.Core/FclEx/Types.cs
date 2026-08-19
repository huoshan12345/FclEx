namespace FclEx;

public static class Types
{
    /// <summary>
    /// 10 built-in integer types.
    /// </summary>
    public static readonly ReadOnlyHashSet<Type> IntegerTypes =
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
    public static readonly ReadOnlyHashSet<Type> FloatingTypes =
    [
        typeof(float),
        typeof(double),
    ];

    /// <summary>
    /// 14 primitive types.
    /// </summary>
    public static readonly ReadOnlyHashSet<Type> PrimitiveTypes =
    [
        ..IntegerTypes,
        ..FloatingTypes,
        typeof(bool),
        typeof(char),
    ];

    /// <summary>
    /// 12 blittable types.
    /// </summary>
    public static readonly ReadOnlyHashSet<Type> BlittableTypes =
    [
        ..IntegerTypes,
        ..FloatingTypes,
    ];

    public static readonly ReadOnlyHashSet<Type> ActionTypes = typeof(Action).Assembly
        .GetExportedTypes()
        .Where(m => m.SimpleName() == nameof(Action))
        .ToReadOnlySet();

    public static readonly ReadOnlyHashSet<Type> FuncTypes = typeof(Func<>).Assembly
        .GetExportedTypes()
        .Where(m => m.SimpleName() == nameof(Func<>))
        .ToReadOnlySet();
}