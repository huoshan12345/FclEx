namespace FclEx.TypeCasters;

public class EnumNullableTests : AbstractTests
{
    private static readonly MethodInfo _methodOfEnumToNullable = typeof(EnumNullableTests).GetRequiredMethod(nameof(EnumToNullable));
    private static readonly MethodInfo _methodOfNullableToEnum = typeof(EnumNullableTests).GetRequiredMethod(nameof(NullableToEnum));

    private static void EnumToNullable<TEnum>(ITypeCaster caster)
        where TEnum : struct, Enum
    {
        var e = (TEnum)((dynamic)1);
        Assert.Equal(typeof(TEnum), e.GetType());
        var actual = caster.CastTo<TEnum, TEnum?>(e);
        var expected = (TEnum?)((dynamic)1);
        Assert.Equal(expected, actual);
    }

    private static void NullableToEnum<TEnum>(ITypeCaster caster)
        where TEnum : struct, Enum
    {
        var e = (TEnum?)((dynamic)1);
        Assert.Equal(typeof(TEnum), e.GetType());
        var actual = caster.CastTo<TEnum?, TEnum>(e);
        var expected = (TEnum)((dynamic)1);
        Assert.Equal(expected, actual);
    }

    public static IEnumerable<object[]> Cases { get; } = EnumTypes
        .SelectMany(m => TypeCasters, (t, c) => new object[] { c, t })
        .ToArray();

    [Theory]
    [MemberData(nameof(Cases))]
    public void EnumToNullableTest(ITypeCaster caster, Type enumType)
    {
        _methodOfEnumToNullable.MakeGenericMethod(enumType)
            .Invoke(null, new object[] { caster });
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void NullableToEnumTest(ITypeCaster caster, Type enumType)
    {
        _methodOfNullableToEnum.MakeGenericMethod(enumType)
            .Invoke(null, new object[] { caster });
    }
}