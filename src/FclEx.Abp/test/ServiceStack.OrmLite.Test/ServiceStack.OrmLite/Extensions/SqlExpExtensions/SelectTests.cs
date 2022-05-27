using Xunit;

namespace ServiceStack.OrmLite.Extensions.SqlExpExtensions
{
    public class SelectTests : DbTests
    {
        [Fact]
        public void Select_Test()
        {
            var sql = CreateExp<TestEntity>()
                .Select((s, m) => new
                {
                    Count = Sql.Max(m.Id),
                    Time = "to_char({0}, 'YYYY-MM-DD-HH24')".Fmt(s.Column(x => x.CreationTime, true))
                });

            Assert.Equal("SELECT Max(\"Id\") AS Count, to_char(\"TestEntity\".\"CreationTime\", \'YYYY-MM-DD-HH24\')", sql.SelectExpression);
        }
    }
}
