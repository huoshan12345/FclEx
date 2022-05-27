using Xunit;

namespace ServiceStack.OrmLite.Extensions.SqlExpExtensions
{
    partial class WhereTests
    {
        [Fact]
        public void IsValid_Test()
        {
            {
                // string
                var sql = CreateExp<TestEntity>().IsValid(m => m.Name);
                var expected = CreateExp().Where(m => m.Name != null).Where(m => m.Name != string.Empty);
                Assert.Equal(expected.WhereExpression, sql.WhereExpression);
            }
            {
                // int
                var sql = CreateExp<TestEntity>().IsValid(m => m.Age);
                var expected = CreateExp().Where(m => m.Age != 0);
                Assert.Equal(expected.WhereExpression, sql.WhereExpression);
            }
            {
                // nullable int
                var sql = CreateExp<TestEntity>().IsValid(m => m.NullableInt);
                var expected = CreateExp().Where(m => m.NullableInt != null).Where(m => m.NullableInt != 0);
                Assert.Equal(expected.WhereExpression, sql.WhereExpression);
            }
        }

        [Fact]
        public void NotEqual_Test()
        {
            {
                // string vs empty
                var sql = CreateExp<TestEntity>().NotEqual(m => m.Name, string.Empty);
                var expected = CreateExp().Where(m => m.Name != string.Empty);
                Assert.Equal(expected.WhereExpression, sql.WhereExpression);
            }
            {
                // string vs null
                var sql = CreateExp<TestEntity>().NotEqual(m => m.Name, null);
                var expected = CreateExp().Where(m => m.Name != null);
                Assert.Equal(expected.WhereExpression, sql.WhereExpression);
            }
            {
                // int vs int
                var sql = CreateExp<TestEntity>().NotEqual(m => m.Age, 1);
                var expected = CreateExp().Where(m => m.Age != 1);
                Assert.Equal(expected.WhereExpression, sql.WhereExpression);
            }
            {
                // nullable int vs int
                var sql = CreateExp<TestEntity>().NotEqual(m => m.NullableInt, 1);
                var expected = CreateExp().Where(m => m.NullableInt != 1);
                Assert.Equal(expected.WhereExpression, sql.WhereExpression);
            }
            {
                // nullable int vs null
                var sql = CreateExp<TestEntity>().NotEqual(m => m.NullableInt, null);
                var expected = CreateExp().Where(m => m.NullableInt != null);
                Assert.Equal(expected.WhereExpression, sql.WhereExpression);
            }
        }
    }
}
