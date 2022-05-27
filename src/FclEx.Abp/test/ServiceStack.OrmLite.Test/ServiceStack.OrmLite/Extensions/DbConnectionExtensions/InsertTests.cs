using Xunit;

namespace ServiceStack.OrmLite.Extensions.DbConnectionExtensions
{
    public class InsertTests : DbTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void InsertObject_Test(bool selectIdentity)
        {
            using var con = OpenMemory(true);
            con.Insert(new TestEntity(), selectIdentity);
            var sql = con.GetLastSql();
            con.InsertObject(new TestEntity(), selectIdentity);
            var expected = con.GetLastSql();
            Assert.Equal(expected, sql);
        }

        [Fact]
        public void InsertBulk_Test()
        {
            var entities = Enumerable.Range(1, 10).Select(m => new TestEntity
            {
                Age = m,
                Name = m.ToString()
            }).ToDictionary(m => m.Name);

            using var con = OpenMemory(true);
            var count = con.InsertBulk(entities.Values);
            Assert.Equal(entities.Count, count);
            var exists = con.Select<TestEntity>();

            Assert.Equal(entities.Count, exists.Count);
            foreach (var exist in exists)
            {
                Assert.True(entities.TryGetValue(exist.Name, out var entity));
                Assert.Equal(entity.Age, exist.Age);
                Assert.Equal(entity.Gender, exist.Gender);
            }
        }
    }
}
