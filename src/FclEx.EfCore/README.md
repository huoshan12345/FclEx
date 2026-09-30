# FclEx.EfCore

Entity Framework Core helpers for FclEx.

## What Is Included

- Query helpers for EF Core `IQueryable` and `DbContext` workflows.
- Update and change-application helpers.
- Context service-registration helpers.
- Soft-delete helpers and entity-state utilities.
- Relational schema and physical-table truncation helpers.
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
```

`TruncateAsync` removes every row from the entity's entire physical table, ignoring
query filters and soft-delete rules. It does not synchronize tracked entities.
Only exclusive, single-table mappings without inheritance are supported; multi-table
and shared-table mappings are rejected. A named shared-type entity set is supported
when it owns its table exclusively; use the `DbSet` overload to preserve its identity.

SQL Server, PostgreSQL, and the Oracle, Pomelo, and Microting MySQL providers are
supported. SQLite and unknown providers throw `NotSupportedException`; no `DELETE`
fallback is performed. SQL Server and MySQL reset identity values, while PostgreSQL
preserves them when using the overloads without boolean options.

The overloads accepting `restartIdentity` and `cascade` require both boolean values
explicitly, with no defaults:

```csharp
await context.TruncateAsync<Customer>(restartIdentity: true, cascade: false, cancellationToken: cancellationToken);
```

PostgreSQL supports all combinations, generating `RESTART IDENTITY` or `CONTINUE
IDENTITY`, and optionally `CASCADE`. SQL Server and MySQL accept only
`restartIdentity: true, cascade: false`; other combinations throw
`NotSupportedException` before opening the connection. PostgreSQL `CASCADE` can
truncate additional referencing tables, including tables outside the EF model.
Mapping validation applies only to the target table.

Foreign keys and other database restrictions may prevent truncation. The command
uses the current transaction, if any, without starting a transaction or retrying
through an execution strategy. Transaction semantics follow the database: MySQL
`TRUNCATE` causes an implicit commit and cannot be rolled back like an ordinary delete.

### Keywords with LIKE wildcards

`ContainsAny` treats keywords literally by default. Pass `escapeWildcards: false`
to allow `%` (zero or more characters) and `_` (one character) in keywords:

```csharp
query.ContainsAny(x => x.Name, ["foo%bar", "item_"], escapeWildcards: false);
```

Each keyword is still surrounded by `%` for substring matching. Backslashes and
opening brackets remain literal in both modes; a backslash cannot escape an
individual wildcard in this input format. Use `QueryableHelper.BuildLike` for a
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
