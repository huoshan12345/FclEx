# FclEx.EfCore

Entity Framework Core helpers for common query, update, schema, and testing scenarios.

## What Is Included

- Query helpers for get-by-id, paging, soft-delete filtering, enabled filtering, and `ContainsAny` search.
- `ExecuteSoftDeleteAsync` and dictionary-based `ExecuteUpdateAsync` helpers.
- `DbContext` save helpers with selective update exclusion.
- Change-tracker rules for common entity state behavior.
- Schema-aware `DbContext` support through `IHasSchema`, `SchemaDbContext`, and model cache keys.
- SSH tunnel support for creating a context through an SSH-forwarded connection string.
- Soft-delete index convention helpers.
- Test helpers for temporarily adding entity types to a context.

## Notes

The query helpers build EF expression trees, so provider support still depends on the active EF Core provider. Test helpers are intended for test infrastructure rather than production model configuration.
