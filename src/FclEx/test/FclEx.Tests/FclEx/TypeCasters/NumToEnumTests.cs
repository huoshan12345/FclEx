namespace FclEx.TypeCasters;

public sealed class NumToEnumTests : AbstractTests
{
    private static readonly MethodInfo _method = typeof(NumToEnumTests).GetRequiredMethod(nameof(NumToEnum));

    private static void NumToEnum<TNumeric, TEnum>(ITypeCaster caster)
        where TNumeric : struct, IComparable<TNumeric>, IEquatable<TNumeric>
        where TEnum : struct, Enum
    {
        var num = (TNumeric)((dynamic)1);
        Assert.Equal(typeof(TNumeric), num.GetType());
        var actual = caster.CastTo<TNumeric, TEnum>(num);
        var expected = (TEnum)((dynamic)1);
        Assert.Equal(expected, actual);
    }

    public static IEnumerable<object[]> Cases { get; } = NumericTypes
        .SelectMany(m => EnumTypes, (t1, t2) => (t1, t2))
        .SelectMany(m => TypeCasters, (t, c) => new object[] { c, t.t1, t.t2 })
        .ToArray();

    [Theory]
    [MemberData(nameof(Cases))]
    public void NumToEnumTest(ITypeCaster caster, Type numericType, Type enumType)
    {
        _method.MakeGenericMethod(numericType, enumType)
            .Invoke(null, new object[] { caster });
    }
}