using FclEx.Test.Enums;
using Xunit;

namespace FclEx.Test.ObjectExtensions
{
    public class CastToTests
    {
        [Fact]
        public void ObjectToIntCastTest()
        {
            object i = 5;
            var actual = i.CastTo<int>();
            var expected = (int)i;
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IntToObjectCastTest()
        {
            var i = 5;
            var actual = i.CastTo<object>();
            var expected = (object)i;
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void DoubleToIntCastTest()
        {
            var i = 5.0;
            var actual = i.CastTo<int>();
            var expected = (int)i;
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IntToDoubleCastTest()
        {
            var i = 5;
            var actual = i.CastTo<double>();
            var expected = (double) i;
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IntToEnumCastTest()
        {
            var i = 1;
            var actual = i.CastTo<IntEnum>();
            var expected = (IntEnum)i;
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IntToShortEnumCastTest()
        {
            var i = 1;
            var actual = i.CastTo<ShortEnum>();
            var expected = (ShortEnum)i;
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void EnumToIntCastTest()
        {
            var i = IntEnum.Yes;
            var actual = i.CastTo<int>();
            var expected = (int)i;
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ShortEnumToIntCastTest()
        {
            var i = ShortEnum.Yes;
            var actual = i.CastTo<int>();
            var expected = (int)i;
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IntToNullableCastTest()
        {
            var i = 1;
            var actual = i.CastTo<int?>();
            var expected = (int?)i;
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NullableToIntCastTest()
        {
            int? i = 1;
            var actual = i.CastTo<int>();
            var expected = (int)i;
            Assert.Equal(expected, actual);
        }
    }
}
