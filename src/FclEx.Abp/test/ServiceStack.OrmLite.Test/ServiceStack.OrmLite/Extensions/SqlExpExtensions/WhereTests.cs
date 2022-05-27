using Xunit;

namespace ServiceStack.OrmLite.Extensions.SqlExpExtensions
{
    public partial class WhereTests : DbTests
    {
        [Fact]
        public void ContainsAny_Test()
        {
            var keywords = new[] { "keyword1", "keyword2", "keyword3" };
            var sql = CreateExp<TestEntity>().ContainsAny(m => m.Name, keywords);
            var expected = CreateExp()
                .Where(m => m.Name.Contains("keyword1")
                            || m.Name.Contains("keyword2")
                            || m.Name.Contains("keyword3"));
            Assert.Equal(expected.WhereExpression, sql.WhereExpression);
        }

        [Fact]
        public void ContainsAny_WithCondition_Test()
        {
            var keywords = new[] { "keyword1", "keyword2", "keyword3" };
            var sql = CreateExp<TestEntity>()
                .Where(m => m.Id != 0)
                .ContainsAny(m => m.Name, keywords)
                .Where(m => m.Age > 0);

            var expected = CreateExp()
                .Where(m => m.Id != 0)
                .Where(m => m.Name.Contains("keyword1")
                            || m.Name.Contains("keyword2")
                            || m.Name.Contains("keyword3"))
                .Where(m => m.Age > 0);
            Assert.Equal(expected.WhereExpression, sql.WhereExpression);
        }

        [Fact]
        public void StartsWithAny_Test()
        {
            var keywords = new[] { "keyword1", "keyword2", "keyword3" };
            var sql = CreateExp<TestEntity>().StartsWithAny(m => m.Name, keywords);
            var expected = CreateExp()
                .Where(m => m.Name.StartsWith("keyword1")
                            || m.Name.StartsWith("keyword2")
                            || m.Name.StartsWith("keyword3"));
            Assert.Equal(expected.WhereExpression, sql.WhereExpression);
        }

        [Fact]
        public void StartsWithAny_WithCondition_Test()
        {
            var keywords = new[] { "keyword1", "keyword2", "keyword3" };
            var sql = CreateExp<TestEntity>()
                .Where(m => m.Id != 0)
                .StartsWithAny(m => m.Name, keywords)
                .Where(m => m.Age > 0);

            var expected = CreateExp()
                .Where(m => m.Id != 0)
                .Where(m => m.Name.StartsWith("keyword1")
                            || m.Name.StartsWith("keyword2")
                            || m.Name.StartsWith("keyword3"))
                .Where(m => m.Age > 0);
            Assert.Equal(expected.WhereExpression, sql.WhereExpression);
        }
    }
}
