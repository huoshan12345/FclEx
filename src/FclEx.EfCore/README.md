# FclEx.EfCore

Entity Framework Core helpers for FclEx.

## What Is Included

- Query helpers for EF Core `IQueryable` and `DbContext` workflows.
- Update and change-application helpers.
- Soft-delete helpers and entity-state utilities.
- Relational schema helpers.
- SSH tunnel helpers for database access during local or integration workflows.
- Test-model and test-data conveniences used by EF-oriented tests.

## Usage Notes

- This package targets `net8.0`, `net9.0`, and `net10.0`.
- It depends on `Microsoft.EntityFrameworkCore.Relational` and `FclEx.Core`.
- Keep provider-specific SQL behavior in application code or provider-specific packages.
