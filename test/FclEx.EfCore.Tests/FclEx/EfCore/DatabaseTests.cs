using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace FclEx.EfCore;

[CollectionDefinition(nameof(EfCoreTestsCollection))]
public class EfCoreTestsCollection : ICollectionFixture<EfCoreFixture>;

[Collection(nameof(EfCoreTestsCollection))]
public class DatabaseTests
{
    public static readonly IEnumerable<object?[]> DbTestCases = DatabaseTypes
        .Select(m => new object[] { m });

    public static readonly IEnumerable<object?[]> SchemaCases = Schemas.Select(m => new object?[] { m });

    public static readonly IEnumerable<object?[]> DbSchemaTestCases = DatabaseTypes
        .SelectMany(m => Schemas, (x, y) => (x, y))
        .Select(m => new object?[] { m.x, m.y });


    public static DbParameter CreateParameter(DbProviderType dbProviderType, string name, object value)
    {
        return dbProviderType switch
        {
            DbProviderType.SqlServer => new SqlParameter(name, value),
            DbProviderType.Sqlite => new SqliteParameter(name, value),
            DbProviderType.Npgsql => new NpgsqlParameter(name, value),
#if !DISABLE_MYSQL
            DbProviderType.MySql => new MySqlParameter(name, value),
            DbProviderType.MySqlConnector => new MySqlConnector.MySqlParameter(name, value),
#endif
            _ => throw new ArgumentOutOfRangeException(nameof(dbProviderType), dbProviderType, null)
        };
    }
}