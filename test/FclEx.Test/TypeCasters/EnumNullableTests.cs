using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using FclEx.TypeCasters;
using Xunit;

namespace FclEx.Test.TypeCasters
{
    public class EnumNullableTests : AbstractTests
    {
        private static readonly MethodInfo _methodOfEnumToNullable = typeof(EnumNullableTests).GetMethod(
            nameof(EnumToNullable), BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo _methodOfNullableToEnum = typeof(EnumNullableTests).GetMethod(
            nameof(NullableToEnum), BindingFlags.NonPublic | BindingFlags.Static);

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
}
