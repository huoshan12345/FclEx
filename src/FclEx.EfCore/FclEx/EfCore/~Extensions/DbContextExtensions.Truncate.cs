using Microsoft.EntityFrameworkCore.Storage;

namespace FclEx.EfCore;

public static partial class DbContextExtensions
{
    /// <summary>
    /// Truncates the entire physical table mapped to the specified entity type using native truncation or the SQLite DELETE fallback.
    /// </summary>
    /// <param name="context">The context whose model and connection are used.</param>
    /// <param name="entityClrType">The CLR type of a non-shared entity mapped exclusively to one table.</param>
    /// <param name="cancellationToken">A token to observe while executing the command.</param>
    /// <returns>A task that completes when the table has been truncated.</returns>
    /// <exception cref="ArgumentNullException">The context or entity CLR type is null.</exception>
    /// <exception cref="InvalidOperationException">The CLR type does not identify an entity in the model.</exception>
    /// <exception cref="NotSupportedException">The connection type or table mapping is unsupported.</exception>
    /// <remarks>
    /// Recognizes SQL Server, PostgreSQL, MySQL, Oracle, and SQLite by the connection's assembly and type name,
    /// including derived connection types, independently of the EF provider name.
    /// SQL Server and MySQL reset identity values; PostgreSQL and Oracle preserve them.
    /// SQLite deletes all rows and resets the table's AUTOINCREMENT sequence if one exists.
    /// Inheritance, multi-table mappings, and tables shared by multiple entities are rejected.
    /// All rows are removed regardless of query filters or soft-delete rules. Tracked entities are not synchronized.
    /// No TRUNCATE CASCADE option is enabled. Foreign keys and other database restrictions can prevent truncation.
    /// SQLite uses DELETE semantics: delete triggers and configured foreign-key actions execute.
    /// Uses the current transaction, if any. SQLite starts a transaction when none exists so deleting rows
    /// and resetting the sequence are atomic. Native transaction and rollback behavior depend on the database;
    /// MySQL and Oracle TRUNCATE cause implicit commits. No execution-strategy retry is started.
    /// </remarks>
    public static Task TruncateAsync(
        this DbContext context,
        Type entityClrType,
        CancellationToken cancellationToken = default)
    {
        Check.NotNull(context);
        Check.NotNull(entityClrType);

        var entityType = context.Model.FindEntityType(entityClrType)
            ?? throw new InvalidOperationException($"{entityClrType.Name} does not identify a non-shared entity in this DbContext.");

        return TruncateTableAsync(context, entityType, null, false, cancellationToken);
    }

    /// <summary>Truncates an entity's entire physical table with explicit identity and cascade behavior.</summary>
    /// <param name="context">The context whose model and connection are used.</param>
    /// <param name="entityClrType">The CLR type of a non-shared entity mapped exclusively to one table.</param>
    /// <param name="restartIdentity">Whether to reset identity values. SQL Server and MySQL require true; Oracle requires false. SQLite controls AUTOINCREMENT sequences.</param>
    /// <param name="cascade">Whether PostgreSQL or Oracle should also truncate referencing tables. Oracle requires ON DELETE CASCADE constraints.</param>
    /// <param name="cancellationToken">A token to observe while executing the command.</param>
    /// <returns>A task that completes when truncation has completed.</returns>
    /// <exception cref="ArgumentNullException">The context or entity CLR type is null.</exception>
    /// <exception cref="InvalidOperationException">The CLR type does not identify an entity in the model.</exception>
    /// <exception cref="NotSupportedException">The connection type, options, or table mapping are unsupported.</exception>
    /// <remarks>
    /// PostgreSQL supports all option combinations. SQL Server and MySQL require restartIdentity=true, cascade=false.
    /// Oracle requires restartIdentity=false and supports CASCADE for references with ON DELETE CASCADE (Oracle 12c or later).
    /// SQLite requires cascade=false and can reset or preserve AUTOINCREMENT sequences; ordinary ROWID allocation
    /// follows SQLite's rules regardless of this option. Unsupported combinations are rejected before opening the connection.
    /// CASCADE can truncate additional tables outside the EF model; mapping validation applies only to the target table.
    /// Query filters and soft-delete rules are bypassed, and tracked entities are not synchronized.
    /// See <see cref="TruncateAsync(DbContext, Type, CancellationToken)"/> for mapping restrictions and transaction behavior.
    /// </remarks>
    public static Task TruncateAsync(
        this DbContext context,
        Type entityClrType,
        bool restartIdentity,
        bool cascade,
        CancellationToken cancellationToken = default)
    {
        Check.NotNull(context);
        Check.NotNull(entityClrType);

        var entityType = context.Model.FindEntityType(entityClrType)
            ?? throw new InvalidOperationException($"{entityClrType.Name} does not identify a non-shared entity in this DbContext.");

        return TruncateTableAsync(context, entityType, restartIdentity, cascade, cancellationToken);
    }

    /// <summary>Truncates the entire physical table mapped exclusively to <typeparamref name="TEntity"/>.</summary>
    /// <typeparam name="TEntity">The non-shared entity type whose table is truncated.</typeparam>
    /// <param name="context">The context whose model and connection are used.</param>
    /// <param name="cancellationToken">A token to observe while executing the command.</param>
    /// <returns>A task that completes when the table has been truncated.</returns>
    /// <exception cref="ArgumentNullException">The context is null.</exception>
    /// <exception cref="InvalidOperationException">The CLR type does not identify an entity in the model.</exception>
    /// <exception cref="NotSupportedException">The connection type or table mapping is unsupported.</exception>
    /// <remarks>
    /// Uses the documented database identity and transaction behavior and bypasses query filters and soft deletion.
    /// Does not synchronize tracked entities. See <see cref="TruncateAsync(DbContext, Type, CancellationToken)"/>
    /// for supported connections, the SQLite fallback, mapping restrictions, and database side effects.
    /// </remarks>
    public static Task TruncateAsync<TEntity>(this DbContext context, CancellationToken cancellationToken = default)
        where TEntity : class
    {
        return context.TruncateAsync(typeof(TEntity), cancellationToken);
    }

    /// <summary>Truncates an entity's entire physical table with explicit identity and cascade behavior.</summary>
    /// <typeparam name="TEntity">The non-shared entity type whose table is truncated.</typeparam>
    /// <param name="context">The context whose model and connection are used.</param>
    /// <param name="restartIdentity">Whether to reset identity values. SQL Server and MySQL require true; Oracle requires false. SQLite controls AUTOINCREMENT sequences.</param>
    /// <param name="cascade">Whether PostgreSQL or Oracle should also truncate referencing tables. Oracle requires ON DELETE CASCADE constraints.</param>
    /// <param name="cancellationToken">A token to observe while executing the command.</param>
    /// <returns>A task that completes when truncation has completed.</returns>
    /// <exception cref="ArgumentNullException">The context is null.</exception>
    /// <exception cref="InvalidOperationException">The CLR type does not identify an entity in the model.</exception>
    /// <exception cref="NotSupportedException">The connection type, options, or table mapping are unsupported.</exception>
    /// <remarks>
    /// Bypasses query filters and soft deletion and does not synchronize tracked entities.
    /// See <see cref="TruncateAsync(DbContext, Type, bool, bool, CancellationToken)"/> for option support,
    /// cascade side effects, mapping restrictions, and transaction behavior.
    /// </remarks>
    public static Task TruncateAsync<TEntity>(
        this DbContext context,
        bool restartIdentity,
        bool cascade,
        CancellationToken cancellationToken = default)
        where TEntity : class
    {
        return context.TruncateAsync(typeof(TEntity), restartIdentity, cascade, cancellationToken);
    }

    /// <summary>Truncates the entire physical table identified by entity metadata using native truncation or the SQLite DELETE fallback.</summary>
    /// <param name="context">The context whose model and connection are used.</param>
    /// <param name="entityType">Entity metadata from this context's runtime model, including named shared-type entities.</param>
    /// <param name="cancellationToken">A token to observe while executing the command.</param>
    /// <returns>A task that completes when the table has been truncated.</returns>
    /// <exception cref="ArgumentNullException">The context or entity metadata is null.</exception>
    /// <exception cref="ArgumentException">The entity metadata does not belong to the context's runtime model.</exception>
    /// <exception cref="NotSupportedException">The connection type or table mapping is unsupported.</exception>
    /// <remarks>
    /// Uses the documented identity and transaction behavior, bypasses query filters and soft deletion,
    /// and does not synchronize tracked entities. The entity must exclusively map to one table without inheritance.
    /// See <see cref="TruncateAsync(DbContext, Type, CancellationToken)"/> for supported connections, the SQLite fallback, and database side effects.
    /// </remarks>
    public static Task TruncateAsync(
        this DbContext context,
        IEntityType entityType,
        CancellationToken cancellationToken = default)
    {
        return TruncateTableAsync(context, entityType, null, false, cancellationToken);
    }

    /// <summary>Truncates the entire physical table identified by entity metadata with explicit identity and cascade behavior.</summary>
    /// <param name="context">The context whose model and connection are used.</param>
    /// <param name="entityType">Entity metadata from this context's runtime model, including named shared-type entities.</param>
    /// <param name="restartIdentity">Whether to reset identity values. SQL Server and MySQL require true; Oracle requires false. SQLite controls AUTOINCREMENT sequences.</param>
    /// <param name="cascade">Whether PostgreSQL or Oracle should also truncate referencing tables. Oracle requires ON DELETE CASCADE constraints.</param>
    /// <param name="cancellationToken">A token to observe while executing the command.</param>
    /// <returns>A task that completes when truncation has completed.</returns>
    /// <exception cref="ArgumentNullException">The context or entity metadata is null.</exception>
    /// <exception cref="ArgumentException">The entity metadata does not belong to the context's runtime model.</exception>
    /// <exception cref="NotSupportedException">The connection type, options, or table mapping are unsupported.</exception>
    /// <remarks>
    /// Bypasses query filters and soft deletion and does not synchronize tracked entities.
    /// See <see cref="TruncateAsync(DbContext, Type, bool, bool, CancellationToken)"/> for option support,
    /// cascade side effects, mapping restrictions, and transaction behavior.
    /// </remarks>
    public static Task TruncateAsync(
        this DbContext context,
        IEntityType entityType,
        bool restartIdentity,
        bool cascade,
        CancellationToken cancellationToken = default)
    {
        return TruncateTableAsync(context, entityType, restartIdentity, cascade, cancellationToken);
    }

    private static Task TruncateTableAsync(
        DbContext context,
        IEntityType entityType,
        bool? restartIdentity,
        bool cascade,
        CancellationToken cancellationToken)
    {
        Check.NotNull(context);
        Check.NotNull(entityType);
        if (ReferenceEquals(entityType.Model, context.Model) == false)
            throw new ArgumentException("The entity metadata must belong to this DbContext's runtime model.", nameof(entityType));

        var dialect = GetTruncateDialect(context.Database.GetDbConnection().GetType());
        if ((dialect is TruncateDialect.SqlServer or TruncateDialect.MySql) && restartIdentity == false)
            throw new NotSupportedException("SQL Server and MySQL TRUNCATE always reset identity values.");
        if (dialect == TruncateDialect.Oracle && restartIdentity == true)
            throw new NotSupportedException("Oracle TRUNCATE does not restart identity sequences.");
        if (cascade && dialect is not (TruncateDialect.PostgreSql or TruncateDialect.Oracle))
            throw new NotSupportedException("TRUNCATE CASCADE is supported only for PostgreSQL and Oracle connections.");

        if (entityType.BaseType is not null || entityType.GetDerivedTypes().Any())
            throw new NotSupportedException("TRUNCATE does not support entity inheritance mappings.");

        var mappings = entityType.GetTableMappings().ToArray();
        if (mappings.Length != 1)
            throw new NotSupportedException("TRUNCATE requires an entity mapped to exactly one physical table.");

        var table = mappings[0].Table;
        if (table.EntityTypeMappings.Any(mapping => mapping.TypeBase != entityType))
            throw new NotSupportedException("TRUNCATE does not support tables shared by multiple entity types.");

        var sqlHelper = context.GetService<ISqlGenerationHelper>();
        var qualifiedTableName = sqlHelper.DelimitIdentifier(table.Name, table.Schema);
        cancellationToken.ThrowIfCancellationRequested();
        if (dialect == TruncateDialect.Sqlite)
            return DeleteSqliteTableAsync(context, qualifiedTableName, table.Name, restartIdentity != false, cancellationToken);

        // Identifiers come from the model and are escaped by the active provider.
        var sql = $"TRUNCATE TABLE {qualifiedTableName}";
        if (dialect == TruncateDialect.PostgreSql)
        {
            if (restartIdentity is { } restart)
                sql += restart ? " RESTART IDENTITY" : " CONTINUE IDENTITY";
        }
        if (cascade)
            sql += " CASCADE";
        // Oracle commands must not include a SQL*Plus statement terminator.
        if (dialect != TruncateDialect.Oracle)
            sql += ";";
        return context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    internal enum TruncateDialect
    {
        SqlServer,
        PostgreSql,
        MySql,
        Oracle,
        Sqlite,
    }

    internal static TruncateDialect GetTruncateDialect(Type connectionType)
    {
        for (var type = connectionType; type is not null; type = type.BaseType)
        {
            var dialect = (type.Assembly.GetName().Name, type.FullName) switch
            {
                ("Microsoft.Data.SqlClient", "Microsoft.Data.SqlClient.SqlConnection") => TruncateDialect.SqlServer,
                ("System.Data.SqlClient", "System.Data.SqlClient.SqlConnection") => TruncateDialect.SqlServer,
                ("Npgsql", "Npgsql.NpgsqlConnection") => TruncateDialect.PostgreSql,
                ("MySql.Data", "MySql.Data.MySqlClient.MySqlConnection") => TruncateDialect.MySql,
                ("MySqlConnector", "MySqlConnector.MySqlConnection") => TruncateDialect.MySql,
                ("Oracle.ManagedDataAccess", "Oracle.ManagedDataAccess.Client.OracleConnection") => TruncateDialect.Oracle,
                ("Microsoft.Data.Sqlite", "Microsoft.Data.Sqlite.SqliteConnection") => TruncateDialect.Sqlite,
                _ => (TruncateDialect?)null,
            };
            if (dialect is { } supported)
                return supported;
        }
        throw new NotSupportedException($"TRUNCATE is not supported for connection type '{connectionType.FullName}'.");
    }

    private static async Task DeleteSqliteTableAsync(
        DbContext context,
        string qualifiedTableName,
        string tableName,
        bool restartIdentity,
        CancellationToken cancellationToken)
    {
        // Keep deleting rows and resetting the sequence atomic, and respect caller-owned transactions.
        await using var transaction = context.Database.CurrentTransaction is null
            ? await context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false)
            : null;
        var deleteSql = $"DELETE FROM {qualifiedTableName};";
        await context.Database.ExecuteSqlRawAsync(deleteSql, cancellationToken).ConfigureAwait(false);
        if (restartIdentity)
        {
            // sqlite_sequence exists only after an AUTOINCREMENT table has been created.
            var hasSequence = await context.Database.SqlQueryRaw<int>(
                "SELECT COUNT(*) AS Value FROM sqlite_master WHERE type = 'table' AND name = 'sqlite_sequence'")
                .SingleAsync(cancellationToken).ConfigureAwait(false);
            if (hasSequence != 0)
                await context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM sqlite_sequence WHERE name = {0};", new object[] { tableName }, cancellationToken).ConfigureAwait(false);
        }
        if (transaction is not null)
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}
