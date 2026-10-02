using Microsoft.EntityFrameworkCore.Storage;

namespace FclEx.EfCore;

public static partial class DbContextExtensions
{
    /// <summary>
    /// Truncates the entire physical table mapped to the specified entity type using the provider's native behavior.
    /// </summary>
    /// <param name="context">The context whose model and connection are used.</param>
    /// <param name="entityClrType">The CLR type of a non-shared entity mapped exclusively to one table.</param>
    /// <param name="cancellationToken">A token to observe while executing the command.</param>
    /// <returns>A task that completes when the table has been truncated.</returns>
    /// <exception cref="ArgumentNullException">The context or entity CLR type is null.</exception>
    /// <exception cref="InvalidOperationException">The CLR type does not identify an entity in the model.</exception>
    /// <exception cref="NotSupportedException">The provider or table mapping is unsupported.</exception>
    /// <remarks>
    /// Supports SQL Server, PostgreSQL, and the Oracle, Pomelo, and Microting MySQL providers.
    /// SQL Server and MySQL reset identity values; PostgreSQL preserves them.
    /// SQLite and unknown providers are rejected; this method never falls back to DELETE.
    /// Inheritance, multi-table mappings, and tables shared by multiple entities are rejected.
    /// All rows are removed regardless of query filters or soft-delete rules. Tracked entities are not synchronized.
    /// No cascading option is enabled. Foreign keys and other database restrictions can prevent truncation.
    /// Uses the current transaction, if any, but transaction and rollback behavior depend on the database;
    /// in particular, MySQL TRUNCATE causes an implicit commit. No transaction or execution-strategy retry is started.
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
    /// <param name="restartIdentity">Whether to reset identity values. SQL Server and MySQL require true.</param>
    /// <param name="cascade">Whether PostgreSQL should also truncate tables that reference the target table.</param>
    /// <param name="cancellationToken">A token to observe while executing the command.</param>
    /// <returns>A task that completes when truncation has completed.</returns>
    /// <exception cref="ArgumentNullException">The context or entity CLR type is null.</exception>
    /// <exception cref="InvalidOperationException">The CLR type does not identify an entity in the model.</exception>
    /// <exception cref="NotSupportedException">The provider, options, or table mapping are unsupported.</exception>
    /// <remarks>
    /// PostgreSQL supports all option combinations. SQL Server and MySQL support only restartIdentity=true,
    /// cascade=false; other combinations are rejected before opening the connection.
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
    /// <exception cref="NotSupportedException">The provider or table mapping is unsupported.</exception>
    /// <remarks>
    /// Uses the provider's native identity and transaction behavior and bypasses query filters and soft deletion.
    /// Does not synchronize tracked entities. See <see cref="TruncateAsync(DbContext, Type, CancellationToken)"/>
    /// for supported providers, mapping restrictions, and database side effects.
    /// </remarks>
    public static Task TruncateAsync<TEntity>(this DbContext context, CancellationToken cancellationToken = default)
        where TEntity : class
    {
        return context.TruncateAsync(typeof(TEntity), cancellationToken);
    }

    /// <summary>Truncates an entity's entire physical table with explicit identity and cascade behavior.</summary>
    /// <typeparam name="TEntity">The non-shared entity type whose table is truncated.</typeparam>
    /// <param name="context">The context whose model and connection are used.</param>
    /// <param name="restartIdentity">Whether to reset identity values. SQL Server and MySQL require true.</param>
    /// <param name="cascade">Whether PostgreSQL should also truncate referencing tables.</param>
    /// <param name="cancellationToken">A token to observe while executing the command.</param>
    /// <returns>A task that completes when truncation has completed.</returns>
    /// <exception cref="ArgumentNullException">The context is null.</exception>
    /// <exception cref="InvalidOperationException">The CLR type does not identify an entity in the model.</exception>
    /// <exception cref="NotSupportedException">The provider, options, or table mapping are unsupported.</exception>
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

    /// <summary>Truncates the entire physical table identified by entity metadata using the provider's native behavior.</summary>
    /// <param name="context">The context whose model and connection are used.</param>
    /// <param name="entityType">Entity metadata from this context's runtime model, including named shared-type entities.</param>
    /// <param name="cancellationToken">A token to observe while executing the command.</param>
    /// <returns>A task that completes when the table has been truncated.</returns>
    /// <exception cref="ArgumentNullException">The context or entity metadata is null.</exception>
    /// <exception cref="ArgumentException">The entity metadata does not belong to the context's runtime model.</exception>
    /// <exception cref="NotSupportedException">The provider or table mapping is unsupported.</exception>
    /// <remarks>
    /// Uses native identity and transaction behavior, bypasses query filters and soft deletion,
    /// and does not synchronize tracked entities. The entity must exclusively map to one table without inheritance.
    /// See <see cref="TruncateAsync(DbContext, Type, CancellationToken)"/> for supported providers and database side effects.
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
    /// <param name="restartIdentity">Whether to reset identity values. SQL Server and MySQL require true.</param>
    /// <param name="cascade">Whether PostgreSQL should also truncate referencing tables.</param>
    /// <param name="cancellationToken">A token to observe while executing the command.</param>
    /// <returns>A task that completes when truncation has completed.</returns>
    /// <exception cref="ArgumentNullException">The context or entity metadata is null.</exception>
    /// <exception cref="ArgumentException">The entity metadata does not belong to the context's runtime model.</exception>
    /// <exception cref="NotSupportedException">The provider, options, or table mapping are unsupported.</exception>
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

        var provider = context.Database.ProviderName;
        switch (provider)
        {
            case "Microsoft.EntityFrameworkCore.SqlServer":
            case "Npgsql.EntityFrameworkCore.PostgreSQL":
            case "MySql.EntityFrameworkCore":
            case "Pomelo.EntityFrameworkCore.MySql":
            case "Microting.EntityFrameworkCore.MySql":
                break;
            default:
                throw new NotSupportedException($"TRUNCATE is not supported for provider '{provider ?? "<none>"}'.");
        }

        var isPostgreSql = provider == "Npgsql.EntityFrameworkCore.PostgreSQL";
        if (isPostgreSql == false)
        {
            if (restartIdentity == false)
                throw new NotSupportedException($"Provider '{provider}' cannot truncate a table without resetting identity values.");
            if (cascade)
                throw new NotSupportedException($"Provider '{provider}' does not support TRUNCATE CASCADE.");
        }

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
        // Identifiers come from the model and are escaped by the active provider.
        var sql = $"TRUNCATE TABLE {qualifiedTableName}";
        if (isPostgreSql)
        {
            if (restartIdentity is { } restart)
                sql += restart ? " RESTART IDENTITY" : " CONTINUE IDENTITY";
            if (cascade)
                sql += " CASCADE";
        }
        sql += ";";
        return context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }
}
