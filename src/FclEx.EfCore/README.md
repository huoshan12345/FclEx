# FclEx.EfCore

Entity Framework Core helpers for FclEx.

## What Is Included

- Query helpers for EF Core `IQueryable` and `DbContext` workflows.
- Update and change-application helpers.
- Context service-registration helpers.
- Soft-delete helpers and entity-state utilities.
- Relational schema, physical-table truncation, and identity-sequence synchronization helpers.
- SSH tunnel helpers for database access during local or integration workflows.
- Test-model and test-data conveniences used by EF-oriented tests.

## Usage Notes

- This package targets `net8.0`, `net9.0`, and `net10.0`.
- It depends on `Microsoft.EntityFrameworkCore.Relational` and `FclEx.Core`.
- Keep provider-specific SQL behavior in application code or provider-specific packages.

### Truncating a table

```csharp
await context.TruncateAsync<Customer>(cancellationToken);
await context.Set<Customer>().TruncateAsync(cancellationToken);
await context.TruncateAsync(context.Model.FindEntityType(typeof(Customer))!, cancellationToken);
```

`TruncateAsync` removes every row from the entity's entire physical table, ignoring
query filters and soft-delete rules. It does not synchronize tracked entities.
Only exclusive, single-table mappings without inheritance are supported; multi-table
and shared-table mappings are rejected. A named shared-type entity set is supported
when it owns its table exclusively; use the `DbSet` or `IEntityType` overload to
preserve its identity. The `IEntityType` overloads require metadata from the context's
runtime model; metadata from a different model throws `ArgumentException`.

Database dialects are recognized by the connection's assembly and type name (including
base types), independently of the EF provider name. SQL Server, PostgreSQL, MySQL,
Oracle Database, and Microsoft.Data.Sqlite connections are supported; unknown
connections throw `NotSupportedException`.

The overloads accepting `restartIdentity` and `cascade` require both boolean values
explicitly, with no defaults:

```csharp
await context.TruncateAsync<Customer>(restartIdentity: true, cascade: false, cancellationToken: cancellationToken);
```

| Database | Without boolean options | Explicit options |
| --- | --- | --- |
| SQL Server / MySQL | Native TRUNCATE; resets identity | Requires `restartIdentity: true, cascade: false` |
| PostgreSQL | Native TRUNCATE; preserves identity | All combinations supported |
| Oracle Database | Native TRUNCATE; preserves identity | Requires `restartIdentity: false`; CASCADE requires `ON DELETE CASCADE` foreign keys and Oracle 12c or later |
| SQLite | DELETE; resets the table's AUTOINCREMENT sequence when present | Resets or preserves the AUTOINCREMENT sequence; requires `cascade: false` |

Unsupported option combinations throw before opening the connection. PostgreSQL and
Oracle `CASCADE` can truncate additional referencing tables outside the EF model;
mapping validation applies only to the target table. Foreign keys and other database
restrictions may prevent truncation.

SQLite resolves temporary tables before main tables and resets the sequence in the
same database as the deleted table. A same-named main table is unaffected when a
temporary table shadows it. Attached SQLite databases are outside this API's scope.

SQLite has DELETE semantics: delete triggers and configured foreign-key actions run,
even with `cascade: false` (that flag controls the native TRUNCATE CASCADE clause).
Sequence reset is parameterized and works when `sqlite_sequence` does not exist.
For an ordinary INTEGER PRIMARY KEY without AUTOINCREMENT, SQLite allocates ROWIDs
according to its own rules; `restartIdentity: false` cannot preserve a removed maximum ROWID.

Commands use the current transaction when present. SQLite starts a transaction when
none exists so row deletion and sequence reset are atomic. Native transaction semantics
follow the database: MySQL and Oracle TRUNCATE cause implicit commits. The helper does
not retry through an execution strategy. Existing tracked entities remain unchanged;
clear tracking or use a fresh context before inserting rows that may reuse old keys.

### Synchronizing an identity sequence

```csharp
bool synchronized = await context.ReseedIdentityAsync<Customer>(cancellationToken);
await context.ReseedIdentityAsync(typeof(Customer), cancellationToken);
await context.ReseedIdentityAsync(context.Model.FindEntityType(typeof(Customer))!, cancellationToken);
```

The model must identify one generated integer primary-key property and exactly one exclusively owned physical
table, without inheritance or multi-table mapping. Table, schema, and key-column names come from EF metadata,
including Fluent API mappings and shadow keys. Use the metadata overload for named shared-type entities.
Returns `false` when the physical table or native identity generator is absent; invalid mappings and database
errors throw. Query filters and soft-delete rules are bypassed. Pending changes are not saved and tracked
entities are not updated.

| Database | Synchronization behavior |
| --- | --- |
| PostgreSQL 10+ | Sets the owned sequence's next key to MAX + its positive increment; empty tables restart at the sequence start. |
| SQL Server | Reseeds an ascending identity to MAX; a previously used empty identity restarts at its seed. |
| MySQL / MariaDB | Requests AUTO_INCREMENT = MAX + 1, or 1 when empty. The engine may retain a higher counter and native increment/offset settings still apply. |
| SQLite | Resets AUTOINCREMENT to MAX, or zero when empty. Ordinary ROWID returns false; temp shadows main and attached databases are excluded. |
| Oracle 12.2+ | Uses START WITH LIMIT VALUE, retaining generation mode and identity options; native limit calculation determines the next key. |

The helper uses EF's relational command pipeline, including command interceptors, configured command timeout,
and the current transaction. An initially closed connection is closed on completion. SQLite starts and commits
its own transaction when none exists, rolling back both sequence commands on failure. Other providers use native
rollback rules: PostgreSQL sequence changes survive rollback, and MySQL/Oracle DDL implicitly commit. Maintenance
commands do not run through an execution-strategy retry. Pause concurrent writes and sequence use during this
operation; resetting a generator may reuse deleted keys. Client-generated, HiLo, trigger-managed, and unrelated
standalone sequences are outside the operation's scope.

### Keywords with LIKE wildcards

`ContainsAny` treats keywords literally by default. Pass `escapeWildcards: false`
to allow `%` (zero or more characters) and `_` (one character) in keywords:

```csharp
query.ContainsAny(x => x.Name, ["foo%bar", "item_"], escapeWildcards: false);
```

Each keyword is still surrounded by `%` for substring matching. Backslashes remain
literal in both modes; a backslash cannot escape an individual wildcard in this input format.
Opening square brackets are escaped by default for SQL Server. For Oracle, pass
`escapeBrackets: false`: brackets are already literal, and `LIKE ... ESCAPE` rejects
a backslash followed by a bracket. This option is independent of `escapeWildcards`.
Use `QueryableHelper.BuildLike` for a
provider-ready pattern with explicit control over the entire LIKE expression.

### Re-registering a context

Call `RemoveDbContext<TContext>()` before `AddDbContext<TContext>()` to replace
an existing context registration with new options or lifetimes:

```csharp
services.RemoveDbContext<AppDbContext>();
services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
```

This removes every registration for `TContext` and `DbContextOptions<TContext>`,
plus `IDbContextOptionsConfiguration<TContext>` on EF Core 9 and later. Repeated
calls are harmless, and registrations for other context types are preserved.

Use it before building the service provider. It does not remove service aliases,
context factories, pooling services, or non-generic `DbContextOptions` registrations.
The remaining non-generic options registrations can fail to resolve until the typed
options are registered again. Existing providers and context instances are unaffected.
