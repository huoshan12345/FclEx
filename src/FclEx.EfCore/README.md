# FclEx.EfCore

Entity Framework Core helpers for FclEx.

## What Is Included

- Query helpers for EF Core `IQueryable` and `DbContext` workflows.
- Update and change-application helpers.
- Context service-registration helpers.
- Soft-delete helpers and entity-state utilities.
- Relational schema helpers.
- SSH tunnel helpers for database access during local or integration workflows.
- Test-model and test-data conveniences used by EF-oriented tests.

## Usage Notes

- This package targets `net8.0`, `net9.0`, and `net10.0`.
- It depends on `Microsoft.EntityFrameworkCore.Relational` and `FclEx.Core`.
- Keep provider-specific SQL behavior in application code or provider-specific packages.

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
