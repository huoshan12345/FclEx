using Xunit;

namespace ServiceStack.OrmLite.Extensions.DbConnectionExtensions
{
    public class DeleteAsyncTests : DbTests
    {
        [Fact]
        public async Task DeleteByIdAsync_Test()
        {
            using var con = await OpenMemoryAsync(true);
            var id = await con.InsertAsync(new TestEntity(), true);
            var exist = await con.ExistsAsync<TestEntity>(m => m.Id == id);
            Assert.True(exist);
            await con.DeleteByIdAsync(typeof(TestEntity), id);
            exist = await con.ExistsAsync<TestEntity>(m => m.Id == id);
            Assert.False(exist);
        }

        [Fact]
        public async Task DeleteAllAsync_Test()
        {
            using var con = await OpenMemoryAsync(true);
            await con.DeleteAllAsync(typeof(TestEntity));
            var sql = con.GetLastSql();
            await con.DeleteAllAsync<TestEntity>();
            var expected = con.GetLastSql();
            Assert.Equal(expected, sql);
        }
    }
}
