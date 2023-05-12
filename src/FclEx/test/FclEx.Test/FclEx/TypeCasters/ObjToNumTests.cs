using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FclEx.Extensions;

namespace FclEx.TypeCasters;

public sealed class ObjToNumTests : AbstractTests
{
    private static readonly MethodInfo _method = typeof(ObjToNumTests).GetMethod(
        nameof(ObjToNum), BindingFlags.NonPublic | BindingFlags.Static);

    private static void ObjToNum<TNumericObject, TNumeric>(ITypeCaster caster)
        where TNumericObject : struct, IComparable<TNumericObject>, IEquatable<TNumericObject>
        where TNumeric : struct, IComparable<TNumeric>, IEquatable<TNumeric>
    {
        object obj = (TNumericObject)((dynamic)1);
        Assert.Equal(typeof(TNumericObject), obj.GetType());
        var actual = caster.CastTo<object, TNumeric>(obj);
        var expected = (TNumeric)((dynamic)1);
        Assert.Equal(typeof(TNumeric), expected.GetType());
        Assert.Equal(expected, actual);
    }

    public static IEnumerable<object[]> Cases { get; } = NumericTypes
        .SelectMany((t1, t2) => (t1, t2))
        .SelectMany(m => TypeCasters.Except(ExpressionTypeCaster.Instance),
            (t, c) => new object[] { c, t.t1, t.t2 })
        .ToArray();

    [Theory]
    [MemberData(nameof(Cases))]
    public void ObjToNumTest(ITypeCaster caster, Type numericObjectType, Type numericType)
    {
        _method.MakeGenericMethod(numericObjectType, numericType)
            .Invoke(null, new object[] { caster });
    }
}