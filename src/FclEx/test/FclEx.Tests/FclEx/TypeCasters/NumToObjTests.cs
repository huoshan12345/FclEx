namespace FclEx.TypeCasters;

public sealed class NumToObjTests : AbstractTests
{
    private static readonly MethodInfo _method = typeof(NumToObjTests).GetRequiredMethod(nameof(NumToObj));

    private static void NumToObj<TNumeric>(ITypeCaster caster)
        where TNumeric : struct, IComparable<TNumeric>, IEquatable<TNumeric>
    {
        var num = (TNumeric)((dynamic)1);
        Assert.Equal(typeof(TNumeric), num.GetType());
        var actual = caster.CastTo<TNumeric, object>(num);
        var expected = (object)num;
        Assert.Equal(expected, actual);
    }

    public static IEnumerable<object[]> Cases { get; } = NumericTypes
        .SelectMany(m => TypeCasters.Except(ExpressionTypeCaster.Instance),
            (t, c) => new object[] { c, t })
        .ToArray();

    [Theory]
    [MemberData(nameof(Cases))]
    public void NumToObjTest(ITypeCaster caster, Type numericType)
    {
        _method.MakeGenericMethod(numericType)
            .Invoke(null, new object[] { caster });
    }
}