using Xunit;

namespace ServiceStack.OrmLite.Extensions.SqlExpExtensions
{
    public class OrderByTests : DbTests
    {
        [Fact]
        public void OrderBy_Test()
        {
            var sql = CreateExp<TestEntity>()
                .OrderBy((s, m) => new
                {
                    Count = Sql.Max(m.Id),
                    Time = "to_char({0}, 'YYYY-MM-DD-HH24')".Fmt(s.Column(x => x.CreationTime, true))
                });

            Assert.Equal("\nORDER BY Max(\"Id\"), to_char(\"TestEntity\".\"CreationTime\", 'YYYY-MM-DD-HH24')", sql.OrderByExpression);
        }
    }
}
