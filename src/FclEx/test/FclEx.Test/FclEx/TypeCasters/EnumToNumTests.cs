using System;
using System.Collections.Generic;
using System.Linq;

namespace FclEx.TypeCasters;

public sealed class EnumToNumTests : AbstractTests
{
    private static readonly MethodInfo _method = typeof(EnumToNumTests).GetRequiredMethod(nameof(EnumToNum));

    private static void EnumToNum<TEnum, TNumeric>(ITypeCaster caster)
        where TEnum : struct, Enum
        where TNumeric : struct, IComparable<TNumeric>, IEquatable<TNumeric>
    {
        var e = (TEnum)((dynamic)1);
        Assert.Equal(typeof(TEnum), e.GetType());
        var actual = caster.CastTo<TEnum, TNumeric>(e);
        var expected = (TNumeric)((dynamic)1);
        Assert.Equal(expected, actual);
    }

    public static IEnumerable<object[]> Cases { get; } = NumericTypes
        .SelectMany(m => EnumTypes, (t1, t2) => (t1, t2))
        .SelectMany(m => TypeCasters, (t, c) => new object[] { c, t.t2, t.t1 })
        .ToArray();

    [Theory]
    [MemberData(nameof(Cases))]
    public void EnumToNumTest(ITypeCaster caster, Type enumType, Type numericType)
    {
        _method.MakeGenericMethod(enumType, numericType)
            .Invoke(null, new object[] { caster });
    }
}