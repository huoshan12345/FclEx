namespace FclEx.EfCore;

/// <summary>Provides physical-table operations for entity sets.</summary>
public static class DbSetExtensions
{
    /// <summary>Truncates the entire physical table mapped exclusively to this entity set.</summary>
    /// <typeparam name="TEntity">The CLR type of the entity set.</typeparam>
    /// <param name="dbSet">The entity set identifying the table, including named shared-type entity sets.</param>
    /// <param name="cancellationToken">A token to observe while executing the command.</param>
    /// <returns>A task that completes when the table has been truncated.</returns>
    /// <exception cref="ArgumentNullException">The entity set is null.</exception>
    /// <exception cref="NotSupportedException">The connection type or table mapping is unsupported.</exception>
    /// <remarks>
    /// Uses native truncation or the SQLite DELETE fallback and bypasses query filters and soft deletion.
    /// Does not synchronize tracked entities. See <see cref="DbContextExtensions.TruncateAsync(DbContext, Type, CancellationToken)"/>
    /// for supported connections, identity behavior, mapping restrictions, and database side effects.
    /// </remarks>
    public static Task TruncateAsync<TEntity>(
        this DbSet<TEntity> dbSet,
        CancellationToken cancellationToken = default)
        where TEntity : class
    {
        Check.NotNull(dbSet);
        var context = dbSet.GetService<ICurrentDbContext>().Context;
        return context.TruncateAsync(dbSet.EntityType, cancellationToken);
    }

    /// <summary>Truncates this entity set's entire physical table with explicit identity and cascade behavior.</summary>
    /// <typeparam name="TEntity">The CLR type of the entity set.</typeparam>
    /// <param name="dbSet">The entity set identifying the table, including named shared-type entity sets.</param>
    /// <param name="restartIdentity">Whether to reset identity values. SQL Server and MySQL require true; Oracle requires false. SQLite controls AUTOINCREMENT sequences.</param>
    /// <param name="cascade">Whether PostgreSQL or Oracle should also truncate referencing tables. Oracle requires ON DELETE CASCADE constraints.</param>
    /// <param name="cancellationToken">A token to observe while executing the command.</param>
    /// <returns>A task that completes when truncation has completed.</returns>
    /// <exception cref="ArgumentNullException">The entity set is null.</exception>
    /// <exception cref="NotSupportedException">The connection type, options, or table mapping are unsupported.</exception>
    /// <remarks>
    /// Bypasses query filters and soft deletion and does not synchronize tracked entities.
    /// See <see cref="DbContextExtensions.TruncateAsync(DbContext, Type, bool, bool, CancellationToken)"/>
    /// for option support, cascade side effects, mapping restrictions, and transaction behavior.
    /// </remarks>
    public static Task TruncateAsync<TEntity>(
        this DbSet<TEntity> dbSet,
        bool restartIdentity,
        bool cascade,
        CancellationToken cancellationToken = default)
        where TEntity : class
    {
        Check.NotNull(dbSet);
        var context = dbSet.GetService<ICurrentDbContext>().Context;
        return context.TruncateAsync(dbSet.EntityType, restartIdentity, cascade, cancellationToken);
    }
}
