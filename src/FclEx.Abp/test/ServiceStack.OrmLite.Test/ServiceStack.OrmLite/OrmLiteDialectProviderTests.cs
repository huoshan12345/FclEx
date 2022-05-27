using System.Data.SQLite;
using Xunit;

namespace ServiceStack.OrmLite
{
    public class OrmLiteDialectProviderTests
    {
        [Fact]
        public void IfDatabaseExists_File_Test()
        {
            var fac = DbTests.ConFacOfFile;
            var conStr = new SQLiteConnectionStringBuilder(fac.ConnectionString);
            var actual = fac.DialectProvider.IfDatabaseExists(fac.ConnectionString);
            Assert.Equal(File.Exists(conStr.DataSource), actual);
        }

        [Fact]
        public void IfDatabaseExists_Memory_Test()
        {
            var fac = DbTests.ConFacOfMemory;
            var actual = fac.DialectProvider.IfDatabaseExists(fac.ConnectionString);
            Assert.False(actual);
        }

        [Fact]
        public async Task IfDatabaseExistsAsync_File_Test()
        {
            var fac = DbTests.ConFacOfFile;
            var conStr = new SQLiteConnectionStringBuilder(fac.ConnectionString);
            var actual = await fac.DialectProvider.IfDatabaseExistsAsync(fac.ConnectionString);
            Assert.Equal(File.Exists(conStr.DataSource), actual);
        }

        [Fact]
        public async Task IfDatabaseExistsAsync_Memory_Test()
        {
            var fac = DbTests.ConFacOfMemory;
            var actual = await fac.DialectProvider.IfDatabaseExistsAsync(fac.ConnectionString);
            Assert.False(actual);
        }

        [Fact]
        public void CreateDatabase_File_Test()
        {
            var fac = DbTests.ConFacOfFile;
            var conStr = new SQLiteConnectionStringBuilder(fac.ConnectionString);
            File.Delete(conStr.DataSource);
            Assert.False(File.Exists(conStr.DataSource));
            fac.DialectProvider.CreateDatabase(fac.ConnectionString);
            Assert.True(File.Exists(conStr.DataSource));
        }


        [Fact]
        public async Task CreateDatabaseAsync_File_Test()
        {
            var fac = DbTests.ConFacOfFile;
            var conStr = new SQLiteConnectionStringBuilder(fac.ConnectionString);
            File.Delete(conStr.DataSource);
            Assert.False(File.Exists(conStr.DataSource));
            await fac.DialectProvider.CreateDatabaseAsync(fac.ConnectionString);
            Assert.True(File.Exists(conStr.DataSource));
        }
    }
}
