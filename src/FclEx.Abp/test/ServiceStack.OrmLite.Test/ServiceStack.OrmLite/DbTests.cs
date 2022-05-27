using System.Data;
using ServiceStack.OrmLite.Sqlite;

namespace ServiceStack.OrmLite
{
    public class DbTests
    {
        public static OrmLiteConnectionFactory ConFacOfMemory { get; } = new("Data Source=:memory:;", SqliteOrmLiteDialectProvider.Instance) { AutoDisposeConnection = true };
        public static OrmLiteConnectionFactory ConFacOfFile { get; } = new("Data Source=./db.sqlite;", SqliteOrmLiteDialectProvider.Instance);

        private static void Init(IDbConnection con, bool createTable = false)
        {
            if (createTable)
            {
                con.CreateTable<TestEntity>(true);
                con.CreateTable<TestEntityWithGuidKey>(true);
            }
        }

        public static IDbConnection OpenMemory(bool createTable = false)
        {
            var con = ConFacOfMemory.Open();
            Init(con, createTable);
            return con;
        }
        public static async Task<IDbConnection> OpenMemoryAsync(bool createTable = false)
        {
            var con = await ConFacOfMemory.OpenAsync().DonotCapture();
            Init(con, createTable);
            return con;
        }

        public static IDbConnection OpenFile(bool createTable = false)
        {
            var con = ConFacOfFile.Open();
            Init(con, createTable);
            return con;
        }

        public static async Task<IDbConnection> OpenFileAsync(bool createTable = false)
        {
            var con = await ConFacOfFile.OpenAsync().DonotCapture();
            Init(con, createTable);
            return con;
        }

        public static SqlExpression<T> CreateExp<T>(IOrmLiteDialectProvider provider = null)
        {
            return (provider ?? SqliteOrmLiteDialectProvider.Instance).SqlExpression<T>();
        }

        public static SqlExpression<TestEntity> CreateExp(IOrmLiteDialectProvider provider = null)
        {
            return CreateExp<TestEntity>(provider);
        }
    }
}
