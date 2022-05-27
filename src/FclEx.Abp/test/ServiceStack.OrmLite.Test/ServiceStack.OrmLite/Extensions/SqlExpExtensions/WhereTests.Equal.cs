using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace ServiceStack.OrmLite.Extensions.SqlExpExtensions
{
    [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
    partial class WhereTests
    {
        [Fact]
        public void WhereById_Test()
        {
            {
                var sql = CreateExp<TestEntity>().WhereById(25);
                var expected = CreateExp().Where(m => m.Id == 25);
                Assert.Equal(expected.WhereExpression, sql.WhereExpression);
            }
            {

                var sql = CreateExp<TestEntity>().WhereById((object)null);
#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
                var expected = CreateExp().Where(m => m.Id == null);
#pragma warning restore CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
                Assert.Equal(expected.WhereExpression, sql.WhereExpression);
            }
        }

        [Theory]
        [InlineData(5)]
        [InlineData(0)]
        [InlineData(null)]
        public void Equal_NullableInt_Test(int? input)
        {
            var sql = CreateExp<TestEntity>().Equal(m => m.NullableInt, input);
            var expected = CreateExp().Where(m => m.NullableInt == input);
            Assert.Equal(expected.WhereExpression, sql.WhereExpression);
        }

        [Theory]
        [InlineData(5)]
        [InlineData(0)]
        [InlineData(null)]
        public void EqualIfValid_Nullable_Test(int? input)
        {
            var sql = CreateExp<TestEntity>().EqualIfValid(m => m.NullableInt, input);
            var expected = CreateExp().WhereIf(input.IsValid(), m => m.NullableInt == input);
            Assert.Equal(expected.WhereExpression, sql.WhereExpression);
        }

        [Theory]
        [InlineData("xxxxxxx")]
        [InlineData("")]
        [InlineData(null)]
        public void EqualIfValid_String_Test(string input)
        {
            var sql = CreateExp<TestEntity>().EqualIfValid(m => m.Name, input);
            var expected = CreateExp().WhereIf(input.IsValid(), m => m.Name == input);
            Assert.Equal(expected.WhereExpression, sql.WhereExpression);
        }

        [Fact]
        public void EqualIfValid_Test()
        {
            {
                var sql = CreateExp<TestEntity>().EqualIfValid(m => m.Age, 0);
                Assert.Null(sql.WhereExpression);
            }
            {
                var sql = CreateExp<TestEntity>().EqualIfValid(m => m.CreationTime, default(DateTime));
                Assert.Null(sql.WhereExpression);
            }
        }

        [Theory]
        [InlineData(Status.Working)]
        [InlineData(Status.None)]
        [InlineData(null)]
        public void EqualIfValid_IntAndEnum_Test(Status? input)
        {
            var sql = CreateExp<TestEntity>().EqualIfValid(m => m.Age, input);
            var expected = CreateExp().WhereIf(input.IsValid(), m => m.Age == input.GetValueOrDefault().ToInt());
            Assert.Equal(expected.WhereExpression, sql.WhereExpression);
        }
    }
}
