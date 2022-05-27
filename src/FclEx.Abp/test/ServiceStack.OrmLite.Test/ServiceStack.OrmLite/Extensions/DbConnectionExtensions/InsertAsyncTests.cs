using Xunit;

namespace ServiceStack.OrmLite.Extensions.DbConnectionExtensions
{
    public class InsertAsyncTests : DbTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task InsertObjectAsync_Test(bool selectIdentity)
        {
            using var con = await OpenMemoryAsync(true);
            await con.InsertAsync(new TestEntity(), selectIdentity);
            var sql = con.GetLastSql();
            await con.InsertObjectAsync(new TestEntity(), selectIdentity);
            var expected = con.GetLastSql();
            Assert.Equal(expected, sql);
        }

        [Fact]
        public async Task InsertBulkAsync_Test()
        {
            var entities = Enumerable.Range(1, 10).Select(m => new TestEntity
            {
                Age = m,
                Name = m.ToString()
            }).ToDictionary(m => m.Name);

            using var con = await OpenMemoryAsync(true);
            var count = await con.InsertBulkAsync(entities.Values);
            Assert.Equal(entities.Count, count);
            var exists = await con.SelectAsync<TestEntity>();

            Assert.Equal(entities.Count, exists.Count);
            foreach (var exist in exists)
            {
                Assert.True(entities.TryGetValue(exist.Name, out var entity));
                Assert.Equal(entity.Age, exist.Age);
                Assert.Equal(entity.Gender, exist.Gender);
            }
        }

        [Fact]
        public async Task InsertAsync_WithGuidKey_Test()
        {
            var entity = new TestEntityWithGuidKey
            {
                Name = nameof(TestEntityWithGuidKey),
                Age = 10,
            };
            using var con = await OpenMemoryAsync(true);
            var count = await con.InsertAsync(entity);
            Assert.Equal(1, count);
            var exists = await con.SelectAsync<TestEntityWithGuidKey>();
            Assert.Single(exists);
            var exist = exists.First();
            Assert.Equal(entity.Name, exist.Name);
        }
    }
}
