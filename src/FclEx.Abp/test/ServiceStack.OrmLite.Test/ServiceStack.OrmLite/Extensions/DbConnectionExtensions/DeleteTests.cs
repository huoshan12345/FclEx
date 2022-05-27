using Xunit;

namespace ServiceStack.OrmLite.Extensions.DbConnectionExtensions
{
    public class DeleteTests : DbTests
    {
        [Fact]
        public void DeleteById_Test()
        {
            using var con = OpenMemory(true);
            var id = con.Insert(new TestEntity(), true);
            var exist = con.Exists<TestEntity>(m => m.Id == id);
            Assert.True(exist);
            con.DeleteById(typeof(TestEntity), id);
            exist = con.Exists<TestEntity>(m => m.Id == id);
            Assert.False(exist);
        }
    }
}
