using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FclEx.Test.Enums;
using FclEx.TypeCasters;
using Xunit;

namespace FclEx.Test.TypeCasters
{
    public class TypeCasterTests
    {
        private static readonly ITypeCaster[] TypeCasters =
        {
            CommonTypeCaster.Instance,
            ExpressionTypeCaster.Instance,
            DelegateTypeCaster.Instance,
        };

        public static IEnumerable<object[]> Cases { get; } = TypeCasters
            .Select(m => new[] { m }).ToArray();

        [Theory]
        [MemberData(nameof(Cases))]
        public void ObjectToIntCastTest(ITypeCaster caster)
        {
            object i = 5;
            var actual = caster.CastTo<object, int>(i);
            var expected = (int)i;
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void IntToObjectCastTest(ITypeCaster caster)
        {
            var i = 5;
            var actual = caster.CastTo<int, object>(i);
            var expected = (object)i;
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void DoubleToIntCastTest(ITypeCaster caster)
        {
            var i = 5.0;
            var actual = caster.CastTo<double, int>(i);
            var expected = (int)i;
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void IntToDoubleCastTest(ITypeCaster caster)
        {
            var i = 5;
            var actual = caster.CastTo<int, double>(i);
            var expected = (double)i;
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void IntToEnumCastTest(ITypeCaster caster)
        {
            var i = 1;
            var actual = caster.CastTo<int, IntEnum>(i);
            var expected = (IntEnum)i;
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void IntToShortEnumCastTest(ITypeCaster caster)
        {
            var i = 1;
            var actual = caster.CastTo<int, ShortEnum>(i);
            var expected = (ShortEnum)i;
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void EnumToIntCastTest(ITypeCaster caster)
        {
            var i = IntEnum.Yes;
            var actual = caster.CastTo<IntEnum, int>(i);
            var expected = (int)i;
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void ShortEnumToIntCastTest(ITypeCaster caster)
        {
            var i = ShortEnum.Yes;
            var actual = caster.CastTo<ShortEnum, int>(i);
            var expected = (int)i;
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void IntToNullableCastTest(ITypeCaster caster)
        {
            var i = 1;
            var actual = caster.CastTo<int, int?>(i);
            var expected = (int?)i;
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void NullableToIntCastTest(ITypeCaster caster)
        {
            int? i = 1;
            var actual = caster.CastTo<int?, int>(i);
            var expected = (int)i;
            Assert.Equal(expected, actual);
        }
    }
}
