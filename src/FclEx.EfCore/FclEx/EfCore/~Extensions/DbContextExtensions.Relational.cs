using Microsoft.EntityFrameworkCore.Storage;

namespace FclEx.EfCore;

partial class DbContextExtensions
{
    internal enum RelationalDialect
    {
        SqlServer,
        PostgreSql,
        MySql,
        Oracle,
        Sqlite,
    }

    internal static RelationalDialect GetRelationalDialect(Type connectionType)
    {
        for (var type = connectionType; type is not null; type = type.BaseType)
        {
            var dialect = (type.Assembly.GetName().Name, type.FullName) switch
            {
                ("Microsoft.Data.SqlClient", "Microsoft.Data.SqlClient.SqlConnection") => RelationalDialect.SqlServer,
                ("System.Data.SqlClient", "System.Data.SqlClient.SqlConnection") => RelationalDialect.SqlServer,
                ("Npgsql", "Npgsql.NpgsqlConnection") => RelationalDialect.PostgreSql,
                ("MySql.Data", "MySql.Data.MySqlClient.MySqlConnection") => RelationalDialect.MySql,
                ("MySqlConnector", "MySqlConnector.MySqlConnection") => RelationalDialect.MySql,
                ("Oracle.ManagedDataAccess", "Oracle.ManagedDataAccess.Client.OracleConnection") => RelationalDialect.Oracle,
                ("Microsoft.Data.Sqlite", "Microsoft.Data.Sqlite.SqliteConnection") => RelationalDialect.Sqlite,
                _ => (RelationalDialect?)null,
            };
            if (dialect is { } supported)
                return supported;
        }
        throw new NotSupportedException($"Relational table maintenance is not supported for connection type '{connectionType.FullName}'.");
    }

    private static ITable GetExclusiveEntityTable(DbContext context, IEntityType entityType)
    {
        Check.NotNull(context);
        Check.NotNull(entityType);
        if (!ReferenceEquals(entityType.Model, context.Model))
            throw new ArgumentException("The entity metadata must belong to this DbContext's runtime model.", nameof(entityType));
        if (entityType.BaseType is not null || entityType.GetDerivedTypes().Any())
            throw new NotSupportedException("Table maintenance does not support entity inheritance mappings.");

        var mappings = entityType.GetTableMappings().ToArray();
        if (mappings.Length != 1)
            throw new NotSupportedException("Table maintenance requires an entity mapped to exactly one physical table.");

        var table = mappings[0].Table;
        if (table.EntityTypeMappings.Any(mapping => mapping.TypeBase != entityType))
            throw new NotSupportedException("Table maintenance does not support tables shared by multiple entity types.");

        return table;
    }
}