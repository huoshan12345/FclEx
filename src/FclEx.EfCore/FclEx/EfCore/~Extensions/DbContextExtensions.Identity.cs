using Microsoft.EntityFrameworkCore.Storage;

namespace FclEx.EfCore;

partial class DbContextExtensions
{
    /// <summary>Synchronizes an entity's native identity generator with its existing physical-table keys.</summary>
    /// <typeparam name="TEntity">The non-shared entity with one database-generated integer key.</typeparam>
    /// <param name="context">The context supplying the model, connection, transaction, and command timeout.</param>
    /// <param name="cancellationToken">Cancels connection opening, metadata reads, and maintenance commands.</param>
    /// <returns>True when synchronized; false when the table or native identity generator is absent.</returns>
    /// <remarks>See the Type overload for mapping requirements, database behavior, and transaction side effects.</remarks>
    /// <exception cref="ArgumentNullException">The context is null.</exception>
    /// <exception cref="InvalidOperationException">The CLR type does not identify a non-shared model entity.</exception>
    /// <exception cref="NotSupportedException">The connection, entity mapping, or identity configuration is unsupported.</exception>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    public static Task<bool> ReseedIdentityAsync<TEntity>(
        this DbContext context, CancellationToken cancellationToken = default) where TEntity : class
        => context.ReseedIdentityAsync(typeof(TEntity), cancellationToken);

    /// <summary>Synchronizes the native identity generator for a specified mapped CLR entity type.</summary>
    /// <param name="context">The context supplying the model and relational command settings; its connection state is preserved.</param>
    /// <param name="entityClrType">A non-shared entity mapped exclusively to one table, with one generated integer key.</param>
    /// <param name="cancellationToken">Cancels opening, metadata reads, and maintenance commands.</param>
    /// <returns>True when synchronized; false when the physical table or native identity generator is absent.</returns>
    /// <remarks>
    /// Reads table, schema, and key-column names from the EF model and bypasses query filters and soft-delete rules.
    /// Inheritance, multi-table mappings, and table sharing are rejected. Pending changes are not saved and tracked
    /// entities are not modified. An OnAdd integer key must correspond to a native identity/owned sequence;
    /// client-generated, HiLo, trigger-managed, and unrelated standalone sequences are not synchronized.
    /// SQL Server and PostgreSQL 10+ support ascending generators and retain their increment and starting value.
    /// SQL Server reseeds to MAX(key), resetting a previously used empty identity to its seed. PostgreSQL sets
    /// the next value to MAX(key)+increment, or the sequence start when empty. MySQL requests MAX(key)+1, or 1
    /// when empty, but may retain a higher counter; native increment/offset settings still apply. SQLite resets
    /// AUTOINCREMENT to MAX(key), or zero when empty, resolving temp before main; ordinary ROWID returns false
    /// and attached databases are excluded. Oracle 12.2+ uses START WITH LIMIT VALUE and preserves generation
    /// mode and identity options; its next key follows native limit calculation.
    /// Uses the current transaction and configured timeout, without execution-strategy retries. SQLite uses a
    /// locally owned transaction when none exists. PostgreSQL sequence changes survive rollback. MySQL and
    /// Oracle DDL implicitly commit. Pause concurrent writes and sequence use during maintenance; this operation
    /// does not coordinate writers and resetting a generator may reuse deleted keys. Database errors propagate.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The context or CLR type is null.</exception>
    /// <exception cref="InvalidOperationException">The CLR type does not identify a non-shared entity in the model.</exception>
    /// <exception cref="NotSupportedException">The connection, mapping, or identity configuration is unsupported.</exception>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    public static Task<bool> ReseedIdentityAsync(
        this DbContext context, Type entityClrType, CancellationToken cancellationToken = default)
    {
        Check.NotNull(context);
        Check.NotNull(entityClrType);
        var entityType = context.Model.FindEntityType(entityClrType)
            ?? throw new InvalidOperationException($"{entityClrType.Name} does not identify a non-shared entity in this DbContext.");
        return context.ReseedIdentityAsync(entityType, cancellationToken);
    }

    /// <summary>Synchronizes the identity generator identified by metadata from the context's runtime model.</summary>
    /// <param name="context">The context supplying the connection, transaction, and timeout.</param>
    /// <param name="entityType">Runtime-model entity metadata, including a named shared-type entity owning its table exclusively.</param>
    /// <param name="cancellationToken">Cancels opening and execution.</param>
    /// <returns>True when synchronized; false when the physical table or native identity generator is absent.</returns>
    /// <remarks>Uses the same mapping restrictions, database semantics, and side effects as the Type overload.</remarks>
    /// <exception cref="ArgumentNullException">The context or entity metadata is null.</exception>
    /// <exception cref="ArgumentException">The metadata belongs to another runtime model.</exception>
    /// <exception cref="NotSupportedException">The connection, mapping, or identity configuration is unsupported.</exception>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    public static Task<bool> ReseedIdentityAsync(
        this DbContext context, IEntityType entityType, CancellationToken cancellationToken = default)
    {
        var table = GetExclusiveEntityTable(context, entityType);
        var keys = entityType.FindPrimaryKey()?.Properties.Where(property => property.ValueGenerated == ValueGenerated.OnAdd).ToArray();
        if (keys is not { Length: 1 })
            throw new NotSupportedException("Identity synchronization requires exactly one database-generated integer key.");
        var key = keys[0];
        var keyType = key.GetTypeMapping().Converter?.ProviderClrType ?? key.ClrType;
        keyType = Nullable.GetUnderlyingType(keyType) ?? keyType;
        if (keyType != typeof(byte) && keyType != typeof(sbyte) && keyType != typeof(short) && keyType != typeof(ushort)
            && keyType != typeof(int) && keyType != typeof(uint) && keyType != typeof(long) && keyType != typeof(ulong))
            throw new NotSupportedException("Identity synchronization requires an integer key store type.");
        var column = key.GetColumnName(StoreObjectIdentifier.Table(table.Name, table.Schema))
            ?? throw new NotSupportedException("The generated key is not mapped to the entity's physical table.");
        var dialect = GetRelationalDialect(context.Database.GetDbConnection().GetType());
        cancellationToken.ThrowIfCancellationRequested();
        return ReseedIdentityCoreAsync(context, table.Name, table.Schema, column, dialect, cancellationToken);
    }

    private static async Task<bool> ReseedIdentityCoreAsync(
        DbContext context, string tableName, string? schema, string columnName,
        RelationalDialect dialect, CancellationToken cancellationToken)
    {
        using var criticalSection = context.GetService<IConcurrencyDetector>().EnterCriticalSection();
        await context.Database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await using var transaction = dialect == RelationalDialect.Sqlite && context.Database.CurrentTransaction is null
                ? await context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false) : null;
            var commands = new IdentitySequenceCommands(context, dialect, tableName, schema, columnName);
            var result = await commands.ReseedAsync(cancellationToken).ConfigureAwait(false);
            if (transaction is not null)
                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return result;
        }
        finally
        {
            await context.Database.CloseConnectionAsync().ConfigureAwait(false);
        }
    }
}
