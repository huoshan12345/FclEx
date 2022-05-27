using Xunit;

namespace ServiceStack.OrmLite.Extensions.DbConnectionExtensions
{
    public class ReadTests : DbTests
    {
        [Fact]
        public void ExistsById_Test()
        {
            using var con = OpenMemory(true);
            const int id = 1;
            con.Exists<TestEntity>(m => m.Id == id);
            var sql = con.GetLastSql();
            con.ExistsById<TestEntity>(id);
            var expected = con.GetLastSql();
            Assert.Equal(expected, sql);
        }
    }
}
