using Xunit;

namespace ServiceStack.OrmLite.Extensions.DbConnectionExtensions
{
    public class ReadAsyncTests : DbTests
    {
        [Fact]
        public async Task ExistsByIdAsync_Test()
        {
            using var con = await OpenMemoryAsync(true);
            const int id = 1;
            await con.ExistsAsync<TestEntity>(m => m.Id == id);
            var sql = con.GetLastSql();
            await con.ExistsByIdAsync<TestEntity>(id);
            var expected = con.GetLastSql();
            Assert.Equal(expected, sql);
        }
    }
}
