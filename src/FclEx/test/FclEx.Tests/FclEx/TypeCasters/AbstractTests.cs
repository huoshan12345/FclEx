using FclEx.Enums;

namespace FclEx.TypeCasters;

public abstract class AbstractTests
{
    protected static readonly ITypeCaster[] TypeCasters =
    {
        CommonTypeCaster.Instance,
        ExpressionTypeCaster.Instance,
        DelegateTypeCaster.Instance,
        DynamicTypeCaster.Instance
    };

    protected static Type[] NumericTypes { get; } =
    {
        typeof(sbyte),
        typeof(int),
        typeof(long),
        typeof(double),
    };

    protected static Type[] EnumTypes { get; } =
    {
        typeof(ByteEnum),
        typeof(IntEnum),
        typeof(ShortEnum),
        typeof(LongEnum),
    };
}